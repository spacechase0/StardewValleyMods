using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace MoonMisadventures
{
    internal static class Assets
    {
        public static Texture2D RadioactiveTools;
        public static Texture2D MythiciteTools;

        public static Texture2D LaunchBackground;
        public static Texture2D LaunchUfo;
        public static Texture2D LaunchMoon;

        public static Texture2D Ufo;

        public static Texture2D AsteroidsSmall;
        public static Texture2D AsteroidsBig;

        internal static void Load( IContentHelper content )
        {
            Assets.RadioactiveTools = content.Load<Texture2D>( "assets/tools-radioactive.png" );
            Assets.MythiciteTools = content.Load<Texture2D>( "assets/tools-mythicite.png" );

            Assets.LaunchBackground = content.Load<Texture2D>( "assets/launch.png" );
            Assets.LaunchUfo = content.Load<Texture2D>( "assets/ufo-small.png" );
            Assets.LaunchMoon = content.Load<Texture2D>( "assets/moon.png" );

            Assets.Ufo = content.Load<Texture2D>( "assets/ufo-big.png" );

            Assets.AsteroidsSmall = content.Load<Texture2D>( "assets/asteroids-small.png" );
            Assets.AsteroidsBig = content.Load<Texture2D>( "assets/asteroids-large.png" );
        }
    }
}
