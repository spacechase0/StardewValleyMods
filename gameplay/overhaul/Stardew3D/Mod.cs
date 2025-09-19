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
using MonoScene.Graphics.Pipeline;
using SpaceShared;
using SpaceShared.Attributes;
using Stardew3D.Data;
using Stardew3D.Rendering;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Enchantments;
using StardewValley.Events;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Mods;
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
        // TODO: Cache this until invalidated
        internal Dictionary<string, ModelData> ModelData => Helper.GameContent.Load<Dictionary<string, ModelData>>($"{ModManifest.UniqueID}/Models");

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

            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            Helper.Events.GameLoop.UpdateTicking += (s, e) => State.ActiveHandler?.BeforeUpdate();
            Helper.Events.GameLoop.UpdateTicked += (s, e) => State.ActiveHandler?.AfterUpdate();

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
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (Config.ToggleThirdDimension.JustPressed())
            {
                string oldId = State.ActiveHandler?.Id ?? "null";
                State.ActiveHandler?.SwitchOff();
                State.ActiveHandlerIndex++;
                State.ActiveHandler?.SwitchOn();
                Log.Debug( $"Switched from game handler \"{oldId}\" to \"{State.ActiveHandler?.Id ?? "null"}\"" );
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            string mapsFolder = PathUtilities.NormalizeAssetName("Maps/a"); // If we just do "Maps/" it removes the /, which is a big part of what we want
            mapsFolder = mapsFolder.Substring( 0, mapsFolder.Length - 1 );
            if (e.DataType == typeof(xTile.Map) && e.NameWithoutLocale.StartsWith(mapsFolder))
            {
                string specific = e.NameWithoutLocale.Name.Substring(mapsFolder.Length);
                string ours = Path.Combine("assets", "maps", $"{specific}.tmx");
                if (Helper.ModContent.DoesAssetExist< xTile.Map >(ours))
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
                        Scale = new( 30 ),
                    } },
                    { $"{ModManifest.UniqueID}/GameTitle", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Title.gltf")}",
                        SubModelPath = "/title",
                        Translation = new( 0, -5.18f, -0.02f ),
                    } },
                    { $"{ModManifest.UniqueID}/GameTitle/Buttons/New/Idle", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Title.gltf")}",
                        SubModelPath = "/buttons/new",
                        Translation = new( 5.6325f-7.25f, 0.2678f-15, -0.2719f ),
                    } },
                    { $"{ModManifest.UniqueID}/GameTitle/Buttons/New/Hover", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Title.gltf")}",
                        SubModelPath = "/buttons/new",
                        TextureMap = { { "titleButtons_idle.png", "titleButtons_hover.png" } },
                        Translation = new( 5.6325f-7.25f, 0.2678f-15, -0.2719f ),
                    } },
                    { $"{ModManifest.UniqueID}/GameTitle/Buttons/Load/Idle", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Title.gltf")}",
                        SubModelPath = "/buttons/load",
                        Translation = new( 1.8538f-6.75f, 0.2678f-15, -0.2719f ),
                    } },
                    { $"{ModManifest.UniqueID}/GameTitle/Buttons/Load/Hover", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Title.gltf")}",
                        SubModelPath = "/buttons/load",
                        TextureMap = { { "titleButtons_idle.png", "titleButtons_hover.png" } },
                        Translation = new( 1.8538f-6.75f, 0.2678f-15, -0.2719f ),
                    } },
                    { $"{ModManifest.UniqueID}/GameTitle/Buttons/Coop/Idle", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Title.gltf")}",
                        SubModelPath = "/buttons/coop",
                        Translation = new( -0.19249f-8f, 0.2678f-15, -0.2719f ),
                    } },
                    { $"{ModManifest.UniqueID}/GameTitle/Buttons/Coop/Hover", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Title.gltf")}",
                        SubModelPath = "/buttons/coop",
                        TextureMap = { { "titleButtons_idle.png", "titleButtons_hover.png" } },
                        Translation = new( -0.19249f-8f, 0.2678f-15, -0.2719f ),
                    } },
                    { $"{ModManifest.UniqueID}/GameTitle/Buttons/Exit/Idle", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Title.gltf")}",
                        SubModelPath = "/buttons/exit",
                        Translation = new( -5.7036f-5.75f, 0.2678f-15, -0.2719f ),
                    } },
                    { $"{ModManifest.UniqueID}/GameTitle/Buttons/Exit/Hover", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Title.gltf")}",
                        SubModelPath = "/buttons/exit",
                        TextureMap = { { "titleButtons_idle.png", "titleButtons_hover.png" } },
                        Translation = new( -5.7036f-5.75f, 0.2678f-15, -0.2719f ),
                    } },
                    { $"{ModManifest.UniqueID}/GameTitle/Buttons/Back/Idle", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Title.gltf")}",
                        SubModelPath = "/buttons/back",
                        Translation = new( -5.6544f-5.75f, 2.5902f-15, -0.2719f ),
                    } },
                    { $"{ModManifest.UniqueID}/GameTitle/Buttons/Back/Hover", new()
                    {
                        ModelFilePath = $"{ModManifest.UniqueID}:{Path.Combine( "assets", "Title.gltf")}",
                        SubModelPath = "/buttons/back",
                        TextureMap = { { "titleButtons_idle.png", "titleButtons_hover.png" } },
                        Translation = new( -5.6544f-5.75f, 2.5902f-15, -0.2719f ),
                    } },
                }, AssetLoadPriority.Exclusive);
        }
    }
}
