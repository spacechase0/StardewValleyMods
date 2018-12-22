using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using Magic.Schools;
using StardewValley;
using StardewValley.Monsters;
using System;

namespace Magic.Spells
{
    // TODO: Change into trap?
    class TendrilsSpell : Spell
    {
        public TendrilsSpell() : base( SchoolId.Nature, "tendrils" )
        {
        }

        public override int getManaCost(StardewValley.Farmer player, int level)
        {
            return 0;
        }

        public override void onCast(StardewValley.Farmer player, int level, int targetX, int targetY)
        {
            foreach ( var npc in player.currentLocation.characters )
            {
                if ( npc is Monster mob )
                {
                    float rad = (7 - level * 2) * Game1.tileSize / 2;
                    int dur = ( level * 2 + 5 ) * 60;
                    if ( Vector2.Distance(mob.position, new Vector2( targetX, targetY ) ) <= rad )
                    {
                        new Tendril(mob, new Vector2(targetX, targetY), rad, dur );
                        player.addMagicExp(3);
                    }
                }
            }
        }

        private class Tendril
        {
            private Monster mob;
            private Vector2 pos;
            private float radius;
            private int duration;
            private Texture2D tex;

            public Tendril( Monster theMob, Vector2 pos, float rad, int dur )
            {
                mob = theMob;
                this.pos = pos;
                radius = rad;
                duration = dur;
                tex = Content.loadTexture("magic/nature/tendrils/tendril.png");

                GameEvents.UpdateTick += update;
                GraphicsEvents.OnPreRenderHudEvent += render;
            }

            private void update( object sender, EventArgs args )
            {
                Vector2 mobPos = new Vector2(mob.GetBoundingBox().Center.X, mob.GetBoundingBox().Center.Y);
                if ( Vector2.Distance( mobPos, pos ) >= radius )
                {
                    Vector2 offset = mob.position - pos;
                    offset.Normalize();
                    offset *= radius;
                    mob.position.Value = pos + offset;
                }

                if ( --duration <= 0 )
                {
                    GameEvents.UpdateTick -= update;
                    GraphicsEvents.OnPreRenderHudEvent -= render;
                }
            }

            private void render( object sender, EventArgs args )
            {
                Vector2 mobPos = new Vector2(mob.GetBoundingBox().Center.X, mob.GetBoundingBox().Center.Y);
                var dist = Vector2.Distance(mobPos, pos);
                Rectangle r = new Rectangle((int)pos.X, (int)pos.Y, 10, (int)dist);
                r = Game1.GlobalToLocal(Game1.viewport, r);
                float rot = (float) -Math.Atan2(pos.Y - mobPos.Y, mobPos.X - pos.X);
                Game1.spriteBatch.Draw(tex, r, new Rectangle(0,0,10,12), Color.White, rot - 3.14f / 2, new Vector2(5, 0), SpriteEffects.None, 1);
            }
        }
    }
}
