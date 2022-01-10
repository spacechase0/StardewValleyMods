using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MoreBuildings.Buildings.BigShed;
using MoreBuildings.Buildings.FishingShack;
using MoreBuildings.Buildings.MiniSpa;
using MoreBuildings.Buildings.SpookyShed;
using SpaceShared.Migrations;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace MoreBuildings.Framework
{
    /// <summary>Handles migrating legacy data for a save file.</summary>
    internal static class LegacyDataMigrator
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Raised after the game reads the save data into <see cref="SaveGame.loaded"/>, but before it begins restoring it.</summary>
        /// <param name="modRegistry">The mod registry with which to check for PyTK.</param>
        public static void OnSaveParsed(IModRegistry modRegistry)
        {
            PyTkMigrator.MigrateBuildings(SaveGame.loaded, new()
            {
                ["MoreBuildings.Buildings.MiniSpa.BigShedBuilding,  MoreBuildings"] = LegacyDataMigrator.Hydrate<BigShedBuilding>,
                ["MoreBuildings.Buildings.FishingShack.FishingShackBuilding,  MoreBuildings"] = LegacyDataMigrator.Hydrate<FishingShackBuilding>,
                ["MoreBuildings.Buildings.MiniSpa.MiniSpaBuilding,  MoreBuildings"] = LegacyDataMigrator.Hydrate<MiniSpaBuilding>,
                ["MoreBuildings.Buildings.SpookyShed.SpookyShedBuilding,  MoreBuildings"] = LegacyDataMigrator.Hydrate<SpookyShedBuilding>
            });
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Convert a serialized PyTK building into the full building instance.</summary>
        /// <typeparam name="TBuilding">The expected building type.</typeparam>
        /// <param name="from">The PyTK building placeholder containing the data to migrate.</param>
        /// <param name="customData">The custom PyTK fields.</param>
        private static Building Hydrate<TBuilding>(Building from, IDictionary<string, string> customData)
            where TBuilding : Building, new()
        {
            TBuilding replacement = new TBuilding
            {
                daysOfConstructionLeft = { Value = from.daysOfConstructionLeft.Value },
                tileX = { Value = from.tileX.Value },
                tileY = { Value = from.tileY.Value }
            };

            LegacyDataMigrator.CopyContents(from.indoors.Value, replacement.indoors.Value);

            return replacement;
        }

        /// <summary>Copy the contents of a previous PyTK-serialized location into the hydrated location.</summary>
        /// <param name="from">The PyTK location from which to copy contents.</param>
        /// <param name="to">The full location to populate.</param>
        private static void CopyContents(GameLocation from, GameLocation to)
        {
            if (from == null || to == null)
                return;

            // previous versions only supported placed objects + terrain features

            foreach ((Vector2 tile, SObject value) in from.Objects.Pairs)
                to.Objects[tile] = value;

            foreach ((Vector2 tile, TerrainFeature value) in from.terrainFeatures.Pairs)
                to.terrainFeatures[tile] = value;
        }
    }
}
