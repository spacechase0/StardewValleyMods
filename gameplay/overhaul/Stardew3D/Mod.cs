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
    [HasConfig< Configuration >]
    [HasState< State >]
    [HasHarmony]
    public partial class Mod : BaseMod< Mod >
    {
        public string DefaultHandler => $"{Mod.Instance.ModManifest.UniqueID}/FirstPerson";

        internal Dictionary<string, ModelData> ModelDataDict => Helper.GameContent.Load<Dictionary<string, ModelData>>($"{ModManifest.UniqueID}/Models");
        internal Dictionary<string, InteractionData> InteractionDataDict => Helper.GameContent.Load<Dictionary<string, InteractionData>>($"{ModManifest.UniqueID}/Interactions");

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
            hooks.SetValue(null, new MyModHooks(( ModHooks ) hooks.GetValue(null)));

            RenderHelper.GenericEffect = new(Game1.graphics.GraphicsDevice)
            {
                Alpha = 1,
                VertexColorEnabled = true,
                //LightingEnabled = false, // TODO
                FogEnabled = false,
                //TextureEnabled = true,
            };
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
            if (Config.ToggleShowInteractionShapes.JustPressed())
            {
                State.RenderDebugInteractions = !State.RenderDebugInteractions;
            }
            // TODO: hook up to keybind
            if (e.Pressed.Contains(SButton.Delete))
            {
                // Can clear render caches and stuff
                State.ActiveHandler?.SwitchOff(State.ActiveHandler);
                State.ActiveHandler?.SwitchOn(State.ActiveHandler);
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            string mapsFolder = PathUtilities.NormalizeAssetName("Maps/meow"); // If we just do "Maps/" it removes the /, which is a big part of what we want
            mapsFolder = mapsFolder.Substring( 0, mapsFolder.Length - "meow".Length );
            if (e.DataType == typeof(xTile.Map) && e.NameWithoutLocale.StartsWith(mapsFolder))
            {
                string specific = e.NameWithoutLocale.Name.Substring(mapsFolder.Length);
                string ours = Path.Combine("assets", "maps", $"{specific}.tmx");
                if (Helper.ModContent.DoesAssetExist<xTile.Map>(ours))
                {
                    e.Edit(a => a.AsMap().PatchMap(Helper.ModContent.Load<xTile.Map>(ours)), AssetEditPriority.Early);
                }
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"{ModManifest.UniqueID}/WallDefinitions"))
                e.LoadFrom(() => new Dictionary<string, WallDefinitionData> // "tilesheet:tileIndex" -> data
                {
                    { $"{ModManifest.UniqueID}/GenericHouseWall", new()
                    {
                        VerticalSegments =
                        [
                            new() { Tilesheet = PathUtilities.NormalizeAssetName("Maps/walls_and_floors"), TextureRegion = new( 0, 0, 16, 16 * 3 - 7), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch },
                            new() { Tilesheet = PathUtilities.NormalizeAssetName("Maps/walls_and_floors"), TextureRegion = new( 0, 16 * 3 - 7, 16, 4) },
                        ],
                    } },
                    { $"{ModManifest.UniqueID}/GenericCliffWall", new()
                    {
                        VerticalSegments =
                        [
                            new() { Tilesheet = PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet"), TextureRegion = new( 17 * 16, 18 * 16, 16, 16) },
                            new() { Tilesheet = PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet"), TextureRegion = new( 17 * 16, 19 * 16, 16, 16), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
                            new() { Tilesheet = PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet"), TextureRegion = new( 17 * 16, 20 * 16, 16, 16) },
                        ],
                    } },
                    { $"{ModManifest.UniqueID}/GenericCaveWall", new()
                    {
                        VerticalSegments =
                        [
                            new() { Tilesheet = PathUtilities.NormalizeAssetName("Maps/Mines/mine"), TextureRegion = new( 10 * 16, 4 * 16, 16, 16) },
                            new() { Tilesheet = PathUtilities.NormalizeAssetName("Maps/Mines/mine"), TextureRegion = new( 10 * 16, 5 * 16, 16, 32), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
                            new() { Tilesheet = PathUtilities.NormalizeAssetName("Maps/Mines/mine"), TextureRegion = new( 10 * 16, 6 * 16, 16, 16) },
                        ],
                    } },
                    { $"{ModManifest.UniqueID}/BusTunnelWall", new()
                    {
                        VerticalSegments =
                        [
                            new() { Tilesheet = PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet"), TextureRegion = new( 80, 1088, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
                        ],
                    } },
                    { $"{ModManifest.UniqueID}/BusTunnelEdgeWall", new()
                    {
                        VerticalSegments =
                        [
                            new() { Tilesheet = PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet"), TextureRegion = new( 96, 1072, 16, 9 ) },
                        ],
                    } },
                }, AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo($"{ModManifest.UniqueID}/FloorWallAssociations"))
                e.LoadFrom(() => new Dictionary<string, FloorWallAssociationData> // "tilesheet:tileIndex" -> data
                {
                    { $"Wood", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericHouseWall" } },
                    { $"Dirt", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"Grass", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"Stone", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCaveWall" } },
#if true
                    { $"{PathUtilities.NormalizeAssetName("Maps/walls_and_floors")}:352", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericHouseWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/walls_and_floors")}:353", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericHouseWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/walls_and_floors")}:336", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericHouseWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/walls_and_floors")}:337", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericHouseWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:460", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCaveWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:509", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCaveWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:217", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCaveWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:1625", new() { WallDefinitionId = $"{ModManifest.UniqueID}/BusTunnelWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:1675", new() { WallDefinitionId = $"{ModManifest.UniqueID}/BusTunnelWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:1026", new() { WallDefinitionId = $"{ModManifest.UniqueID}/BusTunnelWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:562", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:512", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:566", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:537", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:618", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:176", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:488", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:560", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:207", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:623", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:591", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:611", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:589", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:614", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:587", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:564", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:153", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:226", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:513", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:231", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:463", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:206", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:227", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:585", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:613", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:612", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:350", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:351", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:594", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:563", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:588", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:538", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:565", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:568", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:567", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:593", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:838", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:256", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:329", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:352", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:337", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:338", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:339", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:225", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:357", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:407", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet")}:405", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet2")}:752", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet2")}:753", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet2")}:736", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet2")}:737", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
                    { $"{PathUtilities.NormalizeAssetName("Maps/spring_outdoorsTileSheet2")}:741", new() { WallDefinitionId = $"{ModManifest.UniqueID}/GenericCliffWall" } },
#endif
                }, AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo($"{ModManifest.UniqueID}/Interactions"))
                e.LoadFrom(() => new Dictionary<string, InteractionData>
                {
                    { $"({ModManifest.UniqueID}/NPC)", new InteractionData() // No second ID part = default for that type
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/Action",
                                Size = new( 0.875f, 1.875f, 0.875f ),
                                Translation = new( 0, 1.875f / 2, 0 ),
                            }
                        ],
                    } },
                    { $"({ModManifest.UniqueID}/Monster)", new InteractionData() // No second ID part = default for that type
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/Action",
                                Size = new( 0.875f, 1.875f, 0.875f ),
                                Translation = new( 0, 1.875f / 2, 0 ),
                            }
                        ],
                    } },
                    { $"({ModManifest.UniqueID}/CharacterType)Bat", new InteractionData() // A specific monster can still override this with their normal qualified ID
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/Action",
                                Size = new( 0.875f, 0.875f, 0.875f ),
                                Translation = new( 0, 0.875f/2, 0 ),
                            }
                        ],
                    } },
                    { $"({ModManifest.UniqueID}/CharacterType)GreenSlime", new InteractionData() // A specific monster can still override this with their normal qualified ID
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/Action",
                                Size = new( 0.875f, 0.875f, 0.875f ),
                                Translation = new( 0, 0.875f / 2, 0 ),
                            }
                        ],
                    } },
                    { $"(O)", new InteractionData() // No second ID part = default for that type
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/Action",
                                Size = new( 0.75f, 0.75f, 0.75f ),
                                Translation = new( 0, 0.75f / 2, 0 ),
                            }
                        ],
                    } },
                    { $"({ModManifest.UniqueID}/Grass)", new InteractionData() // No second ID part = default for that type
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/Action",
                                Size = new( 0.875f, 0.875f, 0.875f ),
                                Translation = new( 0, 0.875f / 2, 0 ),
                            }
                        ],
                    } },
                    { $"({ModManifest.UniqueID}/Tree)", new InteractionData() // No second ID part = default for that type
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/Action",
                                Size = new( 0.5f, 3, 0.5f ),
                                Translation = new( 0, 1.5f, 0 ),
                            }
                        ],
                    } },
                    { $"({ModManifest.UniqueID}/ResourceClump)Maps/springobjects:672", new InteractionData()
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/Action",
                                Size = new( 1.75f, 1, 1.75f ),
                                Translation = new( 0, 0.5f, 0 ),
                            }
                        ],
                    } },
                    { $"(W){MeleeWeapon.scytheId}", new InteractionData()
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/ToolAction/Impact",
                                Size = new( 0.375f, 0.75f, 0.125f ),
                                Translation = new( -0.25f, 0.6875f, 0 ),
                                Rotation = new( 0, 0, MathHelper.ToRadians( 130 ) ),
                            }
                        ],
                    } },
                    { $"({ModManifest.UniqueID}/T)Axe", new InteractionData() // A specific tool can still override this with their normal qualified ID
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/ToolAction/Impact",
                                Size = new( 0.1875f, 0.5f, 0.125f ),
                                Translation = new( -0.3125f, 0.625f, 0 ),
                                Rotation = new( 0, 0, MathHelper.ToRadians( 45 ) ),
                            }
                        ],
                    } },
                    { $"({ModManifest.UniqueID}/T)Pickaxe", new InteractionData() // A specific tool can still override this with their normal qualified ID
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/ToolAction/Impact",
                                Size = new( 0.1875f, 0.1875f, 0.125f ),
                                Translation = new( -0.4375f, 0.3125f+1f/16, 0 ),
                                Rotation = new( 0, 0, MathHelper.ToRadians( 75 ) ),
                            },
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/ToolAction/Impact",
                                Size = new( 0.1875f, 0.1875f, 0.125f ),
                                Translation = new( -0.1875f+0.25f+1f/32, 0.9375f, 0 ),
                                Rotation = new( 0, 0, MathHelper.ToRadians( 195 ) ),
                            }
                        ],
                    } },
                    { $"({ModManifest.UniqueID}/W){MeleeWeapon.defenseSword}", new InteractionData() // A specific tool can still override this with their normal qualified ID
                    {
                        Areas =
                        [
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/ToolAction/Impact",
                                Size = new( 0.125f, 0.875f, 0.125f ),
                                Translation = new( -4/16f, 10/16f, 0 ),
                                Rotation = new( 0, 0, MathHelper.ToRadians( 45 ) ),
                            },
                            new BoxInteractionArea()
                            {
                                Purpose = $"{ModManifest.UniqueID}/ToolAction/Impact",
                                Size = new( 0.125f, 0.875f, 0.125f ),
                                Translation = new( -2/16f, 12/16f, 0 ),
                                Rotation = new( 0, 0, MathHelper.ToRadians( 45 + 180 ) ),
                            }
                        ],
                    } },
                }, AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo($"{ModManifest.UniqueID}/Models"))
                e.LoadFrom(() => new Dictionary<string, ModelData>
                {
                    // TODO: move default data outside of code
                    { $"({ModManifest.UniqueID}/Location)Farm", new LocationModelData()
                    {
                        Portals = new()
                        {
                            { "Backwoods", new()
                            {
                                OtherLocation = "Backwoods",
                                MatchingPortal = "Farm",
                                Position = new( 41, 0, 0 ),
                                Facing = Vector3.Backward,
                            } },
                            { "BusStop", new()
                            {
                                OtherLocation = "BusStop",
                                MatchingPortal = "Farm",
                                Position = new( 80, 0, 17 ),
                                Facing = Vector3.Left,
                            } }
                        },
                    } },
                    { $"({ModManifest.UniqueID}/Location)BusStop", new LocationModelData()
                    {
                        Portals = new()
                        {
                            { "Farm", new()
                            {
                                OtherLocation = "Farm",
                                MatchingPortal = "BusStop",
                                Position = new( 0, 0, 24 ),
                                Facing = Vector3.Right,
                            } }
                        },
                    } },
                    { $"({ModManifest.UniqueID}/Location)Backwoods", new LocationModelData()
                    {
                        Portals = new()
                        {
                            { "Farm", new()
                            {
                                OtherLocation = "Farm",
                                MatchingPortal = "Backwoods",
                                Position = new( 14.5f, 0, 40 ),
                                Facing = Vector3.Forward,
                            } }
                        },
                    } },
                    { $"{ModManifest.UniqueID}/Skybox", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Skybox.gltf")}",
                        TextureMap = { { "Cursors.png", "LooseSprites/Cursors" } },
                        ForceTransparency = { "/Sky/stars" },
                    } },
                    { $"Debris/Stone/1", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Debris.gltf")}",
                        SubModelPath = "/stone/stone1",
                    } },
                    { $"Debris/Stone/2", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Debris.gltf")}",
                        SubModelPath = "/stone/stone2",
                    } },
                    { $"Debris/Wood/1", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Debris.gltf")}",
                        SubModelPath = "/wood/wood1",
                        Rotation = new( 0, MathHelper.ToRadians( -45 ), 0 ),
                    } },
                    { $"Debris/Wood/2", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Debris.gltf")}",
                        SubModelPath = "/wood/wood2",
                        Rotation = new( 0, MathHelper.ToRadians( 45 ), 0 ),
                    } },
                    { $"(O)450", new()
                    {
                        OtherModels =
                        [
                            new()
                            {
                                ModelId = "Debris/Stone/1",
                                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Stone/1",
                                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Stone/1",
                                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Stone/1",
                                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Stone/2",
                                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Stone/2",
                                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Stone/2",
                                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Stone/2",
                                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
                            },
                        ],
                    } },
                    { $"(O)343", new()
                    {
                        OtherModels =
                        [
                            new()
                            {
                                ModelId = "(O)450",
                            },
                        ],
                    } },
                    { $"(O)294", new()
                    {
                        OtherModels =
                        [
                            new()
                            {
                                ModelId = "Debris/Wood/1",
                                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Wood/1",
                                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Wood/1",
                                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Wood/1",
                                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Wood/2",
                                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Wood/2",
                                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Wood/2",
                                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Wood/2",
                                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
                            },
                        ]
                    } },
                    { $"(O)295", new()
                    {
                        OtherModels =
                        [
                            new()
                            {
                                ModelId = "(O)294",
                            },
                        ],
                    } },
                    { $"(O)167", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "objects", "JojaCola.gltf")}",
                    } },
                    { $"(F)288", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "furniture", "Armchair.gltf")}",
                        SubModelPath = "/BasicArmchair",
                        Rotation = new Vector3(0, MathHelper.ToRadians( 180 ), 0 ), // TODO: temporary
                    } },
                    { $"(F)1466", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "furniture", "TV.gltf")}",
                        SubModelPath = "/BudgetTV",
                    } },
                    { $"Debris/Stump", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Debris.gltf")}",
                        SubModelPath = "/stump/stump1",
                    } },
                    { $"Debris/Log", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Debris.gltf")}",
                        SubModelPath = "/log/log1",
                        Rotation = new( 0, MathHelper.ToRadians( 45 ), 0 ),
                    } },
                    { $"Debris/Boulder", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Debris.gltf")}",
                        SubModelPath = "/boulder/boulder1",
                    } },
                    { $"({ModManifest.UniqueID}/ResourceClump)Maps/springobjects:600", new()
                    {
                        OtherModels =
                        [
                            new()
                            {
                                ModelId = "Debris/Stump",
                                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Stump",
                                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Stump",
                                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Stump",
                                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
                            },
                        ],
                    } },
                    { $"({ModManifest.UniqueID}/ResourceClump)Maps/springobjects:602", new()
                    {
                        OtherModels =
                        [
                            new()
                            {
                                ModelId = "Debris/Log",
                                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Log",
                                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Log",
                                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Log",
                                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
                            },
                        ],
                    } },
                    { $"({ModManifest.UniqueID}/ResourceClump)Maps/springobjects:672", new()
                    {
                        OtherModels =
                        [
                            new()
                            {
                                ModelId = "Debris/Boulder",
                                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Boulder",
                                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Boulder",
                                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
                            },
                            new()
                            {
                                ModelId = "Debris/Boulder",
                                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
                            },
                        ],
                    } },
                    { $"({Mod.Instance.ModManifest.UniqueID}/Menu)StardewValley.Menus.TitleMenu", new MenuModelData()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "menus", "Title.gltf")}",
                        SubModelPath = "/title",

                        UseExistingTransformHierarchy = -1,

                        Clickables = new()
                        {
                            { "New", new()
                            {
                                ModelId = $"({Mod.Instance.ModManifest.UniqueID}/Menu)StardewValley.Menus.TitleMenu/Clickables/New",
                                HoverAnimation = "hover",
                            } },
                            { "Load", new()
                            {
                                ModelId = $"({Mod.Instance.ModManifest.UniqueID}/Menu)StardewValley.Menus.TitleMenu/Clickables/Load",
                                HoverAnimation = "hover",
                            } },
                            { "Co-op", new()
                            {
                                ModelId = $"({Mod.Instance.ModManifest.UniqueID}/Menu)StardewValley.Menus.TitleMenu/Clickables/Coop",
                                HoverAnimation = "hover",
                            } },
                            { "Exit", new()
                            {
                                ModelId = $"({Mod.Instance.ModManifest.UniqueID}/Menu)StardewValley.Menus.TitleMenu/Clickables/Exit",
                                HoverAnimation = "hover",
                            } },
                        },
                    } },
                    { $"({Mod.Instance.ModManifest.UniqueID}/Menu)StardewValley.Menus.TitleMenu/Clickables/New", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "menus", "Title.gltf")}",
                        SubModelPath = "/buttons/new",

                        AdditionalAnimationData = new()
                        {
                            { "hover", new()
                            {
                                Loop = ModelData.AnimationMetadata.LoopFinishMode.Hold,
                                Actions =
                                [
                                    new() { Time = 0.01f, ForReverse = false, Actions = [ "SwapTexture titleButtons_idle.png titleButtons_hover.png" ] },
                                    new() { Time = 0.09f, ForReverse = true, Actions = [ "SwapTexture titleButtons_hover.png, titleButtons_idle.png" ] },
                                ],
                            } }
                        },
                    } },
                    { $"({Mod.Instance.ModManifest.UniqueID}/Menu)StardewValley.Menus.TitleMenu/Clickables/Load", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "menus", "Title.gltf")}",
                        SubModelPath = "/buttons/load",

                        AdditionalAnimationData = new()
                        {
                            { "hover", new()
                            {
                                Loop = ModelData.AnimationMetadata.LoopFinishMode.Hold,
                                Actions =
                                [
                                    new() { Time = 0.01f, ForReverse = false, Actions = [ "SwapTexture titleButtons_idle.png titleButtons_hover.png" ] },
                                    new() { Time = 0.09f, ForReverse = true, Actions = [ "SwapTexture titleButtons_hover.png, titleButtons_idle.png" ] },
                                ],
                            } }
                        },
                    } },
                    { $"({Mod.Instance.ModManifest.UniqueID}/Menu)StardewValley.Menus.TitleMenu/Clickables/Coop", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "menus", "Title.gltf")}",
                        SubModelPath = "/buttons/coop",

                        AdditionalAnimationData = new()
                        {
                            { "hover", new()
                            {
                                Loop = ModelData.AnimationMetadata.LoopFinishMode.Hold,
                                Actions =
                                [
                                    new() { Time = 0.01f, ForReverse = false, Actions = [ "SwapTexture titleButtons_idle.png titleButtons_hover.png" ] },
                                    new() { Time = 0.09f, ForReverse = true, Actions = [ "SwapTexture titleButtons_hover.png, titleButtons_idle.png" ] },
                                ],
                            } }
                        },
                    } },
                    { $"({Mod.Instance.ModManifest.UniqueID}/Menu)StardewValley.Menus.TitleMenu/Clickables/Exit", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "menus", "Title.gltf")}",
                        SubModelPath = "/buttons/exit",

                        AdditionalAnimationData = new()
                        {
                            { "hover", new()
                            {
                                Loop = ModelData.AnimationMetadata.LoopFinishMode.Hold,
                                Actions =
                                [
                                    new() { Time = 0.01f, ForReverse = false, Actions = [ "SwapTexture titleButtons_idle.png titleButtons_hover.png" ] },
                                    new() { Time = 0.09f, ForReverse = true, Actions = [ "SwapTexture titleButtons_hover.png, titleButtons_idle.png" ] },
                                ],
                            } }
                        },
                    } },
                }, AssetLoadPriority.Exclusive);
        }
    }
}
