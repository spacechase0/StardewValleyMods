using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal abstract class BaseModOption
    {
        /*********
        ** Accessors
        *********/
        public string Name { get; }
        public string Description { get; }
        public string Id { get; }
        public bool AvailableInGame { get; set; } = false;
        public ModConfig Owner { get; }


        /*********
        ** Public methods
        *********/
        public abstract void SyncToMod();
        public abstract void Save();


        /*********
        ** Protected methods
        *********/
        protected BaseModOption(string name, string desc, string id, ModConfig mod)
        {
            this.Name = name;
            this.Description = desc;
            this.Id = id;
            this.Owner = mod;
        }
    }
}
