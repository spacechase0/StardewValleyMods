using System;
using System.IO;
using MoreRings.Framework;
using MoreRings.Patches;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace MoreRings
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        private IJsonAssetsApi Ja;
        public int RingFishingLargeBar => this.Ja.GetObjectId("Ring of Wide Nets");
        public int RingCombatRegen => this.Ja.GetObjectId("Ring of Regeneration");
        public int RingDiamondBooze => this.Ja.GetObjectId("Ring of Diamond Booze");
        public int RingRefresh => this.Ja.GetObjectId("Refreshing Ring");
        public int RingQuality => this.Ja.GetObjectId("Quality+ Ring");
        public int RingMageHand => this.Ja.GetObjectId("Ring of Far Reaching");
        public int RingTrueSight => this.Ja.GetObjectId("Ring of True Sight");

        private IMoreRingsApi WearMoreRings;

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

            api.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets", "json-assets"), this.Helper.Translation);

            this.WearMoreRings = this.Helper.ModRegistry.GetApi<IMoreRingsApi>("bcmpinc.WearMoreRings");
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is BobberBar bobber && this.HasRingEquipped(this.RingFishingLargeBar))
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

            int hasHealthRings = this.CountRingsEquipped(this.RingCombatRegen);
            int hasStaminaRings = this.CountRingsEquipped(this.RingRefresh);

            if (hasHealthRings > 0 && this.RegenCounter++ >= 4 / hasHealthRings)
            {
                this.RegenCounter = 0;
                Game1.player.health = Math.Min(Game1.player.health + 1, Game1.player.maxHealth);
            }

            if (hasStaminaRings > 0 && this.RefreshCounter++ >= 4 / hasStaminaRings)
            {
                this.RefreshCounter = 0;
                Game1.player.Stamina = Math.Min(Game1.player.Stamina + 1, Game1.player.MaxStamina);
            }
        }

        private void OnItemEaten(object sender, EventArgs args)
        {
            if (this.HasRingEquipped(this.RingDiamondBooze))
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

        /// <summary>Get whether the player has any ring with the given ID equipped.</summary>
        /// <param name="id">The ring ID to match.</param>
        public bool HasRingEquipped(int id)
        {
            return this.CountRingsEquipped(id) > 0;
        }

        /// <summary>Count the number of rings with the given ID equipped by the player.</summary>
        /// <param name="id">The ring ID to match.</param>
        public int CountRingsEquipped(int id)
        {
            if (this.WearMoreRings != null)
                return this.WearMoreRings.CountEquippedRings(Game1.player, id);

            return
                Game1.player.leftRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0
                + Game1.player.rightRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0;

        }
    }
}
