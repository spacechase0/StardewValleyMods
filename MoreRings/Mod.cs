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

        /// <summary>The remainder to carry over to the next health regen.</summary>
        private float HealthRegenRemainder;

        /// <summary>The remainder to carry over to the next stamina regen.</summary>
        private float StaminaRegenRemainder;


        /*********
        ** Accessors
        *********/
        /// <summary>The mod instance.</summary>
        public static Mod Instance { get; private set; }

        /// <summary>The mod configuration.</summary>
        public ModConfig Config { get; private set; }

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
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            this.Config = helper.ReadConfig<ModConfig>();

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
            // get Wear More Rings API if present
            this.WearMoreRings = this.Helper.ModRegistry.GetApi<IMoreRingsApi>("bcmpinc.WearMoreRings");

            // register rings with Json Assets
            this.JsonAssets = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (this.JsonAssets != null)
                this.JsonAssets.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets", "json-assets"), this.Helper.Translation);
            else
                Log.Error("Couldn't get the Json Assets API, so the new rings won't be available.");

            // register with Generic Mod Config Menu
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => this.Config = new ModConfig(),
                    save: () => this.Helper.WriteConfig(this.Config)
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_QualityRingChance_Name,
                    tooltip: I18n.Config_QualityRingChance_Description,
                    getValue: () => this.Config.QualityRing_ChancePerRing,
                    setValue: value => this.Config.QualityRing_ChancePerRing = value,
                    min: 0.05f,
                    max: 1,
                    interval: 0.05f
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_RingOfWideNetsMultiplier_Name,
                    tooltip: I18n.Config_RingOfWideNetsMultiplier_Description,
                    getValue: () => this.Config.RingOfWideNets_BarSizeMultiplier,
                    setValue: value => this.Config.RingOfWideNets_BarSizeMultiplier = value,
                    min: 1,
                    max: 3,
                    interval: 0.05f
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_RingOfRegenerationRate_Name,
                    tooltip: I18n.Config_RingOfRegenerationRate_Description,
                    getValue: () => this.Config.RingOfRegeneration_RegenPerSecond,
                    setValue: value => this.Config.RingOfRegeneration_RegenPerSecond = value,
                    min: 0.05f,
                    max: 200,
                    interval: 0.05f
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_RingOfRegenerationRate_Name,
                    tooltip: I18n.Config_RingOfRegenerationRate_Description,
                    getValue: () => this.Config.RefreshingRing_RegenPerSecond,
                    setValue: value => this.Config.RefreshingRing_RegenPerSecond = value,
                    min: 0.05f,
                    max: 200,
                    interval: 0.05f
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_RingOfFarReachingDistance_Name,
                    tooltip: I18n.Config_RingOfFarReachingDistance_Description,
                    getValue: () => this.Config.RingOfFarReaching_TileDistance,
                    setValue: value => this.Config.RingOfFarReaching_TileDistance = value,
                    min: 1,
                    max: 200
                );
            }
        }

        /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is BobberBar bobber && this.HasRingEquipped(this.RingFishingLargeBar))
            {
                var field = this.Helper.Reflection.GetField<int>(bobber, "bobberBarHeight");
                field.SetValue((int)(field.GetValue() * this.Config.RingOfWideNets_BarSizeMultiplier));
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

            if (hasHealthRings > 0)
            {
                this.HealthRegenRemainder += this.Config.RingOfRegeneration_RegenPerSecond * hasHealthRings;
                if (this.HealthRegenRemainder > 0)
                {
                    Game1.player.health = Math.Min(Game1.player.health + (int)this.HealthRegenRemainder, Game1.player.maxHealth);
                    this.HealthRegenRemainder %= 1;
                }
            }

            if (hasStaminaRings > 0)
            {
                this.StaminaRegenRemainder += this.Config.RefreshingRing_RegenPerSecond * hasStaminaRings;
                if (this.StaminaRegenRemainder > 0)
                {
                    Game1.player.Stamina = Math.Min(Game1.player.Stamina + (int)this.StaminaRegenRemainder, Game1.player.MaxStamina);
                    this.StaminaRegenRemainder %= 1;
                }
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
                (Game1.player.leftRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0)
                + (Game1.player.rightRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0);

            if (this.WearMoreRings != null)
                count = Math.Max(count, this.WearMoreRings.CountEquippedRings(Game1.player, id));

            return count;
        }
    }
}
