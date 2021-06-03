using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThrowableAxe
{
    public class ThrownAxe : Projectile
    {
        private GameLocation loc;
        private readonly NetInt tier = new NetInt(0);
        private readonly NetInt damage = new NetInt(3);
        public readonly NetVector2 target = new NetVector2();
        private readonly NetFloat speed = new NetFloat(1);
        private float axeRot = 0;
        public bool dead = false;
        public List<NPC> npcsHit = new List<NPC>();
        
        public ThrownAxe()
        {
            this.NetFields.AddFields( this.tier, this.damage, this.target, this.speed );
        }

        public ThrownAxe(Farmer thrower, int tier, int damage, Vector2 target, float speed)
        {
            this.position.X = thrower.getStandingX() - 16;
            this.position.Y = thrower.getStandingY() - 64;

            this.loc = thrower.currentLocation;
            this.theOneWhoFiredMe.Set(this.loc, thrower);
            this.damagesMonsters.Value = true;
            this.tier.Value = tier;
            this.damage.Value = damage;
            this.target.Value = target;
            this.speed.Value = speed;
            boundingBoxWidth = 64;
            boundingBoxHeight = 64;
            this.NetFields.AddFields(this.tier, this.damage, this.target, this.speed);
        }

        public override void behaviorOnCollisionWithMineWall(int tileX, int tileY)
        {
        }

        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
            if (npcsHit.Contains(n))
                return;

            npcsHit.Add(n);
            if (n is Monster mob)
            {
                location.damageMonster(getBoundingBox(), damage, damage, false, (Farmer)theOneWhoFiredMe.Get(location));
            }
        }

        public override void behaviorOnCollisionWithOther(GameLocation location)
        {
        }

        public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
        {
        }

        public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
        {
        }

        public override bool update(GameTime time, GameLocation location)
        {
            base.update(time, location);
            return dead;
        }

        public override void updatePosition(GameTime time)
        {
            Vector2 targetDiff = target.Value - position.Value;
            Vector2 targetDir = targetDiff;
            targetDir.Normalize();

            if (targetDiff.Length() < speed.Value)
                position.Value = target.Value;
            else
                position.Value += targetDir * speed.Value;

            //Log.trace($"{position.Value} {target.Value} {targetDir}");
        }

        public override void draw(SpriteBatch b)
        {
            int sheetShift = tier * 7;
            if (tier > 2)
                sheetShift += 21;
            var sourceRect = Game1.getSquareSourceRectForNonStandardTileSheet(Game1.toolSpriteSheet, 16, 16, 215 + sheetShift);
            //b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, getBoundingBox()), null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
            b.Draw(Game1.toolSpriteSheet, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(32, 32)), sourceRect, Color.White, rotation, new Vector2(8, 8), 4, SpriteEffects.None, 1);
            rotation += 0.3f;
        }

        public override Rectangle getBoundingBox()
        {
            return base.getBoundingBox();
        }

        public Vector2 GetPosition()
        {
            return position.Value;
        }
    }
}
