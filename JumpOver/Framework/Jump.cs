using StardewModdingAPI.Events;
using StardewValley;

namespace JumpOver.Framework
{
    internal class Jump
    {
        private readonly Farmer Player;
        private readonly IModEvents Events;
        private float PrevJumpVel;

        //private bool wasGoingOver = false;

        public Jump(Farmer thePlayer, IModEvents events)
        {
            this.Player = thePlayer;
            this.Events = events;
            this.PrevJumpVel = this.Player.yJumpVelocity;

            this.Player.synchronizedJump(8);

            events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (this.Player.yJumpVelocity == 0 && this.PrevJumpVel < 0)
            {
                this.Player.canMove = true;

                this.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            }
            else
            {
                int ox = 0, oy = 0; // Offset x, y
                switch (this.Player.facingDirection.Value)
                {
                    case Game1.up: oy = -1; break;
                    case Game1.down: oy = 1; break;
                    case Game1.left: ox = -1; break;
                    case Game1.right: ox = 1; break;
                }

                var bb = this.Player.GetBoundingBox();
                var bb1 = this.Player.GetBoundingBox();
                bb1.X += ox * Game1.tileSize;
                bb1.Y += oy * Game1.tileSize;
                var bb2 = this.Player.GetBoundingBox();
                bb2.X += ox * Game1.tileSize * 2;
                bb2.Y += oy * Game1.tileSize * 2;

                bool n0 = this.Player.currentLocation.isCollidingPosition(bb, Game1.viewport, true, 0, false, this.Player);
                bool n1 = this.Player.currentLocation.isCollidingPosition(bb1, Game1.viewport, true, 0, false, this.Player);
                bool n2 = this.Player.currentLocation.isCollidingPosition(bb2, Game1.viewport, true, 0, false, this.Player);

                //Log.trace($"{n0} {n1} {n2}");
                if (n0 || (!n0 && n1 && !n2) /*|| wasGoingOver*/ )
                {
                    //wasGoingOver = true;
                    Game1.player.canMove = false;
                    this.Player.position.X += ox * 5;
                    this.Player.position.Y += oy * 5;
                }
            }

            this.PrevJumpVel = this.Player.yJumpVelocity;
        }
    }
}
