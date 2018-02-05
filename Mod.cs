using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using Microsoft.Xna.Framework;

namespace JumpOver
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            ControlEvents.KeyPressed += keyPressed;
        }

        private void keyPressed(object sender, EventArgsKeyPressed args)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.activeClickableMenu != null)
                return;

            if ( args.KeyPressed == Keys.Space && Game1.player.yJumpVelocity == 0 )
            {
                new Jump(Game1.player);
            }
        }

        internal class Jump
        {
            private StardewValley.Farmer player;
            private float prevJumpVel = 0;

            public Jump(StardewValley.Farmer thePlayer)
            {
                player = thePlayer;
                prevJumpVel = player.yJumpVelocity;

                player.jump(8);

                GameEvents.UpdateTick += update;
            }

            private void update(object sender, EventArgs args)
            {
                if (player.yJumpVelocity == 0 && prevJumpVel < 0)
                {
                    player.canMove = true;

                    GameEvents.UpdateTick -= update;
                }
                else
                {
                    int tx = (int)player.position.X / Game1.tileSize;
                    int ty = (int)player.position.Y / Game1.tileSize;
                    int ox = 0, oy = 0; // Offset x, y
                    switch (player.facingDirection)
                    {
                        case Game1.up:    oy = -1; break;
                        case Game1.down:  oy =  1; break;
                        case Game1.left:  ox = -1; break;
                        case Game1.right: ox =  1; break;
                    }

                    var bb = player.GetBoundingBox();
                    var bb1 = player.GetBoundingBox();
                    bb1.X += ox * Game1.tileSize;
                    bb1.Y += oy * Game1.tileSize;
                    var bb2 = player.GetBoundingBox();
                    bb2.X += ox * Game1.tileSize * 2;
                    bb2.Y += oy * Game1.tileSize * 2;

                    bool n0 = player.currentLocation.isCollidingPosition(bb, Game1.viewport, true, 0, false, player);
                    bool n1 = player.currentLocation.isCollidingPosition(bb1, Game1.viewport, true, 0, false, player);
                    bool n2 = player.currentLocation.isCollidingPosition(bb2, Game1.viewport, true, 0, false, player);
                    
                    if ( n0 || ( !n0 && n1 && !n2 ) )
                    {
                        Game1.player.canMove = false;
                        player.position.X += ox * 5;
                        player.position.Y += oy * 5;
                    }
                }

                prevJumpVel = player.yJumpVelocity;
            }
        }
    }
}
