namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>A mod option which renders a parent checkbox with child checkboxes.
    /// The parent acts as a master toggle - when unchecked, children are visually disabled and non-interactive.
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

        /// <summary>The mod's unique ID, used as key in the shared UI state file.</summary>
        private readonly string ModId;

        /// <summary>Whether the parent group was given an explicit field ID (vs auto-generated GUID).</summary>
        private readonly bool HasExplicitFieldId;


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

        /// <summary>UI state for children: fieldId -> visual checkbox value. Persisted to GMCM's data/state.json.</summary>
        public Dictionary<string, bool> ChildUiState { get; private set; } = new();


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
        /// <param name="modId">The mod's unique ID, used as key in the shared UI state file.</param>
        /// <param name="leftAligned">Whether to use left-aligned layout instead of the default right-aligned GMCM layout.</param>
        public CheckboxGroupModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod, Func<bool> getValue, Action<bool> setValue, string modId, bool leftAligned = false)
            : base(fieldId, name, tooltip, mod)
        {
            this.GetValue = getValue;
            this.SetValue = setValue;
            this.ModId = modId;
            this.HasExplicitFieldId = !string.IsNullOrEmpty(fieldId);
            this.LeftAligned = leftAligned;
            this.CachedValue = getValue();
            this.LoadUiState();
        }

        /// <summary>Add a child checkbox option to this group.</summary>
        /// <param name="child">The child option to add.</param>
        public void AddChild(CheckboxGroupChildOption child)
        {
            this.Children.Add(child);
        }

        /// <summary>Get the persisted UI state for a child, falling back to config value.</summary>
        /// <param name="fieldId">The child's field ID.</param>
        /// <param name="configValue">The child's current config value (fallback).</param>
        public bool GetChildUiState(string fieldId, bool configValue)
        {
            return this.ChildUiState.TryGetValue(fieldId, out bool value) ? value : configValue;
        }

        /// <summary>Set the UI state for a child.</summary>
        /// <param name="fieldId">The child's field ID.</param>
        /// <param name="value">The visual checkbox value.</param>
        public void SetChildUiState(string fieldId, bool value)
        {
            this.ChildUiState[fieldId] = value;
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
            // Snapshot children's visual state before BeforeSave overwrites config values
            foreach (var child in this.Children)
                this.ChildUiState[child.FieldId] = child.Value;

            this.SetValue(this.CachedValue);
            foreach (var child in this.Children)
                child.BeforeSave();
        }

        /// <inheritdoc />
        public override void AfterSave()
        {
            this.SaveUiState();
            foreach (var child in this.Children)
                child.AfterSave();
        }

        /// <inheritdoc />
        public override void BeforeMenuOpened()
        {
            this.CachedValue = this.GetValue();
            this.LoadUiState();
            foreach (var child in this.Children)
                child.BeforeMenuOpened();
        }

        /// <inheritdoc />
        public override void BeforeMenuClosed()
        {
            foreach (var child in this.Children)
                child.BeforeMenuClosed();
        }


        /*********
        ** Private methods
        *********/
        private const string StateFilePath = "data/state.json";

        /// <summary>Load child UI state from GMCM's state file.</summary>
        private void LoadUiState()
        {
            if (!this.HasExplicitFieldId)
                return;

            var state = Mod.instance.Helper.Data.ReadJsonFile<Dictionary<string, ModState>>(StateFilePath);
            if (state == null || !state.TryGetValue(this.ModId, out var modState) || modState?.AddCheckboxGroupState == null)
                return;

            if (modState.AddCheckboxGroupState.TryGetValue(this.FieldId, out var children))
            {
                foreach (var (childId, value) in children)
                    this.ChildUiState[childId] = value;
            }
        }

        /// <summary>Save child UI state to GMCM's state file.</summary>
        private void SaveUiState()
        {
            if (!this.HasExplicitFieldId)
                return;

            var state = Mod.instance.Helper.Data.ReadJsonFile<Dictionary<string, ModState>>(StateFilePath) ?? new();

            if (!state.TryGetValue(this.ModId, out var modState))
            {
                modState = new();
                state[this.ModId] = modState;
            }

            modState.AddCheckboxGroupState ??= new();

            var children = new Dictionary<string, bool>();
            foreach (var child in this.Children)
            {
                if (!child.HasExplicitFieldId)
                    continue;
                children[child.FieldId] = child.Value;
            }

            modState.AddCheckboxGroupState[this.FieldId] = children;

            Mod.instance.Helper.Data.WriteJsonFile(StateFilePath, state);
        }

        /// <summary>Per-mod state stored in GMCM's data/state.json.</summary>
        internal class ModState
        {
            public Dictionary<string, Dictionary<string, bool>> AddCheckboxGroupState { get; set; }
        }
    }
}
