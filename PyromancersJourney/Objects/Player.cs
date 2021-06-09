using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PyromancersJourney.Projectiles;
using StardewValley;

namespace PyromancersJourney.Objects
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

        public Player(World world)
            : base(world)
        {
            this.Health = this.healthMax;

            this.staffTex = Mod.instance.Helper.Content.Load<Texture2D>("assets/staff.png");

            List<VertexPositionColorTexture> staffVerts = new List<VertexPositionColorTexture>();
            staffVerts.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0)));
            staffVerts.Add(new VertexPositionColorTexture(new Vector3(1, 0, 0), Color.White, new Vector2(1, 0)));
            staffVerts.Add(new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 1)));

            staffVerts.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0)));
            staffVerts.Add(new VertexPositionColorTexture(new Vector3(0, 1, 0), Color.White, new Vector2(0, 1)));
            staffVerts.Add(new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 1)));

            this.staffBuffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), 6, BufferUsage.WriteOnly);
            this.staffBuffer.SetData(staffVerts.ToArray());
        }

        public override void Hurt(int amt)
        {
            if (this.immunTimer > 0)
                return;

            base.Hurt(amt);
            this.immunTimer = 1;
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
                this.look -= this.turnSpeed;
            }
            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.D))
            {
                this.look += this.turnSpeed;
            }
            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.Q))
            {
                this.Position.X += (float)Math.Cos(this.look - Math.PI / 2) * this.speed;
                this.Position.Z += (float)Math.Sin(this.look - Math.PI / 2) * this.speed;
            }
            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.E))
            {
                this.Position.X += (float)Math.Cos(this.look + Math.PI / 2) * this.speed;
                this.Position.Z += (float)Math.Sin(this.look + Math.PI / 2) * this.speed;
            }
            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.W))
            {
                this.Position.X += (float)Math.Cos(this.look) * this.speed;
                this.Position.Z += (float)Math.Sin(this.look) * this.speed;
            }
            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.S))
            {
                this.Position.X -= (float)Math.Cos(this.look) * this.speed;
                this.Position.Z -= (float)Math.Sin(this.look) * this.speed;
            }

            this.World.cam.pos = this.Position;
            this.World.cam.target = this.Position + new Vector3((float)Math.Cos(this.look), 0, (float)Math.Sin(this.look));
            this.World.cam.up = Vector3.Up;
        }

        public override void Update()
        {
            base.Update();

            this.manaRegenTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            while (this.manaRegenTimer >= 1)
            {
                this.manaRegenTimer -= 1;
                if (++this.mana > this.manaMax)
                    this.mana = this.manaMax;
            }

            if (this.immunTimer > 0)
            {
                this.immunTimer = Math.Max(0, this.immunTimer - (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds);
            }

            if (Game1.input.GetKeyboardState().IsKeyDown(Keys.Space))
            {
                this.chargeTime += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                if (this.chargeTime >= 1)
                    this.chargeTime = 1;
            }
            else
            {
                if (this.chargeTime > 0)
                {
                    int tier = (int)(this.chargeTime / 0.25f);
                    if (tier >= 1 && this.mana > tier)
                    {
                        this.mana -= tier;
                        var speed = (this.World.cam.target - this.World.cam.pos) / 4;// * ( -4 + tier );
                        var proj = new PlayerFireball(this.World)
                        {
                            Position = new Vector3(this.Position.X, 0, this.Position.Z),
                            Speed = new Vector2(speed.X, speed.Z),
                            Level = tier - 1,
                        };
                        this.World.projectiles.Add(proj);
                        Game1.playSound("fireball");
                    }
                    else
                    {
                        Game1.playSound("steam");
                    }
                    this.chargeTime = 0;
                }
            }

            foreach (var proj in this.World.projectiles)
            {
                if (proj.Dead)
                    continue;

                if ((proj.BoundingBox + new Vector2(proj.Position.X, proj.Position.Z)).Intersects(this.BoundingBox + new Vector2(this.Position.X, this.Position.Z)) && proj.HurtsPlayer)
                {
                    proj.Trigger(this);
                }
            }

            if (this.World.warp != null && (this.BoundingBox + new Vector2(this.Position.X, this.Position.Z)).Intersects(new RectangleF(this.World.warp.Position.X, this.World.warp.Position.Z, 1, 1)))
            {
                Game1.playSound("wand");
                this.World.QueueNextLevel();
            }

        }

        public override void RenderOver(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.RenderOver(device, projection, cam);

            effect.TextureEnabled = true;
            effect.Texture = this.staffTex;
            var lookVec = this.World.cam.target - this.Position;
            var lookSide = new Vector3((float)Math.Cos(this.look + (float)(Math.PI / 2)), 0, (float)Math.Sin(this.look + (float)(Math.PI / 2)));
            var staffOffset = new Vector2(0.45f, -1.15f);
            staffOffset += new Vector2(this.chargeTime * -0.25f, this.chargeTime * 0.25f);
            if (this.chargeTime >= 1)
            {
                staffOffset.X += (float)this.shakeRand.NextDouble() * 0.1f - 0.05f;
                staffOffset.Y += (float)this.shakeRand.NextDouble() * 0.1f - 0.05f;
            }
            effect.World = Matrix.CreateRotationZ((float)(Math.PI / 2)) *
                           Matrix.CreateRotationY(-this.look + (float)(Math.PI / 2)) *
                           Matrix.CreateTranslation(this.Position + lookVec + new Vector3(lookSide.X * staffOffset.X, staffOffset.Y, lookSide.Z * staffOffset.X));
            for (int e = 0; e < effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = effect.CurrentTechnique.Passes[e];
                pass.Apply();
                device.SetVertexBuffer(this.staffBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }

        public override void RenderUi(SpriteBatch b)
        {
            b.Begin();

            for (int i = 0; i < this.healthMax; ++i)
            {
                b.Draw(Game1.objectSpriteSheet, new Vector2(2 + i * 12, 2), new Rectangle(288, 608, 16, 16), (i + 1) <= this.Health ? Color.White : Color.Gray, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            }

            b.Draw(Game1.staminaRect, new Rectangle(4, 18, this.manaMax + 2, 6), Color.MidnightBlue);
            if (this.mana > 0)
                b.Draw(Game1.staminaRect, new Rectangle(5, 19, this.mana, 4), Color.MediumBlue);

            b.Draw(Game1.staminaRect, new Rectangle(4, this.World.ScreenSize - 10, this.World.ScreenSize - 8, 6), Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(5, this.World.ScreenSize - 9, this.World.ScreenSize - 10, 4), Color.Black);
            if (this.chargeTime > 0)
            {
                var col = Color.Khaki;
                if (this.chargeTime >= 1)
                    col = Color.Red;
                else if (this.chargeTime >= 0.75)
                    col = Color.OrangeRed;
                else if (this.chargeTime >= 0.5)
                    col = Color.Orange;
                else if (this.chargeTime >= 0.25)
                    col = Color.Yellow;
                b.Draw(Game1.staminaRect, new Rectangle(5, this.World.ScreenSize - 9, (int)((this.World.ScreenSize - 10) * this.chargeTime), 4), col);
            }
            b.Draw(Game1.staminaRect, new Rectangle((int)(5 + (this.World.ScreenSize - 10) * 0.25f), this.World.ScreenSize - 9, 1, 4), Color.Gray);
            b.Draw(Game1.staminaRect, new Rectangle((int)(5 + (this.World.ScreenSize - 10) * 0.5f), this.World.ScreenSize - 9, 1, 4), Color.Gray);
            b.Draw(Game1.staminaRect, new Rectangle((int)(5 + (this.World.ScreenSize - 10) * 0.75f), this.World.ScreenSize - 9, 1, 4), Color.Gray);

            b.End();
        }
    }
}
