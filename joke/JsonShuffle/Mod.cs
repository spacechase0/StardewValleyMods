using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace JsonShuffle
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private Dictionary<string, string> shuffleCache = new();

        public override void Entry(IModHelper helper)
        {
            Log.Monitor = Monitor;
            instance = this;

            Helper.ConsoleCommands.Add("jsonshuffle", "...", (cmd, args) =>
            {
                if (Game1.IsMasterGame)
                {
                    Shuffle();
                }
                else
                {
                    ShuffleInventory();
                }
            });

            Helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            BuildShuffleCache();

            if (Game1.IsMasterGame)
            {
                Shuffle();
            }
            else
            {
                ShuffleInventory();
            }
        }

        private void BuildShuffleCache()
        {
            shuffleCache.Clear();
            foreach (var type in ItemRegistry.ItemTypes)
            {
                string typeStr = type.Identifier;
                List<string> types = type.GetAllIds().ToList();
                List<string> types2 = new(types);

                foreach (string entry in types)
                {
                    int i = Game1.random.Next(types2.Count);
                    shuffleCache.Add($"{typeStr}{entry}", $"{typeStr}{types2[i]}");
                    types2.RemoveAt(i);
                }
            }
        }

        private void ShuffleItem(Item item)
        {
            if (item == null)
                return;

            if (shuffleCache.TryGetValue(item.QualifiedItemId, out string newQualId))
            {
                item.ItemId = newQualId.Substring(newQualId.IndexOf(')') + 1);
                if (item is StardewValley.Object obj)
                {
                    obj.ParentSheetIndex = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId).SpriteIndex;
                }
            }
        }

        private void Shuffle()
        {
            Utility.ForEachItem((item) =>
            {
                ShuffleItem(item);
                return true;
            });
        }

        private void ShuffleInventory()
        {
            foreach (var item in Game1.player.Items)
                ShuffleItem(item);
            foreach (var item in Game1.player.trinketItems)
                ShuffleItem(item);
            ShuffleItem(Game1.player.leftRing.Value);
            ShuffleItem(Game1.player.rightRing.Value);
            ShuffleItem(Game1.player.hat.Value);
            ShuffleItem(Game1.player.shirtItem.Value);
            ShuffleItem(Game1.player.pantsItem.Value);
            ShuffleItem(Game1.player.boots.Value);
        }
    }
}
