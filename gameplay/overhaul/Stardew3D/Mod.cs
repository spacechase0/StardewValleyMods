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
using Stardew3D.Handlers.Game.ThirdPerson;
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
using xTile.Layers;
using xTile.Tiles;

namespace Stardew3D
{
    [HasConfig<Configuration>]
    [HasContent]
    [HasState<State>]
    [HasHarmony]
    public partial class Mod : BaseMod<Mod>
    {
        public string DefaultHandler => $"{Mod.Instance.ModManifest.UniqueID}/FirstPerson";

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
    }
}
