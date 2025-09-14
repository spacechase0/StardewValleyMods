using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using SObject = StardewValley.Object;

namespace TheftOfTheWinterStar.Framework
{
    public class Witch : Monster
    {
        /*********
        ** Fields
        *********/
        private const int StunTime = 800;
        private const int ShootDelay = 4000;
        private const int SpawnRocksDelay = 10000;
        private const int SpawnEnemyDelay = 6500;

        private const int CursorsPosX = 277;
        private const int CursorsPosY = 1885;
        private const int TexWidth = 34;
        private const int TexHeight = 30;

        private readonly NetBool FacingRight = new(false);
        private readonly NetInt ShootPlayerTimer = new(Witch.ShootDelay);
        private readonly NetInt SpawnRocksTimer = new(Witch.SpawnRocksDelay);
        private readonly NetInt SpawnEnemyTimer = new(Witch.SpawnEnemyDelay);
        private readonly NetInt StunTimer = new(0);
        private int AnimTimer;


        /*********
        ** Accessors
        *********/
        public static int WitchHealth { get; } = 1000;


        /*********
        ** Public methods
        *********/
        public Witch()
            : base("Serpent", new Vector2(-1000, -1000))
        {
            this.HideShadow = true;
            this.isGlider.Value = true;
            this.Name = "Witch";
            this.Health = Witch.WitchHealth;
            this.speed = 7;
            this.Portrait = Mod.Instance.Helper.Content.Load<Texture2D>("assets/witch-portrait.png");
        }

        public override Rectangle GetBoundingBox()
        {
            return new((int)this.Position.X + 4 * Game1.pixelZoom, (int)this.Position.Y, (Witch.TexWidth - 12) * Game1.pixelZoom, (Witch.TexHeight - 4) * Game1.pixelZoom);
        }

        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            if (this.StunTimer.Value <= 0)
                this.StunTimer.Value = Witch.StunTime;
            return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
        }

        public override void setTrajectory(Vector2 trajectory)
        {
        }

        public override void behaviorAtGameTick(GameTime time)
        {
            if (this.GetBoundingBox().Right < -Game1.tileSize)
            {
                this.FacingRight.Value = true;
                this.position.X = -Game1.tileSize;
                this.position.Y = Game1.random.Next(4, 15) * Game1.tileSize;
            }
            else if (this.GetBoundingBox().Left > this.currentLocation.Map.DisplayWidth + Game1.tileSize)
            {
                this.FacingRight.Value = false;
                this.position.X = this.currentLocation.Map.DisplayWidth + Game1.tileSize - Witch.TexWidth;
                this.position.Y = Game1.random.Next(4, 15) * Game1.tileSize;
            }

            this.moveLeft = true;
            this.moveRight = false;
            if (this.FacingRight.Value)
            {
                this.moveLeft = false;
                this.moveRight = true;
            }

            if (this.StunTimer.Value >= Witch.StunTime / 2)
            {
                this.moveLeft = false;
                this.moveRight = false;
            }
            if (this.StunTimer.Value > 0)
            {
                this.StunTimer.Value -= time.ElapsedGameTime.Milliseconds;
            }

            this.position.Y += (float)Math.Sin(time.TotalGameTime.TotalSeconds * 5) * 3;

            base.behaviorAtGameTick(time);

            this.ShootPlayerTimer.Value -= time.ElapsedGameTime.Milliseconds;
            if (this.ShootPlayerTimer.Value-- <= 0)
            {
                Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(this.GetBoundingBox().Center, 15f, this.Player);
                Projectile proj = new DebuffingProjectile(14, 7, 4, 4, 0.1963495f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2(this.GetBoundingBox().X, this.GetBoundingBox().Y), this.currentLocation, this);
                this.currentLocation.projectiles.Add(proj);
                this.ShootPlayerTimer.Value = Witch.ShootDelay;
            }

            this.SpawnRocksTimer.Value -= time.ElapsedGameTime.Milliseconds;
            if (this.SpawnRocksTimer.Value-- <= 0)
            {
                Rectangle region = new Rectangle(4, 7, 15 - 4, 15 - 7);
                for (int i = 3 + Game1.random.Next(5); i >= 0; --i)
                {
                    Vector2 spot = new Vector2(region.X + Game1.random.Next(region.Width), region.Y + Game1.random.Next(region.Height));
                    if (this.currentLocation.Objects.ContainsKey(spot))
                        continue;

                    int rock = Game1.random.Next(47, 54);
                    rock = rock + rock % 2;
                    var obj = new SObject(spot, rock, "Stone", true, false, false, false)
                    {
                        MinutesUntilReady = 3
                    };

                    this.currentLocation.Objects.Add(spot, obj);
                }
                this.SpawnRocksTimer.Value = Witch.SpawnRocksDelay / 2 + Game1.random.Next(Witch.SpawnRocksDelay);
            }

            this.SpawnEnemyTimer.Value -= time.ElapsedGameTime.Milliseconds;
            if (this.SpawnEnemyTimer.Value-- <= 0)
            {
                Rectangle region = new Rectangle(4, 7, 15 - 4, 15 - 7);
                for (int i = 1 + Game1.random.Next(3); i >= 0; --i)
                {
                    int x = region.X + Game1.random.Next(region.Width);
                    int y = region.Y + Game1.random.Next(region.Height);
                    Bat bat = new Bat(new Vector2(x * Game1.tileSize, y * Game1.tileSize), MineShaft.frostArea)
                    {
                        focusedOnFarmers = true
                    };
                    this.currentLocation.characters.Add(bat);
                }
                this.SpawnEnemyTimer.Value = Witch.SpawnEnemyDelay / 3 + Game1.random.Next(Witch.SpawnEnemyDelay);
            }
        }

        public override void drawAboveAllLayers(SpriteBatch b)
        {
            b.Draw(Game1.mouseCursors, this.getLocalPosition(Game1.viewport), new Rectangle(Witch.CursorsPosX, Witch.CursorsPosY + (this.AnimTimer < 20 ? Witch.TexHeight : 0), Witch.TexWidth, Witch.TexHeight), Color.White, 0, Vector2.Zero, 4, this.FacingRight.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);
            if (++this.AnimTimer >= 40)
                this.AnimTimer = 0;
        }


        /*********
        ** Protected methods
        *********/
        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.FacingRight, this.ShootPlayerTimer, this.SpawnRocksTimer, this.SpawnEnemyTimer, this.StunTimer);
        }
    }
}
