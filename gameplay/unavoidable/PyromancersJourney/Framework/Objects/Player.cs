using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PyromancersJourney.Framework.Projectiles;
using StardewValley;

namespace PyromancersJourney.Framework.Objects
{
    internal class Player : Character
    {
        private readonly Texture2D StaffTex;
        private readonly VertexBuffer StaffBuffer;

        public int HealthMax = 5;
        public int Mana = 25, ManaMax = 25;
        public float Speed = 0.065f;
        public float TurnSpeed = 0.045f;
        public float Look;
        public float ChargeTime;
        public float ManaRegenTimer;
        public float ImmunTimer;

        private readonly Random ShakeRand = new();

        public Player(World world)
            : base(world)
        {
            this.Health = this.HealthMax;

            this.StaffTex = Mod.Instance.Helper.ModContent.Load<Texture2D>("assets/staff.png");

            var staffVerts = new VertexPositionColorTexture[]
            {
                new(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0)),
                new(new Vector3(1, 0, 0), Color.White, new Vector2(1, 0)),
                new(new Vector3(1, 1, 0), Color.White, new Vector2(1, 1)),
                new(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0)),
                new(new Vector3(0, 1, 0), Color.White, new Vector2(0, 1)),
                new(new Vector3(1, 1, 0), Color.White, new Vector2(1, 1))
            };


            this.StaffBuffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), staffVerts.Length, BufferUsage.WriteOnly);
            this.StaffBuffer.SetData(staffVerts);
        }

        public override void Hurt(int amt)
        {
            if (this.ImmunTimer > 0)
                return;

            base.Hurt(amt);
            this.ImmunTimer = 1;
            Game1.playSound("ow");

            if (this.Health <= 0)
            {
                this.World.Quit();
                Game1.drawObjectDialogue("You lost.");
            }
        }

        public override void DoMovement()
        {
            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.A))
            {
                this.Look -= this.TurnSpeed;
            }
            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.D))
            {
                this.Look += this.TurnSpeed;
            }
            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.Q))
            {
                this.Position.X += (float)Math.Cos(this.Look - Math.PI / 2) * this.Speed;
                this.Position.Z += (float)Math.Sin(this.Look - Math.PI / 2) * this.Speed;
            }
            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.E))
            {
                this.Position.X += (float)Math.Cos(this.Look + Math.PI / 2) * this.Speed;
                this.Position.Z += (float)Math.Sin(this.Look + Math.PI / 2) * this.Speed;
            }
            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.W))
            {
                this.Position.X += (float)Math.Cos(this.Look) * this.Speed;
                this.Position.Z += (float)Math.Sin(this.Look) * this.Speed;
            }
            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.S))
            {
                this.Position.X -= (float)Math.Cos(this.Look) * this.Speed;
                this.Position.Z -= (float)Math.Sin(this.Look) * this.Speed;
            }

            this.World.Cam.Pos = this.Position;
            this.World.Cam.Target = this.Position + new Vector3((float)Math.Cos(this.Look), 0, (float)Math.Sin(this.Look));
            this.World.Cam.Up = Vector3.Up;
        }

        public override void Update()
        {
            base.Update();

            this.ManaRegenTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            while (this.ManaRegenTimer >= 1)
            {
                this.ManaRegenTimer -= 1;
                if (++this.Mana > this.ManaMax)
                    this.Mana = this.ManaMax;
            }

            if (this.ImmunTimer > 0)
            {
                this.ImmunTimer = Math.Max(0, this.ImmunTimer - (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds);
            }

            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.Space))
            {
                this.ChargeTime += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                if (this.ChargeTime >= 1)
                    this.ChargeTime = 1;
            }
            else
            {
                if (this.ChargeTime > 0)
                {
                    int tier = (int)(this.ChargeTime / 0.25f);
                    if (tier >= 1 && this.Mana > tier)
                    {
                        this.Mana -= tier;
                        var speed = (this.World.Cam.Target - this.World.Cam.Pos) / 4;// * ( -4 + tier );
                        var proj = new PlayerFireball(this.World)
                        {
                            Position = new Vector3(this.Position.X, 0, this.Position.Z),
                            Speed = new Vector2(speed.X, speed.Z),
                            Level = tier - 1
                        };
                        this.World.Projectiles.Add(proj);
                        Game1.playSound("fireball");
                    }
                    else
                    {
                        Game1.playSound("steam");
                    }
                    this.ChargeTime = 0;
                }
            }

            foreach (var proj in this.World.Projectiles)
            {
                if (proj.Dead)
                    continue;

                if ((proj.BoundingBox + new Vector2(proj.Position.X, proj.Position.Z)).Intersects(this.BoundingBox + new Vector2(this.Position.X, this.Position.Z)) && proj.HurtsPlayer)
                {
                    proj.Trigger(this);
                }
            }

            if (this.World.Warp != null && (this.BoundingBox + new Vector2(this.Position.X, this.Position.Z)).Intersects(new RectangleF(this.World.Warp.Position.X, this.World.Warp.Position.Z, 1, 1)))
            {
                Game1.playSound("wand");
                this.World.QueueNextLevel();
            }

        }

        public override void RenderOver(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.RenderOver(device, projection, cam);

            BaseObject.Effect.TextureEnabled = true;
            BaseObject.Effect.Texture = this.StaffTex;
            var lookVec = this.World.Cam.Target - this.Position;
            var lookSide = new Vector3((float)Math.Cos(this.Look + (float)(Math.PI / 2)), 0, (float)Math.Sin(this.Look + (float)(Math.PI / 2)));
            var staffOffset = new Vector2(0.45f, -1.15f);
            staffOffset += new Vector2(this.ChargeTime * -0.25f, this.ChargeTime * 0.25f);
            if (this.ChargeTime >= 1)
            {
                staffOffset.X += (float)this.ShakeRand.NextDouble() * 0.1f - 0.05f;
                staffOffset.Y += (float)this.ShakeRand.NextDouble() * 0.1f - 0.05f;
            }
            BaseObject.Effect.World = Matrix.CreateRotationZ((float)(Math.PI / 2)) *
                                      Matrix.CreateRotationY(-this.Look + (float)(Math.PI / 2)) *
                                      Matrix.CreateTranslation(this.Position + lookVec + new Vector3(lookSide.X * staffOffset.X, staffOffset.Y, lookSide.Z * staffOffset.X));
            foreach (EffectPass pass in BaseObject.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(this.StaffBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }

        public override void RenderUi(SpriteBatch b)
        {
            b.Begin();

            for (int i = 0; i < this.HealthMax; ++i)
            {
                b.Draw(Game1.objectSpriteSheet, new Vector2(2 + i * 12, 2), new Rectangle(288, 608, 16, 16), (i + 1) <= this.Health ? Color.White : Color.Gray, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            }

            b.Draw(Game1.staminaRect, new Rectangle(4, 18, this.ManaMax + 2, 6), Color.MidnightBlue);
            if (this.Mana > 0)
                b.Draw(Game1.staminaRect, new Rectangle(5, 19, this.Mana, 4), Color.MediumBlue);

            b.Draw(Game1.staminaRect, new Rectangle(4, this.World.ScreenSize - 10, this.World.ScreenSize - 8, 6), Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(5, this.World.ScreenSize - 9, this.World.ScreenSize - 10, 4), Color.Black);
            if (this.ChargeTime > 0)
            {
                var col = Color.Khaki;
                if (this.ChargeTime >= 1)
                    col = Color.Red;
                else if (this.ChargeTime >= 0.75)
                    col = Color.OrangeRed;
                else if (this.ChargeTime >= 0.5)
                    col = Color.Orange;
                else if (this.ChargeTime >= 0.25)
                    col = Color.Yellow;
                b.Draw(Game1.staminaRect, new Rectangle(5, this.World.ScreenSize - 9, (int)((this.World.ScreenSize - 10) * this.ChargeTime), 4), col);
            }
            b.Draw(Game1.staminaRect, new Rectangle((int)(5 + (this.World.ScreenSize - 10) * 0.25f), this.World.ScreenSize - 9, 1, 4), Color.Gray);
            b.Draw(Game1.staminaRect, new Rectangle((int)(5 + (this.World.ScreenSize - 10) * 0.5f), this.World.ScreenSize - 9, 1, 4), Color.Gray);
            b.Draw(Game1.staminaRect, new Rectangle((int)(5 + (this.World.ScreenSize - 10) * 0.75f), this.World.ScreenSize - 9, 1, 4), Color.Gray);

            b.End();
        }

        public override void Dispose()
        {
            base.Dispose();

            this.StaffBuffer?.Dispose();
        }
    }
}
