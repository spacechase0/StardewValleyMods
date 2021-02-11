using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.PackData
{
    public abstract class CommonPackData : BasePackData
    {
        /// <summary>
        /// Remove all traces of the item when disabled.
        /// For example, if a recipe is known, or the friendship level of an NPC (if JA supported NPCs).
        /// </summary>
        public bool RemoveAllTracesWhenDisabled { get; set; } = true;

        /// <summary>
        /// The ID of the item.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// A sub-class should remove itself from the game when this is run, taking into account RemoveAllTraceswhenDisabled.
        /// </summary>
        public abstract void OnDisabled();

        /// <summary>
        /// Returns the SDV-Item form of this item, if it exists.
        /// </summary>
        /// <returns>The item as a Stardew Valley Item.</returns>
        public abstract Item ToItem();
    }
}
