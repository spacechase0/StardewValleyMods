using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MoreBuildings.Buildings.BigShed;
using MoreBuildings.Buildings.FishingShack;
using MoreBuildings.Buildings.MiniSpa;
using MoreBuildings.Buildings.SpookyShed;
using MoreBuildings.Framework;
using MoreBuildings.Patches;
using Spacechase.Shared.Patching;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace MoreBuildings
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        private Texture2D Shed2Exterior;
        private Texture2D SpookyExterior;
        private Texture2D FishingExterior;
        private Texture2D SpaExterior;


        /*********
        ** Accessors
        *********/
        public static Mod Instance;
        public Texture2D SpookyGemTex;



        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.Specialized.LoadStageChanged += this.OnLoadStageChanged;
            helper.Events.Specialized.UnvalidatedUpdateTicked += this.OnUnvalidatedUpdateTicked;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            this.Shed2Exterior = this.Helper.ModContent.Load<Texture2D>("assets/BigShed/building.png");
            this.SpookyExterior = this.Helper.ModContent.Load<Texture2D>("assets/SpookyShed/building.png");
            this.FishingExterior = this.Helper.ModContent.Load<Texture2D>("assets/FishingShack/building.png");
            this.SpaExterior = this.Helper.ModContent.Load<Texture2D>("assets/MiniSpa/building.png");
            this.SpookyGemTex = this.Helper.ModContent.Load<Texture2D>("assets/SpookyShed/Shrine_Gem.png");

            HarmonyPatcher.Apply(this,
                new ShedPatcher()
            );
        }

        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var spaceCore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");

            Type[] types = {
                typeof(BigShedBuilding),
                typeof(BigShedLocation),
                typeof(FishingShackBuilding),
                typeof(FishingShackLocation),
                typeof(MiniSpaBuilding),
                typeof(MiniSpaLocation),
                typeof(SpookyShedBuilding),
                typeof(SpookyShedLocation)
            };

            foreach (Type type in types)
                spaceCore.RegisterSerializerType(type);
        }

        /// <inheritdoc cref="ISpecializedEvents.LoadStageChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLoadStageChanged(object sender, LoadStageChangedEventArgs e)
        {
            if (e.NewStage == LoadStage.SaveParsed)
                LegacyDataMigrator.OnSaveParsed(this.Helper.ModRegistry);
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.FixWarps();
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is CarpenterMenu carp)
            {
                var blueprints = this.Helper.Reflection.GetField<List<BluePrint>>(carp, "blueprints").GetValue();
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
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            if (e.OldLocation is MiniSpaLocation)
            {
                Game1.player.changeOutOfSwimSuit();
                Game1.player.swimming.Value = false;
            }

            BuildableGameLocation farm = e.NewLocation as BuildableGameLocation ?? e.OldLocation as BuildableGameLocation;
            if (farm != null)
            {
                for (int i = 0; i < farm.buildings.Count; ++i)
                {
                    var b = farm.buildings[i];

                    // This is probably a new building if it hasn't been converted yet.
                    if (b.buildingType.Value == "Shed2" && b is not BigShedBuilding)
                    {
                        Log.Debug($"Converting big shed at ({b.tileX}, {b.tileY}) to actual big shed.");

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
                    else if (b.buildingType.Value == "SpookyShed" && b is not SpookyShedBuilding)
                    {
                        Log.Debug($"Converting spooky shed at ({b.tileX}, {b.tileY}) to actual spooky shed.");

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
                    else if (b.buildingType.Value == "FishShack" && b is not FishingShackBuilding)
                    {
                        Log.Debug($"Converting fishing shack at ({b.tileX}, {b.tileY}) to actual fishing shack.");

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
                    else if (b.buildingType.Value == "MiniSpa" && b is not MiniSpaBuilding)
                    {
                        Log.Debug($"Converting mini spa at ({b.tileX}, {b.tileY}) to actual mini spa.");

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

        private bool TaskWasThere;

        /// <summary>Raised after the game state is updated (≈60 times per second), regardless of normal SMAPI validation. This event is not thread-safe and may be invoked while game logic is running asynchronously. Changes to game state in this method may crash the game or corrupt an in-progress save. Do not use this event unless you're fully aware of the context in which your code will be run. Mods using this event will trigger a stability warning in the SMAPI console.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUnvalidatedUpdateTicked(object sender, EventArgs e)
        {
            var task = (Task)typeof(Game1).GetField("_newDayTask", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            if (task != null && !this.TaskWasThere)
            {
                foreach (var loc in Game1.locations)
                {
                    if (loc is BuildableGameLocation buildable)
                    {
                        for (int i = 0; i < buildable.buildings.Count; ++i)
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
            this.TaskWasThere = task != null;
        }

        public void FixWarps()
        {
            foreach (var loc in Game1.locations)
            {
                if (loc is BuildableGameLocation buildable)
                {
                    foreach (var building in buildable.buildings)
                    {
                        if (building.indoors.Value == null)
                            continue;


                        building.indoors.Value.updateWarps();
                        building.updateInteriorWarps();
                    }
                }
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data\\Blueprints"))
                e.Edit(this.EditBluePrints);
            else if (e.NameWithoutLocale.IsEquivalentTo("Buildings\\Shed2"))
                e.LoadFrom(() => this.Shed2Exterior, AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps\\Shed2_"))
                e.LoadFromModFile<xTile.Map>("assets/BigShed/map.tbin", AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo("Buildings\\SpookyShed"))
                e.LoadFrom(()=> this.SpookyExterior, AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps\\SpookyShed"))
                e.LoadFromModFile<xTile.Map>("assets/SpookyShed/map.tbin", AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo("Buildings\\FishShack"))
                e.LoadFrom(() => this.FishingExterior, AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps\\FishShack"))
                e.LoadFromModFile<xTile.Map>("assets/FishingShack/map.tbin", AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo("Buildings\\MiniSpa"))
                e.LoadFrom(() => this.SpaExterior, AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps\\MiniSpa"))
                e.LoadFromModFile<xTile.Map>("assets/MiniSpa/map.tbin", AssetLoadPriority.Exclusive);
        }

        public void EditBluePrints(IAssetData asset)
        {
            asset.AsDictionary<string, string>().Data.Add("Shed2", "388 750/7/3/3/2/-1/-1/Shed2/Big Shed/An even bigger Shed./Upgrades/Shed/96/96/20/null/Farm/25000/false");
            asset.AsDictionary<string, string>().Data.Add("SpookyShed", $"156 10 768 25 769 25 337 20 388 500/7/3/3/2/-1/-1/SpookyShed/{I18n.SpookyShed_Name()}/{I18n.SpookyShed_Description()}/Buildings/none/96/96/20/null/Farm/25000/false");
            asset.AsDictionary<string, string>().Data.Add("FishShack", $"163 1 390 250 388 500/7/3/3/2/-1/-1/FishShack/{I18n.FishingShack_Name()}/{I18n.FishingShack_Description()}/Buildings/none/96/96/20/null/Farm/50000/false");
            asset.AsDictionary<string, string>().Data.Add("MiniSpa", $"337 25 390 999 388 999/7/3/3/2/-1/-1/MiniSpa/{I18n.MiniSpa_Name()}/{I18n.MiniSpa_Description()}/Buildings/none/96/96/20/null/Farm/250000/false");
        }
    }
}
