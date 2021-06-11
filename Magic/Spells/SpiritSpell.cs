using Magic.Schools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace Magic.Spells
{
    public class SpiritSpell : Spell
    {
        public SpiritSpell()
            : base(SchoolId.Eldritch, "spirit") { }

        public override int getManaCost(Farmer player, int level)
        {
            return 50;
        }

        public override int getMaxCastingLevel()
        {
            return 1;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            player.AddCustomSkillExperience(Magic.Skill, 25);
            return new SpiritEffect(player);
        }
    }

    public class SpiritEffect : IActiveEffect
    {
        private readonly Farmer summoner;
        private readonly Texture2D tex;
        private Vector2 pos;
        private int timeLeft = 60 * 60;

        private GameLocation prevSummonerLoc;
        private int attackTimer;
        private int animTimer;
        private int animFrame;

        public SpiritEffect(Farmer theSummoner)
        {
            this.summoner = theSummoner;
            this.tex = Game1.content.Load<Texture2D>("Characters\\Junimo");

            this.pos = this.summoner.Position;
            this.prevSummonerLoc = this.summoner.currentLocation;
        }
        public bool Update(UpdateTickedEventArgs e)
        {
            if (this.prevSummonerLoc != Game1.currentLocation)
            {
                this.prevSummonerLoc = Game1.currentLocation;
                this.pos = this.summoner.Position;
            }

            float nearestDist = float.MaxValue;
            Monster nearestMob = null;
            foreach (var character in this.summoner.currentLocation.characters)
            {
                if (character is Monster mob)
                {
                    float dist = Utility.distance(mob.GetBoundingBox().Center.X, this.summoner.Position.X, mob.GetBoundingBox().Center.Y, this.summoner.Position.Y);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestMob = mob;
                    }
                }
            }

            if (this.attackTimer > 0)
                --this.attackTimer;
            if (nearestMob != null)
            {
                Vector2 unit = nearestMob.Position - this.pos;
                unit.Normalize();

                this.pos += unit * 7;

                if (Utility.distance(nearestMob.Position.X, this.pos.X, nearestMob.Position.Y, this.pos.Y) < Game1.tileSize)
                {
                    if (this.attackTimer <= 0)
                    {
                        this.attackTimer = 45;
                        int baseDmg = 20 * (this.summoner.CombatLevel + 1);
                        var oldPos = this.summoner.Position;
                        this.summoner.Position = new Vector2(nearestMob.GetBoundingBox().Center.X, nearestMob.GetBoundingBox().Center.Y);
                        this.summoner.currentLocation.damageMonster(nearestMob.GetBoundingBox(), (int)(baseDmg * 0.75f), (int)(baseDmg * 1.5f), false, 1, 0, 0.1f, 2, false, this.summoner);
                        this.summoner.Position = oldPos;
                    }
                }
            }

            if (--this.timeLeft <= 0)
                return false;
            return true;
        }

        public void Draw(SpriteBatch b)
        {
            if (++this.animTimer >= 6)
            {
                this.animTimer = 0;
                if (++this.animFrame >= 12)
                    this.animFrame = 0;
            }

            int tx = (this.animFrame % 8) * 16;
            int ty = (this.animFrame / 8) * 16;
            b.Draw(this.tex, Game1.GlobalToLocal(Game1.viewport, this.pos), new Rectangle(tx, ty, 16, 16), new Color(1f, 1f, 1f, this.attackTimer > 0 ? 0.1f : 1f), 0, new Vector2(8, 8), 2, SpriteEffects.None, 1);
        }
    }
}
