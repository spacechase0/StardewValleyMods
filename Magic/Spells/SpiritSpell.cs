using Magic.Schools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceCore.Utilities;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace Magic.Spells
{
    public class SpiritSpell : Spell
    {
        public SpiritSpell() : base(SchoolId.Eldritch, "spirit")
        {
        }

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

        private GameLocation prevSummonerLoc = null;
        private int attackTimer = 0;
        private int animTimer = 0;
        private int animFrame = 0;

        public SpiritEffect(Farmer theSummoner)
        {
            summoner = theSummoner;
            tex = Game1.content.Load< Texture2D >("Characters\\Junimo");
            
            pos = summoner.Position;
            prevSummonerLoc = summoner.currentLocation;
        }
        public bool Update(UpdateTickedEventArgs e)
        {
            if (prevSummonerLoc != Game1.currentLocation)
            {
                prevSummonerLoc = Game1.currentLocation;
                pos = summoner.Position;
            }

            float nearestDist = float.MaxValue;
            Monster nearestMob = null;
            foreach ( var character in summoner.currentLocation.characters )
            {
                if ( character is Monster mob )
                {
                    float dist = Utility.distance(mob.GetBoundingBox().Center.X, summoner.Position.X, mob.GetBoundingBox().Center.Y, summoner.Position.Y);
                    if ( dist < nearestDist )
                    {
                        nearestDist = dist;
                        nearestMob = mob;
                    }
                }
            }

            if (attackTimer > 0)
                --attackTimer;
            if ( nearestMob != null )
            {
                Vector2 unit = nearestMob.Position - pos;
                unit.Normalize();

                pos += unit * 7;
                
                if ( Utility.distance(nearestMob.Position.X, pos.X, nearestMob.Position.Y, pos.Y) < Game1.tileSize )
                {
                    if ( attackTimer <= 0 )
                    {
                        attackTimer = 45;
                        int baseDmg = 20 * (summoner.CombatLevel + 1);
                        var oldPos = summoner.Position;
                        summoner.Position = new Vector2(nearestMob.GetBoundingBox().Center.X, nearestMob.GetBoundingBox().Center.Y);
                        summoner.currentLocation.damageMonster(nearestMob.GetBoundingBox(), (int)(baseDmg * 0.75f), (int)(baseDmg * 1.5f), false, 1, 0, 0.1f, 2, false, summoner);
                        summoner.Position = oldPos;
                    }
                }
            }

            if ( --timeLeft <= 0 )
                return false;
            return true;
        }

        public void Draw(SpriteBatch b)
        {
            if ( ++animTimer >= 6 )
            {
                animTimer = 0;
                if (++animFrame >= 12)
                    animFrame = 0;
            }

            int tx = (animFrame % 8) * 16;
            int ty = (animFrame / 8) * 16;
            b.Draw(tex, Game1.GlobalToLocal(Game1.viewport, pos), new Rectangle(tx, ty, 16, 16), new Color(1f, 1f, 1f, attackTimer > 0 ? 0.1f : 1f), 0, new Vector2(8, 8), 2, SpriteEffects.None, 1);
        }
    }
}
