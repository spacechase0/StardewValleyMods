using System;
using GenericModConfigMenu.Framework;
using SpaceShared;

namespace GenericModConfigMenu.ModOption
{
    /// <summary>A mod option which renders a numeric field.</summary>
    internal class NumericModOption<T> : SimpleModOption<T>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The minimum allowed value.</summary>
        public T Minimum { get; set; }

        /// <summary>The maximum allowed value.</summary>
        public T Maximum { get; set; }

        /// <summary>The interval of values that can be selected.</summary>
        public T Interval { get; set; }

        /// <inheritdoc />
        public override T Value
        {
            get => base.Value;
            set => base.Value = Util.Adjust(Util.Clamp(this.Minimum, value, this.Maximum), this.Interval);
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
        /// <param name="min">The minimum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="max">The maximum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="interval">The interval of values that can be selected.</param>
        /// <param name="id">The unique field ID used when raising field-changed events.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        public NumericModOption(string name, string tooltip, Type type, Func<T> getValue, Action<T> setValue, T min, T max, T interval, string id, ModConfig mod)
            : base(name, tooltip, type, getValue, setValue, id, mod)
        {
            this.Minimum = min;
            this.Maximum = max;
            this.Interval = interval;
        }
    }
}
