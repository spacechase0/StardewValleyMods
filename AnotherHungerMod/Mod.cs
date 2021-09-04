using System;
using AnotherHungerMod.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;

namespace AnotherHungerMod
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Configuration Config;

        private Texture2D HungerBar;

        /// <summary>Handles migrating legacy data for a save file.</summary>
        private LegacyDataMigrator LegacyDataMigrator;

        /// <summary>The in-game time when the fullness meter last decreased.</summary>
        private int LastDrainTime;

        /// <summary>The decimal remainder for the last starvation damage.</summary>
        private float StarvationDamageRemainder;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            Mod.Config = helper.ReadConfig<Configuration>();
            this.HungerBar = helper.Content.Load<Texture2D>("assets/hungerbar.png");
            this.LegacyDataMigrator = new LegacyDataMigrator(helper.Data, this.Monitor);

            helper.ConsoleCommands.Add("player_addfullness", "Add to your fullness", this.Commands);

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.RenderedHud += this.RenderHungerBar;
            SpaceEvents.AfterGiftGiven += this.OnGiftGiven;
            SpaceEvents.OnItemEaten += this.OnItemEaten;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.DayEnding += this.CheckFedSpouse;
            helper.Events.GameLoop.UpdateTicked += this.AfterTick;
            helper.Events.GameLoop.TimeChanged += this.TimeChanged;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => Mod.Config = new Configuration(), () => this.Helper.WriteConfig(Mod.Config));
                capi.RegisterSimpleOption(this.ModManifest, "Fullness UI (X)", "The X position of the fullness UI.", () => Mod.Config.FullnessUiX, val => Mod.Config.FullnessUiX = val);
                capi.RegisterSimpleOption(this.ModManifest, "Fullness UI (Y)", "The Y position of the fullness UI.", () => Mod.Config.FullnessUiY, val => Mod.Config.FullnessUiY = val);
                capi.RegisterSimpleOption(this.ModManifest, "Max Fullness", "Maximum amount of fullness you can have.", () => Mod.Config.MaxFullness, val => Mod.Config.MaxFullness = val);
                capi.RegisterSimpleOption(this.ModManifest, "Edibility Multiplier", "A multiplier for the amount of fullness you get, based on the food's edibility.", () => Mod.Config.EdibilityMultiplier, val => Mod.Config.EdibilityMultiplier = val);
                capi.RegisterSimpleOption(this.ModManifest, "Fullness Drain", "The amount of fullness to drain per in-game minute.", () => Mod.Config.DrainPerMinute, val => Mod.Config.DrainPerMinute = val);
                capi.RegisterSimpleOption(this.ModManifest, "Positive Buff Threshold", "The amount of fullness you need for positive buffs to apply.", () => Mod.Config.PositiveBuffThreshold, val => Mod.Config.PositiveBuffThreshold = val);
                capi.RegisterSimpleOption(this.ModManifest, "Negative Buff Threshold", "The amount of fullness you need before negative buffs apply.", () => Mod.Config.NegativeBuffThreshold, val => Mod.Config.NegativeBuffThreshold = val);
                capi.RegisterSimpleOption(this.ModManifest, "Starvation Damage", "The amount of starvation damage taken every in-game minute when you have no fullness.", () => Mod.Config.StarvationDamagePerMinute, val => Mod.Config.StarvationDamagePerMinute = val);
                capi.RegisterSimpleOption(this.ModManifest, "Unfed Spouse Penalty", "The relationship points penalty for not feeding your spouse.", () => Mod.Config.RelationshipHitForNotFeedingSpouse, val => Mod.Config.RelationshipHitForNotFeedingSpouse = val);
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            this.LastDrainTime = Game1.timeOfDay;
            this.StarvationDamageRemainder = 0;
        }

        private void Commands(string cmd, string[] args)
        {
            if (cmd == "player_addfullness")
            {
                if (args.Length != 1)
                    Log.Info("Usage: player_addfullness <amt>");
                else
                    Game1.player.UseFullness(-float.Parse(args[0]));
            }
        }

        private void RenderHungerBar(object sender, RenderedHudEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null || Game1.eventUp)
                return;

            SpriteBatch b = e.SpriteBatch;

            Vector2 pos = new Vector2(Mod.Config.FullnessUiX, Mod.Config.FullnessUiY);
            b.Draw(this.HungerBar, pos, new Rectangle(0, 0, this.HungerBar.Width, this.HungerBar.Height), Color.White, 0, new Vector2(), 4, SpriteEffects.None, 1);
            if (Game1.player.GetFullness() > 0)
            {
                Rectangle targetArea = new Rectangle(3, 13, 6, 41);
                float percentage = Game1.player.GetFullness() / Game1.player.GetMaxFullness();
                int height = (int)(targetArea.Height * percentage);
                targetArea.Y += targetArea.Height - height;
                targetArea.Height = height;

                targetArea.X *= 4;
                targetArea.Y *= 4;
                targetArea.Width *= 4;
                targetArea.Height *= 4;
                targetArea.X += (int)pos.X;
                targetArea.Y += (int)pos.Y;
                b.Draw(Game1.staminaRect, targetArea, new Rectangle(0, 0, 1, 1), Color.Orange);

                if (Game1.getOldMouseX() >= (double)targetArea.X && Game1.getOldMouseY() >= (double)targetArea.Y && Game1.getOldMouseX() < (double)targetArea.X + targetArea.Width && Game1.getOldMouseY() < targetArea.Y + targetArea.Height)
                    Game1.drawWithBorder(Math.Max(0, (int)Game1.player.GetFullness()) + "/" + Game1.player.GetMaxFullness(), Color.Black * 0.0f, Color.White, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY() - 32));
            }


        }

        private void OnItemEaten(object sender, EventArgs e)
        {
            if (sender != Game1.player)
                return;

            int foodVal = (int)((Game1.player.itemToEat as SObject).Edibility * Mod.Config.EdibilityMultiplier);
            Log.Trace("Player ate food for " + foodVal + " fullness");
            Game1.player.UseFullness(-foodVal);
        }

        private void OnGiftGiven(object sender, EventArgsGiftGiven e)
        {
            if (sender != Game1.player)
                return;

            if (e.Npc == Game1.player.getSpouse())
            {
                if (e.Gift.Category == SObject.CookingCategory)
                {
                    Log.Trace("Player gave spouse a meal");
                    Game1.player.SetFedSpouse(true);
                }
            }
        }

        private void CheckFedSpouse(object sender, DayEndingEventArgs e)
        {
            if (Game1.player.HasFedSpouse() && Game1.player.getSpouse() != null)
            {
                Log.Trace("Player didn't feed spouse");
                Game1.player.changeFriendship(-Mod.Config.RelationshipHitForNotFeedingSpouse, Game1.player.getSpouse());
                Game1.player.SetFedSpouse(false);
            }
            else
            {
                Log.Trace("Player fed spouse");
            }
        }

        private void AfterTick(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            double fullness = Game1.player.GetFullness();

            Buff fullBuff = Game1.buffsDisplay.otherBuffs.Find(b => b.source == "Fullness");
            if (fullness > Mod.Config.PositiveBuffThreshold)
            {
                if (fullBuff == null)
                {
                    fullBuff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 2, 10, "Fullness", "Fullness");
                    Game1.buffsDisplay.addOtherBuff(fullBuff);
                }
                fullBuff.millisecondsDuration = 7000 * (int)((fullness - Mod.Config.PositiveBuffThreshold) / (10 * Mod.Config.DrainPerMinute));
            }
            else if (fullBuff != null)
            {
                fullBuff.millisecondsDuration = 0;
            }

            Buff hungryBuff = Game1.buffsDisplay.otherBuffs.Find(b => b.source == "Hungry");
            if (fullness < Mod.Config.NegativeBuffThreshold)
            {
                if (hungryBuff == null)
                {
                    hungryBuff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, -2, 0, 0, 10, "Hungry", "Hungry");
                    Game1.buffsDisplay.addOtherBuff(hungryBuff);
                }
                hungryBuff.millisecondsDuration = 7000 * (int)(fullness / (10 * Mod.Config.DrainPerMinute));
            }
            else if (hungryBuff != null)
            {
                hungryBuff.millisecondsDuration = 0;
            }
        }

        private void TimeChanged(object sender, TimeChangedEventArgs e)
        {
            // reset time
            if (this.LastDrainTime <= 0 || e.NewTime <= this.LastDrainTime)
            {
                this.LastDrainTime = e.NewTime;
                return;
            }

            // reduce fullness
            int minutes = Math.Min(Utility.CalculateMinutesBetweenTimes(this.LastDrainTime, e.NewTime), Mod.Config.MaxTransitionMinutes);
            Game1.player.UseFullness(minutes * Mod.Config.DrainPerMinute);
            this.LastDrainTime = e.NewTime;

            // apply starvation
            if (Game1.player.GetFullness() <= 0)
            {
                float damage = (minutes * Mod.Config.StarvationDamagePerMinute) + this.StarvationDamageRemainder;
                this.StarvationDamageRemainder = damage % 1;
                Game1.player.takeDamage((int)damage, true, null);

                if (Game1.player.health <= 0)
                {
                    Log.Trace("Player starved to death, resetting hunger");
                    if (Mod.Config.NegativeBuffThreshold != 0)
                        Game1.player.UseFullness(-Mod.Config.NegativeBuffThreshold);
                    else
                        Game1.player.UseFullness(-25); // Just in case they set the negative buff threshold to 0
                }
            }
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            this.LegacyDataMigrator.OnSaveLoaded();
        }
    }
}
