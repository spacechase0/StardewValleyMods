using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace ThrowableAxe
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        private ThrownAxe Thrown;
        private bool Clicking;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.Input.ButtonPressed += this.OnButtonPress;
            helper.Events.Input.ButtonReleased += this.OnButtonRelease;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            helper.Events.Player.Warped += this.OnWarped;
        }

        private void OnButtonPress(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (e.Button == SButton.MouseRight && Game1.player.CurrentTool is Axe axe && this.Thrown == null)
            {
                int dmg = new[] { 8, 15, 30, 45, 60, 80 }[axe.UpgradeLevel]; // 6 for support for prismatic tools
                float speed = new float[] { 10, 12, 14, 16, 18, 20 }[axe.UpgradeLevel]; // 6 for support for prismatic tools

                this.Thrown = new ThrownAxe(Game1.player, axe.UpgradeLevel, dmg, e.Cursor.AbsolutePixels, speed);
                Game1.currentLocation.projectiles.Add(this.Thrown);

                Log.Trace("Throwing axe");
                this.Clicking = true;
            }
        }

        private void OnButtonRelease(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.MouseRight)
            {
                this.Clicking = false;
            }
        }

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (this.Thrown != null)
            {
                /*
                if(clicking)
                {
                    thrown.target.Value = Helper.Input.GetCursorPosition().AbsolutePixels;
                }
                */
                if (!this.Clicking || (this.Thrown.GetPosition() - this.Thrown.Target).Length() < 1)
                {
                    var playerPos = Game1.player.getStandingPosition();
                    playerPos.X -= 16;
                    playerPos.Y -= 64;
                    this.Thrown.Target.Value = playerPos;
                    if ((this.Thrown.GetPosition() - playerPos).Length() < 16)
                    {
                        this.Thrown.Dead = true;
                    }
                }

                if (this.Thrown.Dead)
                {
                    Log.Trace("Axe destroyed");
                    this.Thrown = null;
                }
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (this.Thrown != null)
            {
                this.Thrown.Dead = true;
                this.Thrown = null;
            }
        }
    }

}
