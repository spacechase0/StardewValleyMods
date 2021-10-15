using System;

namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>A mod option which renders a basic form input (e.g. a checkbox, textbox, or key binding).</summary>
    internal class SimpleModOption<T> : BaseModOption
    {
        /*********
        ** Fields
        *********/
        /// <summary>The cached value fetched from the mod config.</summary>
        private T CachedValue;

        /// <summary>Get the latest value from the mod config.</summary>
        protected readonly Func<T> GetValue;

        /// <summary>Update the mod config with the given value.</summary>
        protected readonly Action<T> SetValue;


        /*********
        ** Accessors
        *********/
        /// <summary>The option value type.</summary>
        public Type Type => typeof(T);

        /// <summary>The cached value fetched from the mod config.</summary>
        public virtual T Value
        {
            get => this.CachedValue;
            set
            {
                if (!this.CachedValue.Equals(value))
                    this.Owner.ChangeHandlers.ForEach(handler => handler(this.FieldId, value));

                this.CachedValue = value;
            }
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fieldId">The unique field ID used when raising field-changed events, or <c>null</c> to generate a random one.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        /// <param name="getValue">Get the latest value from the mod config.</param>
        /// <param name="setValue">Update the mod config with the given value.</param>
        public SimpleModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod, Func<T> getValue, Action<T> setValue)
            : base(fieldId, name, tooltip, mod)
        {
            this.GetValue = getValue;
            this.SetValue = setValue;

            this.CachedValue = this.GetValue();
        }

        /// <inheritdoc />
        public override void GetLatest()
        {
            this.CachedValue = this.GetValue();
        }

        /// <inheritdoc />
        public override void Save()
        {
            SpaceShared.Log.Trace("saving " + this.Name() + " " + this.Tooltip());
            this.SetValue(this.CachedValue);
        }
    }
}
