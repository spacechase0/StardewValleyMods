using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.PackData
{
    public abstract class BasePackData
    {
        internal ContentPack parent;

        /// <summary>
        /// Conditions for if an item is enabled.
        /// If not met, they will be removed from the game.
        /// This is checked at the beginning of each day.
        /// These are ExpandedPreconditionsUtility conditions.
        /// </summary>
        public string[] EnableConditions { get; set; }

        /// <summary>
        /// If the current pack data is currently enabled or not.
        /// </summary>
        public bool Enabled { get; set; }
    }
}
