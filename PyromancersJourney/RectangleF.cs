using Microsoft.Xna.Framework;

namespace PyromancersJourney
{
    public class RectangleF
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Width { get; set; } = 0;
        public float Height { get; set; } = 0;

        public RectangleF() { }
        public RectangleF(float x, float y, float w, float h)
        {
            this.X = x;
            this.Y = y;
            this.Width = w;
            this.Height = h;
        }

        public bool Intersects(RectangleF other)
        {
            if (this.X + this.Width < other.X || this.X > other.X + other.Width || this.Y + this.Height < other.Y || this.Y > other.Y + other.Height)
            {
                return false;
            }
            return true;
        }

        public static RectangleF operator +(RectangleF rect, Vector2 vec)
        {
            return new(rect.X + vec.X, rect.Y + vec.Y, rect.Width, rect.Height);
        }
    }
}
