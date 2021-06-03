using Microsoft.Xna.Framework;

namespace SpaceCore.Events
{
    public class EventArgsBombExploded
    {
        internal EventArgsBombExploded(Vector2 tileLocation, int radius)
        {
            Position = tileLocation;
            Radius = radius;
        }
        
        public Vector2 Position { get; }
        public int Radius { get; }
    }
}
