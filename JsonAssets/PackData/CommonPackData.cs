using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.PackData
{
    public abstract class CommonPackData
    {
        internal ContentPack parent;

        /// <summary>
        /// Conditions for if an item is enabled.
        /// If not met, they will be removed from the game.
        /// This is checked at the beginning of each day.
        /// </summary>
        public string[] EnableConditions { get; set; }

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
    }
}
