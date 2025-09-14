using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Monsters;

namespace MoonMisadventures.Game.Monsters
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_CrystalBehemonth" )]
    public class CrystalBehemoth : Monster
    {
        public readonly NetInt stun = new();

        public CrystalBehemoth() { }

        public CrystalBehemoth(Vector2 position)
        : base("Stone Golem", position)
        {
            base.Slipperiness = 2;
            base.jitteriness.Value = 0;
            base.HideShadow = false;

            Name = "CrystalBehemoth";
            reloadSprite();
            Health = MaxHealth = 500;
            willDestroyObjectsUnderfoot = true;
            DamageToFarmer = 40;
            speed = 8;
            moveTowardPlayerThreshold.Value = 64 * 10;

            objectsToDrop.Clear();
            // TODO: Better drops
            for ( int i = 7 + Game1.random.Next( 24 ); i > 0; --i )
                objectsToDrop.Add("80");
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(stun, nameof(stun));
        }

        public override Rectangle GetBoundingBox()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, 16, 16);
        }

        public Rectangle GetHurtBox()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, 32, 32);
        }

        public override bool TakesDamageFromHitbox(Rectangle area_of_effect)
        {
            return GetHurtBox().Intersects(area_of_effect);
        }

        public override bool OverlapsFarmerForDamage(Farmer who)
        {
            return GetHurtBox().Intersects(who.GetBoundingBox());
        }

        public override void reloadSprite()
        {
            Sprite = new AnimatedSprite(Mod.instance.Helper.ModContent.GetInternalAssetName("assets/enemies/crystal-behemoth.png").BaseName, 0, 32, 48);
            //Sprite.textureUsesFlippedRightForLeft = true;
        }

        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            damage /= 2;
            if (damage > 20)
                stun.Value = 30;
            return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
        }

        public override void behaviorOnFarmerLocationEntry(GameLocation location, Farmer who)
        {
            // Fixes reverting to 16x16 sprite since ours is so tall
        }

        public override void update(GameTime time, GameLocation location)
        {
            if (stun.Value > 0)
            {
                stun.Value--;
                return;
            }

            base.update(time, location);
        }

        protected override void localDeathAnimation()
        {
            base.localDeathAnimation();
            base.currentLocation.localSound("hammer");
            DelayedAction.playSoundAfterDelay("hammer", 50);
            DelayedAction.playSoundAfterDelay("boulderCrack", 150);
        }
    }
}
