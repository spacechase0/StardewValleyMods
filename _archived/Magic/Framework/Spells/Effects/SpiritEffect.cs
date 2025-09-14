using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace Magic.Framework.Spells.Effects
{
    internal class SpiritEffect : IActiveEffect
    {
        /*********
        ** Fields
        *********/
        private readonly Farmer Summoner;
        private readonly Texture2D Tex;
        private Vector2 Pos;
        private int TimeLeft = 60 * 60;

        private GameLocation PrevSummonerLoc;
        private int AttackTimer;
        private int AnimTimer;
        private int AnimFrame;


        /*********
        ** Public methods
        *********/
        public SpiritEffect(Farmer theSummoner)
        {
            this.Summoner = theSummoner;
            this.Tex = Game1.content.Load<Texture2D>("Characters\\Junimo");

            this.Pos = this.Summoner.Position;
            this.PrevSummonerLoc = this.Summoner.currentLocation;
        }

        public bool Update(UpdateTickedEventArgs e)
        {
            if (this.PrevSummonerLoc != Game1.currentLocation)
            {
                this.PrevSummonerLoc = Game1.currentLocation;
                this.Pos = this.Summoner.Position;
            }

            float nearestDist = float.MaxValue;
            Monster nearestMob = null;
            foreach (var character in this.Summoner.currentLocation.characters)
            {
                if (character is Monster mob)
                {
                    float dist = Utility.distance(mob.GetBoundingBox().Center.X, this.Summoner.Position.X, mob.GetBoundingBox().Center.Y, this.Summoner.Position.Y);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestMob = mob;
                    }
                }
            }

            if (this.AttackTimer > 0)
                --this.AttackTimer;
            if (nearestMob != null)
            {
                Vector2 unit = nearestMob.Position - this.Pos;
                unit.Normalize();

                this.Pos += unit * 7;

                if (Utility.distance(nearestMob.Position.X, this.Pos.X, nearestMob.Position.Y, this.Pos.Y) < Game1.tileSize)
                {
                    if (this.AttackTimer <= 0)
                    {
                        this.AttackTimer = 45;
                        int baseDmg = 20 * (this.Summoner.CombatLevel + 1);
                        var oldPos = this.Summoner.Position;
                        this.Summoner.Position = new Vector2(nearestMob.GetBoundingBox().Center.X, nearestMob.GetBoundingBox().Center.Y);
                        this.Summoner.currentLocation.damageMonster(nearestMob.GetBoundingBox(), (int)(baseDmg * 0.75f), (int)(baseDmg * 1.5f), false, 1, 0, 0.1f, 2, false, this.Summoner);
                        this.Summoner.Position = oldPos;
                    }
                }
            }

            if (--this.TimeLeft <= 0)
                return false;
            return true;
        }

        public void Draw(SpriteBatch b)
        {
            if (++this.AnimTimer >= 6)
            {
                this.AnimTimer = 0;
                if (++this.AnimFrame >= 12)
                    this.AnimFrame = 0;
            }

            int tx = (this.AnimFrame % 8) * 16;
            int ty = (this.AnimFrame / 8) * 16;
            b.Draw(this.Tex, Game1.GlobalToLocal(Game1.viewport, this.Pos), new Rectangle(tx, ty, 16, 16), new Color(1f, 1f, 1f, this.AttackTimer > 0 ? 0.1f : 1f), 0, new Vector2(8, 8), 2, SpriteEffects.None, 1);
        }
    }
}
