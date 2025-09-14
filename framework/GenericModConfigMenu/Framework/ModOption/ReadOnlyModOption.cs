using System;

namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>The base implementation for a readonly config option.</summary>
    internal abstract class ReadOnlyModOption : BaseModOption
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void BeforeReset() { }

        /// <inheritdoc />
        public override void AfterReset() { }

        /// <inheritdoc />
        public override void BeforeSave() { }

        /// <inheritdoc />
        public override void AfterSave() { }

        /// <inheritdoc />
        public override void BeforeMenuOpened() { }

        /// <inheritdoc />
        public override void BeforeMenuClosed() { }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        protected ReadOnlyModOption(Func<string> name, Func<string> tooltip, ModConfig mod)
            : base(null, name, tooltip, mod) { }
    }
}
