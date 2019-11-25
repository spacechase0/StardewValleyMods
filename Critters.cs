using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugNet
{
    public class Critters
    {
        public static Critter MakeButterfly(int x, int y, int baseFrame)
        {
            var bfly = new Butterfly(new Vector2(x, y));
            bfly.baseFrame = baseFrame;
            bfly.sprite.CurrentFrame = baseFrame;
            return bfly;
        }

        public static Critter MakeBird(int x, int y, bool blue)
        {
            return new Birdie(x, y, blue ? 45 : 25);
        }

        public static Critter MakeCloud(int x, int y)
        {
            return new Cloud(new Vector2(x, y));
        }

        public static Critter MakeCrow(int x, int y)
        {
            return new Crow(x, y);
        }

        public static Critter MakeFirefly(int x, int y)
        {
            return new Firefly(new Vector2(x, y));
        }

        public static Critter MakeFrog(int x, int y, bool olive)
        {
            return new Frog(new Vector2(x, y), olive);
        }

        public static Critter MakeOwl(int x, int y)
        {
            return new Owl(new Vector2(x * Game1.tileSize, y * Game1.tileSize));
        }

        public static Critter MakeRabbit(int x, int y, bool white)
        {
            var rabbit = new Rabbit(new Vector2(x, y), false);
            rabbit.baseFrame = white ? 74 : 54;
            return rabbit;
        }

        public static Critter MakeSeagull(int x, int y)
        {
            return new Seagull(new Vector2(x * Game1.tileSize, y * Game1.tileSize), Seagull.stopped);
        }

        public static Critter MakeSquirrel(int x, int y)
        {
            return new Squirrel(new Vector2(x, y), false);
        }

        public static Critter MakeWoodpecker(int x, int y)
        {
            return new Woodpecker(new StardewValley.TerrainFeatures.Tree(), new Vector2(x, y));
        }
    }
}
