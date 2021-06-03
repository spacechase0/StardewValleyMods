using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;

namespace CustomCritters
{
    public class CustomCritter : Critter
    {
        /// <summary>The light type IDs recognised by the game.</summary>
        /// <remarks>Setting an invalid light ID will crash the game. Valid IDs are based on <see cref="LightSource.loadTextureFromConstantValue"/>.</remarks>
        private readonly HashSet<int> validLightIds = new HashSet<int>(new[] { LightSource.lantern, LightSource.windowLight, LightSource.sconceLight, LightSource.cauldronLight, LightSource.indoorWindowLight });

        private CritterEntry data;
        private LightSource light;
        private Random rand;

        public CustomCritter( Vector2 pos, CritterEntry data )
        {
            this.position = this.startingPosition = pos;
            this.data = data;
            this.rand = new Random(((int)startingPosition.X) << 32 | ((int)startingPosition.Y));

            var tex = Mod.instance.Helper.Content.Load<Texture2D>("Critters/" + data.Id + "/critter.png");
            var texStr = Mod.instance.Helper.Content.GetActualAssetKey($"Critters/{data.Id}/critter.png");

            this.baseFrame = Game1.random.Next(data.SpriteData.Variations) * (tex.Width / data.SpriteData.FrameWidth);
            
            List <FarmerSprite.AnimationFrame> frames = new List<FarmerSprite.AnimationFrame>();
            foreach ( var frame in data.Animations[ "default" ].Frames )
            {
                frames.Add(new FarmerSprite.AnimationFrame(baseFrame + frame.Frame, frame.Duration));
            }
            this.sprite = new AnimatedSprite(texStr, baseFrame, data.SpriteData.FrameWidth, data.SpriteData.FrameHeight);
            sprite.setCurrentAnimation(frames);
            
            if ( data.Light != null )
            {
                var col = new Color(255 - data.Light.Color.R, 255 - data.Light.Color.G, 255 - data.Light.Color.B);
                light = this.validLightIds.Contains(data.Light.VanillaLightId) 
                    ? new LightSource(data.Light.VanillaLightId, position, data.Light.Radius, col) 
                    : new LightSource(LightSource.sconceLight, position, data.Light.Radius, col);
                Game1.currentLightSources.Add(light);
            }
        }
        
        private int patrolIndex = 0;
        private int patrolWait = 0;
        private bool needTarget = true;
        private bool waiting = false;
        private Vector2 target;
        public override bool update(GameTime time, GameLocation environment)
        {
            if (data == null)
                return false;

            if ( data.Behavior != null )
            {
                switch ( data.Behavior.Type )
                {
                    case "idle":
                        break;
                    
                    case "patrol":
                    case "random":
                        {
                            if (waiting)
                            {
                                if (patrolWait <= 0)
                                {
                                    needTarget = true;
                                    waiting = false;
                                }
                                else
                                    patrolWait -= time.ElapsedGameTime.Milliseconds;
                            }
                            else
                            {
                                if (needTarget)
                                {
                                    var pt = data.Behavior.PatrolPoints[data.Behavior.Type == "patrol" ? patrolIndex : Game1.random.Next(data.Behavior.PatrolPoints.Count)];

                                    target = startingPosition;
                                    if (pt.Type == "start") ; // We just did this
                                    else if (pt.Type == "startoffset")
                                        target += new Vector2(pt.X * Game1.tileSize, pt.Y * Game1.tileSize);
                                    else if (pt.Type == "offset")
                                        target = position + new Vector2(pt.X * Game1.tileSize, pt.Y * Game1.tileSize);
                                    else if (pt.Type == "startrandom")
                                        target += new Vector2((float)(rand.NextDouble() * pt.X - pt.X / 2f) * Game1.tileSize, (float)(rand.NextDouble() * pt.Y - pt.Y / 2f) * Game1.tileSize);
                                    else if (pt.Type == "random")
                                        target = position + new Vector2((float)(rand.NextDouble() * pt.X - pt.X / 2f) * Game1.tileSize, (float)(rand.NextDouble() * pt.Y - pt.Y / 2f) * Game1.tileSize);
                                    else if (pt.Type == "wait")
                                        ;
                                    else
                                        Log.warn("Bad patrol point type: " + pt.Type);

                                    needTarget = false;
                                }

                                var dist = Vector2.Distance(position, target);
                                if (dist <= data.Behavior.Speed)
                                {
                                    position = target;
                                    patrolWait = data.Behavior.PatrolPointDelay + Game1.random.Next(data.Behavior.PatrolPointDelayAddRandom);
                                    ++patrolIndex;
                                    if (patrolIndex >= data.Behavior.PatrolPoints.Count)
                                        patrolIndex = 0;
                                    waiting = true;
                                }
                                else
                                {
                                    var v = (target - position);
                                    Vector2 unit = (target - position) / dist;
                                    //Log.trace($"{v.X} {v.Y} {unit.X} {unit.Y}");
                                    position += unit * data.Behavior.Speed;
                                }
                            }
                        }
                        break;

                    default:
                        Log.warn("Bad custom critter behavior: " + data.Behavior.Type);
                        break;
                }
            }

            if (light != null)
                light.position.Value = this.position;

            return base.update(time, environment);
        }

        public override void draw(SpriteBatch b)
        {
            if (data == null)
                return;

            //base.draw(b);
            float z = (float)((double)this.position.Y / 10000.0 + (double)this.position.X / 100000.0);
            if (!data.SpriteData.Flying)
                z = (float)((this.position.Y - 1.0) / 10000.0);
            this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.position - new Vector2( 8, 8 )), z, 0, 0, Color.White, this.flip, data.SpriteData.Scale, 0.0f, false);
        }
    }
}
