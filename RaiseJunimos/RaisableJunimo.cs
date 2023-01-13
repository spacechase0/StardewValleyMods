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
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

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
        public const int LifetimeInDays = 28 * 3;

        private readonly NetString name = new("Junimo");
        private readonly NetInt age = new(0);
        [XmlIgnore]
        public string Name { get { return name.Value; } set { name.Value = value; } }
        [XmlIgnore]
        public int Age { get { return age.Value; } set { age.Value = value; } }

        public AgePhase GetCurrentAgePhase()
        {
            if (Age < 14) return AgePhase.Baby;
            if (Age < 14 + 28) return AgePhase.Child;
            return AgePhase.Adult;
        }

        private int GetCurrentAgePhasePercent()
        {
            if (Age < 14)
                return (int)(Age / 14f * 100);
            else if (Age < 14 + 28)
                return (int)((Age - 14) / 28f * 100);
            else
                return (int)((Age - 14 - 28) / (28f + 14f) * 100);
        }

        private readonly NetBool wasPetToday = new(false);
        private readonly NetInt giftsGivenToday = new(0);
        private readonly NetArray<string, NetString> recentGifts = new(5);
        [XmlIgnore]
        public bool WasPetToday { get { return wasPetToday.Value; } set { wasPetToday.Value = value; } }
        [XmlIgnore]
        public int GiftsGivenToday { get { return giftsGivenToday.Value; } set { giftsGivenToday.Value = value; } }

        public const int MaxHappiness = 500;

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
        [XmlIgnore]
        public int Happiness { get { return happiness.Value; } set { happiness.Value = Math.Min( Math.Max( 0, value ), MaxHappiness ); } }

        
        private readonly NetStringDictionary<int, NetInt> skills = new();

        private readonly NetRef<Hat> hat = new();
        [XmlIgnore]
        public Hat Hat { get { return hat.Value; } set { hat.Value = value; } }

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
            NetFields.AddFields(name, age);
            NetFields.AddFields(wasPetToday, giftsGivenToday, recentGifts);
            NetFields.AddFields(speedStat, strengthStat, staminaStat, mindStat, magicStat);
            NetFields.AddFields(happiness);
            NetFields.AddFields(skills);
            NetFields.AddFields(hat);
        }

        public override Rectangle GetBoundingBox()
        {
            return new Rectangle(( int ) Position.X + 8, ( int ) Position.Y + 8, 48, 48);
        }

        private static string MakeStatBar(StatData stat)
        {
            float statPerc = stat.Value / (float)StatData.StatCap;
            string bar = "@";
            for (int i = 0; i < 20; ++i, statPerc -= 0.05f)
            {
                if (statPerc > 0)
                    bar += "*";
                else if (statPerc >= -0.05)
                    bar += "|";
                else
                    bar += " ";
            }
            bar += ">";

            if (stat.Disposition <= 0.85) // starts at 0.70
                bar += $" {I18n.StatDisposition_VeryDifficult()}";
            else if (stat.Disposition <= 0.95)
                bar += $" {I18n.StatDisposition_Difficult()}";
            else if (stat.Disposition <= 1.05)
                bar += $" {I18n.StatDisposition_Normal()}";
            else if (stat.Disposition <= 1.5)
                bar += $" {I18n.StatDisposition_Easy()}";
            else // ends at 2.0
                bar += $" {I18n.StatDisposition_VeryEasy()}";

            return bar;
        }

        public void ShowInfo()
        {
            string mainInfo = "";
            mainInfo += I18n.Info_Title(Name) + "^";
            mainInfo += I18n.Info_Age( Mod.instance.Helper.Translation.Get( "age." + Enum.GetName( typeof( AgePhase ), Age ).ToLower() ) + " (" + GetCurrentAgePhasePercent() + "%)") + "^";
            mainInfo += I18n.Info_Happiness( (int)( Happiness / (float) MaxHappiness * 100 ) + "%" ) + "^";
            mainInfo += I18n.Info_Stats_Speed(MakeStatBar(SpeedStat)) + "^";
            mainInfo += I18n.Info_Stats_Strength(MakeStatBar(StrengthStat)) + "^";
            mainInfo += I18n.Info_Stats_Mind(MakeStatBar(MindStat)) + "^";
            mainInfo += I18n.Info_Stats_Magic(MakeStatBar(MagicStat)) + "^";
            mainInfo += I18n.Info_Stats_Stamina(MakeStatBar(StaminaStat));

            string skillsInfo = "";
            skillsInfo += I18n.Info_Skills_Header() + "^";
            if (skills.Count() == 0)
                skillsInfo += I18n.Info_Skills_None();
            else
                skillsInfo += "~!@#$%^&*()_+[]\\{}|:\";',./<>?-=";

            Game1.multipleDialogues(new string[] { mainInfo, skillsInfo });
        }

        public void Pet( Farmer who )
        {
            if (!WasPetToday)
            {
                WasPetToday = true;
                doEmote(Character.heartEmote);
                Happiness += 40;
            }
        }

        public void GiveGift(Farmer who)
        {
            var obj = who.ActiveObject;
            float mult = 1;
            for (int i = 0; i < recentGifts.Count; ++i)
            {
                if (recentGifts[i] == "(O)" + who.ActiveObject.ParentSheetIndex.ToString())
                {
                    mult -= 0.075f;
                }
            }
            int statMod = (int)(obj.sellToStorePrice(who.UniqueMultiplayerID) * mult);

            bool goodItem = false;
            if (obj.ParentSheetIndex == StardewValley.Object.prismaticShardIndex || obj.ParentSheetIndex == 797 /* pearl */)
            {
                goodItem = true;
                doEmote(Character.heartEmote);
                Happiness += (int)(50 * mult);
                SpeedStat.Value += (int)(statMod * SpeedStat.Disposition);
                StrengthStat.Value += (int)(statMod * StrengthStat.Disposition);
                MindStat.Value += (int)(statMod * MindStat.Disposition);
                MagicStat.Value += (int)(statMod * MagicStat.Disposition);
                StaminaStat.Value += (int)(statMod * StaminaStat.Disposition);
            }
            else
            {
                switch (who.ActiveObject.Category)
                {
                    case StardewValley.Object.FishCategory:
                    case StardewValley.Object.flowersCategory:
                    case StardewValley.Object.GreensCategory:
                        SpeedStat.Value += (int)(statMod * SpeedStat.Disposition);
                        goodItem = true;
                        break;
                    case StardewValley.Object.EggCategory:
                    case StardewValley.Object.MilkCategory:
                    case StardewValley.Object.sellAtPierresAndMarnies:
                    case StardewValley.Object.VegetableCategory:
                    case StardewValley.Object.FruitsCategory:
                        StrengthStat.Value += (int)(statMod * StrengthStat.Disposition);
                        goodItem = true;
                        break;
                    case StardewValley.Object.metalResources:
                    case StardewValley.Object.artisanGoodsCategory:
                    case StardewValley.Object.syrupCategory:
                        MindStat.Value += (int)(statMod * MindStat.Disposition);
                        goodItem = true;
                        break;
                    case StardewValley.Object.GemCategory:
                    case StardewValley.Object.mineralsCategory:
                    case StardewValley.Object.monsterLootCategory:
                        MagicStat.Value += (int)(statMod * MagicStat.Disposition);
                        goodItem = true;
                        break;
                    case StardewValley.Object.CookingCategory:
                    case StardewValley.Object.ingredientsCategory:
                        StaminaStat.Value += (int)(statMod * StaminaStat.Disposition);
                        goodItem = true;
                        break;
                }

                if (goodItem)
                {
                    Happiness += (int)(30 * mult);
                    doEmote(Character.happyEmote);
                }
            }

            if (goodItem)
            {
                who.reduceActiveItemByOne();
                for (int i = 1; i < recentGifts.Count; ++i)
                {
                    recentGifts[i] = recentGifts[i - 1];
                }
                recentGifts[0] = "(O)" + obj.ParentSheetIndex;
                ++GiftsGivenToday;
            }
            else
            {
                doEmote(Character.angryEmote);
                Happiness -= 10;
            }
        }

        public void Rename()
        {
            Game1.activeClickableMenu = new NamingMenu((s) => { Name = s; Game1.exitActiveMenu(); Game1.player.CanMove = true; }, "Name the junimo", Name);
        }

        public void DayUpdate()
        {
            int happinessLost = 0;
            switch (GetCurrentAgePhase())
            {
                case AgePhase.Baby: happinessLost = 75; break;
                case AgePhase.Child: happinessLost = 50; break;
                case AgePhase.Adult: happinessLost = 25; break;
            }
            Happiness = Math.Max( 0, Happiness - happinessLost );

            WasPetToday = false;
            GiftsGivenToday = 0;
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
            var pos = base.getLocalPosition(Game1.viewport) + new Vector2(this.Sprite.SpriteWidth * 4 / 2, (float)this.Sprite.SpriteHeight * 3f / 4f * 4f / (float)Math.Pow(this.Sprite.SpriteHeight / 16, 2.0) + (float)base.yJumpOffset - 8f) /*+ ((base.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)*/;
            float layer = Math.Max(0f, base.drawOnTop ? 0.991f : ((float)base.getStandingY() / 10000f));
            var origin = new Vector2(this.Sprite.SpriteWidth * 4 / 2, (float)(this.Sprite.SpriteHeight * 4) * 3f / 4f) / 4f;
            b.Draw(this.Sprite.Texture, pos, this.Sprite.SourceRect, Color, 0, origin, Math.Max(0.2f, base.scale) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layer);
            hat.Value?.draw(b, pos - origin * Math.Max(0.2f, base.scale) * 4f - new Vector2( 10, 16 ), 3 / 2f, 1, layer + 1e-07f, FacingDirection);
            if (this.IsEmoting)
            {
                Vector2 emotePosition = this.getLocalPosition(Game1.viewport);
                emotePosition.Y -= 96f;
                b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(this.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, this.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.getStandingY() / 10000f);
            }
        }
    }
}
