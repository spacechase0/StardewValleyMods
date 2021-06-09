using Microsoft.Xna.Framework;

namespace PyromancersJourney.Objects
{
    public class Enemy : Character
    {
        public Enemy(World world)
            : base(world) { }

        public override void Hurt(int amt)
        {
            base.Hurt(amt);
            if (Health <= 0)
            {
                Dead = true;
            }
        }

        public override void Update()
        {
            base.Update();

            foreach (var proj in World.projectiles)
            {
                if (proj.Dead)
                    continue;

                if ((proj.BoundingBox + new Vector2(proj.Position.X, proj.Position.Z)).Intersects(BoundingBox + new Vector2(Position.X, Position.Z)) && !proj.HurtsPlayer)
                {
                    proj.Trigger(this);
                }
            }

            if ((World.player.BoundingBox + new Vector2(World.player.Position.X, World.player.Position.Z)).Intersects(BoundingBox + new Vector2(Position.X, Position.Z)))
            {
                World.player.Hurt(1);
            }
        }
    }
}
