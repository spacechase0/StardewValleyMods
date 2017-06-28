using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCritters
{
    public class CustomCritter : Critter
    {
        private CritterEntry data;

        public CustomCritter( Vector2 pos, CritterEntry data )
        {
            this.position = this.startingPosition = pos;
            this.startingPosition = position;
            this.data = data;

            var tex = Mod.instance.Helper.Content.Load<Texture2D>("Critters/" + data.Id + "/critter.png");

            this.baseFrame = Game1.random.Next(data.SpriteData.Variations) * (tex.Width / data.SpriteData.FrameWidth);
            
            List <FarmerSprite.AnimationFrame> frames = new List<FarmerSprite.AnimationFrame>();
            foreach ( var frame in data.Animations[ "default" ].Frames )
            {
                frames.Add(new FarmerSprite.AnimationFrame(baseFrame + frame.Frame, frame.Duration));
            }
            this.sprite = new AnimatedSprite(tex, baseFrame, data.SpriteData.FrameWidth, data.SpriteData.FrameHeight);
            sprite.setCurrentAnimation(frames);
        }
        
        private int patrolIndex = 0;
        private int patrolWait = 0;
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
                        {
                            if (patrolWait <= 0)
                            {
                                var pt = data.Behavior.PatrolPoints[patrolIndex];

                                Vector2 targ = startingPosition;
                                if (pt.Type == "start") ; // We just did this
                                else if ( pt.Type == "startoffset" )
                                    targ += new Vector2(pt.X * Game1.tileSize, pt.Y * Game1.tileSize);
                                else
                                    Log.warn("Bad patrol point type: " + pt.Type);

                                var dist = Vector2.Distance(position, targ);
                                if (dist <= data.Behavior.Speed)
                                {
                                    position = targ;
                                    patrolWait = data.Behavior.PatrolPointDelay + Game1.random.Next( data.Behavior.PatrolPointDelayAddRandom );
                                    ++patrolIndex;
                                    if (patrolIndex >= data.Behavior.PatrolPoints.Count)
                                        patrolIndex = 0;
                                }
                                else
                                {
                                    var v = (targ - position);
                                    Vector2 unit = ( targ - position ) / dist;
                                    //Log.trace($"{v.X} {v.Y} {unit.X} {unit.Y}");
                                    position += unit * data.Behavior.Speed;
                                }
                            }
                            else patrolWait -= time.ElapsedGameTime.Milliseconds;
                        }
                        break;

                    default:
                        Log.warn("Bad custom critter behavior: " + data.Behavior.Type);
                        break;
                }
            }
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
