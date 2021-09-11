using Microsoft.Xna.Framework;
using StardewValley;

namespace SurfingFestival.Framework
{
    internal class Obstacle
    {
        public ObstacleType Type { get; set; }
        public Vector2 Position { get; set; }
        public string HomingTarget { get; set; }

        public TemporaryAnimatedSprite UnderwaterSprite { get; set; }

        public Rectangle GetBoundingBox()
        {
            int w = 48, h = 16;
            int ox = 0, oy = 0;
            if (this.Type is ObstacleType.Item or ObstacleType.HomingProjectile or ObstacleType.FirstPlaceProjectile)
                w = 16;
            else if (this.Type == ObstacleType.Rock)
            {
                oy = -16 * Game1.pixelZoom;
                h += 16;
            }
            w *= Game1.pixelZoom;
            h *= Game1.pixelZoom;
            return new Rectangle((int)this.Position.X + ox /*- w / 2*/, (int)this.Position.Y + oy /*- h / 2*/, w, h);
        }
    }
}
