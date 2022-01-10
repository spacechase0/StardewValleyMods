using System;
using SpaceShared;

namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>A mod option which renders a numeric field.</summary>
    internal class NumericModOption<T> : SimpleModOption<T>
        where T : struct
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get the display text to show for a value, or <c>null</c> to show the value as-is.</summary>
        private readonly Func<T, string> FormatValueImpl;


        /*********
        ** Accessors
        *********/
        /// <summary>The minimum allowed value, or <c>null</c> to allow any.</summary>
        public T? Minimum { get; }

        /// <summary>The maximum allowed value, or <c>null</c> to allow any.</summary>
        public T? Maximum { get; }

        /// <summary>The interval of values that can be selected.</summary>
        public T? Interval { get; }

        /// <inheritdoc />
        public override T Value
        {
            get => base.Value;
            set
            {
                if (this.Minimum.HasValue || this.Maximum.HasValue)
                    value = Util.Clamp(t: value, min: this.Minimum ?? value, max: this.Maximum ?? value);

                if (this.Interval.HasValue)
                    value = Util.Adjust(value, this.Interval.Value);

                base.Value = value;
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
        /// <param name="min">The minimum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="max">The maximum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="interval">The interval of values that can be selected.</param>
        /// <param name="formatValue">Get the display text to show for a value, or <c>null</c> to show the number as-is.</param>
        public NumericModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod, Func<T> getValue, Action<T> setValue, T? min, T? max, T? interval, Func<T, string> formatValue)
            : base(fieldId, name, tooltip, mod, getValue, setValue)
        {
            this.Minimum = min;
            this.Maximum = max;
            this.Interval = interval;
            this.FormatValueImpl = formatValue;
        }

        /// <inheritdoc />
        public override string FormatValue()
        {
            return
                this.FormatValueImpl?.Invoke(this.Value)
                ?? this.Value.ToString();
        }
    }
}
