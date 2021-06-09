using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace ThrowableAxe
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        private ThrownAxe thrown;
        private bool clicking = false;

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.Input.ButtonPressed += this.onButtonPress;
            helper.Events.Input.ButtonReleased += this.onButtonRelease;
            helper.Events.GameLoop.UpdateTicking += this.onUpdateTicking;
            helper.Events.Player.Warped += this.onWarped;
        }

        private void onButtonPress(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (e.Button == SButton.MouseRight && Game1.player.CurrentTool is Axe axe && this.thrown == null)
            {
                int[] dmg_ = new int[] { 8, 15, 30, 45, 60, 80 }; // 6 for support for prismatic tools
                float[] speed_ = new float[] { 10, 12, 14, 16, 18, 20 }; // 6 for support for prismatic tools
                int dmg = dmg_[axe.UpgradeLevel];
                float speed = speed_[axe.UpgradeLevel];

                this.thrown = new ThrownAxe(Game1.player, axe.UpgradeLevel, dmg, e.Cursor.AbsolutePixels, speed);
                Game1.currentLocation.projectiles.Add(this.thrown);

                Log.trace("Throwing axe");
                this.clicking = true;
            }
        }

        private void onButtonRelease(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.MouseRight)
            {
                this.clicking = false;
            }
        }

        private void onUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (this.thrown != null)
            {
                /*
                if(clicking)
                {
                    thrown.target.Value = Helper.Input.GetCursorPosition().AbsolutePixels;
                }
                */
                if (!this.clicking || (this.thrown.GetPosition() - this.thrown.target).Length() < 1)
                {
                    var playerPos = Game1.player.getStandingPosition();
                    playerPos.X -= 16;
                    playerPos.Y -= 64;
                    this.thrown.target.Value = playerPos;
                    if ((this.thrown.GetPosition() - playerPos).Length() < 16)
                    {
                        this.thrown.dead = true;
                    }
                }

                if (this.thrown.dead)
                {
                    Log.trace("Axe destroyed");
                    this.thrown = null;
                }
            }
        }

        private void onWarped(object sender, WarpedEventArgs e)
        {
            if (this.thrown != null)
            {
                this.thrown.dead = true;
                this.thrown = null;
            }
        }
    }

}
