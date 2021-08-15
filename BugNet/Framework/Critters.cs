using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace BugNet.Framework
{
    /// <summary>Builds vanilla critter instances.</summary>
    internal class CritterBuilder
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Create a butterfly.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        /// <param name="baseFrame">The base frame in the critter tilesheet.</param>
        /// <param name="island">Whether to create an island butterfly.</param>
        public static Critter MakeButterfly(int x, int y, int baseFrame, bool island = false)
        {
            var butterfly = new Butterfly(new Vector2(x, y))
            {
                baseFrame = baseFrame,
                sprite =
                {
                    CurrentFrame = baseFrame
                }
            };
            if (island)
                Mod.Instance.Helper.Reflection.GetField<bool>(butterfly, "islandButterfly").SetValue(true);

            return butterfly;
        }

        /// <summary>Create a bird.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        /// <param name="frame">The base frame in the critter tilesheet.</param>
        public static Critter MakeBird(int x, int y, int frame)
        {
            return new Birdie(x, y, frame);
        }

        /// <summary>Create a cloud.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        public static Critter MakeCloud(int x, int y)
        {
            return new Cloud(new Vector2(x, y));
        }

        /// <summary>Create a crow.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        public static Critter MakeCrow(int x, int y)
        {
            return new Crow(x, y);
        }

        /// <summary>Create a firefly.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        public static Critter MakeFirefly(int x, int y)
        {
            return new Firefly(new Vector2(x, y));
        }

        /// <summary>Create a frog.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        /// <param name="olive">Whether to create an olive frog, similar to the one Sebastian's fourteen-heart event.</param>
        public static Critter MakeFrog(int x, int y, bool olive)
        {
            return new Frog(new Vector2(x, y), olive);
        }

        /// <summary>Create an owl.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        public static Critter MakeOwl(int x, int y)
        {
            return new Owl(new Vector2(x * Game1.tileSize, y * Game1.tileSize));
        }

        /// <summary>Create a rabbit.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        /// <param name="white">Whether to create a white rabbit.</param>
        public static Critter MakeRabbit(int x, int y, bool white)
        {
            return new Rabbit(new Vector2(x, y), false)
            {
                baseFrame = white ? 74 : 54
            };
        }

        /// <summary>Create a seagull.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        public static Critter MakeSeagull(int x, int y)
        {
            return new Seagull(new Vector2(x * Game1.tileSize, y * Game1.tileSize), Seagull.stopped);
        }

        /// <summary>Create a squirrel.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        public static Critter MakeSquirrel(int x, int y)
        {
            return new Squirrel(new Vector2(x, y), false);
        }

        /// <summary>Create a woodpecker.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        public static Critter MakeWoodpecker(int x, int y)
        {
            return new Woodpecker(new StardewValley.TerrainFeatures.Tree(), new Vector2(x, y));
        }

        /// <summary>Create a monkey.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        public static Critter MakeMonkey(int x, int y)
        {
            return new CalderaMonkey(new Vector2(x, y));
        }

        /// <summary>Create a parrot.</summary>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">the tile Y position.</param>
        /// <param name="blue">Whether to create a blue parrot.</param>
        public static Critter MakeParrot(int x, int y, bool blue)
        {
            return new OverheadParrot(new Vector2(x, y))
            {
                sourceRect = new Rectangle(0, (Game1.random.Next(2) + (blue ? 2 : 0)) * 24, 24, 24)
            };
        }
    }
}
