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
        public string FieldId { get; }

        /// <summary>The label text to show in the form.</summary>
        public Func<string> Name { get; }

        /// <summary>The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</summary>
        public Func<string> Tooltip { get; }

        /// <summary>Whether the option can be edited from the in-game options menu. If this is false, it can only be edited from the title screen.</summary>
        public bool EditableInGame { get; }

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
        /// <param name="fieldId">The unique field ID used when raising field-changed events, or <c>null</c> to generate a random one.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        protected BaseModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod)
        {
            fieldId ??= Guid.NewGuid().ToString("N");
            tooltip ??= () => null;

            this.Name = name;
            this.Tooltip = tooltip;
            this.FieldId = fieldId;
            this.EditableInGame = mod.DefaultEditableInGame;
            this.Owner = mod;
        }
    }
}
