using System;
using System.Collections.Generic;
using FireArcadeGame.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace FireArcadeGame.Objects
{
    public class Player : Character
    {
        private Texture2D staffTex;
        private VertexBuffer staffBuffer;

        public int healthMax = 5;
        public int mana = 25, manaMax = 25;
        public float speed = 0.065f;
        public float turnSpeed = 0.045f;
        public float look = 0;
        public float chargeTime = 0;
        public float manaRegenTimer = 0;
        public float immunTimer = 0;

        private Random shakeRand = new Random();

        public Player( World world )
        :   base( world )
        {
            Health = healthMax;

            staffTex = Mod.instance.Helper.Content.Load<Texture2D>( "assets/staff.png" );

            List<VertexPositionColorTexture> staffVerts = new List<VertexPositionColorTexture>();
            staffVerts.Add( new VertexPositionColorTexture( new Vector3( 0, 0, 0 ), Color.White, new Vector2( 0, 0 ) ) );
            staffVerts.Add( new VertexPositionColorTexture( new Vector3( 1, 0, 0 ), Color.White, new Vector2( 1, 0 ) ) );
            staffVerts.Add( new VertexPositionColorTexture( new Vector3( 1, 1, 0 ), Color.White, new Vector2( 1, 1 ) ) );

            staffVerts.Add( new VertexPositionColorTexture( new Vector3( 0, 0, 0 ), Color.White, new Vector2( 0, 0 ) ) );
            staffVerts.Add( new VertexPositionColorTexture( new Vector3( 0, 1, 0 ), Color.White, new Vector2( 0, 1 ) ) );
            staffVerts.Add( new VertexPositionColorTexture( new Vector3( 1, 1, 0 ), Color.White, new Vector2( 1, 1 ) ) );

            staffBuffer = new VertexBuffer( Game1.game1.GraphicsDevice, typeof( VertexPositionColorTexture ), 6, BufferUsage.WriteOnly );
            staffBuffer.SetData( staffVerts.ToArray() );
        }

        public override void Hurt( int amt )
        {
            if ( immunTimer > 0 )
                return;

            base.Hurt( amt );
            immunTimer = 1;
            Game1.playSound( "ow" );

            if ( Health <= 0 )
            {
                World.Quit();
                Game1.drawObjectDialogue( "You lost." );
            }
        }

        public override void DoMovement()
        {
            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.A ) )
            {
                look -= turnSpeed;
            }
            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.D ) )
            {
                look += turnSpeed;
            }
            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.Q ) )
            {
                Position.X += ( float ) Math.Cos( look - Math.PI / 2 ) * speed;
                Position.Z += ( float ) Math.Sin( look - Math.PI / 2 ) * speed;
            }
            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.E ) )
            {
                Position.X += ( float ) Math.Cos( look + Math.PI / 2 ) * speed;
                Position.Z += ( float ) Math.Sin( look + Math.PI / 2 ) * speed;
            }
            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.W ) )
            {
                Position.X += ( float ) Math.Cos( look ) * speed;
                Position.Z += ( float ) Math.Sin( look ) * speed;
            }
            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.S ) )
            {
                Position.X -= ( float ) Math.Cos( look ) * speed;
                Position.Z -= ( float ) Math.Sin( look ) * speed;
            }

            World.cam.pos = Position;
            World.cam.target = Position + new Vector3( ( float ) Math.Cos( look ), 0, ( float ) Math.Sin( look ) );
            World.cam.up = Vector3.Up;
        }

        public override void Update()
        {
            base.Update();

            manaRegenTimer += ( float ) Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            while ( manaRegenTimer >= 1 )
            {
                manaRegenTimer -= 1;
                if ( ++mana > manaMax )
                    mana = manaMax;
            }

            if ( immunTimer > 0 )
            {
                immunTimer = Math.Max( 0, immunTimer - (float) Game1.currentGameTime.ElapsedGameTime.TotalSeconds );
            }

            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.Space ) )
            {
                chargeTime += (float) Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                if ( chargeTime >= 1 )
                    chargeTime = 1;
            }
            else
            {
                if ( chargeTime > 0 )
                {
                    int tier = ( int )( chargeTime / 0.25f );
                    if ( tier >= 1 && mana > tier )
                    {
                        mana -= tier;
                        var speed = ( World.cam.target - World.cam.pos ) / 4;// * ( -4 + tier );
                        var proj = new PlayerFireball( World )
                        {
                            Position = new Vector3( Position.X, 0, Position.Z ),
                            Speed = new Vector2( speed.X, speed.Z ),
                            Level = tier - 1,
                        };
                        World.projectiles.Add( proj );
                        Game1.playSound( "fireball" );
                    }
                    else
                    {
                        Game1.playSound( "steam" );
                    }
                    chargeTime = 0;
                }
            }

            foreach ( var proj in World.projectiles )
            {
                if ( proj.Dead )
                    continue;

                if ( ( proj.BoundingBox + new Vector2( proj.Position.X, proj.Position.Z ) ).Intersects( BoundingBox + new Vector2( Position.X, Position.Z ) ) && proj.HurtsPlayer )
                {
                    proj.Trigger( this );
                }
            }

            if ( World.warp != null && ( BoundingBox + new Vector2( Position.X, Position.Z ) ).Intersects( new RectangleF( World.warp.Position.X, World.warp.Position.Z, 1, 1 ) ) )
            {
                Game1.playSound( "wand" );
                World.QueueNextLevel();
            }

        }

        public override void RenderOver( GraphicsDevice device, Matrix projection, Camera cam )
        {
            base.RenderOver( device, projection, cam );

            effect.TextureEnabled = true;
            effect.Texture = staffTex;
            var lookVec = World.cam.target - Position;
            var lookSide = new Vector3( (float) Math.Cos( look + (float)( Math.PI / 2 ) ), 0, (float) Math.Sin( look + (float)( Math.PI / 2 ) ) );
            var staffOffset = new Vector2( 0.45f, -1.15f );
            staffOffset += new Vector2( chargeTime * -0.25f, chargeTime * 0.25f );
            if ( chargeTime >= 1 )
            {
                staffOffset.X += ( float ) shakeRand.NextDouble() * 0.1f - 0.05f;
                staffOffset.Y += ( float ) shakeRand.NextDouble() * 0.1f - 0.05f;
            }
            effect.World = Matrix.CreateRotationZ( (float)( Math.PI / 2 ) ) * 
                           Matrix.CreateRotationY( -look + ( float )( Math.PI / 2 ) ) *
                           Matrix.CreateTranslation( Position + lookVec + new Vector3( lookSide.X * staffOffset.X, staffOffset.Y, lookSide.Z * staffOffset.X ) );
            for ( int e = 0; e < effect.CurrentTechnique.Passes.Count; ++e )
            {
                var pass = effect.CurrentTechnique.Passes[ e ];
                pass.Apply();
                device.SetVertexBuffer( staffBuffer );
                device.DrawPrimitives( PrimitiveType.TriangleList, 0, 2 );
            }
        }

        public override void RenderUi( SpriteBatch b )
        {
            b.Begin();

            for ( int i = 0; i < healthMax; ++i )
            {
                b.Draw( Game1.objectSpriteSheet, new Vector2( 2 + i * 12, 2 ), new Rectangle( 288, 608, 16, 16 ), ( i + 1 ) <= Health ? Color.White : Color.Gray, 0, Vector2.Zero, 1, SpriteEffects.None, 1 );
            }

            b.Draw( Game1.staminaRect, new Rectangle( 4, 18, manaMax + 2, 6 ), Color.MidnightBlue );
            if ( mana > 0 )
                b.Draw( Game1.staminaRect, new Rectangle( 5, 19, mana, 4 ), Color.MediumBlue );

            b.Draw( Game1.staminaRect, new Rectangle( 4, World.ScreenSize - 10, World.ScreenSize - 8, 6 ), Color.White );
            b.Draw( Game1.staminaRect, new Rectangle( 5, World.ScreenSize - 9, World.ScreenSize - 10, 4 ), Color.Black );
            if ( chargeTime > 0 )
            {
                var col = Color.Khaki;
                if ( chargeTime >= 1 )
                    col = Color.Red;
                else if ( chargeTime >= 0.75 )
                    col = Color.OrangeRed;
                else if ( chargeTime >= 0.5 )
                    col = Color.Orange;
                else if ( chargeTime >= 0.25 )
                    col = Color.Yellow;
                b.Draw( Game1.staminaRect, new Rectangle( 5, World.ScreenSize - 9, ( int ) ( ( World.ScreenSize - 10 ) * chargeTime ), 4 ), col );
            }
            b.Draw( Game1.staminaRect, new Rectangle( ( int ) ( 5 + ( World.ScreenSize - 10 ) * 0.25f ), World.ScreenSize - 9, 1, 4 ), Color.Gray );
            b.Draw( Game1.staminaRect, new Rectangle( ( int ) ( 5 + ( World.ScreenSize - 10 ) * 0.5f ), World.ScreenSize - 9, 1, 4 ), Color.Gray );
            b.Draw( Game1.staminaRect, new Rectangle( ( int ) ( 5 + ( World.ScreenSize - 10 ) * 0.75f ), World.ScreenSize - 9, 1, 4 ), Color.Gray );

            b.End();
        }
    }
}
