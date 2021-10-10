using System;
using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    /// <summary>The base implementation for a config option.</summary>
    internal abstract class BaseModOption
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique field ID used when raising field-changed events.</summary>
        public string Id { get; }

        /// <summary>The label text to show in the form.</summary>
        public string Name { get; }

        /// <summary>The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</summary>
        public string Tooltip { get; }

        /// <summary>Whether the option can be edited from the in-game options menu. If this is false, it can only be edited from the title screen.</summary>
        public bool EditableInGame { get; set; }

        /// <summary>The mod config UI that contains this option.</summary>
        public ModConfig Owner { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Update the value from the mod configuration.</summary>
        public abstract void GetLatest();

        /// <summary>Save the value to the mod configuration.</summary>
        public abstract void Save();


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="id">The unique field ID used when raising field-changed events.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        protected BaseModOption(string name, string tooltip, string id, ModConfig mod)
        {
            this.Name = name;
            this.Tooltip = tooltip;
            this.Id = id;
            this.Owner = mod;
        }

        /// <summary>Generate a random ID for an option field.</summary>
        protected static string RandomId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
