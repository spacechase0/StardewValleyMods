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
        public static Mod Instance;

        private IJsonAssetsApi Ja;
        public int RingFishingLargeBar { get { return this.Ja.GetObjectId("Ring of Wide Nets"); } }
        public int RingCombatRegen { get { return this.Ja.GetObjectId("Ring of Regeneration"); } }
        public int RingDiamondBooze { get { return this.Ja.GetObjectId("Ring of Diamond Booze"); } }
        public int RingRefresh { get { return this.Ja.GetObjectId("Refreshing Ring"); } }
        public int RingQuality { get { return this.Ja.GetObjectId("Quality+ Ring"); } }
        public int RingMageHand { get { return this.Ja.GetObjectId("Ring of Far Reaching"); } }
        public int RingTrueSight { get { return this.Ja.GetObjectId("Ring of True Sight"); } }

        private IMoreRingsApi MoreRings;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Display.RenderedWorld += TrueSight.OnDrawWorld;

            SpaceEvents.OnItemEaten += this.OnItemEaten;

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
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (api == null)
            {
                Log.Error("No Json Assets API???");
                return;
            }
            this.Ja = api;

            api.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets"));

            this.MoreRings = this.Helper.ModRegistry.GetApi<IMoreRingsApi>("bcmpinc.WearMoreRings");
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is BobberBar bobber && this.HasRingEquipped(this.RingFishingLargeBar) > 0)
            {
                var field = this.Helper.Reflection.GetField<int>(bobber, "bobberBarHeight");
                field.SetValue((int)(field.GetValue() * 1.50));
            }
        }

        private int RegenCounter;
        private int RefreshCounter;

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.IsOneSecond)
                return;

            if (this.HasRingEquipped(this.RingCombatRegen) > 0 && this.RegenCounter++ >= 4 / this.HasRingEquipped(this.RingCombatRegen))
            {
                this.RegenCounter = 0;
                Game1.player.health = Math.Min(Game1.player.health + 1, Game1.player.maxHealth);
            }

            if (this.HasRingEquipped(this.RingRefresh) > 0 && this.RefreshCounter++ >= 4 / this.HasRingEquipped(this.RingRefresh))
            {
                this.RefreshCounter = 0;
                Game1.player.Stamina = Math.Min(Game1.player.Stamina + 1, Game1.player.MaxStamina);
            }
        }

        private void OnItemEaten(object sender, EventArgs args)
        {
            if (this.HasRingEquipped(this.RingDiamondBooze) > 0)
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
                        int[] attrs = Game1.buffsDisplay.drink.buffAttributes;
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

        public int HasRingEquipped(int id)
        {
            if (this.MoreRings != null)
                return this.MoreRings.CountEquippedRings(Game1.player, id);

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
