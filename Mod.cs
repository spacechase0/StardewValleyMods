using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoreBuildings.SpookyShed;
using StardewValley.Locations;
using MoreBuildings.BigShed;
using System.Reflection;
using MoreBuildings.FishingShack;
using MoreBuildings.MiniSpa;
using PyTK.CustomElementHandler;
using Harmony;
using MoreBuildings.Overrides;
using SpaceShared;

namespace MoreBuildings
{
    public class Mod : StardewModdingAPI.Mod, IAssetEditor, IAssetLoader
    {
        public static Mod instance;
        private Texture2D shed2Exterior;
        private Texture2D spookyExterior;
        private Texture2D fishingExterior;
        private Texture2D spaExterior;
        public Texture2D spookyGemTex;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            
            helper.Events.Display.MenuChanged += onMenuChanged;
            helper.Events.Player.Warped += onWarped;
            helper.Events.Specialized.UnvalidatedUpdateTicked += onUnvalidatedUpdateTicked;
            SaveHandler.FinishedRebuilding += fixWarps;

            shed2Exterior = Helper.Content.Load<Texture2D>("BigShed/building.png");
            spookyExterior = Helper.Content.Load<Texture2D>("SpookyShed/building.png");
            fishingExterior = Helper.Content.Load<Texture2D>("FishingShack/building.png");
            spaExterior = Helper.Content.Load<Texture2D>("MiniSpa/building.png");
            spookyGemTex = Helper.Content.Load<Texture2D>("SpookyShed\\Shrine_Gem.png");

            var harmony = HarmonyInstance.Create("spacechase0.MoreBuildings");
            harmony.Patch(typeof(Shed).GetMethod(nameof(Shed.updateLayout)), prefix: new HarmonyMethod(typeof(ShedUpdateLayoutWorkaround).GetMethod("Prefix")));
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if ( e.NewMenu is CarpenterMenu carp )
            {
                var blueprints = Helper.Reflection.GetField<List<BluePrint>>(carp, "blueprints").GetValue();
                //if ( Game1.getFarm().isBuildingConstructed("Shed"))
                //    blueprints.Add(new BluePrint("Shed2"));
                blueprints.Add(new BluePrint("SpookyShed"));
                blueprints.Add(new BluePrint("FishShack"));
                blueprints.Add(new BluePrint("MiniSpa"));
            }
        }

        /// <summary>Raised after a player warps to a new location. NOTE: this event is currently only raised for the current player.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            if ( e.OldLocation is MiniSpaLocation )
            {
                Game1.player.changeOutOfSwimSuit();
                Game1.player.swimming.Value = false;
            }

            BuildableGameLocation farm = e.NewLocation as BuildableGameLocation;
            if (farm == null)
                farm = e.OldLocation as BuildableGameLocation;
            if ( farm != null )
            {
                for ( int i = 0; i < farm.buildings.Count; ++i )
                {
                    var b = farm.buildings[i];

                    // This is probably a new building if it hasn't been converted yet.
                    if ( b.buildingType.Value == "Shed2" && !(b is BigShedBuilding))
                    {
                        Log.debug($"Converting big shed at ({b.tileX}, {b.tileY}) to actual big shed.");
                        
                        farm.buildings[i] = new BigShedBuilding();
                        farm.buildings[i].buildingType.Value = b.buildingType.Value;
                        farm.buildings[i].daysOfConstructionLeft.Value = b.daysOfConstructionLeft.Value;
                        farm.buildings[i].indoors.Value = b.indoors.Value;
                        farm.buildings[i].tileX.Value = b.tileX.Value;
                        farm.buildings[i].tileY.Value = b.tileY.Value;
                        farm.buildings[i].tilesWide.Value = b.tilesWide.Value;
                        farm.buildings[i].tilesHigh.Value = b.tilesHigh.Value;
                        farm.buildings[i].load();
                    }
                    else if (b.buildingType.Value == "SpookyShed" && !(b is SpookyShedBuilding))
                    {
                        Log.debug($"Converting spooky shed at ({b.tileX}, {b.tileY}) to actual spooky shed.");

                        farm.buildings[i] = new SpookyShedBuilding();
                        farm.buildings[i].buildingType.Value = b.buildingType.Value;
                        farm.buildings[i].daysOfConstructionLeft.Value = b.daysOfConstructionLeft.Value;
                        farm.buildings[i].indoors.Value = b.indoors.Value;
                        farm.buildings[i].tileX.Value = b.tileX.Value;
                        farm.buildings[i].tileY.Value = b.tileY.Value;
                        farm.buildings[i].tilesWide.Value = b.tilesWide.Value;
                        farm.buildings[i].tilesHigh.Value = b.tilesHigh.Value;
                        farm.buildings[i].load();
                    }
                    else if (b.buildingType.Value == "FishShack" && !(b is FishingShackBuilding))
                    {
                        Log.debug($"Converting fishing shack at ({b.tileX}, {b.tileY}) to actual fishing shack.");

                        farm.buildings[i] = new FishingShackBuilding();
                        farm.buildings[i].buildingType.Value = b.buildingType.Value;
                        farm.buildings[i].daysOfConstructionLeft.Value = b.daysOfConstructionLeft.Value;
                        farm.buildings[i].indoors.Value = b.indoors.Value;
                        //farm.buildings[i].nameOfIndoors = b.nameOfIndoors;
                        farm.buildings[i].tileX.Value = b.tileX.Value;
                        farm.buildings[i].tileY.Value = b.tileY.Value;
                        farm.buildings[i].tilesWide.Value = b.tilesWide.Value;
                        farm.buildings[i].tilesHigh.Value = b.tilesHigh.Value;
                        farm.buildings[i].load();
                    }
                    else if (b.buildingType.Value  == "MiniSpa" && !(b is MiniSpaBuilding))
                    {
                        Log.debug($"Converting mini spa at ({b.tileX}, {b.tileY}) to actual mini spa.");

                        farm.buildings[i] = new MiniSpaBuilding();
                        farm.buildings[i].buildingType.Value = b.buildingType.Value;
                        farm.buildings[i].daysOfConstructionLeft.Value = b.daysOfConstructionLeft.Value;
                        farm.buildings[i].indoors.Value = b.indoors.Value;
                        farm.buildings[i].tileX.Value = b.tileX.Value;
                        farm.buildings[i].tileY.Value = b.tileY.Value;
                        farm.buildings[i].tilesWide.Value = b.tilesWide.Value;
                        farm.buildings[i].tilesHigh.Value = b.tilesHigh.Value;
                        farm.buildings[i].load();
                    }
                }
            }
        }

        bool taskWasThere = false;

        /// <summary>Raised after the game state is updated (≈60 times per second), regardless of normal SMAPI validation. This event is not thread-safe and may be invoked while game logic is running asynchronously. Changes to game state in this method may crash the game or corrupt an in-progress save. Do not use this event unless you're fully aware of the context in which your code will be run. Mods using this event will trigger a stability warning in the SMAPI console.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUnvalidatedUpdateTicked(object sender, EventArgs e)
        {
            var task = (Task) typeof(Game1).GetField("_newDayTask", BindingFlags.Static | BindingFlags.NonPublic).GetValue( null );
            if ( task != null && !taskWasThere )
            {
                foreach ( var loc in Game1.locations )
                {
                    if ( loc is BuildableGameLocation buildable )
                    {
                        for ( int i = 0; i < buildable.buildings.Count; ++i )
                        {/*
                            if ( buildable.buildings[ i ].buildingType.Value == "Shed" && buildable.buildings[i].daysUntilUpgrade.Value == 1 )
                            {
                                var b = buildable.buildings[i];
                                buildable.buildings[i] = new BigShedBuilding();
                                buildable.buildings[i].buildingType.Value = b.buildingType.Value;
                                buildable.buildings[i].daysOfConstructionLeft.Value = 1;
                                buildable.buildings[i].humanDoor.Value = b.humanDoor.Value;
                                buildable.buildings[i].indoors.Value = b.indoors.Value;
                                buildable.buildings[i].tileX.Value = b.tileX.Value;
                                buildable.buildings[i].tileY.Value = b.tileY.Value;
                                buildable.buildings[i].tilesWide.Value = b.tilesWide.Value;
                                buildable.buildings[i].tilesHigh.Value = b.tilesHigh.Value;
                                buildable.buildings[i].load();
                            }*/
                        }
                    }
                }
            }
            taskWasThere = task != null;
        }

        public void fixWarps(object sender, EventArgs args)
        {
            foreach (var loc in Game1.locations)
                if (loc is BuildableGameLocation buildable)
                    foreach (var building in buildable.buildings)
                    {
                        if (building.indoors.Value == null)
                            continue;


                        building.indoors.Value.updateWarps();
                        building.updateInteriorWarps();
                    }
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\Blueprints");
        }

        public void Edit<T>(IAssetData asset)
        {
            //*
            asset.AsDictionary<string, string>().Data.Add("Shed2", "388 750/7/3/3/2/-1/-1/Shed2/Big Shed/An even bigger Shed./Upgrades/Shed/96/96/20/null/Farm/25000/false");
            asset.AsDictionary<string, string>().Data.Add("SpookyShed", "156 10 768 25 769 25 337 20 388 500/7/3/3/2/-1/-1/SpookyShed/Spooky Shed/An empty building. But spooky, too./Buildings/none/96/96/20/null/Farm/25000/false");
            asset.AsDictionary<string, string>().Data.Add("FishShack", "163 1 390 250 388 500/7/3/3/2/-1/-1/FishShack/Fishing Shack/A shack for fishing./Buildings/none/96/96/20/null/Farm/50000/false");
            asset.AsDictionary<string, string>().Data.Add("MiniSpa", "337 25 390 999 388 999/7/3/3/2/-1/-1/MiniSpa/Mini Spa/A place to relax and recharge./Buildings/none/96/96/20/null/Farm/250000/false");
            //*/
            /*
            asset.AsDictionary<string, string>().Data.Add("Shed2", "388 1/7/3/3/2/-1/-1/Shed2/Big Shed/An even bigger Shed./Upgrades/Shed/96/96/20/null/Farm/25000/false");
            asset.AsDictionary<string, string>().Data.Add("SpookyShed", "388 1/7/3/3/2/-1/-1/SpookyShed/Spooky Shed/An empty building. But spooky, too./Buildings/none/96/96/20/null/Farm/25000/false");
            asset.AsDictionary<string, string>().Data.Add("FishShack", "388 1/7/3/3/2/-1/-1/FishShack/Fishing Shack/A shack for fishing./Buildings/none/96/96/20/null/Farm/50000/false");
            asset.AsDictionary<string, string>().Data.Add("MiniSpa", "388 1/7/3/3/2/-1/-1/MiniSpa/Mini Spa/A place to relax and recharge./Buildings/none/96/96/20/null/Farm/250000/false");
            //*/
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Buildings\\Shed2") || asset.AssetNameEquals("Maps\\Shed2_"))
                return true;
            if (asset.AssetNameEquals("Buildings\\SpookyShed") || asset.AssetNameEquals("Maps\\SpookyShed"))
                return true;
            if (asset.AssetNameEquals("Buildings\\FishShack") || asset.AssetNameEquals("Maps\\FishShack"))
                return true;
            if (asset.AssetNameEquals("Buildings\\MiniSpa") || asset.AssetNameEquals("Maps\\MiniSpa"))
                return true;
            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Buildings\\Shed2"))
                return (T)(object)shed2Exterior;
            else if (asset.AssetNameEquals("Maps\\Shed2_"))
                return (T)(object)Helper.Content.Load<xTile.Map>("BigShed/map.tbin");
            if (asset.AssetNameEquals("Buildings\\SpookyShed"))
                return (T)(object)spookyExterior;
            else if (asset.AssetNameEquals("Maps\\SpookyShed"))
                return (T)(object)Helper.Content.Load<xTile.Map>("SpookyShed/map.tbin");
            if (asset.AssetNameEquals("Buildings\\FishShack"))
                return (T)(object)fishingExterior;
            else if (asset.AssetNameEquals("Maps\\FishShack"))
                return (T)(object)Helper.Content.Load<xTile.Map>("FishingShack/map.tbin");
            if (asset.AssetNameEquals("Buildings\\MiniSpa"))
                return (T)(object)spaExterior;
            else if (asset.AssetNameEquals("Maps\\MiniSpa"))
                return (T)(object)Helper.Content.Load<xTile.Map>("MiniSpa/map.tbin");

            return (T)(object)null;
        }
    }
}
