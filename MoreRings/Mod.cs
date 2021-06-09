using System;
using System.IO;
using MoreRings.Patches;
using Spacechase.Shared.Harmony;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace MoreRings
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private JsonAssetsAPI ja;
        public int Ring_Fishing_LargeBar { get { return this.ja.GetObjectId("Ring of Wide Nets"); } }
        public int Ring_Combat_Regen { get { return this.ja.GetObjectId("Ring of Regeneration"); } }
        public int Ring_DiamondBooze { get { return this.ja.GetObjectId("Ring of Diamond Booze"); } }
        public int Ring_Refresh { get { return this.ja.GetObjectId("Refreshing Ring"); } }
        public int Ring_Quality { get { return this.ja.GetObjectId("Quality+ Ring"); } }
        public int Ring_MageHand { get { return this.ja.GetObjectId("Ring of Far Reaching"); } }
        public int Ring_TrueSight { get { return this.ja.GetObjectId("Ring of True Sight"); } }

        private MoreRingsApi moreRings;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.Display.MenuChanged += this.onMenuChanged;
            helper.Events.GameLoop.UpdateTicked += this.onUpdateTicked;
            helper.Events.Display.RenderedWorld += TrueSight.onDrawWorld;

            SpaceEvents.OnItemEaten += this.onItemEaten;

            HarmonyPatcher.Apply(
                this,
                new AxePatcher(),
                new CropPatcher(),
                new Game1Patcher(),
                new HoePatcher(),
                new PickaxePatcher(),
                new WateringCanPatcher()
            );
        }

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = this.Helper.ModRegistry.GetApi<JsonAssetsAPI>("spacechase0.JsonAssets");
            if (api == null)
            {
                Log.error("No Json Assets API???");
                return;
            }
            this.ja = api;

            api.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets"));

            this.moreRings = this.Helper.ModRegistry.GetApi<MoreRingsApi>("bcmpinc.WearMoreRings");
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is BobberBar bobber && this.hasRingEquipped(this.Ring_Fishing_LargeBar) > 0)
            {
                var field = this.Helper.Reflection.GetField<int>(bobber, "bobberBarHeight");
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

            if (this.hasRingEquipped(this.Ring_Combat_Regen) > 0 && this.regenCounter++ >= 4 / this.hasRingEquipped(this.Ring_Combat_Regen))
            {
                this.regenCounter = 0;
                Game1.player.health = Math.Min(Game1.player.health + 1, Game1.player.maxHealth);
            }

            if (this.hasRingEquipped(this.Ring_Refresh) > 0 && this.refreshCounter++ >= 4 / this.hasRingEquipped(this.Ring_Refresh))
            {
                this.refreshCounter = 0;
                Game1.player.Stamina = Math.Min(Game1.player.Stamina + 1, Game1.player.MaxStamina);
            }
        }

        private void onItemEaten(object sender, EventArgs args)
        {
            if (this.hasRingEquipped(this.Ring_DiamondBooze) > 0)
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
                        var attrs = this.Helper.Reflection.GetField<int[]>(Game1.buffsDisplay.drink, "buffAttributes").GetValue();
                        if (attrs[Buff.speed] == -1)
                        {
                            Game1.buffsDisplay.drink.removeBuff();
                            Game1.buffsDisplay.drink = null;
                        }
                        else if (attrs[Buff.speed] < 0)
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

        public int hasRingEquipped(int id)
        {
            if (this.moreRings != null)
                return this.moreRings.CountEquippedRings(Game1.player, id);

            int num = 0;
            if (Game1.player.leftRing.Value != null && Game1.player.leftRing.Value.ParentSheetIndex == id)
                ++num;
            if (Game1.player.leftRing.Value is CombinedRing lcring)
            {
                foreach (var ring in lcring.combinedRings)
                {
                    if (ring.ParentSheetIndex == id)
                        ++num;
                }
            }
            if (Game1.player.rightRing.Value != null && Game1.player.rightRing.Value.ParentSheetIndex == id)
                ++num;
            if (Game1.player.rightRing.Value is CombinedRing rcring)
            {
                foreach (var ring in rcring.combinedRings)
                {
                    if (ring.ParentSheetIndex == id)
                        ++num;
                }
            }
            return num;
        }
    }
}
