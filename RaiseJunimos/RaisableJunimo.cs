using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewValley;
using StardewValley.Network;

namespace RaiseJunimos
{
    [XmlType("Mods_spacechase0_RaisableJunimo")]
    public class RaisableJunimo : Character
    {
        public enum AgePhase
        {
            Baby,
            Child,
            Adult,
        }

        [XmlType( "Mods_spacechase0_RaisableJunimoStatData" )]
        public class StatData : INetObject< NetFields >
        {
            public const int StatCap = 65535;

            public NetFields NetFields { get; } = new();

            private readonly NetInt val = new(0);
            private readonly NetFloat disp = new(1);

            public int Value { get { return val.Value; } set { val.Value = value; } }
            public float Disposition { get { return disp.Value; } set { disp.Value = value; } }

            public StatData()
            {
                NetFields.AddFields(val, disp);
            }
        }
        public const int LifetimeInDays = 28 * 4; // A year

        private readonly NetString name = new("Junimo");
        private readonly NetInt age = new(0);
        public string Name { get { return name.Value; } set { name.Value = value; } }
        public int Age { get { return age.Value; } set { age.Value = value; } }

        public AgePhase GetCurrentAgePhase()
        {
            if (Age < 28) return AgePhase.Baby;
            if (Age < 28 + 28 * 2) return AgePhase.Child;
            return AgePhase.Adult;
        }

        public readonly NetRef<StatData> staminaStat = new( new StatData() );
        public readonly NetRef<StatData> speedStat = new( new StatData() );
        public readonly NetRef<StatData> strengthStat = new( new StatData() );
        public readonly NetRef<StatData> mindStat = new( new StatData() );
        public readonly NetRef<StatData> magicStat = new( new StatData() );
        [XmlIgnore]
        public StatData StaminaStat => staminaStat.Value;
        [XmlIgnore]
        public StatData SpeedStat => speedStat.Value;
        [XmlIgnore]
        public StatData StrengthStat => strengthStat.Value;
        [XmlIgnore]
        public StatData MindStat => mindStat.Value;
        [XmlIgnore]
        public StatData MagicStat => magicStat.Value;

        private readonly NetInt happiness = new(300);
        public int Happiness { get { return happiness.Value; } set { happiness.Value = value; } }

        
        private readonly NetStringDictionary<int, NetInt> skills = new();
        public IDictionary<string, int> Skills => (IDictionary<string, int>) skills;

        [XmlIgnore]
        public Color Color
        {
            get
            {
                Vector2 towards = Vector2.Zero;
                towards += new Vector2( MathF.Cos( 0 * MathF.PI / 180 ), MathF.Sin(0 * MathF.PI / 180) )
                           * ( StrengthStat.Value / (float) StatData.StatCap );
                towards += new Vector2( MathF.Cos( 120 * MathF.PI / 180 ), MathF.Sin(120 * MathF.PI / 180) )
                           * ( SpeedStat.Value / (float) StatData.StatCap );
                towards += new Vector2( MathF.Cos( 240 * MathF.PI / 180 ), MathF.Sin(240 * MathF.PI / 180) )
                           * ( MindStat.Value / (float) StatData.StatCap );
                towards.Normalize();

                double hue = Math.Atan2( towards.Y, towards.X ) * 180 / Math.PI + 180;
                double sat = Math.Max( 0, Math.Min( ( SpeedStat.Value + StrengthStat.Value + MindStat.Value ) / (double)( StatData.StatCap * 2 ), 1.0 ) );
                double val = 0.75 + sat / 4; // Maybe base this on age phase instead? Or stamina?

                if (double.IsNaN(hue))
                {
                    hue = 0;
                }

                var col = Util.ColorFromHsv(hue, sat, val);

                double period = ( MagicStat.Value < StatData.StatCap / 10 )
                                ? 0
                                : ( 0.35 + 1.0 - MagicStat.Value / (double) StatData.StatCap );
                if (period > 0)
                {
                    int min = 255 - (int)( MagicStat.Value / (double) StatData.StatCap * 10 ) * 10;
                    int span = 255 - min;
                    double atPerc = ( Game1.currentGameTime.TotalGameTime.TotalSeconds % ( period * 2 ) ) / period;
                    if (atPerc >= 1)
                        atPerc = 1 - (atPerc - 1);
                    int at = ( int )( span * atPerc );
                    col.A = (byte)(min + at);
                }

                return col;
            }
        }

        public RaisableJunimo()
        {
            Sprite = new AnimatedSprite("Characters/Junimo", 0, 16, 16);
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields(speedStat, strengthStat, staminaStat, mindStat, magicStat);
            NetFields.AddFields(happiness);
            NetFields.AddFields(skills);
        }

        public override Rectangle GetBoundingBox()
        {
            return new Rectangle(( int ) Position.X + 8, ( int ) Position.Y + 8, 48, 48);
        }

        public override void update(GameTime time, GameLocation location)
        {
            base.update(time, location);
            // todo
        }

        public override void draw(SpriteBatch b, float alpha = 1)
        {
            // update spritesheet based on age
            Sprite.UpdateSourceRect();
            b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(this.Sprite.SpriteWidth * 4 / 2, (float)this.Sprite.SpriteHeight * 3f / 4f * 4f / (float)Math.Pow(this.Sprite.SpriteHeight / 16, 2.0) + (float)base.yJumpOffset - 8f) /*+ ((base.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)*/, this.Sprite.SourceRect, Color, 0, new Vector2(this.Sprite.SpriteWidth * 4 / 2, (float)(this.Sprite.SpriteHeight * 4) * 3f / 4f) / 4f, Math.Max(0.2f, base.scale) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)base.getStandingY() / 10000f)));
            if (this.IsEmoting)
            {
                Vector2 emotePosition = this.getLocalPosition(Game1.viewport);
                emotePosition.Y -= 96f;
                b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(this.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, this.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.getStandingY() / 10000f);
            }
        }
    }
}
