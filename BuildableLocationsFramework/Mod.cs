using System.Collections.Generic;
using System.Linq;
using BuildableLocationsFramework.Patches;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;

namespace BuildableLocationsFramework
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            this.Helper.Events.Display.MenuChanged += this.MenuChanged;

            this.Helper.ConsoleCommands.Add("blf_adddummy", "", this.DoCommand);

            HarmonyPatcher.Apply(this,
                new BuildingPatcher(),
                new CarpenterMenuPatcher(),
                new FarmAnimalPatcher(),
                new GameLocationPatcher(),
                new MilkPailPatcher(),
                new PurchaseAnimalsMenuPatcher(),
                new SaveGamePatcher(),
                new ShearsPatcher(),
                new UtilityPatcher()
            );
        }

        private void DoCommand(string cmd, string[] args)
        {
            Game1.locations.Add(new BuildableAnimalLocation("Maps\\Beach", "Farm2"));
            Game1.game1.parseDebugInput("warp Farm2 25 10");
        }

        private void DoMenuUpdate(object sender, UpdateTickedEventArgs e)
        {
        }

        private void DoMenuRender(object sender, RenderedActiveMenuEventArgs e)
        {
        }

        private int BuildableLocIndex;
        private void DoMenuButtons(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu is CarpenterMenu carpMenu)
            {
                if (carpMenu.CurrentBlueprint?.displayName == "Stone Cabin" || carpMenu.CurrentBlueprint?.displayName == "Plank Cabin" || carpMenu.CurrentBlueprint?.displayName == "Log Cabin" || carpMenu.CurrentBlueprint?.displayName == "Stable")
                    return;
            }

            int newLocIndex = this.BuildableLocIndex;
            if (e.Button == SButton.Z || e.Button == SButton.LeftShoulder)
                newLocIndex--;
            else if (e.Button == SButton.X || e.Button == SButton.RightShoulder)
                newLocIndex++;

            if (this.BuildableLocIndex != newLocIndex)
            {
                var buildableLocs = Mod.GetAllLocations().FindAll(loc => loc is BuildableGameLocation);
                while (newLocIndex < 0)
                    newLocIndex += buildableLocs.Count;
                while (newLocIndex >= buildableLocs.Count)
                    newLocIndex -= buildableLocs.Count;

                Game1.currentLocation = buildableLocs[newLocIndex];
                Game1.viewport.X = 0;
                Game1.viewport.Y = 0;
                this.BuildableLocIndex = newLocIndex;
            }
        }

        [EventPriority(EventPriority.Low)]
        private void MenuChanged(object sender, MenuChangedEventArgs e)
        {
            CarpenterMenu menu1 = e.NewMenu as CarpenterMenu;
            PurchaseAnimalsMenu menu2 = e.NewMenu as PurchaseAnimalsMenu;
            if (menu1 != null || menu2 != null)
            {
                if (menu1 != null)
                {
                    var blueprints = this.Helper.Reflection.GetField<List<BluePrint>>(menu1, "blueprints").GetValue();

                    var locs = Mod.GetAllLocations();
                    foreach (var loc in locs)
                    {
                        if (loc is BuildableGameLocation bloc)
                        {
                            if (bloc.isBuildingConstructed("Coop") && blueprints.FirstOrDefault(bp => bp.name == "Big Coop") == null)
                                blueprints.Add(new BluePrint("Big Coop"));
                            if (bloc.isBuildingConstructed("Big Coop") && blueprints.FirstOrDefault(bp => bp.name == "Deluxe Coop") == null)
                                blueprints.Add(new BluePrint("Deluxe Coop"));
                            if (bloc.isBuildingConstructed("Barn") && blueprints.FirstOrDefault(bp => bp.name == "Big Barn") == null)
                                blueprints.Add(new BluePrint("Big Barn"));
                            if (bloc.isBuildingConstructed("Big Barn") && blueprints.FirstOrDefault(bp => bp.name == "Deluxe Barn") == null)
                                blueprints.Add(new BluePrint("Deluxe Barn"));
                            if (bloc.isBuildingConstructed("Shed") && blueprints.FirstOrDefault(bp => bp.name == "Big Shed") == null)
                                blueprints.Add(new BluePrint("Big Shed"));
                        }
                    }
                }

                this.Helper.Events.GameLoop.UpdateTicked += this.DoMenuUpdate;
                this.Helper.Events.Display.RenderedActiveMenu += this.DoMenuRender;
                this.Helper.Events.Input.ButtonPressed += this.DoMenuButtons;
            }
            if (e.OldMenu is CarpenterMenu || e.OldMenu is PurchaseAnimalsMenu)
            {
                this.Helper.Events.GameLoop.UpdateTicked -= this.DoMenuUpdate;
                this.Helper.Events.Display.RenderedActiveMenu -= this.DoMenuRender;
                this.Helper.Events.Input.ButtonPressed -= this.DoMenuButtons;
            }
        }

        public static List<GameLocation> GetAllLocations()
        {
            var ret = new List<GameLocation>();
            foreach (var loc in Game1.locations)
            {
                ret.Add(loc);
                if (loc is BuildableGameLocation bloc)
                {
                    Mod.AddLocations(ret, bloc);
                }
            }
            return ret;
        }

        private static void AddLocations(List<GameLocation> list, BuildableGameLocation bloc)
        {
            foreach (var building in bloc.buildings)
            {
                if (building.indoors.Value != null)
                {
                    list.Add(building.indoors.Value);
                    if (building.indoors.Value is BuildableGameLocation bloc2)
                    {
                        Mod.AddLocations(list, bloc2);
                    }
                }
            }
        }

        internal static GameLocation FindOutdoorsOf(Building building)
        {
            foreach (var loc in Mod.GetAllLocations())
            {
                if (loc is BuildableGameLocation bgl)
                {
                    if (bgl.buildings.Contains(building))
                        return loc;
                }
            }
            if (SaveGamePatcher.Locations != null)
            {
                var oldLocs = Game1.locations;
                Game1.game1._locations = SaveGamePatcher.Locations;
                var locs = Mod.GetAllLocations();
                Game1.game1._locations = oldLocs;

                foreach (var loc in locs)
                {
                    if (loc is BuildableGameLocation bgl)
                    {
                        if (bgl.buildings.Contains(building))
                            return loc;
                    }
                }
            }
            //throw new ArgumentException("Building not in any location");
            return null;
        }
    }
}
