using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.Attributes;
using Stardew3D.DataModels;
using Stardew3D.GameModes;
using Stardew3D.GameModes.Editor;
using Stardew3D.GameModes.FirstPerson;
using Stardew3D.GameModes.FirstPersonVR;
using Stardew3D.GameModes.ThirdPerson;
using Stardew3D.GameModes.VR;
using Stardew3D.Handlers;
using Stardew3D.Handlers.Gameplay;
using Stardew3D.Handlers.Menu;
using Stardew3D.Handlers.Render;
using Stardew3D.Patches;
using Stardew3D.Rendering;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Valve.VR;

// TODO: Stop using OpenVR.NET and remove this
[HarmonyPatch(typeof(Valve.VR.OpenVR), nameof(Valve.VR.OpenVR.InitInternal2))]
public static class WorkaroundMaybeBugInOpenVRDotNet
{
    public static bool Prefix(ref EVRInitError peError, EVRApplicationType eApplicationType, string pchStartupInfo, ref uint __result)
    {
        __result = Valve.VR.OpenVR.InitInternal(ref peError, eApplicationType);
        return false;
    }
}

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
        public string DefaultVrHandler => $"{Mod.Instance.ModManifest.UniqueID}/FirstPersonVR";

#if DEBUG
        public static string GetDevAssetsFolder()
        {
            return Path.Combine(GetDevModFolderImpl(), "assets");
        }

        private static string GetDevModFolderImpl([CallerFilePath]string path = "")
        {
            return Path.GetDirectoryName(path);
        }
#endif

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
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            Helper.Events.GameLoop.UpdateTicking += (s, e) => State.ActiveMode?.BeforeUpdate();
            Helper.Events.GameLoop.UpdateTicked += (s, e) => State.ActiveMode?.AfterUpdate();
            State.AddingGameModes += (s, e) =>
            {
                var state = s as State;
                state.AddGameMode(new FirstPersonGameMode());
                state.AddGameMode(new ThirdPersonGameMode());
                state.AddGameMode(new FirstPersonVRGameMode());
                state.AddGameMode(new EditorGameMode());
            };
            State.GameModesFinalized += (s, e) =>
            {
                var state = s as State;
                state.SetRenderHandlerForGameModeTags<GameLocation>([], handler => obj => new LocationRenderer(obj as GameLocation));
                state.SetRenderHandlerForGameModeTags<Item>([], handler => obj => new ItemRenderer<ModelData, Item>(obj as Item));
                state.SetRenderHandlerForGameModeTags<StardewValley.Object>([], handler => obj => new ObjectRenderer(obj as StardewValley.Object));
                state.SetRenderHandlerForGameModeTags<Tool>([], handler => obj => new ToolRenderer(obj as Tool));
                state.SetRenderHandlerForGameModeTags<TV>([], handler => obj => new TelevisionRenderer(obj as TV));
                state.SetRenderHandlerForGameModeTags<TerrainFeature>([], handler => obj => new RendererFor<ModelData, TerrainFeature>(obj as TerrainFeature));
                state.SetRenderHandlerForGameModeTags<ResourceClump>([], handler => obj => new ResourceClumpRenderer(obj as ResourceClump));
                state.SetRenderHandlerForGameModeTags<Tree>([], handler => obj => new TreeRenderer(obj as Tree));
                //state.SetRenderHandlerForGameHandlerTags<FruitTree>([], handler => obj => new FruitTreeRenderer(obj as FruitTree));
                state.SetRenderHandlerForGameModeTags<Flooring>([], handler => obj => new FlooringRenderer(obj as Flooring));
                state.SetRenderHandlerForGameModeTags<Grass>([], handler => obj => new GrassRenderer(obj as Grass));
                state.SetRenderHandlerForGameModeTags<HoeDirt>([], handler => obj => new HoeDirtRenderer(obj as HoeDirt));
                //state.SetRenderHandlerForGameHandlerTags<Bush>([], handler => obj => new BushRenderer(obj as Bush));
                state.SetRenderHandlerForGameModeTags<Character>([], handler => obj => new CharacterRenderer(obj as Character));
                state.SetRenderHandlerForGameModeTags<Debris>([], handler => obj => new DebrisRenderer(obj as Debris));
                state.SetRenderHandlerForGameModeTags<Building>([], handler => obj => new BuildingRenderer(obj as Building));
                state.SetRenderHandlerForGameModeTags<Crop>([], handler => obj => new CropRenderer(obj as Crop));

                state.AddJointHandlerAddonForGameModeTags<Farmer, FarmerPointAndClickControlsHandler>([IGameMode.FeaturePointAndClick], (handler) => (obj) => new FarmerPointAndClickControlsHandler(handler, obj as Farmer));
                
                state.SetJointHandlerForGameModeTags<IClickableMenu, GenericMenuHandler<IClickableMenu>>([IGameMode.CategoryVR], (handler) => (menu) => new GenericMenuHandler<IClickableMenu>(handler as VRGameMode, menu as IClickableMenu));
                state.SetJointHandlerForGameModeTags<TitleMenu, TitleMenuHandler>([IGameMode.CategoryVR], (handler) => (menu) => new TitleMenuHandler(handler as VRGameMode, menu as TitleMenu));
                state.AddUpdateHandlerAddonForGameModeTags<Farmer>([IGameMode.CategoryVR, IGameMode.FeatureMotionControls], (handler) => (obj) => new FarmerMotionControlsHandler(handler as VRGameMode, obj as Farmer));
            };

            RenderHelper.quadVbo = new VertexBuffer(Game1.graphics.GraphicsDevice, typeof(SimpleVertex), 6, BufferUsage.WriteOnly);

            CharacterHandlers.ManualBootstrap(Harmony);

            Helper.ConsoleCommands.Add("stardew3d_setmode", "...", (cmd, args) =>
            {
                if (!ArgUtility.TryGet(args, 0, out string modeId, out string error))
                {
                    Log.Error($"Error: {error}");
                    return;
                }

                var mode = State.GetGameMode(modeId);
                if (mode == null && modeId != "null")
                {
                    Log.Error($"Unknown mode: {modeId}");
                    Log.Info("Options are: ");
                    // ...
                    return;
                }
                State.ActiveMode = mode;
            });
        }

        [EventPriority(EventPriority.Low)]
        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            State.InvokeAddingGameModes();
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (Config.ToggleThirdDimension.JustPressed())
            {
                var currHandler = State.ActiveMode;
                var targetHandler = State.GetGameMode(DefaultHandler);
                if (currHandler != null)
                {
                    if (currHandler.Tags.Contains(IGameMode.CategoryFirstPerson))
                    {
                        string[] tags = currHandler.Tags.Select(t => t == IGameMode.CategoryFirstPerson ? IGameMode.CategoryThirdPerson : t).ToArray();
                        targetHandler = State.FindGameModesMatching(tags).FirstOrDefault() ?? targetHandler;
                    }
                    else
                    {
                        targetHandler = null;
                    }
                }

                State.ActiveMode = targetHandler;
            }

            if (Config.ToggleVirtualReality.JustPressed())
            {
                var currHandler = State.ActiveMode;
                var targetHandler = State.GetGameMode(DefaultVrHandler);
                if (currHandler != null)
                {
                    if (!currHandler.Tags.Contains(IGameMode.CategoryVR))
                    {
                        string[] tags = currHandler.Tags.Select(t => t == IGameMode.CategoryFlatscreen ? IGameMode.CategoryVR : t).ToArray();
                        targetHandler = State.FindGameModesMatching(tags).FirstOrDefault() ?? targetHandler;
                    }
                    else
                    {
                        targetHandler = null;
                    }
                }

                State.ActiveMode = targetHandler;
            }

            if (Config.ToggleEditor.JustPressed())
            {
                State.ActiveMode = !(State.ActiveMode?.Tags.Contains(IGameMode.CategoryEditor) ?? false)
                    ? State.FindGameModesMatching([IGameMode.CategoryEditor]).FirstOrDefault()
                    : null;
            }

            if (Config.ToggleShowInteractionShapes.JustPressed())
            {
                State.RenderDebugInteractions = !State.RenderDebugInteractions;
            }


            // TODO: hook up to keybind
            if (e.Pressed.Contains(SButton.Delete) && e.Pressed.Contains(SButton.LeftControl))
            {
                // Can clear render caches and stuff
                State.ActiveMode?.SwitchOff(State.ActiveMode);
                State.ClearHandlerState();
                State.ActiveMode?.SwitchOn(State.ActiveMode);
            }
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            // TODO: A better way of doing this. Is there an event for splitscreen start/end ing?
            if (Game1.hooks.GetType().Name == "SModHooks")
                Game1.hooks = new MyModHooks(Game1.hooks);
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
                    e.Edit(a =>
                    {
                        var ourMap = Helper.ModContent.Load<xTile.Map>(ours);
                        var theirMap = a.AsMap();
                        theirMap.PatchMap(ourMap);
                        foreach (var prop in ourMap.Properties)
                            theirMap.Data.Properties.Add(prop.Key, prop.Value);
                    }, AssetEditPriority.Early);
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
