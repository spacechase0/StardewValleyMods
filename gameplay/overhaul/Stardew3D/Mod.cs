using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoScene.Graphics;
using MonoScene.Graphics.Pipeline;
using SpaceShared;
using SpaceShared.Attributes;
using Stardew3D.Data;
using Stardew3D.Handlers;
using Stardew3D.Handlers.Game;
using Stardew3D.Handlers.Game.Editor;
using Stardew3D.Handlers.Game.FirstPerson;
using Stardew3D.Handlers.Game.FirstPersonVR;
using Stardew3D.Handlers.Game.ThirdPerson;
using Stardew3D.Handlers.Gameplay;
using Stardew3D.Handlers.Menu;
using Stardew3D.Handlers.Render;
using Stardew3D.Patches;
using Stardew3D.Rendering;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Enchantments;
using StardewValley.Events;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using Valve.VR;
using xTile.Layers;
using xTile.Tiles;

// This might be incorrect since I don't understand matrices super well, but:
//
// MonoGame docs and code say (or seem to say) the following: Row major, pre-multiplication, right handed, forward = -Z
//      Row major: operator[] docs
//      Pre-multiplication: Code in EffectHelpers.SetWorldViewProjAndFog does world * view * projection, not projection * view * world
//      Right handed: Matrix docs
//      Forward: Vector3.Forward = (0, 0, -1)
//
// OpenVR docs and code say (or seem to say) the following: Column major, post-multiplication, right handed, forward = -Z
//      Column major (logically): From https://github.com/ValveSoftware/openvr/wiki/Matrix-Usage-Example
//      Post-multiplication: Same link as previous, the example does projection * view * world
//      Right handed: openvr.h, comment right above the matrix struct definitions
//      Forward: openvr.h, same spot as previous
//
// OpenVR.Net docs and code say (or seem to say) the following: Column major, post-multiplication, left handed, forward = ?
//      Column major: It doesn't change how the values are used compared to base OpenVR
//      Post-multiplication: README shows it, the example has projection on the first of the multiplied terms
//      Left handed: README mentions it
//      Forward: ? (not sure if it even has concept of this)
//
// You might have noticed that OpenVR.Net said that their coordinates are left handed, unlike OpenVR and MonoGame.
// I don't see any conversion in the projection matrix code (not that I know if there would be any for projection matrices).
// But I *do* see conversion in their function to extract the position from an OpenVR matrix.
// (It seems like for rotations too, but it's harder to be sure because I understand quaternions even less than matrices.)
//
// So... basically we're gonna pretend OpenVR.Net's wrapper level doesn't exist for anything but initialization, cleanup, and the Update* methods. :P
// (I was using a different wrapper library previously but was having weird problems that went away when I switched...
// though that likely was just me doing things wrong)
//
// So we just gotta transpose OpenVR matrices before using them.
// 
// We try to stick with MonoGame conventions here, though that's hard when I don't know what I'm doing.

namespace Stardew3D
{
    [HasConfig<Configuration>]
    [HasContent]
    [HasState<State>]
    [HasHarmony]
    public partial class Mod : BaseMod<Mod>
    {
        public string DefaultHandler => $"{Mod.Instance.ModManifest.UniqueID}/FirstPerson";
        public string DefaultVrHandler => $"{Mod.Instance.ModManifest.UniqueID}/FirstPerson";

        protected override void ModEntry()
        {
#if false
            int expectedMajor = 4, expectedMinor = 0, expectedPatch = 0;
            if (Constants.ApiVersion.MajorVersion != expectedMajor &&
                Constants.ApiVersion.MinorVersion != expectedMinor &&
                Constants.ApiVersion.PatchVersion != expectedPatch)
            {
                Log.Error($"SMAPI version {expectedMajor}.{expectedMinor}.{expectedPatch} required! This mod will not run.");
                ShouldRun = false;
                return;
            }
#endif

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            Helper.Events.GameLoop.UpdateTicking += (s, e) => State.ActiveHandler?.BeforeUpdate();
            Helper.Events.GameLoop.UpdateTicked += (s, e) => State.ActiveHandler?.AfterUpdate();
            State.AddingGameHandlers += (s, e) =>
            {
                State.AddGameHandler(new FirstPersonGameHandler());
                State.AddGameHandler(new ThirdPersonGameHandler());
                State.AddGameHandler(new FirstPersonVRGameHandler());
                State.AddGameHandler(new EditorGameHandler());
            };
            State.GameHandlersFinalized += (s, e) =>
            {
                State.SetRenderHandlerForGameHandlerTags<GameLocation>([], handler => obj => new LocationRenderer(obj as GameLocation));
                State.SetRenderHandlerForGameHandlerTags<Item>([], handler => obj => new ItemRenderer<ModelData, Item>(obj as Item));
                State.SetRenderHandlerForGameHandlerTags<StardewValley.Object>([], handler => obj => new ObjectRenderer(obj as StardewValley.Object));
                State.SetRenderHandlerForGameHandlerTags<Tool>([], handler => obj => new ToolRenderer(obj as Tool));
                State.SetRenderHandlerForGameHandlerTags<TV>([], handler => obj => new TelevisionRenderer(obj as TV));
                State.SetRenderHandlerForGameHandlerTags<TerrainFeature>([], handler => obj => new RendererFor<ModelData, TerrainFeature>(obj as TerrainFeature));
                State.SetRenderHandlerForGameHandlerTags<ResourceClump>([], handler => obj => new ResourceClumpRenderer(obj as ResourceClump));
                State.SetRenderHandlerForGameHandlerTags<Tree>([], handler => obj => new TreeRenderer(obj as Tree));
                //State.SetRenderHandlerForGameHandlerTags<FruitTree>([], handler => obj => new FruitTreeRenderer(obj as FruitTree));
                //State.SetRenderHandlerForGameHandlerTags<Flooring>([], handler => obj => new FlooringRenderer(obj as Flooring));
                State.SetRenderHandlerForGameHandlerTags<Grass>([], handler => obj => new GrassRenderer(obj as Grass));
                //State.SetRenderHandlerForGameHandlerTags<HoeDirt>([], handler => obj => new HoeDirtRenderer(obj as HoeDirt));
                //State.SetRenderHandlerForGameHandlerTags<Bush>([], handler => obj => new BushRenderer(obj as Bush));
                State.SetRenderHandlerForGameHandlerTags<Character>([], handler => obj => new CharacterRenderer<ModelData, Character>(obj as Character));
                State.SetRenderHandlerForGameHandlerTags<Debris>([], handler => obj => new DebrisRenderer(obj as Debris));

                State.SetJointHandlerForGameHandlerTags<IClickableMenu, GenericMenuHandler<IClickableMenu>>([IGameHandler.CategoryVR], (handler) => (menu) => new GenericMenuHandler<IClickableMenu>(handler as VRGameHandler, menu as IClickableMenu));
                State.SetJointHandlerForGameHandlerTags<TitleMenu, TitleMenuHandler>([IGameHandler.CategoryVR], (handler) => (menu) => new TitleMenuHandler(handler as VRGameHandler, menu as TitleMenu));
                State.AddUpdateHandlerAddonForGameHandlerTags<Farmer>([IGameHandler.CategoryVR, IGameHandler.FeatureMotionControls], (handler) => (obj) => new FarmerMotionControlsHandler(handler as VRGameHandler, obj as Farmer));
                State.AddJointHandlerAddonForGameHandlerTags<Farmer, FarmerPointAndClickControlsHandler>([IGameHandler.CategoryVR, IGameHandler.FeaturePointAndClick], (handler) => (obj) => new FarmerPointAndClickControlsHandler(handler as VRGameHandler, obj as Farmer));
            };

            var hooks = AccessTools.Field(typeof(Game1), "hooks");
            hooks.SetValue(null, new MyModHooks((ModHooks)hooks.GetValue(null)));

            RenderHelper.quadVbo = new VertexBuffer(Game1.graphics.GraphicsDevice, typeof(SimpleVertex), 6, BufferUsage.WriteOnly);

            CharacterHandlers.ManualBootstrap(Harmony);
        }

        [EventPriority(EventPriority.Low)]
        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            State.InvokeAddingGameHandlers();
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (Config.ToggleThirdDimension.JustPressed())
            {
                var currHandler = State.ActiveHandler;
                var targetHandler = State.GetGameHandler(DefaultHandler);
                if (currHandler != null)
                {
                    if (currHandler.Tags.Contains(IGameHandler.CategoryFirstPerson))
                    {
                        string[] tags = currHandler.Tags.Select(t => t == IGameHandler.CategoryFirstPerson ? IGameHandler.CategoryThirdPerson : t).ToArray();
                        targetHandler = State.FindGameHandlersMatching(tags).FirstOrDefault() ?? targetHandler;
                    }
                    else
                    {
                        targetHandler = null;
                    }
                }

                State.ActiveHandler = targetHandler;
            }

            if (Config.ToggleVirtualReality.JustPressed())
            {
                var currHandler = State.ActiveHandler;
                var targetHandler = State.GetGameHandler(DefaultVrHandler);
                if (currHandler != null)
                {
                    if (!currHandler.Tags.Contains(IGameHandler.CategoryVR))
                    {
                        string[] tags = currHandler.Tags.Select(t => t == IGameHandler.CategoryFlatscreen ? IGameHandler.CategoryVR : t).ToArray();
                        targetHandler = State.FindGameHandlersMatching(tags).FirstOrDefault() ?? targetHandler;
                    }
                    else
                    {
                        targetHandler = null;
                    }
                }

                State.ActiveHandler = targetHandler;
            }

            if (Config.ToggleEditor.JustPressed())
            {
                State.ActiveHandler = !(State.ActiveHandler?.Tags.Contains(IGameHandler.CategoryEditor) ?? false)
                    ? State.FindGameHandlersMatching([IGameHandler.CategoryEditor]).FirstOrDefault()
                    : null;
            }

            if (Config.ToggleShowInteractionShapes.JustPressed())
            {
                State.RenderDebugInteractions = !State.RenderDebugInteractions;
            }


            // TODO: hook up to keybind
            if (e.Pressed.Contains(SButton.Delete))
            {
                // Can clear render caches and stuff
                State.ActiveHandler?.SwitchOff(State.ActiveHandler);
                //State.ClearHandlerState();
                State.ActiveHandler?.SwitchOn(State.ActiveHandler);
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            string mapsFolder = PathUtilities.NormalizeAssetName("Maps/meow"); // If we just do "Maps/" it removes the /, which is a big part of what we want
            mapsFolder = mapsFolder.Substring(0, mapsFolder.Length - "meow".Length);
            if (e.DataType == typeof(xTile.Map) && e.NameWithoutLocale.StartsWith(mapsFolder))
            {
                string specific = e.NameWithoutLocale.Name.Substring(mapsFolder.Length);
                string ours = Path.Combine("assets", "maps", $"{specific}.tmx");
                if (Helper.ModContent.DoesAssetExist<xTile.Map>(ours))
                {
                    e.Edit(a => a.AsMap().PatchMap(Helper.ModContent.Load<xTile.Map>(ours)), AssetEditPriority.Early);
                }
            }
        }
        internal static Texture_t GetTextureFrom(RenderTarget2D target)
        {
            // TODO: Use SMAPI reflection since it caches
            var fieldInfo = typeof(Texture2D).GetField("glTexture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var handle = new IntPtr((int)fieldInfo.GetValue(target));

            var tex = new Texture_t();
            tex.handle = handle;
            tex.eType = ETextureType.OpenGL;
            tex.eColorSpace = EColorSpace.Auto;
            return tex;
        }

        // Based on https://eecs.qmul.ac.uk/~gslabaugh/publications/euler.pdf
        public static Vector3 GetRotationFrom(Matrix mat)
        {
            // roll yaw pitch
            // o u o_?

            if (MathF.Abs(mat.M31) != 1)
            {
                var o1 = -MathF.Asin(mat.M31);
                //var o2 = MathF.PI - o1;
                var u1 = MathF.Atan2(mat.M32 / MathF.Cos(o1), mat.M33 / MathF.Cos(o1));
                //var u2 = MathF.Atan2(mat.M32 / MathF.Cos(o2), mat.M33 / MathF.Cos(o2));
                var o_1 = MathF.Atan2(mat.M21 / MathF.Cos(o1), mat.M11 / MathF.Cos(o1));
                //var o_2 = MathF.Atan2(mat.M21 / MathF.Cos(o2), mat.M11 / MathF.Cos(o2));
                return new(o1, u1, o_1);
            }
            else
            {
                var o_ = 0f;
                if (mat.M31 == -1)
                {
                    var o = MathF.PI / 2;
                    var u = o_ + MathF.Atan2(mat.M12, mat.M13);
                    return new(o, u, o_);
                }
                else
                {
                    var o = -MathF.PI / 2;
                    var u = -o_ + MathF.Atan2(-mat.M12, -mat.M13);
                    return new(o, u, o_);
                }
            }
        }
    }
}
