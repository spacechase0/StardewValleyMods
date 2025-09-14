using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace CustomCritters.Framework
{
    internal class CustomCritter : Critter
    {
        /// <summary>The light type IDs recognised by the game.</summary>
        /// <remarks>Setting an invalid light ID will crash the game. Valid IDs are based on <see cref="LightSource.loadTextureFromConstantValue"/>.</remarks>
        private readonly HashSet<int> ValidLightIds = new(new[] { LightSource.lantern, LightSource.windowLight, LightSource.sconceLight, LightSource.cauldronLight, LightSource.indoorWindowLight });

        private readonly LightSource Light;
        private readonly Random Rand;

        public CritterEntry Data { get; }

        public CustomCritter(Vector2 pos, CritterEntry data)
        {
            this.position = this.startingPosition = pos;
            this.Data = data;
            this.Rand = new Random(((int)this.startingPosition.X) << 32 | ((int)this.startingPosition.Y));

            var tex = CustomCritter.LoadCritterTexture(data.Id);
            string texStr = Mod.Instance.Helper.ModContent.GetInternalAssetName($"Critters/{data.Id}/critter.png").BaseName;

            this.baseFrame = Game1.random.Next(data.SpriteData.Variations) * (tex.Width / data.SpriteData.FrameWidth);

            List<FarmerSprite.AnimationFrame> frames = new List<FarmerSprite.AnimationFrame>();
            foreach (var frame in data.Animations["default"].Frames)
            {
                frames.Add(new FarmerSprite.AnimationFrame(this.baseFrame + frame.Frame, frame.Duration));
            }
            this.sprite = new AnimatedSprite(texStr, this.baseFrame, data.SpriteData.FrameWidth, data.SpriteData.FrameHeight);
            this.sprite.setCurrentAnimation(frames);

            if (data.Light != null)
            {
                var col = new Color(255 - data.Light.Color.R, 255 - data.Light.Color.G, 255 - data.Light.Color.B);
                this.Light = this.ValidLightIds.Contains(data.Light.VanillaLightId)
                    ? new LightSource(data.Light.VanillaLightId, this.position, data.Light.Radius, col)
                    : new LightSource(LightSource.sconceLight, this.position, data.Light.Radius, col);
                Game1.currentLightSources.Add(this.Light);
            }
        }

        public static Texture2D LoadCritterTexture(string id)
        {
            return Mod.Instance.Helper.ModContent.Load<Texture2D>($"Critters/{id}/critter.png");
        }

        private int PatrolIndex;
        private int PatrolWait;
        private bool NeedTarget = true;
        private bool Waiting;
        private Vector2 Target;
        public override bool update(GameTime time, GameLocation environment)
        {
            if (this.Data == null)
                return false;

            if (this.Data.Behavior != null)
            {
                switch (this.Data.Behavior.Type)
                {
                    case "idle":
                        break;

                    case "patrol":
                    case "random":
                        {
                            if (this.Waiting)
                            {
                                if (this.PatrolWait <= 0)
                                {
                                    this.NeedTarget = true;
                                    this.Waiting = false;
                                }
                                else
                                    this.PatrolWait -= time.ElapsedGameTime.Milliseconds;
                            }
                            else
                            {
                                if (this.NeedTarget)
                                {
                                    var pt = this.Data.Behavior.PatrolPoints[this.Data.Behavior.Type == "patrol" ? this.PatrolIndex : Game1.random.Next(this.Data.Behavior.PatrolPoints.Count)];

                                    this.Target = this.startingPosition;
                                    switch (pt.Type)
                                    {
                                        case "start":
                                            break; // We just did this

                                        case "startoffset":
                                            this.Target += new Vector2(pt.X * Game1.tileSize, pt.Y * Game1.tileSize);
                                            break;

                                        case "offset":
                                            this.Target = this.position + new Vector2(pt.X * Game1.tileSize, pt.Y * Game1.tileSize);
                                            break;

                                        case "startrandom":
                                            this.Target += new Vector2((float)(this.Rand.NextDouble() * pt.X - pt.X / 2f) * Game1.tileSize, (float)(this.Rand.NextDouble() * pt.Y - pt.Y / 2f) * Game1.tileSize);
                                            break;

                                        case "random":
                                            this.Target = this.position + new Vector2((float)(this.Rand.NextDouble() * pt.X - pt.X / 2f) * Game1.tileSize, (float)(this.Rand.NextDouble() * pt.Y - pt.Y / 2f) * Game1.tileSize);
                                            break;

                                        case "wait":
                                            break;

                                        default:
                                            Log.Warn("Bad patrol point type: " + pt.Type);
                                            break;
                                    }

                                    this.NeedTarget = false;
                                }

                                float dist = Vector2.Distance(this.position, this.Target);
                                if (dist <= this.Data.Behavior.Speed)
                                {
                                    this.position = this.Target;
                                    this.PatrolWait = this.Data.Behavior.PatrolPointDelay + Game1.random.Next(this.Data.Behavior.PatrolPointDelayAddRandom);
                                    ++this.PatrolIndex;
                                    if (this.PatrolIndex >= this.Data.Behavior.PatrolPoints.Count)
                                        this.PatrolIndex = 0;
                                    this.Waiting = true;
                                }
                                else
                                {
                                    Vector2 unit = (this.Target - this.position) / dist;
                                    //Log.trace($"{v.X} {v.Y} {unit.X} {unit.Y}");
                                    this.position += unit * this.Data.Behavior.Speed;
                                }
                            }
                        }
                        break;

                    default:
                        Log.Warn("Bad custom critter behavior: " + this.Data.Behavior.Type);
                        break;
                }
            }

            if (this.Light != null)
                this.Light.position.Value = this.position;

            return base.update(time, environment);
        }

        public override void draw(SpriteBatch b)
        {
            if (this.Data == null)
                return;

            //base.draw(b);
            float z = (float)(this.position.Y / 10000.0 + this.position.X / 100000.0);
            if (!this.Data.SpriteData.Flying)
                z = (float)((this.position.Y - 1.0) / 10000.0);
            this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.position - new Vector2(8, 8)), z, 0, 0, Color.White, this.flip, this.Data.SpriteData.Scale);
        }
    }
}
