using System;
using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
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
        public Type Type { get; }

        /// <summary>The cached value fetched from the mod config.</summary>
        public virtual T Value
        {
            get => this.CachedValue;
            set
            {
                if (!this.CachedValue.Equals(value))
                    this.Owner.ChangeHandlers.ForEach(handler => handler(this.Id, value));

                this.CachedValue = value;
            }
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="type">The option value type.</param>
        /// <param name="getValue">Get the latest value from the mod config.</param>
        /// <param name="setValue">Update the mod config with the given value.</param>
        /// <param name="id">The unique field ID used when raising field-changed events.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        public SimpleModOption(string name, string tooltip, Type type, Func<T> getValue, Action<T> setValue, string id, ModConfig mod)
            : base(name, tooltip, id, mod)
        {
            this.Type = type;
            this.GetValue = getValue;
            this.SetValue = setValue;

            this.CachedValue = this.GetValue.Invoke();
        }

        /// <inheritdoc />
        public override void GetLatest()
        {
            this.CachedValue = this.GetValue.Invoke();
        }

        /// <inheritdoc />
        public override void Save()
        {
            SpaceShared.Log.Trace("saving " + this.Name + " " + this.Tooltip);
            this.SetValue.Invoke(this.CachedValue);
        }
    }
}
