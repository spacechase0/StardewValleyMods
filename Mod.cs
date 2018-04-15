using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using MoreRings.Other;
using System.IO;
using StardewValley.Menus;
using StardewValley;
using SpaceCore.Events;

namespace MoreRings
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private JsonAssetsApi ja;
        public int Ring_Fishing_LargeBar { get { return ja.GetObjectId("Ring of Wide Nets"); } }
        public int Ring_Combat_Regen { get { return ja.GetObjectId("Ring of Regeneration"); } }
        public int Ring_DiamondBooze { get { return ja.GetObjectId("Ring of Diamond Booze"); } }
        public int Ring_Refresh { get { return ja.GetObjectId("Refreshing Ring"); } }

        public override void Entry(IModHelper helper)
        {
            instance = this;

            GameEvents.FirstUpdateTick += firstUpdate;
            MenuEvents.MenuChanged += menuChanged;
            GameEvents.OneSecondTick += oneSecond;

            SpaceEvents.OnItemEaten += onItemEaten;
        }

        private void firstUpdate(object sender, EventArgs args)
        {
            var api = Helper.ModRegistry.GetApi<JsonAssetsApi>("spacechase0.JsonAssets");
            if ( api == null )
            {
                Log.error("No Json Assets API???");
                return;
            }
            ja = api;

            api.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));
        }

        private void menuChanged(object sender, EventArgsClickableMenuChanged args)
        {
            if ( args.NewMenu is BobberBar bobber && hasRingEquipped( Ring_Fishing_LargeBar ) > 0 )
            {
                var field = Helper.Reflection.GetField<int>(bobber, "bobberBarHeight");
                field.SetValue((int)(field.GetValue() * 1.50));
            }
        }

        private int regenCounter = 0;
        private int refreshCounter = 0;
        private void oneSecond(object sender, EventArgs args)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            if ( hasRingEquipped( Ring_Combat_Regen ) > 0 && regenCounter++ >= 4 / hasRingEquipped( Ring_Combat_Regen ) )
            {
                regenCounter = 0;
                Game1.player.health = Math.Min(Game1.player.health + 1, Game1.player.maxHealth);
            }

            if (hasRingEquipped(Ring_Refresh) > 0 && refreshCounter++ >= 4 / hasRingEquipped(Ring_Refresh))
            {
                refreshCounter = 0;
                Game1.player.Stamina = Math.Min(Game1.player.Stamina + 1, Game1.player.MaxStamina);
            }
        }

        private void onItemEaten( object sender, EventArgs args )
        {
            if (hasRingEquipped(Ring_DiamondBooze) > 0)
            {
                Buff tipsyBuff = null;
                foreach (var buff in Game1.buffsDisplay.otherBuffs)
                    if (buff.which == Buff.tipsy)
                    {
                        tipsyBuff = buff;
                        break;
                    }
                if (tipsyBuff != null)
                {
                    tipsyBuff.removeBuff();
                    Game1.buffsDisplay.otherBuffs.Remove(tipsyBuff);
                }

                if (Game1.buffsDisplay.drink != null)
                {
                    if (Game1.buffsDisplay.drink.which == Buff.tipsy)
                    {
                        Game1.buffsDisplay.drink.removeBuff();
                        Game1.buffsDisplay.drink = null;
                    }
                    else
                    {
                        var attrs = Helper.Reflection.GetField<int[]>(Game1.buffsDisplay.drink, "buffAttributes").GetValue();
                        if (attrs[Buff.speed] == -1)
                        {
                            Game1.buffsDisplay.drink.removeBuff();
                            Game1.buffsDisplay.drink = null;
                        }
                        else if ( attrs[Buff.speed] < 0 )
                        {
                            Game1.buffsDisplay.drink.removeBuff();
                            attrs[Buff.speed]++;
                            Game1.buffsDisplay.drink.addBuff();
                        }
                    }
                }
                Game1.buffsDisplay.syncIcons();
            }
        }

        private int hasRingEquipped( int id )
        {
            int num = 0;
            if (Game1.player.leftRing != null && Game1.player.leftRing.parentSheetIndex == id)
                ++num;
            if (Game1.player.rightRing != null && Game1.player.rightRing.parentSheetIndex == id)
                ++num;
            return num;
        }
    }
}
