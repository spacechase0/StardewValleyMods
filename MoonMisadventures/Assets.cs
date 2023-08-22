using Microsoft.Xna.Framework.Graphics;

using MoonMisadventures.Game.Locations;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

namespace MoonMisadventures
{
    internal static class Assets
    {
        //public static Texture2D RadioactiveTools;
        //public static Texture2D MythiciteTools;
        //public static Texture2D AnimalGauntlets;
        public static Texture2D LunarKey;
        //public static Texture2D Necklaces;

        public static Texture2D LaunchBackground;
        public static Texture2D LaunchUfo;
        public static Texture2D LaunchMoon;

        public static Texture2D Ufo;

        public static Texture2D AsteroidsSmall;
        public static Texture2D AsteroidsBig;

        public static Texture2D HoeDirt;

        public static Texture2D NecklaceBg;

        public static Texture2D Laser;

        internal static void Load( IModContentHelper content )
        {
            //Assets.RadioactiveTools = content.Load<Texture2D>( "assets/tools-radioactive.png" );
            //Assets.MythiciteTools = content.Load<Texture2D>( "assets/tools-mythicite.png" );
            //Assets.AnimalGauntlets = content.Load<Texture2D>( "assets/animal-gauntlets.png" );
            Assets.LunarKey = content.Load<Texture2D>( "assets/key.png" );
            //Assets.Necklaces = content.Load<Texture2D>( "assets/dga/necklaces.png" );

            Assets.LaunchBackground = content.Load<Texture2D>( "assets/launch.png" );
            Assets.LaunchUfo = content.Load<Texture2D>( "assets/ufo-small.png" );
            Assets.LaunchMoon = content.Load<Texture2D>( "assets/moon.png" );

            Assets.Ufo = content.Load<Texture2D>( "assets/ufo-big.png" );

            Assets.AsteroidsSmall = content.Load<Texture2D>( "assets/asteroids-small.png" );
            Assets.AsteroidsBig = content.Load<Texture2D>( "assets/asteroids-large.png" );
            
            Assets.HoeDirt = content.Load<Texture2D>( "assets/hoedirt.png" );

            Assets.NecklaceBg = content.Load<Texture2D>( "assets/necklace-bg.png" );

            Laser = content.Load<Texture2D>("assets/laser.png");
        }
    }
}
