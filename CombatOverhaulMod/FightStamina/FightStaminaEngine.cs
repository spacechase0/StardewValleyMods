using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace CombatOverhaulMod.FightStamina
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "<Pending>")]
    internal class FightStaminaEngine
    {
        private BasicEffect effect;

        public static float regenTimer = 0;

        public FightStaminaEngine()
        {
            effect = new BasicEffect(Game1.graphics.GraphicsDevice);
            effect.World = Matrix.Identity;
            effect.View = Matrix.CreateLookAt(
                new Vector3(0, 0, -1),
                new Vector3(0, 0, 0),
                new Vector3(0, -1, 0));
            effect.Projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateOrthographicOffCenter(0, Game1.uiViewport.Width, Game1.uiViewport.Height, 0, 0, 1);
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;

            Mod.instance.Helper.Events.Input.ButtonReleased += this.Input_ButtonReleased;
            Mod.instance.Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            Mod.instance.Helper.Events.Display.RenderedHud += this.Display_RenderedHud;
            Mod.instance.Ready += OnReady;

            Mod.instance.Helper.ConsoleCommands.Add("player_setfightstamina", "Set current fight stamina for the player.", OnSetFightStamina);
            Mod.instance.Helper.ConsoleCommands.Add("player_setmaxfightstamina", "Set max fight stamina (default 1) for the player.", OnSetMaxFightStamina);
        }

        private int dashDir = -1;
        private float dashTimer = 0;
        private void Input_ButtonReleased(object sender, StardewModdingAPI.Events.ButtonReleasedEventArgs e)
        {
            int dir = -1;
            if (e.Button == Game1.options.moveLeftButton[0].ToSButton())
                dir = Game1.left;
            else if (e.Button == Game1.options.moveRightButton[0].ToSButton())
                dir = Game1.right;
            else if (e.Button == Game1.options.moveUpButton[0].ToSButton())
                dir = Game1.up;
            else if (e.Button == Game1.options.moveDownButton[0].ToSButton())
                dir = Game1.down;

            if (dir == -1)
                return;

            if (dashDir != dir || dashTimer <= 0)
            {
                dashDir = dir;
                dashTimer = 0.375f;
            }
            else if ( dashTimer > 0 && Game1.player.GetFightStamina() > 0.25f )
            {
                Vector2 velChange = Vector2.Zero;
                switch (dashDir)
                {
                    case Game1.left : velChange.X = -1; break;
                    case Game1.right: velChange.X =  1; break;
                    case Game1.up   : velChange.Y =  1; break;
                    case Game1.down : velChange.Y = -1; break;
                }

                velChange *= 12;
                Game1.player.xVelocity += velChange.X;
                Game1.player.yVelocity += velChange.Y;

                Game1.player.AddFightStamina(-0.25f);
                regenTimer = 1f;
                Game1.player.currentLocation.playSound("daggerswipe", Game1.player.Tile);
            }
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !Game1.shouldTimePass())
                return;

            dashTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;

            if (regenTimer > 0)
            {
                regenTimer -= (float) Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                return;
            }

            Game1.player.AddFightStamina(1 / 3f * (float) Game1.currentGameTime.ElapsedGameTime.TotalSeconds);
        }

        private void Display_RenderedHud(object sender, StardewModdingAPI.Events.RenderedHudEventArgs e)
        {
            float stam = Game1.player.GetFightStamina();

            effect.Projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateOrthographicOffCenter(0, Game1.uiViewport.Width, Game1.uiViewport.Height, 0, 0, 1);

            Vector2 origin = new Vector2(
                Game1.uiViewport.Width / 2 - 384 - 16 - 128 + 48,
                Game1.onScreenMenus.First(m => m is Toolbar).yPositionOnScreen - 48 - 8
            );

            const int Slices = 32;

            for (int i = 0; i < Math.Ceiling(stam); ++i)
            {
                float circStam = Math.Min(1, stam - i);

                List<Vector2> vs = new();
                for (int iv = 0; iv < Slices; ++iv)
                {
                    float circSpot = MathF.PI * 2 / Slices * iv;
                    float nextSpot = MathF.PI * 2 / Slices * ( iv + 1 );
                    bool stop = false;
                    if ((iv + 1f) / Slices > circStam)
                    {
                        nextSpot = MathF.PI * 2 * circStam;
                        stop = true;
                    }
                    circSpot -= 3.14f / 2;
                    nextSpot -= 3.14f / 2;

                    float minRad = 16 * i + 8;
                    float maxRad = 16 * i + 8 + 12;

                    vs.Add(new Vector2(MathF.Cos(circSpot) * minRad, MathF.Sin(circSpot) * minRad));
                    vs.Add(new Vector2(MathF.Cos(circSpot) * maxRad, MathF.Sin(circSpot) * maxRad));
                    vs.Add(new Vector2(MathF.Cos(nextSpot) * maxRad, MathF.Sin(nextSpot) * maxRad));
                    vs.Add(new Vector2(MathF.Cos(nextSpot) * maxRad, MathF.Sin(nextSpot) * maxRad));
                    vs.Add(new Vector2(MathF.Cos(nextSpot) * minRad, MathF.Sin(nextSpot) * minRad));
                    vs.Add(new Vector2(MathF.Cos(circSpot) * minRad, MathF.Sin(circSpot) * minRad));

                    if (stop)
                        break;
                }

                var vpcs = new VertexPositionColor[ vs.Count ];
                for (int iv = 0; iv < vs.Count; ++iv)
                {
                    vpcs[iv] = new(new Vector3(origin.X + vs[iv].X, origin.Y + vs[iv].Y, 0), Color.LimeGreen);
                }

                var r = Game1.graphics.GraphicsDevice.RasterizerState;
                Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Game1.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vpcs, 0, vpcs.Length / 3);
                }
                Game1.graphics.GraphicsDevice.RasterizerState = r;
            }
        }

        private void OnReady(object sender, EventArgs args)
        {
            Farmer_FightStamina.Register();
        }

        private void OnSetFightStamina(string cmd, string[] args)
        {
            if (args.Length < 1)
            {
                Log.Info("Proper usage: player_setfightstamina <amt>");
                return;
            }

            float val = 0;
            if (!float.TryParse(args[0], out val))
            {
                Log.Info("Amount must be an float");
                return;
            }

            Game1.player.SetFightStamina(val);
        }

        private void OnSetMaxFightStamina(string cmd, string[] args)
        {
            if (args.Length < 1)
            {
                Log.Info("Proper usage: player_setmaxfightstamina <amt>");
                return;
            }

            float val = 0;
            if (!float.TryParse(args[0], out val))
            {
                Log.Info("Amount must be an float");
                return;
            }

            Game1.player.SetMaxFightStamina(val);
        }
    }
}
