using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyromancersJourney.Framework.Objects;
using PyromancersJourney.Framework.Projectiles;
using SpaceShared;
using StardewValley;
using SObject = StardewValley.Object;

namespace PyromancersJourney.Framework
{
    internal class World : IDisposable
    {
        public static readonly int Scale = 4;

        public Player Player;
        public LevelWarp Warp;
        public Camera Cam = new();
        private readonly Matrix Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70), Game1.game1.GraphicsDevice.DisplayMode.AspectRatio, 0.01f, 100);

        private readonly RenderTarget2D Target;
        private readonly SpriteBatch SpriteBatch;

        private bool NextLevelQueued;
        private int CurrLevel;
        private Vector2 WarpPos;
        public Map Map;
        public List<BaseObject> Objects = new();
        public List<BaseProjectile> Projectiles = new();
        private readonly List<BaseObject> QueuedObjects = new();

        public int ScreenSize => this.Target.Width;

        public bool HasQuit;

        public World()
        {
            this.Target = new RenderTarget2D(Game1.game1.GraphicsDevice, 500 / World.Scale, 500 / World.Scale, false, Game1.game1.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            this.SpriteBatch = new SpriteBatch(Game1.game1.GraphicsDevice);

            this.InitLevel("0");

            Game1.changeMusicTrack("VolcanoMines", track_interruptable: false, music_context: StardewValley.GameData.MusicContext.MiniGame);
        }

        public void Quit()
        {
            this.HasQuit = true;
            Game1.changeMusicTrack("none");
        }

        public void QueueObject(BaseObject obj)
        {
            this.QueuedObjects.Add(obj);
        }

        public void Update()
        {
            if (this.NextLevelQueued)
            {
                this.NextLevelQueued = false;
                this.NextLevel();
            }

            foreach (var obj in this.Objects)
            {
                obj.Update();
            }
            foreach (var proj in this.Projectiles)
            {
                proj.Update();
            }

            for (int i = this.Objects.Count - 1; i >= 0; --i)
            {
                if (this.Objects[i].Dead)
                {
                    this.Objects[i].Dispose();
                    this.Objects.RemoveAt(i);
                }
            }

            for (int i = this.Projectiles.Count - 1; i >= 0; --i)
            {
                if (this.Projectiles[i].Dead)
                    this.Projectiles.RemoveAt(i);
            }

            foreach (var obj in this.QueuedObjects)
            {
                this.Objects.Add(obj);
            }
            this.QueuedObjects.Clear();

            if (!this.Objects.OfType<Enemy>().Any())
            {
                if (this.Warp == null)
                {
                    this.Map.Floor[(int)this.WarpPos.X, (int)this.WarpPos.Y] = FloorTile.Stone;
                    this.Objects.Add(this.Warp = new LevelWarp(this) { Position = new Vector3(this.WarpPos.X, 0, this.WarpPos.Y) });
                    Game1.playSound("detector");
                }
            }

            /*
            camAngle = ( camAngle + 0.025f ) % 360;
            cam.pos = baseCamPos + new Vector3( ( float ) Math.Cos( camAngle ) * map.Size.X, 0, (float) Math.Sin( camAngle ) * map.Size.Y );
            cam.target = baseCamPos;
            */
        }

        public void Render()
        {
            var device = Game1.game1.GraphicsDevice;
            var oldTargets = device.GetRenderTargets();
            device.SetRenderTarget(this.Target);
            var oldDepth = device.DepthStencilState;
            device.DepthStencilState = new DepthStencilState { DepthBufferEnable = true };
            device.Clear(this.Map.Sky);

            var oldRast = device.RasterizerState;
            device.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };
            var oldSample = device.SamplerStates[0];
            device.SamplerStates[0] = SamplerState.PointClamp;
            {
                /*
                cam.pos = new Vector3( 4.5f, 4.5f, 4.5f );
                cam.target = new Vector3( 4.5f, 0, 4.5f );
                cam.up = new Vector3( 0, 0, 1 );
                //*/

                foreach (var obj in this.Objects)
                {
                    obj.Render(device, this.Projection, this.Cam);
                }
                foreach (var proj in this.Projectiles)
                {
                    proj.Render(device, this.Projection, this.Cam);
                }
            }
            {
                var oldStencilState = device.DepthStencilState;
                device.DepthStencilState = new DepthStencilState
                {
                    DepthBufferFunction = CompareFunction.Always
                };
                {
                    foreach (var obj in this.Objects)
                        obj.RenderOver(device, this.Projection, this.Cam);
                }
                device.DepthStencilState = oldStencilState;
            }
            device.SamplerStates[0] = oldSample;
            device.RasterizerState = oldRast;
            device.SetVertexBuffer(null);
            device.DepthStencilState = oldDepth;

            foreach (var obj in this.Objects)
            {
                obj.RenderUi(this.SpriteBatch);
            }

            device.SetRenderTargets(oldTargets);
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            //var oldTarget = oldTargets[0].RenderTarget as RenderTarget2D;
            Game1.spriteBatch.Draw(this.Target, new Vector2((Game1.graphics.PreferredBackBufferWidth - 500) / 2, (Game1.graphics.PreferredBackBufferHeight - 500) / 2), null, Color.White, 0, Vector2.Zero, World.Scale, SpriteEffects.None, 1);
            Game1.spriteBatch.End();
        }

        public void QueueNextLevel()
        {
            this.NextLevelQueued = true;
        }

        private void NextLevel()
        {
            switch (++this.CurrLevel)
            {
                case 1: this.InitLevel("1"); break;
                case 2: this.InitLevel("2"); break;
                case 3: this.InitLevel("boss"); break;
                case 4: this.InitLevel("ending"); break;
                case 5:
                    this.Quit();
                    Game1.drawObjectDialogue("You won!");
                    if (!Game1.player.hasOrWillReceiveMail("BeatPyromancersJourney"))
                    {
                        Game1.player.mailReceived.Add("BeatPyromancersJourney");
                        Game1.player.addItemByMenuIfNecessaryElseHoldUp(new SObject("848", 25));
                    }
                    break;
            }
        }

        [SuppressMessage("Reliability", "CA2000", Justification = DiagnosticMessages.DisposableOutlivesScope)]
        private void InitLevel(string path)
        {
            string[] lines = File.ReadAllLines(Path.Combine(Mod.Instance.Helper.DirectoryPath, "assets", "levels", path + ".txt"));

            string[] toks = lines[0].Split(' ');

            this.Warp = null;

            foreach (var obj in this.Objects)
                obj.Dispose();

            this.Objects.Clear();
            this.Projectiles.Clear();

            Vector2 playerPos = Vector2.Zero;

            this.Map = new Map(new Vector2(int.Parse(toks[0]), int.Parse(toks[1])));
            if (toks.Length > 2 && toks[2] == "sky")
            {
                this.Map.Sky = Color.SkyBlue;
            }
            for (int i = 1; i <= this.Map.Size.Y; ++i)
            {
                int iy = i - 1;
                for (int ix = 0; ix < this.Map.Size.X; ++ix)
                {
                    this.Map.Floor[ix, iy] = FloorTile.Stone;
                    this.Map.Walls[ix, iy] = WallTile.Empty;
                    switch (lines[i][ix])
                    {
                        case ' ': break;
                        case '#':
                        case 'M':
                            this.Map.Walls[ix, iy] = WallTile.Stone;
                            if (lines[i][ix] == 'M')
                            {
                                this.Objects.Add(new MuralThing(this) { Position = new Vector3(ix, 0, iy - 0.01f) });
                            }
                            break;
                        case 'L':
                            this.Map.Floor[ix, iy] = FloorTile.Lava;
                            break;
                        case 'F':
                            // TODO: Forge
                            break;
                        case 'P': playerPos = new Vector2(ix, iy); break;
                        case 's': this.Objects.Add(new TigerSlimeEnemy(this) { Position = new Vector3(ix + 0.5f, 0, iy + 0.5f) }); break;
                        case 'b': this.Objects.Add(new BatEnemy(this) { Position = new Vector3(ix + 0.5f, 0.5f, iy + 0.5f) }); break;
                        case 'W':
                        case 'G':
                            this.WarpPos = new Vector2(ix, iy);
                            if (lines[i][ix] == 'G')
                            {
                                this.Map.Floor[ix, iy] = FloorTile.Lava;
                                this.Objects.Add(new GolemEnemy(this) { Position = new Vector3(ix + 0.5f, -0.65f, iy + 0.5f) });
                            }
                            break;

                        default:
                            Log.Warn("Unknown tile type " + lines[i][ix] + "!");
                            break;
                    }
                }
            }


            this.Objects.Insert(0, new Floor(this, path == "ending"));
            this.Objects.Insert(1, new Walls(this, path == "ending"));
            this.Objects.Add(this.Player = new Player(this) { Position = new Vector3(playerPos.X, 0.5f, playerPos.Y) });

            if (path is "0" or "ending")
            {
                this.Objects.Add(this.Warp = new LevelWarp(this) { Position = new Vector3(this.WarpPos.X, 0, this.WarpPos.Y) });
            }
        }

        public void Dispose()
        {
            foreach (var obj in this.Objects)
                obj.Dispose();

            foreach (var obj in this.QueuedObjects)
                obj.Dispose();

            this.Objects.Clear();
            this.QueuedObjects.Clear();

            this.Player.Dispose();
            this.Warp?.Dispose();
        }
    }
}
