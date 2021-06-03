using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            instance = this;
            Log.Monitor = Monitor;

            Config = helper.ReadConfig<Configuration>();
            hungerBar = helper.Content.Load<Texture2D>("assets/hungerbar.png");

            helper.ConsoleCommands.Add("player_addfullness", "Add to your fullness", commands);

            helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.Display.RenderedHud += renderHungerBar;
            SpaceEvents.AfterGiftGiven += onGiftGiven;
            SpaceEvents.OnItemEaten += onItemEaten;
            helper.Events.GameLoop.DayEnding += checkFedSpouse;
            helper.Events.GameLoop.UpdateTicked += afterTick;
            helper.Events.GameLoop.TimeChanged += timeChanged;
            helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            helper.Events.Multiplayer.PeerContextReceived += onPeerContextReceived;
            helper.Events.Multiplayer.ModMessageReceived += onModMessageReceived;
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(ModManifest, () => Config = new Configuration(), () => Helper.WriteConfig(Config));
                capi.RegisterSimpleOption(ModManifest, "Fullness UI (X)", "The X position of the fullness UI.", () => Config.FullnessUiX, (int val) => Config.FullnessUiX = val);
                capi.RegisterSimpleOption(ModManifest, "Fullness UI (Y)", "The Y position of the fullness UI.", () => Config.FullnessUiY, (int val) => Config.FullnessUiY = val);
                capi.RegisterSimpleOption(ModManifest, "Max Fullness", "Maximum amount of fullness you can have.", () => Config.MaxFullness, (int val) => Config.MaxFullness = val);
                capi.RegisterSimpleOption(ModManifest, "Edibility Multiplier", "A multiplier for the amount of fullness you get, based on the food's edibility.", () => (float) Config.EdibilityMultiplier, (float val) => Config.EdibilityMultiplier = val);
                capi.RegisterSimpleOption(ModManifest, "Fullness Drain", "The amount of fullness to drain every 10 minutes in-game.", () => (float) Config.DrainPer10Min, (float val) => Config.DrainPer10Min = val);
                capi.RegisterSimpleOption(ModManifest, "Positive Buff Threshold", "The amount of fullness you need for positive buffs to apply.", () => Config.PositiveBuffThreshold, (int val) => Config.PositiveBuffThreshold = val);
                capi.RegisterSimpleOption(ModManifest, "Negative Buff Threshold", "The amount of fullness you need before negative buffs apply.", () => Config.NegativeBuffThreshold, (int val) => Config.NegativeBuffThreshold = val);
                capi.RegisterSimpleOption(ModManifest, "Starvation Damage", "The amount of starvation damage taken every 10 minutes when you have no fullness.", () => Config.StarvationDamagePer10Min, (int val) => Config.StarvationDamagePer10Min = val);
                capi.RegisterSimpleOption(ModManifest, "Unfed Spouse Penalty", "The relationship points penalty for not feeding your spouse.", () => Config.RelationshipHitForNotFeedingSpouse, (int val) => Config.RelationshipHitForNotFeedingSpouse = val);
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

            Vector2 pos = new Vector2(Config.FullnessUiX, Config.FullnessUiY);
            b.Draw(hungerBar, pos, new Rectangle(0, 0, hungerBar.Width, hungerBar.Height), Color.White, 0, new Vector2(), 4, SpriteEffects.None, 1);
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

                if ((double)Game1.getOldMouseX() >= (double)targetArea.X && (double)Game1.getOldMouseY() >= (double)targetArea.Y && (double)Game1.getOldMouseX() < (double)targetArea.X + targetArea.Width && Game1.getOldMouseY() < targetArea.Y + targetArea.Height)
                    Game1.drawWithBorder(Math.Max(0, (int)Game1.player.GetFullness()).ToString() + "/" + Game1.player.GetMaxFullness(), Color.Black * 0.0f, Color.White, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY() - 32 ));
            }


        }

        private void onItemEaten(object sender, EventArgs e)
        {
            if (sender != Game1.player)
                return;

            int foodVal = (int)((Game1.player.itemToEat as StardewValley.Object).Edibility * Config.EdibilityMultiplier);
            Log.trace("Player ate food for " + foodVal + " fullness");
            Game1.player.UseFullness(-foodVal);
        }

        private void onGiftGiven(object sender, EventArgsGiftGiven e)
        {
            if (sender != Game1.player)
                return;

            if ( e.Npc == Game1.player.getSpouse() )
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
            if ( Game1.player.HasFedSpouse() && Game1.player.getSpouse() != null )
            {
                Log.trace("Player didn't feed spouse");
                Game1.player.changeFriendship(-Config.RelationshipHitForNotFeedingSpouse, Game1.player.getSpouse());
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
            if ( fullness > Config.PositiveBuffThreshold )
            {
                if (fullBuff == null)
                {
                    fullBuff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 2, 10, "Fullness", "Fullness");
                    Game1.buffsDisplay.addOtherBuff(fullBuff);
                }
                fullBuff.millisecondsDuration = 7000 * (int)((fullness - Config.PositiveBuffThreshold) / Config.DrainPer10Min);
            }
            else if ( fullBuff != null )
            {
                fullBuff.millisecondsDuration = 0;
            }

            Buff hungryBuff = Game1.buffsDisplay.otherBuffs.Find(b => b.source == "Hungry");
            if ( fullness < Config.NegativeBuffThreshold )
            {
                if (hungryBuff == null)
                {
                    hungryBuff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, -2, 0, 0, 10, "Hungry", "Hungry");
                    Game1.buffsDisplay.addOtherBuff(hungryBuff);
                }
                hungryBuff.millisecondsDuration = 7000 * (int)(fullness / Config.DrainPer10Min);
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
            Game1.player.UseFullness(Config.DrainPer10Min);

            if (Game1.player.GetFullness() <= 0)
            {
                Game1.player.takeDamage(Config.StarvationDamagePer10Min, true, null);
                if (Game1.player.health <= 0)
                {
                    Log.trace("Player starved to death, resetting hunger");
                    if (Config.NegativeBuffThreshold != 0)
                        Game1.player.UseFullness(-Config.NegativeBuffThreshold);
                    else
                        Game1.player.UseFullness(-25); // Just incase they set the negative buff threshold to 0
                }
            }
        }

        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                Data = Helper.Data.ReadSaveData<SaveData>($"spacechase0.AnotherHungerMod.{Game1.player.UniqueMultiplayerID}") ?? new SaveData();
            }
        }

        private void onPeerContextReceived(object sender, PeerContextReceivedEventArgs e)
        {
            if (!Game1.IsServer)
                return;
            //Log.debug($"Sending hunger data to {e.Peer.PlayerID}");
            var data = Helper.Data.ReadSaveData<SaveData>($"spacechase0.AnotherHungerMod.{e.Peer.PlayerID}") ?? new SaveData();
            Helper.Multiplayer.SendMessage(data, MSG_HUNGERDATA, null, new long[] { e.Peer.PlayerID });
        }

        private void onModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModManifest.UniqueID && e.Type == MSG_HUNGERDATA)
            {
                //Log.debug($"Got hunger data from {e.FromPlayerID}");
                var data = e.ReadAs<SaveData>();
                if (Context.IsMainPlayer)
                {
                    Helper.Data.WriteSaveData<SaveData>($"spacechase0.AnotherHungerMod.{e.FromPlayerID}", data);
                }
                else
                    Data = data;
            }
        }
    }
}
