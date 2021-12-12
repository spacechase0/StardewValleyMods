using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;

namespace MisappliedPhysicalities
{
    public enum Side
    {
        Up,
        Right,
        Down,
        Left,
        Above,
        Below,
        AsItem
    }

    public static class SideExtensions
    {
        public static Vector3 GetOffset( this Side side )
        {
            switch ( side )
            {
                case Side.Up: return new Vector3( 0, -1, 0 );
                case Side.Right: return new Vector3( 1, 0, 0 );
                case Side.Down: return new Vector3( 0, 1, 0 );
                case Side.Left: return new Vector3( -1, 0, 0 );
                case Side.Above: return new Vector3( 0, 0, 1 );
                case Side.Below: return new Vector3( 0, 0, -1 );
            }
            return Vector3.Zero;
        }

        public static Side GetOpposite( this Side side )
        {
            switch ( side )
            {
                case Side.Up: return Side.Down;
                case Side.Right: return Side.Left;
                case Side.Down: return Side.Up;
                case Side.Left: return Side.Right;
                case Side.Above: return Side.Below;
                case Side.Below: return Side.Above;
            }

            return side;
        }

        public static Side? GetSideFromFacingDirection( this int facing )
        {
            switch ( facing )
            {
                case Game1.up: return Side.Up;
                case Game1.right: return Side.Right;
                case Game1.down: return Side.Down;
                case Game1.left: return Side.Left;
            }

            return null;
        }
    }
}
