using System;
using System.Linq;
using StardewModdingAPI;

namespace GenericModConfigMenu.ModOption
{
    internal class ChoiceModOption<T> : SimpleModOption<T>
    {
        public T[] Choices { get; set; }

        public override T Value
        {
            get => base.Value;
            set { if (this.Choices.Contains(value)) base.Value = value; }
        }

        public ChoiceModOption(string name, string desc, Type type, Func<T> theGetter, Action<T> theSetter, T[] choices, string id, IManifest mod)
            : base(name, desc, type, theGetter, theSetter, id, mod)
        {
            this.Choices = choices;
        }
    }
}
