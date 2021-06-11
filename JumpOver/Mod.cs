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
            Mod.instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.Input.ButtonPressed += this.onButtonPressed;
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => Mod.Config = new Configuration(), () => this.Helper.WriteConfig(Mod.Config));
                capi.RegisterSimpleOption(this.ModManifest, "Jump Key", "The key to jump", () => Mod.Config.keyJump, (SButton val) => Mod.Config.keyJump = val);
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.activeClickableMenu != null)
                return;

            if (e.Button == Mod.Config.keyJump && Game1.player.yJumpVelocity == 0)
            {
                // This is terrible for this case, redo it
                new Jump(Game1.player, this.Helper.Events);
            }
        }

        internal class Jump
        {
            private readonly Farmer player;
            private readonly IModEvents events;
            private float prevJumpVel;

            //private bool wasGoingOver = false;

            public Jump(Farmer thePlayer, IModEvents events)
            {
                this.player = thePlayer;
                this.events = events;
                this.prevJumpVel = this.player.yJumpVelocity;

                this.player.synchronizedJump(8);

                events.GameLoop.UpdateTicked += this.onUpdateTicked;
            }

            /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The event arguments.</param>
            private void onUpdateTicked(object sender, UpdateTickedEventArgs e)
            {
                if (this.player.yJumpVelocity == 0 && this.prevJumpVel < 0)
                {
                    this.player.canMove = true;

                    this.events.GameLoop.UpdateTicked -= this.onUpdateTicked;
                }
                else
                {
                    int tx = (int)this.player.position.X / Game1.tileSize;
                    int ty = (int)this.player.position.Y / Game1.tileSize;
                    int ox = 0, oy = 0; // Offset x, y
                    switch (this.player.facingDirection.Value)
                    {
                        case Game1.up: oy = -1; break;
                        case Game1.down: oy = 1; break;
                        case Game1.left: ox = -1; break;
                        case Game1.right: ox = 1; break;
                    }

                    var bb = this.player.GetBoundingBox();
                    var bb1 = this.player.GetBoundingBox();
                    bb1.X += ox * Game1.tileSize;
                    bb1.Y += oy * Game1.tileSize;
                    var bb2 = this.player.GetBoundingBox();
                    bb2.X += ox * Game1.tileSize * 2;
                    bb2.Y += oy * Game1.tileSize * 2;

                    bool n0 = this.player.currentLocation.isCollidingPosition(bb, Game1.viewport, true, 0, false, this.player);
                    bool n1 = this.player.currentLocation.isCollidingPosition(bb1, Game1.viewport, true, 0, false, this.player);
                    bool n2 = this.player.currentLocation.isCollidingPosition(bb2, Game1.viewport, true, 0, false, this.player);

                    //Log.trace($"{n0} {n1} {n2}");
                    if (n0 || (!n0 && n1 && !n2) /*|| wasGoingOver*/ )
                    {
                        //wasGoingOver = true;
                        Game1.player.canMove = false;
                        this.player.position.X += ox * 5;
                        this.player.position.Y += oy * 5;
                    }
                }

                this.prevJumpVel = this.player.yJumpVelocity;
            }
        }
    }
}
