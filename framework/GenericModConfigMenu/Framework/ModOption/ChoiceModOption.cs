using System;
using System.Linq;

namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>A mod option which renders a dropdown field.</summary>
    internal class ChoiceModOption<T> : SimpleModOption<T>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The values that can be selected.</summary>
        public T[] Choices { get; }

        /// <summary>Get the display text to show for a value from <see cref="Choices"/>, or <c>null</c> to show the values as-is.</summary>
        public Func<string, string> FormatChoice { get; }

        /// <inheritdoc />
        public override T Value
        {
            get => base.Value;
            set
            {
                if (this.Choices.Contains(value))
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
        /// <param name="choices">The values that can be selected.</param>
        /// <param name="formatChoice">Get the display text to show for a value from <see cref="Choices"/>, or <c>null</c> to show the values as-is.</param>
        public ChoiceModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod, Func<T> getValue, Action<T> setValue, T[] choices, Func<string, string> formatChoice = null)
            : base(fieldId, name, tooltip, mod, getValue, setValue)
        {
            this.Choices = choices;
            this.FormatChoice = formatChoice;
        }
    }
}
