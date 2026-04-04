namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>A mod option which renders a parent checkbox with child checkboxes.
    /// The parent acts as a master toggle - when unchecked, children are visually disabled and non-interactive.
    /// Children retain their own config values regardless of parent state.
    /// By default, the parent checkbox is right-aligned (matching standard GMCM layout) with children prefixed by ' > '.
    /// When <see cref="LeftAligned"/> is true, the checkbox is left-aligned with indented children.</summary>
    internal class CheckboxGroupModOption : BaseModOption
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
        /// <summary>The current value of the parent checkbox.</summary>
        public bool Value
        {
            get => this.CachedValue;
            set
            {
                if (this.CachedValue != value)
                    this.Owner.ChangeHandlers.ForEach(handler => handler(this.FieldId, value));

                this.CachedValue = value;
            }
        }

        /// <summary>Whether to use left-aligned layout (checkbox before label) instead of the default right-aligned GMCM layout.</summary>
        public bool LeftAligned { get; }

        /// <summary>The child checkbox options under this group.</summary>
        public List<CheckboxGroupChildOption> Children { get; } = new();


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
        /// <param name="leftAligned">Whether to use left-aligned layout instead of the default right-aligned GMCM layout.</param>
        public CheckboxGroupModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod, Func<bool> getValue, Action<bool> setValue, bool leftAligned = false)
            : base(fieldId, name, tooltip, mod)
        {
            this.GetValue = getValue;
            this.SetValue = setValue;
            this.LeftAligned = leftAligned;
            this.CachedValue = getValue();
        }

        /// <summary>Add a child checkbox option to this group.</summary>
        /// <param name="child">The child option to add.</param>
        public void AddChild(CheckboxGroupChildOption child)
        {
            this.Children.Add(child);
        }

        /// <inheritdoc />
        public override void BeforeReset()
        {
            this.CachedValue = this.GetValue();
            foreach (var child in this.Children)
                child.BeforeReset();
        }

        /// <inheritdoc />
        public override void AfterReset()
        {
            this.CachedValue = this.GetValue();
            foreach (var child in this.Children)
                child.AfterReset();
        }

        /// <inheritdoc />
        public override void BeforeSave()
        {
            this.SetValue(this.CachedValue);
            foreach (var child in this.Children)
                child.BeforeSave();
        }

        /// <inheritdoc />
        public override void AfterSave()
        {
            foreach (var child in this.Children)
                child.AfterSave();
        }

        /// <inheritdoc />
        public override void BeforeMenuOpened()
        {
            this.CachedValue = this.GetValue();
            foreach (var child in this.Children)
                child.BeforeMenuOpened();
        }

        /// <inheritdoc />
        public override void BeforeMenuClosed()
        {
            foreach (var child in this.Children)
                child.BeforeMenuClosed();
        }
    }
}
