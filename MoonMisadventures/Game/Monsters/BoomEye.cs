using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures.Game.Projectiles;
using Netcode;
using StardewValley;
using StardewValley.Monsters;

namespace MoonMisadventures.Game.Monsters
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_BoomEye" )]
    public class BoomEye : Monster
    {
        private readonly NetFloat shootTimer = new();
        private readonly NetBool seenPlayer = new();

        public BoomEye() { }
        public BoomEye( Vector2 pos )
        :   base( "Bat", pos )
        {
            Name = "BoomEye";
            ReloadShoot();
            Health = MaxHealth = 100 + Game1.random.Next( 200 );
            DamageToFarmer = 0;
            isGlider.Value = true;
            // todo - loot
            speed = 3;
            ExperienceGained = 20;
            displayName = Mod.instance.Helper.Translation.Get( "monster.boom-eye.name" );
            Slipperiness = 60 + Game1.random.Next( -10, 11 );
            Halt();
            IsWalkingTowardPlayer = false;
            reloadSprite();
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields( shootTimer, seenPlayer );
        }

        public override void reloadSprite()
        {
            Sprite = new AnimatedSprite( Mod.instance.Helper.Content.GetActualAssetKey( "assets/enemies/boom-eye.png" ), 0, 16, 32 );
        }

        public void ReloadShoot()
        {
            shootTimer.Value = ( float ) ( 3f + Game1.random.NextDouble() * 2 );
        }

        public override int takeDamage( int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who )
        {
            if ( isBomb )
                return 0;
            return base.takeDamage( damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who );
        }

        public override void behaviorAtGameTick( GameTime time )
        {
            base.behaviorAtGameTick( time );
            if ( !seenPlayer.Value )
            {
                if ( withinPlayerThreshold( 10 ) )
                    seenPlayer.Value = true;
                else
                {
                    // TODO: do idle movement
                    return;
                }
            }

            if ( !withinPlayerThreshold( 17 ) )
            {
                seenPlayer.Value = false;
                return;
            }

            shootTimer.Value -= ( float ) time.ElapsedGameTime.TotalSeconds;
            if ( shootTimer.Value <= 0 )
            {
                var dir = Position - Player.Position;
                dir.Normalize();
                float force = 3 + Game1.recentMultiplayerRandom.Next( 4 );
                xVelocity = dir.X * force;
                yVelocity = dir.Y * force;

                currentLocation.projectiles.Add( new BoomProjectile( Position, Player.Position ) );

                ReloadShoot();
            }
        }

        protected override void updateAnimation( GameTime time )
        {
            Sprite.Animate( time, 0, 80, 100 );
        }

        public override void draw( SpriteBatch b )
        {
            Vector2 draw_position = this.Position;
            this.Sprite.draw( b, Game1.GlobalToLocal( Game1.viewport, draw_position ), ( float ) this.GetBoundingBox().Center.Y / 10000f );
        }
    }
}
