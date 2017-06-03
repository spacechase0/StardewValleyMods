using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore
{
    public static class Commands
    {
        internal static void register()
        {
            Command.register("player_addwallpaper", addWallpaper);
            Command.register("player_addflooring", addFlooring);
        }

        private static void addWallpaper(string[] args)
        {
            addWallpaperItem(args, false);
        }

        private static void addFlooring(string[] args)
        {
            addWallpaperItem(args, true);
        }

        private static void addWallpaperItem(string[] args, bool isFloor)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("ID parameter required");
                return;
            }
            int id = 0;
            if ( !Int32.TryParse( args[ 0 ], out id ) )
            {
                Console.WriteLine("Invalid ID provided");
                return;
            }

            Game1.player.addItemByMenuIfNecessary(new Wallpaper(id, isFloor));
        }
    }
}
