using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
