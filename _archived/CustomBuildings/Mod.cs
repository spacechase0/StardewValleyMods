using System.Collections.Generic;
using System.IO;
using CustomBuildings.Framework;
using CustomBuildings.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;

namespace CustomBuildings
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        internal Dictionary<string, BuildingData> Buildings = new();

        // do I need to copy JA's object resolver here too?
        internal static int ResolveObjectId(object data)
        {
            if (data is long inputId)
                return (int)inputId;

            foreach (var obj in Game1.objectInformation)
            {
                if (obj.Value.Split('/')[0] == (string)data)
                    return obj.Key;
            }

            Log.Warn($"No idea what '{data}' is!");
            return 0;
        }

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            HarmonyPatcher.Apply(this,
                new CoopPatcher()
            );

            Log.Debug("Loading content packs...");
            foreach (var cp in helper.ContentPacks.GetOwned())
            {
                DirectoryInfo buildingsDir = new DirectoryInfo(Path.Combine(cp.DirectoryPath, "Buildings"));
                foreach (var dir in buildingsDir.EnumerateDirectories())
                {
                    string relDir = $"Buildings/{dir.Name}";
                    BuildingData buildingData = cp.ReadJsonFile<BuildingData>(Path.Combine(relDir, "building.json"));
                    if (buildingData == null)
                        continue;
                    buildingData.Texture = cp.ModContent.Load<Texture2D>(Path.Combine(relDir, "building.png"));
                    buildingData.MapLoader = () => cp.ModContent.Load<xTile.Map>(Path.Combine(relDir, "building.tbin"));
                    this.Buildings.Add(buildingData.Id, buildingData);
                }
            }
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is CarpenterMenu carp)
            {
                var blueprints = this.Helper.Reflection.GetField<List<BluePrint>>(carp, "blueprints").GetValue();
                foreach (var building in this.Buildings)
                {
                    if (building.Value.PreviousTier == null || Game1.getFarm().isBuildingConstructed(building.Value.PreviousTier))
                        blueprints.Add(new BluePrint(building.Value.Id));
                }
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            BuildableGameLocation farm = e.NewLocation as BuildableGameLocation ?? e.OldLocation as BuildableGameLocation;
            if (farm != null)
            {
                for (int i = 0; i < farm.buildings.Count; ++i)
                {
                    var b = farm.buildings[i];

                    // This is probably a new building if it hasn't been converted yet.
                    if (this.Buildings.TryGetValue(b.buildingType.Value, out BuildingData buildingData) && b is not Coop)
                    {
                        farm.buildings[i] = new Coop(new BluePrint(b.buildingType.Value), new Vector2(b.tileX.Value, b.tileY.Value));
                        farm.buildings[i].indoors.Value = b.indoors.Value;
                        farm.buildings[i].load();
                        (farm.buildings[i].indoors.Value as AnimalHouse).animalLimit.Value = buildingData.MaxOccupants;
                    }
                }
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.StartsWith("Buildings/")
                && this.Buildings.TryGetValue(e.NameWithoutLocale.BaseName["Buildings/".Length..], out var data))
            {
                e.LoadFrom(() => data.Texture, AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.StartsWith("Maps/")
                && this.Buildings.TryGetValue(e.NameWithoutLocale.BaseName["Maps/".Length..], out data))
            {
                e.LoadFrom(() => data.MapLoader(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data\\Blueprints"))
            {
                e.Edit((asset) =>
                {
                    var dict = asset.AsDictionary<string, string>();
                    foreach (var building in this.Buildings)
                    {
                        dict.Data.Add(building.Value.Id, building.Value.BlueprintString());
                    }
                });
            }
        }
    }
}
