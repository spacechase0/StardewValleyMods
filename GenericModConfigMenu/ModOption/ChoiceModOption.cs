using System;
using System.Linq;
using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal class ChoiceModOption<T> : SimpleModOption<T>
    {
        /*********
        ** Accessors
        *********/
        public T[] Choices { get; set; }

        /// <inheritdoc />
        public override T Value
        {
            get => base.Value;
            set { if (this.Choices.Contains(value)) base.Value = value; }
        }


        /*********
        ** Public methods
        *********/
        public ChoiceModOption(string name, string desc, Type type, Func<T> theGetter, Action<T> theSetter, T[] choices, string id, ModConfig mod)
            : base(name, desc, type, theGetter, theSetter, id, mod)
        {
            this.Choices = choices;
        }
    }
}
