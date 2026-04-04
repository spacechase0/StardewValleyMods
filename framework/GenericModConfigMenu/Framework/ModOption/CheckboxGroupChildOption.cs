namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>A child checkbox option within a <see cref="CheckboxGroupModOption"/>.</summary>
    internal class CheckboxGroupChildOption
    {
        /*********
        ** Fields
        *********/
        /// <summary>The cached value fetched from the mod config.</summary>
        private bool CachedValue;

        /// <summary>Get the latest value from the mod config.</summary>
        private readonly Func<bool> GetValue;

        /// <summary>Update the mod config with the given value.</summary>
        private readonly Action<bool> SetValue;


        /*********
        ** Accessors
        *********/
        /// <summary>The unique field ID used when raising field-changed events.</summary>
        public string FieldId { get; }

        /// <summary>The label text to show in the form.</summary>
        public Func<string> Name { get; }

        /// <summary>The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</summary>
        public Func<string> Tooltip { get; }

        /// <summary>The parent group that owns this child.</summary>
        public CheckboxGroupModOption Parent { get; }

        /// <summary>The current value of the child checkbox.</summary>
        public bool Value
        {
            get => this.CachedValue;
            set
            {
                if (this.CachedValue != value)
                    this.Parent.Owner.ChangeHandlers.ForEach(handler => handler(this.FieldId, value));

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
        /// <param name="parent">The parent group that owns this child.</param>
        /// <param name="getValue">Get the latest value from the mod config.</param>
        /// <param name="setValue">Update the mod config with the given value.</param>
        public CheckboxGroupChildOption(string fieldId, Func<string> name, Func<string> tooltip, CheckboxGroupModOption parent, Func<bool> getValue, Action<bool> setValue)
        {
            fieldId ??= Guid.NewGuid().ToString("N");
            tooltip ??= () => null;

            this.FieldId = fieldId;
            this.Name = name;
            this.Tooltip = tooltip;
            this.Parent = parent;
            this.GetValue = getValue;
            this.SetValue = setValue;
            this.CachedValue = getValue();
        }

        /// <summary>Perform any logic needed before the form is reset.</summary>
        public void BeforeReset()
        {
            this.CachedValue = this.GetValue();
        }

        /// <summary>Perform any logic needed after the form is reset.</summary>
        public void AfterReset()
        {
            this.CachedValue = this.GetValue();
        }

        /// <summary>Save the child's own value to the mod config.</summary>
        public void BeforeSave()
        {
            this.SetValue(this.CachedValue);
        }

        /// <summary>Perform any logic needed after the form is saved.</summary>
        public void AfterSave() { }

        /// <summary>Perform any logic needed before the menu is opened.</summary>
        public void BeforeMenuOpened()
        {
            this.CachedValue = this.GetValue();
        }

        /// <summary>Perform any logic needed before the menu is closed.</summary>
        public void BeforeMenuClosed() { }
    }
}
