using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace SleepyEye.Framework
{
    /// <summary>Handles migrating legacy data.</summary>
    internal class LegacyDataMigrator
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Migrate legacy data when the save is loaded.</summary>
        public static void OnSaveLoaded()
        {
            if (!Context.IsMainPlayer)
                return;

            // player items
            foreach (var player in Game1.getAllFarmers())
                LegacyDataMigrator.TryMigrate(player.Items);

            // map objects
            foreach (GameLocation location in CommonHelper.GetLocations())
            {
                foreach (Vector2 key in location.Objects.Keys)
                    LegacyDataMigrator.TryMigrate(location.Objects[key], _ => { /* can't be placed directly on a map */ });

                foreach (StorageFurniture furniture in location.furniture.OfType<StorageFurniture>())
                    LegacyDataMigrator.TryMigrate(furniture.heldItems);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Replace any tent tools found starting from the given items.</summary>
        /// <param name="items">The items to scan for tents.</param>
        private static void TryMigrate(IList<Item> items)
        {
            for (int i = 0; i < items.Count; i++)
                LegacyDataMigrator.TryMigrate(items[i], tent => items[i] = tent);
        }

        /// <summary>Replace any tent tools found starting from the given item.</summary>
        /// <param name="item">The item to scan for tents.</param>
        /// <param name="replaceWith">Replace the <paramref name="item"/> with a tent if needed.</param>
        private static void TryMigrate(Item item, Action<TentTool> replaceWith)
        {
            switch (item)
            {
                case null:
                    return;

                case SObject { Name: "PyTK|Item|SleepyEye.TentTool,  SleepyEye|" }:
                    replaceWith(new TentTool());
                    return;

                case Chest chest:
                    for (int i = 0; i < chest.items.Count; i++)
                        LegacyDataMigrator.TryMigrate(chest.items[i], item => chest.items[i] = item);
                    return;
            }
        }
    }
}
