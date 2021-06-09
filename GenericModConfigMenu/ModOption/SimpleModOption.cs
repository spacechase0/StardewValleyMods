using System;
using StardewModdingAPI;

namespace GenericModConfigMenu.ModOption
{
    internal class SimpleModOption<T> : BaseModOption
    {
        public Type Type { get; }
        protected Func<T> getter;
        protected Action<T> setter;

        private T state;
        public virtual T Value
        {
            get { return this.state; }
            set
            {
                if (!this.state.Equals(value))
                    Mod.instance.configs[this.Owner].Options[Mod.instance.configs[this.Owner].ActiveDisplayPage.Name].ChangeHandler.ForEach(c => c.Invoke(this.Id, value));

                this.state = value;
            }
        }

        public override void SyncToMod()
        {
            this.state = this.getter.Invoke();
        }

        public override void Save()
        {
            SpaceShared.Log.trace("saving " + this.Name + " " + this.Description);
            this.setter.Invoke(this.state);
        }

        public SimpleModOption(string name, string desc, Type type, Func<T> theGetter, Action<T> theSetter, string id, IManifest mod)
            : base(name, desc, id, mod)
        {
            this.Type = type;
            this.getter = theGetter;
            this.setter = theSetter;

            this.state = this.getter.Invoke();
        }
    }
}
