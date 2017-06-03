using Microsoft.Xna.Framework;
using SpaceCore.Events;
using SpaceCore.Locations;
using SpaceCore.Utilities;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SObject = StardewValley.Object;

namespace SpaceCore.Overrides
{
    class NewGame1
    {
        internal static void hijack()
        {
            Hijack.hijack(typeof(   Game1).GetMethod("showEndOfNightStuff",  BindingFlags.Static | BindingFlags.Public),
                          typeof(NewGame1).GetMethod("showEndOfNightStuff",  BindingFlags.Static | BindingFlags.Public));
            Hijack.hijack(typeof(   Game1).GetMethod("setGraphicsForSeason", BindingFlags.Static | BindingFlags.Public),
                          typeof(NewGame1).GetMethod("setGraphicsForSeason", BindingFlags.Static | BindingFlags.Public));
        }
        
        public static void showEndOfNightStuff()
        {
            var ev = new EventArgsShowNightEndMenus();
            SpaceEvents.InvokeShowNightEndMenus(ev);

            bool flag1 = false;
            if (ev.ProcessShippedItems && Game1.getFarm().shippingBin.Count > 0)
            {
                Game1.endOfNightMenus.Push((IClickableMenu)new ShippingMenu(Game1.getFarm().shippingBin));
                Game1.getFarm().shippingBin.Clear();
                flag1 = true;
            }
            bool flag2 = false;
            if (Game1.player.newLevels.Count > 0 && !flag1)
                Game1.endOfNightMenus.Push((IClickableMenu)new SaveGameMenu());
            while (Game1.player.newLevels.Count > 0)
            {
                Game1.endOfNightMenus.Push((IClickableMenu)new LevelUpMenu(Game1.player.newLevels.Last<Point>().X, Game1.player.newLevels.Last<Point>().Y));
                Game1.player.newLevels.RemoveAt(Game1.player.newLevels.Count - 1);
                flag2 = true;
            }
            if (flag2)
                Game1.playSound("newRecord");
            if (Game1.endOfNightMenus.Count > 0)
            {
                Game1.showingEndOfNightStuff = true;
                Game1.activeClickableMenu = Game1.endOfNightMenus.Pop();
            }
            else if (Game1.saveOnNewDay)
            {
                Game1.showingEndOfNightStuff = true;
                Game1.activeClickableMenu = (IClickableMenu)new SaveGameMenu();
            }
            else
            {
                Game1.currentLocation.resetForPlayerEntry();
                Game1.globalFadeToClear(new Game1.afterFadeFunction(Game1.playMorningSong), 0.02f);
            }
        }

        public static void setGraphicsForSeason()
        {
            // All I've done is add checks relating to ISeasonalLocation
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
    }
}
