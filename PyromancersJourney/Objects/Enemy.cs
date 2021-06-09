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
            if (this.Health <= 0)
            {
                this.Dead = true;
            }
        }

        public override void Update()
        {
            base.Update();

            foreach (var proj in this.World.projectiles)
            {
                if (proj.Dead)
                    continue;

                if ((proj.BoundingBox + new Vector2(proj.Position.X, proj.Position.Z)).Intersects(this.BoundingBox + new Vector2(this.Position.X, this.Position.Z)) && !proj.HurtsPlayer)
                {
                    proj.Trigger(this);
                }
            }

            if ((this.World.player.BoundingBox + new Vector2(this.World.player.Position.X, this.World.player.Position.Z)).Intersects(this.BoundingBox + new Vector2(this.Position.X, this.Position.Z)))
            {
                this.World.player.Hurt(1);
            }
        }
    }
}
