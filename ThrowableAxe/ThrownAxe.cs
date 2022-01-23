using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace ThrowableAxe
{
    [XmlType("Mods_spacechase0_ThrowableAxe_ThrownAxe")]
    public class ThrownAxe : Projectile
    {
        private readonly NetInt Tier = new(0);
        private readonly NetInt Damage = new(3);
        public readonly NetVector2 Target = new();
        private readonly NetFloat Speed = new(1);
        public bool Dead = false;
        public List<NPC> NpcsHit = new();

        public ThrownAxe()
        {
            this.NetFields.AddFields(this.Tier, this.Damage, this.Target, this.Speed);
        }

        public ThrownAxe(Farmer thrower, int tier, int damage, Vector2 target, float speed)
        {
            this.position.X = thrower.getStandingX() - 16;
            this.position.Y = thrower.getStandingY() - 64;

            this.theOneWhoFiredMe.Set(thrower.currentLocation, thrower);
            this.damagesMonsters.Value = true;
            this.Tier.Value = tier;
            this.Damage.Value = damage;
            this.Target.Value = target;
            this.Speed.Value = speed;
            Projectile.boundingBoxWidth = 64;
            Projectile.boundingBoxHeight = 64;
            this.NetFields.AddFields(this.Tier, this.Damage, this.Target, this.Speed);
        }

        public override void behaviorOnCollisionWithMineWall(int tileX, int tileY)
        {
        }

        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
            if (this.NpcsHit.Contains(n))
                return;

            this.NpcsHit.Add(n);
            if (n is Monster)
            {
                location.damageMonster(this.getBoundingBox(), this.Damage.Value, this.Damage.Value, false, (Farmer)this.theOneWhoFiredMe.Get(location));
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
            return this.Dead;
        }

        public override void updatePosition(GameTime time)
        {
            Vector2 targetDiff = this.Target.Value - this.position.Value;
            Vector2 targetDir = targetDiff;
            targetDir.Normalize();

            if (targetDiff.Length() < this.Speed.Value)
                this.position.Value = this.Target.Value;
            else
                this.position.Value += targetDir * this.Speed.Value;

            //Log.trace($"{position.Value} {target.Value} {targetDir}");
        }

        public override void draw(SpriteBatch b)
        {
            int sheetShift = this.Tier.Value * 7;
            if (this.Tier.Value > 2)
                sheetShift += 21;
            var sourceRect = Game1.getSquareSourceRectForNonStandardTileSheet(Game1.toolSpriteSheet, 16, 16, 215 + sheetShift);
            //b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, getBoundingBox()), null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
            b.Draw(Game1.toolSpriteSheet, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(32, 32)), sourceRect, Color.White, this.rotation, new Vector2(8, 8), 4, SpriteEffects.None, 1);
            this.rotation += 0.3f;
        }

        public Vector2 GetPosition()
        {
            return this.position.Value;
        }
    }
}
