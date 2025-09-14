using System.ComponentModel;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public abstract class CommonPackData : BasePackData
    {
        /// <summary>
        /// Remove all traces of the item when disabled.
        /// For example, if a recipe is known, or the friendship level of an NPC (if JA supported NPCs).
        /// </summary>
        [DefaultValue(true)]
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

        /// <summary>
        /// Returns the primary texture, if any. Used mainly for recipe drawing.
        /// </summary>
        /// <returns>The primary texture.</returns>
        public abstract TexturedRect GetTexture();
    }
}
