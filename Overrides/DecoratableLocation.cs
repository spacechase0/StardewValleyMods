using SpaceCore.Locations;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Overrides
{
    public class WallpaperHook
    {
        public static bool Prefix(DecoratableLocation __instance, int which, int whichRoom, bool persist)
        {
            var cloc = __instance as CustomDecoratableLocation;
            var wallPaper = SpaceCore.instance.Helper.Reflection.GetField<DecorationFacade>(__instance, "wallPaper").GetValue();
            List<Microsoft.Xna.Framework.Rectangle> rooms = cloc != null ? cloc.getWalls() : DecoratableLocation.getWalls();
            if (persist)
            {
                wallPaper.SetCountAtLeast(rooms.Count);
                if (whichRoom == -1)
                {
                    for (int i = 0; i < wallPaper.Count; i++)
                    {
                        wallPaper[i] = which;
                    }
                    return false;
                }
                if (whichRoom <= wallPaper.Count - 1)
                {
                    wallPaper[whichRoom] = which;
                }
            }

            return false;
        }
    }
    public class FlooringHook
    {
        public static bool Prefix(DecoratableLocation __instance, int which, int whichRoom, bool persist)
        {
            var cloc = __instance as CustomDecoratableLocation;
            var floor = SpaceCore.instance.Helper.Reflection.GetField<DecorationFacade>(__instance, "floor").GetValue();
            List<Microsoft.Xna.Framework.Rectangle> rooms = cloc != null ? cloc.getFloors() : DecoratableLocation.getFloors();
            if (!persist)
            {
                return false;
            }
            floor.SetCountAtLeast(rooms.Count);
            if (whichRoom == -1)
            {
                for (int i = 0; i < floor.Count; i++)
                {
                    floor[i] = which;
                }
                return false;
            }
            if (whichRoom <= floor.Count - 1)
            {
                floor[whichRoom] = which;
                return false;
            }

            return false;
        }
    }
}
