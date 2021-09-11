using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.TerrainFeatures;

namespace BugNet.Framework
{
    /// <summary>Builds a vanilla critter instance.</summary>
    internal class CritterBuilder
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Create a critter instance at the given X and Y tile position.</summary>
        public Func<int, int, Critter> MakeCritter;

        /// <summary>Get whether a given critter instance matches this critter.</summary>
        public Func<Critter, bool> IsThisCritter;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="makeCritter">Create a critter instance at the given X and Y tile position.</param>
        /// <param name="isThisCritter">Get whether a given critter instance matches this critter.</param>
        public CritterBuilder(Func<int, int, Critter> makeCritter, Func<Critter, bool> isThisCritter)
        {
            this.MakeCritter = makeCritter;
            this.IsThisCritter = isThisCritter;
        }

        /// <summary>Create a butterfly.</summary>
        /// <param name="baseFrame">The base frame in the critter tilesheet.</param>
        /// <param name="island">Whether to create an island butterfly.</param>
        public static CritterBuilder ForButterfly(int baseFrame, bool island = false)
        {
            return new(
                makeCritter: (x, y) =>
                {
                    Butterfly butterfly = new Butterfly(new Vector2(x, y))
                    {
                        baseFrame = baseFrame,
                        sprite = { CurrentFrame = baseFrame }
                    };
                    if (island)
                        Mod.Instance.Helper.Reflection.GetField<bool>(butterfly, "islandButterfly").SetValue(true);
                    return butterfly;
                },
                isThisCritter: critter =>
                    critter is Butterfly butterfly
                    && butterfly.baseFrame == baseFrame
                    && island == Mod.Instance.Helper.Reflection.GetField<bool>(critter, "islandButterfly").GetValue()
            );
        }

        /// <summary>Create a bird.</summary>
        /// <param name="baseFrame">The base frame in the critter tilesheet.</param>
        public static CritterBuilder ForBird(int baseFrame)
        {
            return new(
                makeCritter: (x, y) => new Birdie(x, y, baseFrame),
                isThisCritter: critter => critter is Birdie birdie && birdie.baseFrame == baseFrame
            );
        }

        /// <summary>Create a cloud.</summary>
        public static CritterBuilder ForCloud()
        {
            return new(
                makeCritter: (x, y) => new Cloud(new Vector2(x, y)),
                isThisCritter: critter => critter is Cloud
            );
        }

        /// <summary>Create a crow.</summary>
        public static CritterBuilder ForCrow()
        {
            return new(
                makeCritter: (x, y) => new Crow(x, y),
                isThisCritter: critter => critter is Crow
            );
        }

        /// <summary>Create a firefly.</summary>
        public static CritterBuilder ForFirefly()
        {
            return new(
                makeCritter: (x, y) => new Firefly(new Vector2(x, y)),
                isThisCritter: critter => critter is Firefly
            );
        }

        /// <summary>Create a frog.</summary>
        /// <param name="olive">Whether to create an olive frog.</param>
        public static CritterBuilder ForFrog(bool olive)
        {
            return new(
                makeCritter: (x, y) => new Frog(new Vector2(x, y), waterLeaper: olive),
                isThisCritter: critter => critter is Frog frog && Mod.Instance.Helper.Reflection.GetField<bool>(frog, "waterLeaper").GetValue() == olive
            );
        }

        /// <summary>Create an owl.</summary>
        public static CritterBuilder ForOwl()
        {
            return new(
                makeCritter: (x, y) => new Owl(new Vector2(x * Game1.tileSize, y * Game1.tileSize)),
                isThisCritter: critter => critter is Owl
            );
        }

        /// <summary>Create a rabbit.</summary>
        /// <param name="white">Whether to create a white rabbit.</param>
        public static CritterBuilder ForRabbit(bool white)
        {
            int baseFrame = white ? 74 : 54;

            return new(
                makeCritter: (x, y) => new Rabbit(new Vector2(x, y), false)
                {
                    baseFrame = baseFrame
                },
                isThisCritter: critter => critter is Rabbit && critter.baseFrame == baseFrame
            );
        }

        /// <summary>Create a seagull.</summary>
        public static CritterBuilder ForSeagull()
        {
            return new(
                makeCritter: (x, y) => new Seagull(new Vector2(x * Game1.tileSize, y * Game1.tileSize), Seagull.stopped),
                isThisCritter: critter => critter is Seagull
            );
        }

        /// <summary>Create a squirrel.</summary>
        public static CritterBuilder ForSquirrel()
        {
            return new(
                makeCritter: (x, y) => new Squirrel(new Vector2(x, y), false),
                isThisCritter: critter => critter is Squirrel
            );
        }

        /// <summary>Create a woodpecker.</summary>
        public static CritterBuilder ForWoodpecker()
        {
            return new(
                makeCritter: (x, y) => new Woodpecker(new Tree(), new Vector2(x, y)),
                isThisCritter: critter => critter is Woodpecker
            );
        }

        /// <summary>Create a monkey.</summary>
        public static CritterBuilder ForMonkey()
        {
            return new(
                makeCritter: (x, y) => new CalderaMonkey(new Vector2(x, y)),
                isThisCritter: critter => critter is CalderaMonkey
            );
        }

        /// <summary>Create a parrot.</summary>
        /// <param name="green">Whether to create a green parrot.</param>
        public static CritterBuilder ForParrot(bool green)
        {
            int index = green ? 2 : 0;
            int minYOffset = index * 24;
            int maxYOffset = (index + 1) * 24;

            return new(
                makeCritter: (x, y) => new OverheadParrot(new Vector2(x, y))
                {
                    sourceRect = new Rectangle(0, Game1.random.Next(minYOffset, maxYOffset + 1), 24, 24)
                },
                isThisCritter: critter =>
                    critter is OverheadParrot parrot
                    && parrot.sourceRect.Y >= minYOffset
                    && parrot.sourceRect.Y <= maxYOffset
            );
        }
    }
}
