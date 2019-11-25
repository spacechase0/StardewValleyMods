using System;
using StardewModdingAPI;
using System.IO;
using StardewValley.Menus;
using StardewValley;
using SpaceCore.Events;
using StardewModdingAPI.Events;
using SpaceShared.APIs;
using SpaceShared;

namespace MoreRings
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private JsonAssetsAPI ja;
        public int Ring_Fishing_LargeBar { get { return ja.GetObjectId("Ring of Wide Nets"); } }
        public int Ring_Combat_Regen { get { return ja.GetObjectId("Ring of Regeneration"); } }
        public int Ring_DiamondBooze { get { return ja.GetObjectId("Ring of Diamond Booze"); } }
        public int Ring_Refresh { get { return ja.GetObjectId("Refreshing Ring"); } }

        private MoreRingsApi moreRings;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.Display.MenuChanged += onMenuChanged;
            helper.Events.GameLoop.UpdateTicked += onUpdateTicked;

            SpaceEvents.OnItemEaten += onItemEaten;
        }

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = Helper.ModRegistry.GetApi<JsonAssetsAPI>("spacechase0.JsonAssets");
            if ( api == null )
            {
                Log.error("No Json Assets API???");
                return;
            }
            ja = api;

            api.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));

            moreRings = Helper.ModRegistry.GetApi<MoreRingsApi>("bcmpinc.WearMoreRings");
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if ( e.NewMenu is BobberBar bobber && hasRingEquipped( Ring_Fishing_LargeBar ) > 0 )
            {
                var field = Helper.Reflection.GetField<int>(bobber, "bobberBarHeight");
                field.SetValue((int)(field.GetValue() * 1.50));
            }
        }

        private int regenCounter = 0;
        private int refreshCounter = 0;

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.IsOneSecond)
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
            if (moreRings != null)
                return moreRings.CountEquippedRings(Game1.player, id);

            int num = 0;
            if (Game1.player.leftRing.Value != null && Game1.player.leftRing.Value.ParentSheetIndex == id)
                ++num;
            if (Game1.player.rightRing.Value != null && Game1.player.rightRing.Value.ParentSheetIndex == id)
                ++num;
            return num;
        }
    }
}
