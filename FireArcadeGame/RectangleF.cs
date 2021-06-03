using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireArcadeGame
{
    public class RectangleF
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Width { get; set; } = 0;
        public float Height { get; set; } = 0;

        public RectangleF() { }
        public RectangleF( float x, float y, float w, float h )
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public bool Intersects( RectangleF other )
        {
            if ( X + Width < other.X || X > other.X + other.Width ||
                 Y + Height < other.Y || Y > other.Y + other.Height )
            {
                return false;
            }
            return true;
        }

        public static RectangleF operator + ( RectangleF rect, Vector2 vec )
        {
            return new RectangleF( rect.X + vec.X, rect.Y + vec.Y, rect.Width, rect.Height );
        }
    }
}
