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
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Json Assets mod API.</summary>
        private IJsonAssetsApi JsonAssets;

        /// <summary>The Wear More Rings mod API.</summary>
        private IMoreRingsApi WearMoreRings;

        /// <summary>The number of ticks until the next health regen point.</summary>
        private int HealthRegenCounter;

        /// <summary>The number of ticks until the next stamina regen point.</summary>
        private int StaminaRegenCounter;


        /*********
        ** Accessors
        *********/
        /// <summary>The mod instance.</summary>
        public static Mod Instance;

        /// <summary>The item ID for the Ring of Wide Nets.</summary>
        public int RingFishingLargeBar => this.JsonAssets.GetObjectId("Ring of Wide Nets");

        /// <summary>The item ID for the Ring of Regeneration.</summary>
        public int RingCombatRegen => this.JsonAssets.GetObjectId("Ring of Regeneration");

        /// <summary>The item ID for the Ring of Diamond Booze.</summary>
        public int RingDiamondBooze => this.JsonAssets.GetObjectId("Ring of Diamond Booze");

        /// <summary>The item ID for the Refreshing Ring.</summary>
        public int RingRefresh => this.JsonAssets.GetObjectId("Refreshing Ring");

        /// <summary>The item ID for the Quality+ Ring.</summary>
        public int RingQuality => this.JsonAssets.GetObjectId("Quality+ Ring");

        /// <summary>The item ID for the Ring of Far Reaching.</summary>
        public int RingMageHand => this.JsonAssets.GetObjectId("Ring of Far Reaching");

        /// <summary>The item ID for the Ring of True Sight.</summary>
        public int RingTrueSight => this.JsonAssets.GetObjectId("Ring of True Sight");


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Display.RenderedWorld += this.OnRenderedWorld;

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


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
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
            this.JsonAssets = api;

            api.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets", "json-assets"), this.Helper.Translation);

            this.WearMoreRings = this.Helper.ModRegistry.GetApi<IMoreRingsApi>("bcmpinc.WearMoreRings");
        }

        /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
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

        /// <inheritdoc cref="IDisplayEvents.RenderedWorld"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            TrueSight.DrawOverWorld(e.SpriteBatch);
        }

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.IsOneSecond)
                return;

            int hasHealthRings = this.CountRingsEquipped(this.RingCombatRegen);
            int hasStaminaRings = this.CountRingsEquipped(this.RingRefresh);

            if (hasHealthRings > 0 && this.HealthRegenCounter++ >= 4 / hasHealthRings)
            {
                this.HealthRegenCounter = 0;
                Game1.player.health = Math.Min(Game1.player.health + 1, Game1.player.maxHealth);
            }

            if (hasStaminaRings > 0 && this.StaminaRegenCounter++ >= 4 / hasStaminaRings)
            {
                this.StaminaRegenCounter = 0;
                Game1.player.Stamina = Math.Min(Game1.player.Stamina + 1, Game1.player.MaxStamina);
            }
        }

        /// <inheritdoc cref="SpaceEvents.OnItemEaten"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnItemEaten(object sender, EventArgs e)
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
            int count =
                Game1.player.leftRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0
                + Game1.player.rightRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0;

            if (this.WearMoreRings != null)
                count = Math.Max(count, this.WearMoreRings.CountEquippedRings(Game1.player, id));

            return count;
        }
    }
}
