using System;
using System.Collections.Generic;
using HybridCropEngine.Framework;
using Microsoft.Xna.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace HybridCropEngine
{
    internal class Mod : StardewModdingAPI.Mod, IAssetLoader
    {
        public static Mod Instance;
        public static Configuration Config;

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data/HybridCrops");
        }

        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)new Dictionary<int, HybridCropData>();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => Mod.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(Mod.Config)
                );
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_ScanEverywhere_Name,
                    tooltip: I18n.Config_ScanEverywhere_Tooltip,
                    getValue: () => Mod.Config.ScanEverywhere,
                    setValue: value => Mod.Config.ScanEverywhere = value
                );
            }
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;

            var hybrids = Game1.content.Load<Dictionary<int, HybridCropData>>("Data/HybridCrops");
            var hybridIndexByCrop = this.MakeHybridIndex(hybrids);
            var cropsByIndex = this.MakeCropIndex();

            //*
            foreach (var hybrid in hybrids)
            {
                Log.Trace("Hybrids: " + hybrid.Key + " " + hybrid.Value.BaseCropA + " " + hybrid.Value.BaseCropB + " " + hybrid.Value.Chance);
            }
            foreach (var index in hybridIndexByCrop)
            {
                Log.Trace("Hybrid Index: " + index.Key + " " + index.Value);
            }
            foreach (var index in cropsByIndex)
            {
                Log.Trace("Crop Index: " + index.Key + " " + index.Value);
            }
            //*/

            if (Mod.Config.ScanEverywhere)
            {
                var locs = new List<GameLocation>(Game1.locations);
                var moreLocs = new List<GameLocation>();
                foreach (var loc in locs)
                {
                    if (loc is BuildableGameLocation buildLoc)
                    {
                        foreach (var building in buildLoc.buildings)
                        {
                            if (building.indoors.Value != null)
                                moreLocs.Add(building.indoors.Value);
                        }
                    }

                    this.GrowHybrids(loc, hybrids, hybridIndexByCrop, cropsByIndex);
                }
                foreach (var loc in moreLocs)
                    this.GrowHybrids(loc, hybrids, hybridIndexByCrop, cropsByIndex);
            }
            else
            {
                this.GrowHybrids(Game1.getFarm(), hybrids, hybridIndexByCrop, cropsByIndex);
                this.GrowHybrids(Game1.getLocationFromName("Greenhouse"), hybrids, hybridIndexByCrop, cropsByIndex);
                this.GrowHybrids(Game1.getLocationFromName("IslandWest"), hybrids, hybridIndexByCrop, cropsByIndex);
            }
        }

        private Dictionary<ulong, int> MakeHybridIndex(Dictionary<int, HybridCropData> data)
        {
            var ret = new Dictionary<ulong, int>();
            foreach (var entry in data)
            {
                ulong la = (ulong)entry.Value.BaseCropA;
                ulong lb = (ulong)entry.Value.BaseCropB;

                ret.Add((la << 32) | lb, entry.Key);
                if (entry.Value.BaseCropA != entry.Value.BaseCropB)
                    ret.Add((lb << 32) | la, entry.Key);
            }
            return ret;
        }

        private Dictionary<int, int> MakeCropIndex()
        {
            var ret = new Dictionary<int, int>();
            var crops = Game1.content.Load<Dictionary<int, string>>("Data/Crops");
            foreach (var crop in crops)
            {
                if (crop.Key is >= 495 and <= 498)
                    continue;

                string[] fields = crop.Value.Split('/');
                ret.Add(int.Parse(fields[2]), crop.Key);
            }
            return ret;
        }

        private void GrowHybrids(GameLocation loc, Dictionary<int, HybridCropData> hybrids, Dictionary<ulong, int> hybridIndexes, Dictionary<int, int> cropSeedIndex)
        {
            int baseSeed = loc.NameOrUniqueName.GetHashCode();
            baseSeed ^= (int)Game1.uniqueIDForThisGame;
            baseSeed += (int)Game1.stats.DaysPlayed;

            for (int ix = 0; ix < loc.Map.Layers[0].LayerSize.Width; ++ix)
            {
                for (int iy = 0; iy < loc.Map.Layers[0].LayerSize.Height; ++iy)
                {
                    HoeDirt GetHoeDirt(int x, int y) => (loc.terrainFeatures.TryGetValue(new Vector2(ix + x, iy + y), out TerrainFeature feature) ? feature as HoeDirt : null);

                    HoeDirt[] dirts = new[]
                    {
                        GetHoeDirt(-1, -1), GetHoeDirt(0, -1), GetHoeDirt(1, -1),
                        GetHoeDirt(-1, 0), GetHoeDirt(0, 0), GetHoeDirt(1, 0),
                        GetHoeDirt(-1, 1), GetHoeDirt(0, 1), GetHoeDirt(1, 1)
                    };

                    if (dirts[4] == null || dirts[4].crop != null)
                        continue;

                    // Make only hoe dirts with fully grown crops remain in the dirts array
                    for (int h = 0; h < dirts.Length; ++h)
                    {
                        if (h == 4)
                            continue;
                        var hd = dirts[h];
                        if (hd != null && hd.crop == null)
                            dirts[h] = null;
                        else if (hd != null)
                        {
                            //Log.trace( "crop:" + hd.crop.currentPhase.Value + " " + (hd.crop.phaseDays.Count - 1) + " " + hd.crop.dayOfCurrentPhase );
                            if (hd.crop.currentPhase.Value == hd.crop.phaseDays.Count - 1 /*&& hd.crop.dayOfCurrentPhase.Value == 0*/ )
                            {
                                //Log.trace( "Crop is ready @ " +ix + " " + iy + " " + h );
                            }
                            else
                            {
                                dirts[h] = null;
                            }
                        }
                    }

                    //*
                    string d = "";
                    foreach (var dirt in dirts)
                        d += (dirt == null) ? "null" : dirt.ToString();
                    Log.Trace("dirts:" + d);
                    //*/

                    var combos = new List<HoeDirt[]>();

                    void AddIfCombo(int a, int b)
                    {
                        if (dirts[a] != null && dirts[b] != null)
                            combos.Add(new[] { dirts[a], dirts[b] });
                    }

                    AddIfCombo(0, 1);
                    AddIfCombo(1, 2);
                    AddIfCombo(0, 3);
                    AddIfCombo(2, 5);
                    AddIfCombo(3, 6);
                    AddIfCombo(5, 8);
                    AddIfCombo(6, 7);
                    AddIfCombo(7, 8);
                    AddIfCombo(1, 3);
                    AddIfCombo(1, 5);
                    AddIfCombo(3, 7);
                    AddIfCombo(5, 7);
                    //Log.trace( "Combo size: " + combos.Count );

                    Random r = new Random(baseSeed + ix * loc.Map.Layers[0].LayerSize.Height + iy);
                    foreach (var combo in combos)
                    {
                        ulong ca = (ulong)combo[0].crop.rowInSpriteSheet.Value;
                        ulong cb = (ulong)combo[1].crop.rowInSpriteSheet.Value;
                        ulong code = (ca << 32) | cb;

                        if (!hybridIndexes.TryGetValue(code, out int index))
                        {
                            //Log.trace( "No hybrid for " + ca + "/" + cb );
                            continue;
                        }

                        var hybridData = hybrids[index];
                        if (r.NextDouble() < hybridData.Chance)
                        {
                            //Log.trace( "Making hybrid @ " + ix + " " + iy );
                            dirts[4].crop = new Crop(cropSeedIndex[index], ix, iy);
                            break;
                        }
                    }
                }
            }
        }
    }
}
