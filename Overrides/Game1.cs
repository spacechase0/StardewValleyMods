using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpaceCore.Events;
using SpaceCore.Locations;
using SpaceCore.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace SpaceCore.Overrides
{
    [HarmonyPatch(typeof(Game1), "showEndOfNightStuff")]
    public class ShowEndOfNightStuffHook
    {
        public static void showEndOfNightStuff_mid()
        {
            var ev = new EventArgsShowNightEndMenus();
            SpaceEvents.InvokeShowNightEndMenus(ev);
        }
        
        internal static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldstr && (string)insn.operand == "newRecord")
                {
                    newInsns.Insert(newInsns.Count - 2, new CodeInstruction(OpCodes.Call, typeof(ShowEndOfNightStuffHook).GetMethod("showEndOfNightStuff_mid")));
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }
    }

    [HarmonyPatch(typeof(Game1), "setGraphicsForSeason")]
    internal static class SeasonGraphicsForSeasonalLocationsPatch
    {
        // All I've done is add checks relating to ISeasonalLocation
        public static void setGraphicsForSeason()
        {
            try
            {
                foreach (GameLocation location in Game1.locations)
                {
                    location.seasonUpdate(Game1.currentSeason, true);
                    if (location.IsOutdoors)
                    {
                        var seasonalLoc = location as ISeasonalLocation;
                        if (!location.Name.Equals("Desert") && seasonalLoc == null)
                        {
                            for (int index = 0; index < location.Map.TileSheets.Count; ++index)
                            {
                                if (!location.Map.TileSheets[index].ImageSource.Contains('_'))
                                    continue;
                                if (!location.Map.TileSheets[index].ImageSource.Contains("path") && !location.Map.TileSheets[index].ImageSource.Contains("object"))
                                {
                                    location.Map.TileSheets[index].ImageSource = "Maps\\" + Game1.currentSeason + "_" + location.Map.TileSheets[index].ImageSource.Split('_')[1];
                                    location.Map.DisposeTileSheets(Game1.mapDisplayDevice);
                                    location.Map.LoadTileSheets(Game1.mapDisplayDevice);
                                }
                            }
                        }
                        if (Game1.currentSeason.Equals("spring") || (seasonalLoc != null && seasonalLoc.Season == "spring"))
                        {
                            foreach (KeyValuePair<Vector2, SObject> keyValuePair in (Dictionary<Vector2, SObject>)location.Objects)
                            {
                                if ((keyValuePair.Value.Name.Contains("Stump") || keyValuePair.Value.Name.Contains("Boulder") || (keyValuePair.Value.Name.Equals("Stick") || keyValuePair.Value.Name.Equals("Stone"))) && (keyValuePair.Value.ParentSheetIndex >= 378 && keyValuePair.Value.ParentSheetIndex <= 391))
                                    keyValuePair.Value.ParentSheetIndex -= 376;
                            }
                            Game1.eveningColor = new Color((int)byte.MaxValue, (int)byte.MaxValue, 0);
                        }
                        else if (Game1.currentSeason.Equals("summer") || (seasonalLoc != null && seasonalLoc.Season == "summer"))
                        {
                            foreach (KeyValuePair<Vector2, SObject> keyValuePair in (Dictionary<Vector2, SObject>)location.Objects)
                            {
                                if (keyValuePair.Value.Name.Contains("Weed"))
                                {
                                    if (keyValuePair.Value.parentSheetIndex == 792)
                                        ++keyValuePair.Value.ParentSheetIndex;
                                    else if (Game1.random.NextDouble() < 0.3)
                                        keyValuePair.Value.ParentSheetIndex = 676;
                                    else if (Game1.random.NextDouble() < 0.3)
                                        keyValuePair.Value.ParentSheetIndex = 677;
                                }
                            }
                            Game1.eveningColor = new Color((int)byte.MaxValue, (int)byte.MaxValue, 0);
                        }
                        else if (Game1.currentSeason.Equals("fall") || (seasonalLoc != null && seasonalLoc.Season == "fall"))
                        {
                            foreach (KeyValuePair<Vector2, SObject> keyValuePair in (Dictionary<Vector2, SObject>)location.Objects)
                            {
                                if (keyValuePair.Value.Name.Contains("Weed"))
                                {
                                    if (keyValuePair.Value.parentSheetIndex == 793)
                                        ++keyValuePair.Value.ParentSheetIndex;
                                    else
                                        keyValuePair.Value.ParentSheetIndex = Game1.random.NextDouble() >= 0.5 ? 679 : 678;
                                }
                            }
                            Game1.eveningColor = new Color((int)byte.MaxValue, (int)byte.MaxValue, 0);
                            foreach (WeatherDebris weatherDebris in Game1.debrisWeather)
                                weatherDebris.which = 2;
                        }
                        else if (Game1.currentSeason.Equals("winter") || (seasonalLoc != null && seasonalLoc.Season == "winter"))
                        {
                            for (int index = location.Objects.Count - 1; index >= 0; --index)
                            {
                                SObject @object = location.Objects[location.Objects.Keys.ElementAt<Vector2>(index)];
                                if (@object.Name.Contains("Weed"))
                                    location.Objects.Remove(location.Objects.Keys.ElementAt<Vector2>(index));
                                else if ((!@object.Name.Contains("Stump") && !@object.Name.Contains("Boulder") && (!@object.Name.Equals("Stick") && !@object.Name.Equals("Stone")) || @object.ParentSheetIndex > 100) && (location.IsOutdoors && !@object.isHoedirt))
                                    @object.name.Equals("HoeDirt");
                            }
                            foreach (WeatherDebris weatherDebris in Game1.debrisWeather)
                                weatherDebris.which = 3;
                            Game1.eveningColor = new Color(245, 225, 170);
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                Log.error("Something went wrong! " + e);
            }
        }

        // TODO: Make this do IL hooking instead of pre + no execute original
        internal static bool Prefix()
        {
            setGraphicsForSeason();
            return false;
        }
    }
}
