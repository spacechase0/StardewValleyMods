using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace MoreRings.Framework
{
    /// <summary>Manages the Ring of True Sight effects.</summary>
    internal static class TrueSight
    {
        /*********
        ** Fields
        *********/
        /// <summary>The object sprites to render over the world.</summary>
        private static readonly Dictionary<string, SObject> DrawObjects = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Draw the True Sight effects if needed.</summary>
        /// <param name="spriteBatch">The sprite batch being rendered.</param>
        public static void DrawOverWorld(SpriteBatch spriteBatch)
        {
            if (!Context.IsWorldReady || !Mod.Instance.HasRingEquipped(Mod.Instance.RingTrueSight))
                return;

            foreach (var pair in Game1.currentLocation.netObjects.Pairs)
            {
                var pos = pair.Key;
                var obj = pair.Value;
                string doDraw = "-1";

                if (!(Game1.currentLocation.Name.StartsWith("UndergroundMine")))
                {
                    if (obj.ItemID is "343" or "450")
                    {

                        Random rand = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)pos.X * 2000 + (int)pos.Y);
                        if (rand.NextDouble() < 0.035 && Game1.stats.DaysPlayed > 1U)
                            doDraw = (535 + (Game1.stats.DaysPlayed <= 60U || rand.NextDouble() >= 0.2 ? (Game1.stats.DaysPlayed <= 120U || rand.NextDouble() >= 0.2 ? 0 : 2) : 1)).ToString();
                        if (rand.NextDouble() < 0.035 * (Game1.player.professions.Contains(21) ? 2.0 : 1.0) && Game1.stats.DaysPlayed > 1U)
                            doDraw = "382";
                        if (rand.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 1U)
                            doDraw = "390";

                        if (doDraw == "390") // 390 is more stone
                            continue;

                    }
                }
                else if (obj.Name.Contains("Stone"))
                {
                    doDraw = TrueSight.GetMineStoneDrop(obj.ItemID, (int)pos.X, (int)pos.Y, Game1.player, (Game1.currentLocation as MineShaft));
                }

                if (obj.ItemID == "590")
                    doDraw = TrueSight.GetArtifactSpotDrop((int)pos.X, (int)pos.Y, Game1.player);

                if (doDraw != "-1")
                {
                    if (doDraw == "-2")
                    {
                        var ts = Game1.content.Load<Texture2D>(Game1.currentLocation.Map.TileSheets[0].ImageSource);
                        spriteBatch.Draw(ts, Game1.GlobalToLocal(Game1.viewport, new Vector2(pos.X * 64, pos.Y * 64)), new Rectangle(208, 160, 16, 16), new Color(255, 255, 255, 128), 0, Vector2.Zero, 4, SpriteEffects.None, 1);
                    }
                    else
                    {
                        if (!TrueSight.DrawObjects.TryGetValue(doDraw, out SObject drawObj))
                        {
                            drawObj = new SObject(new Vector2(0, 0), doDraw, 1);
                            TrueSight.DrawObjects.Add(doDraw, drawObj);
                        }

                        drawObj.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(pos.X * 64, pos.Y * 64)), 0.8f, 0.5f, 1, StackDrawType.Hide, Color.White, false);
                    }
                }
            }

            if (Game1.currentLocation is IslandLocation il)
            {
                for (int ix = 0; ix < Game1.currentLocation.Map.Layers[0].LayerWidth; ++ix)
                {
                    for (int iy = 0; iy < Game1.currentLocation.Map.Layers[0].LayerWidth; ++iy)
                    {
                        if (il.IsBuriedNutLocation(new Point(ix, iy)) && !Game1.netWorldState.Value.FoundBuriedNuts.ContainsKey($"{il.NameOrUniqueName}_{ix}_{iy}"))
                        {
                            if (!TrueSight.DrawObjects.TryGetValue("73", out SObject drawObj))
                            {
                                drawObj = new SObject(new Vector2(0, 0), "73", 1);
                                TrueSight.DrawObjects.Add("73", drawObj);
                            }

                            var pos = new Vector2(ix, iy);
                            drawObj.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(pos.X * 64, pos.Y * 64)), 0.8f, 0.5f, 1, StackDrawType.Hide, Color.White, false);
                        }
                    }
                }
            }
        }

        /// <summary>Get the item ID that will be dropped by the given stone in a mineshaft.</summary>
        /// <param name="tileIndexOfStone">The tilesheet index of the stone.</param>
        /// <param name="x">The stone's tile X position.</param>
        /// <param name="y">The stone's tile Y position.</param>
        /// <param name="player">The player performing the check.</param>
        /// <param name="mine">The mineshaft containing the stone.</param>
        public static string GetMineStoneDrop(string tileIndexOfStone, int x, int y, Farmer player, MineShaft mine)
        {
            int mineLevel = mine.mineLevel;
            int stonesLeftOnThisLevel = Mod.Instance.Helper.Reflection.GetProperty<int>(mine, "stonesLeftOnThisLevel").GetValue();
            bool ladderHasSpawned = Mod.Instance.Helper.Reflection.GetField<bool>(mine, "ladderHasSpawned").GetValue();

            player ??= Game1.player;
            double num1 = player.DailyLuck / 2.0 + player.MiningLevel * 0.005 + player.LuckLevel * 0.001;
            Random r = new Random(x * 1000 + y + mineLevel + (int)Game1.uniqueIDForThisGame / 2);
            r.NextDouble();
            double num2 = tileIndexOfStone is "40" or "42" ? 1.2 : 0.8;
            //if (tileIndexOfStone != 34 && tileIndexOfStone != 36 && tileIndexOfStone != 50)
            //    ;
            --stonesLeftOnThisLevel;
            double num3 = 0.02 + 1.0 / Math.Max(1, stonesLeftOnThisLevel) + player.LuckLevel / 100.0 + player.team.sharedDailyLuck.Value / 5.0;
            if (mine.characters.Count == 0)
                num3 += 0.04;
            if (!ladderHasSpawned && !mine.mustKillAllMonstersToAdvance() && (stonesLeftOnThisLevel == 0 || r.NextDouble() < num3))
                return "-2";
            string baseStone = TrueSight.GetBaseMineStoneDrop(tileIndexOfStone, x, y, player, r, mine);
            if (baseStone != "-1")
                return baseStone;

            if (tileIndexOfStone == "44")
            {
                int num4 = r.Next(59, 70);
                string objectIndex = (num4 + num4 % 2).ToString();
                if (player.timesReachedMineBottom == 0)
                {
                    if (mineLevel < 40 && objectIndex != "66" && objectIndex != "68")
                        objectIndex = r.NextDouble() < 0.5 ? "66" : "68";
                    else if (mineLevel < 80 && (objectIndex is "64" or "60"))
                        objectIndex = r.NextDouble() < 0.5 ? (r.NextDouble() < 0.5 ? "66" : "70") : (r.NextDouble() < 0.5 ? "68" : "62");
                }
                return objectIndex;
            }
            else
            {
                if (r.NextDouble() < 0.022 * (1.0 + num1) * (player.professions.Contains(22) ? 2.0 : 1.0))
                {
                    string objectIndex = (535 + (mine.getMineArea() == 40 ? 1 : (mine.getMineArea() == 80 ? 2 : 0))).ToString();
                    if (mine.getMineArea() == 121)
                        objectIndex = "749";
                    if (player.professions.Contains(19) && r.NextDouble() < 0.5)
                        return objectIndex;
                    return objectIndex;
                }
                if (mineLevel > 20 && r.NextDouble() < 0.005 * (1.0 + num1) * (player.professions.Contains(22) ? 2.0 : 1.0))
                {
                    if (player.professions.Contains(19) && r.NextDouble() < 0.5)
                        return "749";
                    return "749";
                }
                if (r.NextDouble() < 0.05 * (1.0 + num1) * num2)
                {
                    r.Next(1, 3);
                    r.NextDouble();
                    if (r.NextDouble() < 0.25 * (player.professions.Contains(21) ? 2.0 : 1.0))
                    {
                        return "382";
                    }
                    else
                        return mine.getOreIndexForLevel(mineLevel, r);
                }
                else
                {
                    if (r.NextDouble() >= 0.5)
                        return "-1";
                }
            }

            return "-1";
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the item ID that will be dropped by breaking the given stone in a mineshaft.</summary>
        /// <param name="indexOfStone">The tilesheet index of the stone.</param>
        /// <param name="x">The stone's tile X position.</param>
        /// <param name="y">The stone's tile Y position.</param>
        /// <param name="player">The player performing the check.</param>
        /// <param name="r">The random number generator to use for randomized drop checks.</param>
        /// <param name="mine">The mineshaft containing the stone.</param>
        private static string GetBaseMineStoneDrop(string indexOfStone, int x, int y, Farmer player, Random r, MineShaft mine)
        {
            // Note: I can't get this to work for 74 (prismatic shards) correctly
            // I commented out the part that makes it appear for now

            string ret = "-1";
            if (indexOfStone == "44")
                indexOfStone = (Game1.random.Next(1, 8) * 2).ToString();
            switch (indexOfStone)
            {
                case "2":
                    ret = "72";
                    break;
                case "4":
                    ret = "64";
                    break;
                case "6":
                    ret = "70";
                    break;
                case "8":
                    ret = "66";
                    break;
                case "10":
                    ret = "68";
                    break;
                case "12":
                    ret = "60";
                    break;
                case "14":
                    ret = "62";
                    break;
                case "25":
                    ret = "719";
                    r.Next(2, 5);
                    if (r.NextDouble() < 0.1 && Game1.player.team.limitedNutDrops["MusselStone"] < 5)
                        ret = "73";
                    break;
                case "75":
                    ret = "535";
                    break;
                case "76":
                    ret = "536";
                    break;
                case "77":
                    ret = "537";
                    break;
                case "95":
                    ret = "909";
                    r.Next(1, 3);
                    r.NextDouble(); r.NextDouble();
                    break;
                case "290":
                case "850":
                    ret = "380";
                    r.Next(1, 4);
                    r.NextDouble(); r.NextDouble();
                    break;
                case "668":
                case "670":
                case "845":
                case "846":
                case "847":
                    ret = "390";
                    r.NextDouble(); r.NextDouble();
                    if (r.NextDouble() < 0.08)
                        ret = "382";
                    break;
                case "751":
                case "849":
                    ret = "378";
                    r.Next(1, 4);
                    r.NextDouble(); r.NextDouble();
                    break;
                case "764":
                    ret = "384";
                    r.Next(1, 4);
                    r.NextDouble(); r.NextDouble();
                    break;
                case "765":
                    ret = "386";
                    r.Next(1, 4);
                    r.NextDouble(); r.NextDouble();
                    if (r.NextDouble() < 0.04)
                        ret = "74";
                    break;
                case "816":
                case "817":
                    ret = "881";
                    r.Next(1, 3);
                    r.NextDouble(); r.NextDouble();
                    if (r.NextDouble() < 0.1)
                        ret = "823";
                    else if (r.NextDouble() < 0.015)
                        ret = "824";
                    else if (r.NextDouble() < 0.1)
                        ret = (579 + r.Next(11)).ToString();
                    break;
                case "818":
                    ret = "330";
                    r.Next(1, 3);
                    r.NextDouble(); r.NextDouble();
                    break;
                case "843":
                case "844":
                    ret = "848";
                    r.Next(1, 3);
                    r.NextDouble(); r.NextDouble();
                    break;

            }
            if (player.professions.Contains(19) && r.NextDouble() < 0.5)
            {
                ret = indexOfStone switch
                {
                    "2" => "72",
                    "4" => "64",
                    "6" => "70",
                    "8" => "66",
                    "10" => "68",
                    "12" => "60",
                    "14" => "62",
                    _ => ret
                };
            }
            if (indexOfStone == "46")
            {
                r.Next(1, 4);
                r.Next(1, 5);
                if (r.NextDouble() < 0.25)
                    ;// ret = 74;
            }
            if ((mine.IsOutdoors || mine.treatAsOutdoors.Value) && ret == "-1")
            {
                double num2 = player.DailyLuck / 2.0 + player.MiningLevel * 0.005 + player.LuckLevel * 0.001;
                Random random = new Random(x * 1000 + y + (int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);

                if (player.professions.Contains(21) && random.NextDouble() < 0.05 * (1.0 + num2))
                    ret = "382";
                if (random.NextDouble() < 0.05 * (1.0 + num2))
                {
                    random.Next(1, 3);
                    random.NextDouble();
                    random.NextDouble();
                    ret = "382";
                }
            }
            if (player.hasMagnifyingGlass && r.NextDouble() < 3.0 / 400.0)
            {
                var unseenSecretNote = mine.tryToCreateUnseenSecretNote(player);
                if (unseenSecretNote != null)
                    ret = unseenSecretNote.ItemID;
            }
            return ret;
        }

        /// <summary>Get the item ID that will be dropped by digging up a given artifact spot.</summary>
        /// <param name="x">The stone's tile X position.</param>
        /// <param name="y">The stone's tile Y position.</param>
        /// <param name="player">The player performing the check.</param>
        private static string GetArtifactSpotDrop(int x, int y, Farmer player)
        {
            Random random = new Random(x * 2000 + y + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
            bool archeologyEnchant = player?.CurrentTool is Hoe && player.CurrentTool.hasEnchantmentOfType<ArchaeologistEnchantment>();
            string objectIndex = "-1";
            foreach (KeyValuePair<string, string> keyValuePair in Game1.objectInformation)
            {
                string[] strArray1 = keyValuePair.Value.Split('/');
                if (strArray1[3].Contains("Arch"))
                {
                    string[] strArray2 = strArray1[6].Split(' ');
                    int index = 0;
                    while (index < strArray2.Length)
                    {
                        if (strArray2[index].Equals(Game1.currentLocation.Name) && random.NextDouble() < (archeologyEnchant ? 2 : 1) * Convert.ToDouble(strArray2[index + 1], CultureInfo.InvariantCulture))
                        {
                            objectIndex = keyValuePair.Key;
                            break;
                        }
                        index += 2;
                    }
                }
                if (objectIndex != "-1")
                    break;
            }
            if (random.NextDouble() < 0.2 && Game1.currentLocation is not Farm)
                objectIndex = "102";
            if (objectIndex == "102" && player.archaeologyFound.TryGetValue("102", out int[] archeologyValues) && archeologyValues[0] >= 21)
                objectIndex = "770";
            if (objectIndex != "-1")
            {
                return objectIndex;
            }
            else if (Game1.currentSeason.Equals("winter") && random.NextDouble() < 0.5 && Game1.currentLocation is not Desert)
            {
                if (random.NextDouble() < 0.4)
                {
                    random.NextDouble();
                    return "416";
                }
                else
                {
                    random.NextDouble();
                    return "412";
                }
            }
            else
            {
                //*
                if (random.NextDouble() <= 0.2 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
                {
                    return "890";
                }
                if (Game1.GetSeasonForLocation(Game1.currentLocation).Equals("spring") && random.NextDouble() < 0.0625 && Game1.currentLocation is not Desert && Game1.currentLocation is not Beach)
                {
                    return "273";
                }
                if (Game1.random.NextDouble() <= 0.2 && (Game1.MasterPlayer.mailReceived.Contains("guntherBones") || Game1.player.team.specialOrders.Any(p => p.questKey.Value == "Gunther")))
                {
                    return "881";
                }
                //*/

                Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
                if (!dictionary.TryGetValue(Game1.currentLocation.name, out string rawLocationData))
                    return "-1";
                string[] strArray = rawLocationData.Split('/')[8].Split(' ');
                if (strArray.Length == 0 || strArray[0].Equals("-1"))
                    return "-1";
                int index1 = 0;
                while (index1 < strArray.Length)
                {
                    if (random.NextDouble() <= Convert.ToDouble(strArray[index1 + 1]))
                    {
                        string index2 = /*Convert.ToInt32*/(strArray[index1]);
                        if (Game1.objectInformation.TryGetValue(index2, out string objData))
                        {
                            if (objData.Split('/')[3].Contains("Arch") || index2 == "102")
                            {
                                if (index2 == "102" && Game1.netWorldState.Value.LostBooksFound.Value >= 21)
                                    index2 = "770";
                                return index2;
                            }
                        }
                        if (index2 == "330" && Game1.currentLocation.HasUnlockedAreaSecretNotes(player) && random.NextDouble() < 0.11)
                        {
                            SObject unseenSecretNote = Game1.currentLocation.tryToCreateUnseenSecretNote(player);
                            if (unseenSecretNote != null)
                            {
                                return "79";
                            }
                        }
                        else if (index2 == "330" && Game1.stats.DaysPlayed > 28 && random.NextDouble() < 0.1)
                        {
                            return (688 + random.Next(3)).ToString();
                        }
                        return index2;
                    }
                    index1 += 2;
                }
            }
            return "-1";
        }
    }
}
