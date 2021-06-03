using System;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JumpOver
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Configuration Config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.Input.ButtonPressed += onButtonPressed;
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(ModManifest, () => Config = new Configuration(), () => Helper.WriteConfig(Config));
                capi.RegisterSimpleOption(ModManifest, "Jump Key", "The key to jump", () => Config.keyJump, (SButton val) => Config.keyJump = val);
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.activeClickableMenu != null)
                return;
            
            if ( e.Button == Config.keyJump && Game1.player.yJumpVelocity == 0 )
            {
                // This is terrible for this case, redo it
                new Jump(Game1.player, Helper.Events);
            }
        }

        internal class Jump
        {
            private readonly Farmer player;
            private readonly IModEvents events;
            private float prevJumpVel = 0;

            //private bool wasGoingOver = false;

            public Jump(StardewValley.Farmer thePlayer, IModEvents events)
            {
                player = thePlayer;
                this.events = events;
                prevJumpVel = player.yJumpVelocity;

                player.synchronizedJump(8);

                events.GameLoop.UpdateTicked += onUpdateTicked;
            }

            /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The event arguments.</param>
            private void onUpdateTicked(object sender, UpdateTickedEventArgs e)
            {
                if (player.yJumpVelocity == 0 && prevJumpVel < 0)
                {
                    player.canMove = true;

                    events.GameLoop.UpdateTicked -= onUpdateTicked;
                }
                else
                {
                    int tx = (int)player.position.X / Game1.tileSize;
                    int ty = (int)player.position.Y / Game1.tileSize;
                    int ox = 0, oy = 0; // Offset x, y
                    switch (player.facingDirection.Value)
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

                    //Log.trace($"{n0} {n1} {n2}");
                    if ( n0 || ( !n0 && n1 && !n2 ) /*|| wasGoingOver*/ )
                    {
                        //wasGoingOver = true;
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
