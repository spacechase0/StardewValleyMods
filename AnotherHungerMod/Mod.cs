using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace AnotherHungerMod
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Configuration Config;
        internal static SaveData Data;

        public const string MSG_HUNGERDATA = "HungerData";

        private Texture2D hungerBar;

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            Mod.Config = helper.ReadConfig<Configuration>();
            this.hungerBar = helper.Content.Load<Texture2D>("assets/hungerbar.png");

            helper.ConsoleCommands.Add("player_addfullness", "Add to your fullness", this.commands);

            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.Display.RenderedHud += this.renderHungerBar;
            SpaceEvents.AfterGiftGiven += this.onGiftGiven;
            SpaceEvents.OnItemEaten += this.onItemEaten;
            helper.Events.GameLoop.DayEnding += this.checkFedSpouse;
            helper.Events.GameLoop.UpdateTicked += this.afterTick;
            helper.Events.GameLoop.TimeChanged += this.timeChanged;
            helper.Events.GameLoop.SaveLoaded += this.onSaveLoaded;
            helper.Events.Multiplayer.PeerContextReceived += this.onPeerContextReceived;
            helper.Events.Multiplayer.ModMessageReceived += this.onModMessageReceived;
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => Mod.Config = new Configuration(), () => this.Helper.WriteConfig(Mod.Config));
                capi.RegisterSimpleOption(this.ModManifest, "Fullness UI (X)", "The X position of the fullness UI.", () => Mod.Config.FullnessUiX, (int val) => Mod.Config.FullnessUiX = val);
                capi.RegisterSimpleOption(this.ModManifest, "Fullness UI (Y)", "The Y position of the fullness UI.", () => Mod.Config.FullnessUiY, (int val) => Mod.Config.FullnessUiY = val);
                capi.RegisterSimpleOption(this.ModManifest, "Max Fullness", "Maximum amount of fullness you can have.", () => Mod.Config.MaxFullness, (int val) => Mod.Config.MaxFullness = val);
                capi.RegisterSimpleOption(this.ModManifest, "Edibility Multiplier", "A multiplier for the amount of fullness you get, based on the food's edibility.", () => (float)Mod.Config.EdibilityMultiplier, (float val) => Mod.Config.EdibilityMultiplier = val);
                capi.RegisterSimpleOption(this.ModManifest, "Fullness Drain", "The amount of fullness to drain every 10 minutes in-game.", () => (float)Mod.Config.DrainPer10Min, (float val) => Mod.Config.DrainPer10Min = val);
                capi.RegisterSimpleOption(this.ModManifest, "Positive Buff Threshold", "The amount of fullness you need for positive buffs to apply.", () => Mod.Config.PositiveBuffThreshold, (int val) => Mod.Config.PositiveBuffThreshold = val);
                capi.RegisterSimpleOption(this.ModManifest, "Negative Buff Threshold", "The amount of fullness you need before negative buffs apply.", () => Mod.Config.NegativeBuffThreshold, (int val) => Mod.Config.NegativeBuffThreshold = val);
                capi.RegisterSimpleOption(this.ModManifest, "Starvation Damage", "The amount of starvation damage taken every 10 minutes when you have no fullness.", () => Mod.Config.StarvationDamagePer10Min, (int val) => Mod.Config.StarvationDamagePer10Min = val);
                capi.RegisterSimpleOption(this.ModManifest, "Unfed Spouse Penalty", "The relationship points penalty for not feeding your spouse.", () => Mod.Config.RelationshipHitForNotFeedingSpouse, (int val) => Mod.Config.RelationshipHitForNotFeedingSpouse = val);
            }
        }

        private void commands(string cmd, string[] args)
        {
            if (cmd == "player_addfullness")
            {
                if (args.Length != 1)
                    Log.info("Usage: player_addfullness <amt>");
                else
                    Game1.player.UseFullness(-double.Parse(args[0]));
            }
        }

        private void renderHungerBar(object sender, RenderedHudEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null || Game1.eventUp)
                return;

            SpriteBatch b = e.SpriteBatch;

            Vector2 pos = new Vector2(Mod.Config.FullnessUiX, Mod.Config.FullnessUiY);
            b.Draw(this.hungerBar, pos, new Rectangle(0, 0, this.hungerBar.Width, this.hungerBar.Height), Color.White, 0, new Vector2(), 4, SpriteEffects.None, 1);
            if (Game1.player.GetFullness() > 0)
            {
                Rectangle targetArea = new Rectangle(3, 13, 6, 41);
                float perc = (float)(Game1.player.GetFullness() / Game1.player.GetMaxFullness());
                int h = (int)(targetArea.Height * perc);
                targetArea.Y += targetArea.Height - h;
                targetArea.Height = h;

                targetArea.X *= 4;
                targetArea.Y *= 4;
                targetArea.Width *= 4;
                targetArea.Height *= 4;
                targetArea.X += (int)pos.X;
                targetArea.Y += (int)pos.Y;
                b.Draw(Game1.staminaRect, targetArea, new Rectangle(0, 0, 1, 1), Color.Orange);

                if (Game1.getOldMouseX() >= (double)targetArea.X && Game1.getOldMouseY() >= (double)targetArea.Y && Game1.getOldMouseX() < (double)targetArea.X + targetArea.Width && Game1.getOldMouseY() < targetArea.Y + targetArea.Height)
                    Game1.drawWithBorder(Math.Max(0, (int)Game1.player.GetFullness()).ToString() + "/" + Game1.player.GetMaxFullness(), Color.Black * 0.0f, Color.White, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY() - 32));
            }


        }

        private void onItemEaten(object sender, EventArgs e)
        {
            if (sender != Game1.player)
                return;

            int foodVal = (int)((Game1.player.itemToEat as StardewValley.Object).Edibility * Mod.Config.EdibilityMultiplier);
            Log.trace("Player ate food for " + foodVal + " fullness");
            Game1.player.UseFullness(-foodVal);
        }

        private void onGiftGiven(object sender, EventArgsGiftGiven e)
        {
            if (sender != Game1.player)
                return;

            if (e.Npc == Game1.player.getSpouse())
            {
                if (e.Gift.Category == StardewValley.Object.CookingCategory)
                {
                    Log.trace("Player gave spouse a meal");
                    Game1.player.SetFedSpouse(true);
                }
            }
        }

        private void checkFedSpouse(object sender, DayEndingEventArgs e)
        {
            if (Game1.player.HasFedSpouse() && Game1.player.getSpouse() != null)
            {
                Log.trace("Player didn't feed spouse");
                Game1.player.changeFriendship(-Mod.Config.RelationshipHitForNotFeedingSpouse, Game1.player.getSpouse());
                Game1.player.SetFedSpouse(false);
            }
            else
            {
                Log.trace("Player fed spouse");
            }
        }

        private void afterTick(object sender, UpdateTickedEventArgs e)
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
                fullBuff.millisecondsDuration = 7000 * (int)((fullness - Mod.Config.PositiveBuffThreshold) / Mod.Config.DrainPer10Min);
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
                hungryBuff.millisecondsDuration = 7000 * (int)(fullness / Mod.Config.DrainPer10Min);
            }
            else if (hungryBuff != null)
            {
                hungryBuff.millisecondsDuration = 0;
            }
        }

        private void timeChanged(object sender, TimeChangedEventArgs e)
        {
            int hourDiff = e.NewTime / 100 - e.NewTime / 100;
            int minDiff = e.NewTime % 100 - e.OldTime % 100;

            if (minDiff != 10 && (hourDiff != 1 && minDiff != -50))
                return;
            Game1.player.UseFullness(Mod.Config.DrainPer10Min);

            if (Game1.player.GetFullness() <= 0)
            {
                Game1.player.takeDamage(Mod.Config.StarvationDamagePer10Min, true, null);
                if (Game1.player.health <= 0)
                {
                    Log.trace("Player starved to death, resetting hunger");
                    if (Mod.Config.NegativeBuffThreshold != 0)
                        Game1.player.UseFullness(-Mod.Config.NegativeBuffThreshold);
                    else
                        Game1.player.UseFullness(-25); // Just incase they set the negative buff threshold to 0
                }
            }
        }

        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                Mod.Data = this.Helper.Data.ReadSaveData<SaveData>($"spacechase0.AnotherHungerMod.{Game1.player.UniqueMultiplayerID}") ?? new SaveData();
            }
        }

        private void onPeerContextReceived(object sender, PeerContextReceivedEventArgs e)
        {
            if (!Game1.IsServer)
                return;
            //Log.debug($"Sending hunger data to {e.Peer.PlayerID}");
            var data = this.Helper.Data.ReadSaveData<SaveData>($"spacechase0.AnotherHungerMod.{e.Peer.PlayerID}") ?? new SaveData();
            this.Helper.Multiplayer.SendMessage(data, Mod.MSG_HUNGERDATA, null, new[] { e.Peer.PlayerID });
        }

        private void onModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == this.ModManifest.UniqueID && e.Type == Mod.MSG_HUNGERDATA)
            {
                //Log.debug($"Got hunger data from {e.FromPlayerID}");
                var data = e.ReadAs<SaveData>();
                if (Context.IsMainPlayer)
                {
                    this.Helper.Data.WriteSaveData<SaveData>($"spacechase0.AnotherHungerMod.{e.FromPlayerID}", data);
                }
                else
                    Mod.Data = data;
            }
        }
    }
}
