using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace TheftOfTheWinterStar
{
    public class Witch : Monster
    {
        private const int STUN_TIME = 800;
        private const int SHOOT_DELAY = 4000;
        private const int SPAWN_ROCKS_DELAY = 10000;
        private const int SPAWN_ENEMY_DELAY = 6500;

        private const int CURSORS_POS_X = 277;
        private const int CURSORS_POS_Y = 1885;
        private const int TEX_WIDTH = 34;
        private const int TEX_HEIGHT = 30;

        public const int WITCH_HEALTH = 1000;

        private readonly NetBool facingRight = new NetBool(false);
        private readonly NetInt shootPlayerTimer = new NetInt(SHOOT_DELAY);
        private readonly NetInt spawnRocksTimer = new NetInt(SPAWN_ROCKS_DELAY);
        private readonly NetInt spawnEnemyTimer = new NetInt(SPAWN_ENEMY_DELAY);
        private readonly NetInt stunTimer = new NetInt(0);
        private int animTimer = 0;

        public Witch()
            : base("Serpent", new Vector2(-1000, -1000))
        {
            this.HideShadow = true;
            this.isGlider.Value = true;
            this.Name = "Witch";
            this.Health = WITCH_HEALTH;
            this.speed = 7;
            this.Portrait = Mod.instance.Helper.Content.Load<Texture2D>("assets/witch-portrait.png");
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.facingRight, this.shootPlayerTimer, this.spawnRocksTimer, this.spawnEnemyTimer, this.stunTimer);
        }

        public override Rectangle GetBoundingBox()
        {
            return new Rectangle((int)this.Position.X + 4 * Game1.pixelZoom, (int)this.Position.Y, (TEX_WIDTH - 12) * Game1.pixelZoom, (TEX_HEIGHT - 4) * Game1.pixelZoom);
        }

        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            if (this.stunTimer.Value <= 0)
                this.stunTimer.Value = STUN_TIME;
            return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
        }

        public override void setTrajectory(Vector2 trajectory)
        {
        }

        public override void behaviorAtGameTick(GameTime time)
        {
            if (this.GetBoundingBox().Right < -Game1.tileSize)
            {
                this.facingRight.Value = true;
                this.position.X = -Game1.tileSize;
                this.position.Y = Game1.random.Next(4, 15) * Game1.tileSize;
            }
            else if (this.GetBoundingBox().Left > this.currentLocation.Map.DisplayWidth + Game1.tileSize)
            {
                this.facingRight.Value = false;
                this.position.X = this.currentLocation.Map.DisplayWidth + Game1.tileSize - TEX_WIDTH;
                this.position.Y = Game1.random.Next(4, 15) * Game1.tileSize;
            }

            this.moveLeft = true;
            this.moveRight = false;
            if (this.facingRight.Value)
            {
                this.moveLeft = false;
                this.moveRight = true;
            }

            if (this.stunTimer.Value >= STUN_TIME / 2)
            {
                this.moveLeft = false;
                this.moveRight = false;
            }
            if (this.stunTimer.Value > 0)
            {
                this.stunTimer.Value -= time.ElapsedGameTime.Milliseconds;
            }

            this.position.Y += (float)Math.Sin(time.TotalGameTime.TotalSeconds * 5) * 3;

            base.behaviorAtGameTick(time);

            this.shootPlayerTimer.Value -= time.ElapsedGameTime.Milliseconds;
            if (this.shootPlayerTimer.Value-- <= 0)
            {
                Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(this.GetBoundingBox().Center, 15f, this.Player);
                Projectile proj = new DebuffingProjectile(14, 7, 4, 4, 0.1963495f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2((float)this.GetBoundingBox().X, (float)this.GetBoundingBox().Y), this.currentLocation, this);
                this.currentLocation.projectiles.Add(proj);
                this.shootPlayerTimer.Value = SHOOT_DELAY;
            }

            this.spawnRocksTimer.Value -= time.ElapsedGameTime.Milliseconds;
            if (this.spawnRocksTimer.Value-- <= 0)
            {
                Rectangle region = new Rectangle(4, 7, 15 - 4, 15 - 7);
                for (int i = 3 + Game1.random.Next(5); i >= 0; --i)
                {
                    Vector2 spot = new Vector2(region.X + Game1.random.Next(region.Width), region.Y + Game1.random.Next(region.Height));
                    if (this.currentLocation.Objects.ContainsKey(spot))
                        continue;

                    int rock = Game1.random.Next(47, 54);
                    rock = rock + rock % 2;
                    var obj = new StardewValley.Object(spot, rock, "Stone", true, false, false, false)
                    {
                        MinutesUntilReady = 3
                    };

                    this.currentLocation.Objects.Add(spot, obj);
                }
                this.spawnRocksTimer.Value = SPAWN_ROCKS_DELAY / 2 + Game1.random.Next(SPAWN_ROCKS_DELAY);
            }

            this.spawnEnemyTimer.Value -= time.ElapsedGameTime.Milliseconds;
            if (this.spawnEnemyTimer.Value-- <= 0)
            {
                Rectangle region = new Rectangle(4, 7, 15 - 4, 15 - 7);
                for (int i = 1 + Game1.random.Next(3); i >= 0; --i)
                {
                    int x = region.X + Game1.random.Next(region.Width);
                    int y = region.Y + Game1.random.Next(region.Height);
                    Bat bat = new Bat(new Vector2(x * Game1.tileSize, y * Game1.tileSize), MineShaft.frostArea);
                    bat.focusedOnFarmers = true;
                    this.currentLocation.characters.Add(bat);
                }
                this.spawnEnemyTimer.Value = SPAWN_ENEMY_DELAY / 3 + Game1.random.Next(SPAWN_ENEMY_DELAY);
            }
        }

        public override void drawAboveAllLayers(SpriteBatch b)
        {
            b.Draw(Game1.mouseCursors, this.getLocalPosition(Game1.viewport), new Rectangle(CURSORS_POS_X, CURSORS_POS_Y + (this.animTimer < 20 ? TEX_HEIGHT : 0), TEX_WIDTH, TEX_HEIGHT), Color.White, 0, Vector2.Zero, 4, this.facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);
            if (++this.animTimer >= 40)
                this.animTimer = 0;
        }
    }
}
