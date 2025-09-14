using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace JumpOver.Framework
{
    internal class Jump
    {
        private readonly Farmer Player;
        private readonly IModEvents Events;
        private float PrevJumpVel;
        private Vector2 origPos;
        private Vector2 targetPos;

        private int ox, oy;
        private bool wasGoingOver = false;

        public Jump(Farmer thePlayer, IModEvents events)
        {
            this.Player = thePlayer;
            this.Events = events;
            this.PrevJumpVel = this.Player.yJumpVelocity;

            origPos = Player.Position;

            switch (this.Player.facingDirection.Value)
            {
                case Game1.up: oy = -1; break;
                case Game1.down: oy = 1; break;
                case Game1.left: ox = -1; break;
                case Game1.right: ox = 1; break;
            }

            targetPos = origPos + new Vector2(ox, oy) * Game1.tileSize * 2;

            var bb = this.Player.GetBoundingBox();
            var bb1 = this.Player.GetBoundingBox();
            bb1.X += ox * Game1.tileSize;
            bb1.Y += oy * Game1.tileSize;
            var bb2 = this.Player.GetBoundingBox();
            bb2.X += ox * Game1.tileSize * 2;
            bb2.Y += oy * Game1.tileSize * 2;

            var n0 = isCollidingPosition(bb);
            var n1 = isCollidingPosition(bb1);
            var n2 = isCollidingPosition(bb2);

            //SpaceShared.Log.Trace($"{n0} {n1} {n2}");
            if (n0 != Spot.Empty || (n0 == Spot.Empty && n1 == Spot.Solid && n2 == Spot.Empty) /*|| wasGoingOver*/ )
            {
                wasGoingOver = true;
                Game1.player.canMove = false;
                this.Player.synchronizedJump(8);
            }
            else if (Mod.Config.RockCrabCrushing && n1 == Spot.Crab && n2 != Spot.Solid)
            {
                wasGoingOver = true;
                Game1.player.canMove = false;
                this.Player.synchronizedJump(5);
            }
            else
            {
                this.Player.synchronizedJump(8);
            }


            events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        enum Spot
        {
            Empty,
            Solid,
            Crab,
        }

        private Spot isCollidingPosition(Rectangle bb)
        {
            foreach (var character in this.Player.currentLocation.characters)
            {
                if (character is RockCrab rc && rc.Health > 0 && rc.GetBoundingBox().Intersects(bb))
                {
                    return Spot.Crab;
                }
            }

            if (this.Player.currentLocation.isCollidingPosition(bb, Game1.viewport, true, 0, false, this.Player))
            {
                return Spot.Solid;
            }


            return Spot.Empty;
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
                if (wasGoingOver)
                {
                    this.Player.position.X += ox * 5;
                    this.Player.position.Y += oy * 5;
                }

                if (Mod.Config.RockCrabCrushing)
                {
                    if (this.Player.yJumpVelocity < -3)
                    {
                        foreach (var character in this.Player.currentLocation.characters)
                        {
                            if (character is RockCrab rc && rc.Health > 0 && rc.GetBoundingBox().Intersects(this.Player.GetBoundingBox()))
                            {
                                if (Mod.Instance.Helper.Reflection.GetField<NetBool>(rc, "shellGone").GetValue().Value)
                                {
                                    rc.takeDamage(100, 0, 0, false, 0, this.Player);
                                }
                                else
                                {
                                    Mod.Instance.Helper.Reflection.GetField<NetInt>(rc, "shellHealth").GetValue().Value = 1;
                                    rc.hitWithTool(new Pickaxe() { lastUser = this.Player });
                                }

                                new Jump(this.Player, this.Events);
                                this.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;

                                break;
                            }
                        }
                    }
                }
            }

            this.PrevJumpVel = this.Player.yJumpVelocity;
        }
    }
}
