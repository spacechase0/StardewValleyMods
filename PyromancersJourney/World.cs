using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyromancersJourney.Objects;
using PyromancersJourney.Projectiles;
using SpaceShared;
using StardewValley;
using SObject = StardewValley.Object;

namespace PyromancersJourney
{
    public class World
    {
        public static readonly int SCALE = 4;

        public Player player;
        public LevelWarp warp;
        public Camera cam = new Camera();
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70), Game1.game1.GraphicsDevice.DisplayMode.AspectRatio, 0.01f, 100);

        private RenderTarget2D target;
        private SpriteBatch spriteBatch;

        private bool nextLevelQueued = false;
        private int currLevel = 0;
        private Vector2 warpPos;
        public Map map;
        public List<BaseObject> objects = new List<BaseObject>();
        public List<BaseProjectile> projectiles = new List<BaseProjectile>();
        private List<BaseObject> queuedObjects = new List<BaseObject>();

        public int ScreenSize => this.target.Width;

        public bool HasQuit = false;

        public World()
        {
            this.target = new RenderTarget2D(Game1.game1.GraphicsDevice, 500 / World.SCALE, 500 / World.SCALE, false, Game1.game1.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            this.spriteBatch = new SpriteBatch(Game1.game1.GraphicsDevice);

            this.InitLevel("0");

            Game1.changeMusicTrack("VolcanoMines", track_interruptable: false, Game1.MusicContext.MiniGame);
        }

        public void Quit()
        {
            this.HasQuit = true;
            Game1.changeMusicTrack("none");
        }

        public void QueueObject(BaseObject obj)
        {
            this.queuedObjects.Add(obj);
        }

        private Vector3 baseCamPos = new Vector3(4.5f, 2, 4.5f);
        private float camAngle;
        public void Update()
        {
            if (this.nextLevelQueued)
            {
                this.nextLevelQueued = false;
                this.NextLevel();
            }

            foreach (var obj in this.objects)
            {
                obj.Update();
            }
            foreach (var proj in this.projectiles)
            {
                proj.Update();
            }

            for (int i = this.objects.Count - 1; i >= 0; --i)
            {
                if (this.objects[i].Dead)
                    this.objects.RemoveAt(i);
            }

            for (int i = this.projectiles.Count - 1; i >= 0; --i)
            {
                if (this.projectiles[i].Dead)
                    this.projectiles.RemoveAt(i);
            }

            foreach (var obj in this.queuedObjects)
            {
                this.objects.Add(obj);
            }
            this.queuedObjects.Clear();

            if (this.objects.OfType<Enemy>().Count() == 0)
            {
                if (this.warp == null)
                {
                    this.map.Floor[(int)this.warpPos.X, (int)this.warpPos.Y] = FloorTile.Stone;
                    this.objects.Add(this.warp = new LevelWarp(this) { Position = new Vector3(this.warpPos.X, 0, this.warpPos.Y) });
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
            device.SetRenderTarget(this.target);
            var oldDepth = device.DepthStencilState;
            device.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            device.Clear(this.map.Sky);

            RasterizerState rast = new RasterizerState();
            rast.CullMode = CullMode.None;
            var oldRast = device.RasterizerState;
            device.RasterizerState = rast;
            var oldSample = device.SamplerStates[0];
            device.SamplerStates[0] = SamplerState.PointClamp;
            {
                /*
                cam.pos = new Vector3( 4.5f, 4.5f, 4.5f );
                cam.target = new Vector3( 4.5f, 0, 4.5f );
                cam.up = new Vector3( 0, 0, 1 );
                //*/

                foreach (var obj in this.objects)
                {
                    obj.Render(device, this.projection, this.cam);
                }
                foreach (var proj in this.projectiles)
                {
                    proj.Render(device, this.projection, this.cam);
                }
            }
            {
                DepthStencilState depth2 = new DepthStencilState();
                depth2.DepthBufferFunction = CompareFunction.Always;
                var oldDepth2 = device.DepthStencilState;
                device.DepthStencilState = depth2;
                {
                    foreach (var obj in this.objects)
                    {
                        obj.RenderOver(device, this.projection, this.cam);
                    }
                }
                device.DepthStencilState = oldDepth2;
            }
            device.SamplerStates[0] = oldSample;
            device.RasterizerState = oldRast;
            device.SetVertexBuffer(null);
            device.DepthStencilState = oldDepth;

            foreach (var obj in this.objects)
            {
                obj.RenderUi(this.spriteBatch);
            }

            device.SetRenderTargets(oldTargets);
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            //var oldTarget = oldTargets[0].RenderTarget as RenderTarget2D;
            Game1.spriteBatch.Draw(this.target, new Vector2((Game1.graphics.PreferredBackBufferWidth - 500) / 2, (Game1.graphics.PreferredBackBufferHeight - 500) / 2), null, Color.White, 0, Vector2.Zero, World.SCALE, SpriteEffects.None, 1);
            Game1.spriteBatch.End();
        }

        public void QueueNextLevel()
        {
            this.nextLevelQueued = true;
        }

        private void NextLevel()
        {
            switch (++this.currLevel)
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
                        Game1.player.addItemByMenuIfNecessaryElseHoldUp(new SObject(848, 25));
                    }
                    break;
            }
        }

        private void InitLevel(string path)
        {
            string[] lines = File.ReadAllLines(Path.Combine(Mod.instance.Helper.DirectoryPath, "assets", "levels", path + ".txt"));

            string[] toks = lines[0].Split(' ');

            this.warp = null;
            this.objects.Clear();
            this.projectiles.Clear();

            Vector2 playerPos = Vector2.Zero;

            this.map = new Map(new Vector2(int.Parse(toks[0]), int.Parse(toks[1])));
            if (toks.Length > 2 && toks[2] == "sky")
            {
                this.map.Sky = Color.SkyBlue;
            }
            for (int i = 1; i <= this.map.Size.Y; ++i)
            {
                int iy = i - 1;
                for (int ix = 0; ix < this.map.Size.X; ++ix)
                {
                    this.map.Floor[ix, iy] = FloorTile.Stone;
                    this.map.Walls[ix, iy] = WallTile.Empty;
                    switch (lines[i][ix])
                    {
                        case ' ': break;
                        case '#':
                        case 'M':
                            this.map.Walls[ix, iy] = WallTile.Stone;
                            if (lines[i][ix] == 'M')
                            {
                                this.objects.Add(new MuralThing(this) { Position = new Vector3(ix, 0, iy - 0.01f) });
                            }
                            break;
                        case 'L':
                            this.map.Floor[ix, iy] = FloorTile.Lava;
                            break;
                        case 'F':
                            // TODO: Forge
                            break;
                        case 'P': playerPos = new Vector2(ix, iy); break;
                        case 's': this.objects.Add(new TigerSlimeEnemy(this) { Position = new Vector3(ix + 0.5f, 0, iy + 0.5f) }); break;
                        case 'b': this.objects.Add(new BatEnemy(this) { Position = new Vector3(ix + 0.5f, 0.5f, iy + 0.5f) }); break;
                        case 'W':
                        case 'G':
                            this.warpPos = new Vector2(ix, iy);
                            if (lines[i][ix] == 'G')
                            {
                                this.map.Floor[ix, iy] = FloorTile.Lava;
                                this.objects.Add(new GolemEnemy(this) { Position = new Vector3(ix + 0.5f, -0.65f, iy + 0.5f) });
                            }
                            break;

                        default:
                            Log.warn("Unknown tile type " + lines[i][ix] + "!");
                            break;
                    }
                }
            }


            this.objects.Insert(0, new Floor(this, path == "ending"));
            this.objects.Insert(1, new Walls(this, path == "ending"));
            this.objects.Add(this.player = new Player(this) { Position = new Vector3(playerPos.X, 0.5f, playerPos.Y) });

            if (path == "0" || path == "ending")
            {
                this.objects.Add(this.warp = new LevelWarp(this) { Position = new Vector3(this.warpPos.X, 0, this.warpPos.Y) });
            }
        }
    }
}
