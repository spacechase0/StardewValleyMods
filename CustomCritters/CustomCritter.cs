using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace CustomCritters
{
    public class CustomCritter : Critter
    {
        /// <summary>The light type IDs recognised by the game.</summary>
        /// <remarks>Setting an invalid light ID will crash the game. Valid IDs are based on <see cref="LightSource.loadTextureFromConstantValue"/>.</remarks>
        private readonly HashSet<int> validLightIds = new(new[] { LightSource.lantern, LightSource.windowLight, LightSource.sconceLight, LightSource.cauldronLight, LightSource.indoorWindowLight });

        private CritterEntry data;
        private LightSource light;
        private Random rand;

        public CustomCritter(Vector2 pos, CritterEntry data)
        {
            this.position = this.startingPosition = pos;
            this.data = data;
            this.rand = new Random(((int)this.startingPosition.X) << 32 | ((int)this.startingPosition.Y));

            var tex = Mod.instance.Helper.Content.Load<Texture2D>("Critters/" + data.Id + "/critter.png");
            string texStr = Mod.instance.Helper.Content.GetActualAssetKey($"Critters/{data.Id}/critter.png");

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
                this.light = this.validLightIds.Contains(data.Light.VanillaLightId)
                    ? new LightSource(data.Light.VanillaLightId, this.position, data.Light.Radius, col)
                    : new LightSource(LightSource.sconceLight, this.position, data.Light.Radius, col);
                Game1.currentLightSources.Add(this.light);
            }
        }

        private int patrolIndex;
        private int patrolWait;
        private bool needTarget = true;
        private bool waiting;
        private Vector2 target;
        public override bool update(GameTime time, GameLocation environment)
        {
            if (this.data == null)
                return false;

            if (this.data.Behavior != null)
            {
                switch (this.data.Behavior.Type)
                {
                    case "idle":
                        break;

                    case "patrol":
                    case "random":
                        {
                            if (this.waiting)
                            {
                                if (this.patrolWait <= 0)
                                {
                                    this.needTarget = true;
                                    this.waiting = false;
                                }
                                else
                                    this.patrolWait -= time.ElapsedGameTime.Milliseconds;
                            }
                            else
                            {
                                if (this.needTarget)
                                {
                                    var pt = this.data.Behavior.PatrolPoints[this.data.Behavior.Type == "patrol" ? this.patrolIndex : Game1.random.Next(this.data.Behavior.PatrolPoints.Count)];

                                    this.target = this.startingPosition;
                                    if (pt.Type == "start") ; // We just did this
                                    else if (pt.Type == "startoffset")
                                        this.target += new Vector2(pt.X * Game1.tileSize, pt.Y * Game1.tileSize);
                                    else if (pt.Type == "offset")
                                        this.target = this.position + new Vector2(pt.X * Game1.tileSize, pt.Y * Game1.tileSize);
                                    else if (pt.Type == "startrandom")
                                        this.target += new Vector2((float)(this.rand.NextDouble() * pt.X - pt.X / 2f) * Game1.tileSize, (float)(this.rand.NextDouble() * pt.Y - pt.Y / 2f) * Game1.tileSize);
                                    else if (pt.Type == "random")
                                        this.target = this.position + new Vector2((float)(this.rand.NextDouble() * pt.X - pt.X / 2f) * Game1.tileSize, (float)(this.rand.NextDouble() * pt.Y - pt.Y / 2f) * Game1.tileSize);
                                    else if (pt.Type == "wait")
                                        ;
                                    else
                                        Log.Warn("Bad patrol point type: " + pt.Type);

                                    this.needTarget = false;
                                }

                                float dist = Vector2.Distance(this.position, this.target);
                                if (dist <= this.data.Behavior.Speed)
                                {
                                    this.position = this.target;
                                    this.patrolWait = this.data.Behavior.PatrolPointDelay + Game1.random.Next(this.data.Behavior.PatrolPointDelayAddRandom);
                                    ++this.patrolIndex;
                                    if (this.patrolIndex >= this.data.Behavior.PatrolPoints.Count)
                                        this.patrolIndex = 0;
                                    this.waiting = true;
                                }
                                else
                                {
                                    var v = (this.target - this.position);
                                    Vector2 unit = (this.target - this.position) / dist;
                                    //Log.trace($"{v.X} {v.Y} {unit.X} {unit.Y}");
                                    this.position += unit * this.data.Behavior.Speed;
                                }
                            }
                        }
                        break;

                    default:
                        Log.Warn("Bad custom critter behavior: " + this.data.Behavior.Type);
                        break;
                }
            }

            if (this.light != null)
                this.light.position.Value = this.position;

            return base.update(time, environment);
        }

        public override void draw(SpriteBatch b)
        {
            if (this.data == null)
                return;

            //base.draw(b);
            float z = (float)(this.position.Y / 10000.0 + this.position.X / 100000.0);
            if (!this.data.SpriteData.Flying)
                z = (float)((this.position.Y - 1.0) / 10000.0);
            this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.position - new Vector2(8, 8)), z, 0, 0, Color.White, this.flip, this.data.SpriteData.Scale, 0.0f, false);
        }
    }
}
