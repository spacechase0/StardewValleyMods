using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.ModOption
{
    internal abstract class BaseModOption
    {
        public string Name { get; }
        public string Description { get; }

        public string Id { get; }

        public bool AvailableInGame { get; set; } = false;

        public IManifest Owner { get; }
        
        public abstract void SyncToMod();
        public abstract void Save();

        public BaseModOption( string name, string desc, string id, IManifest mod)
        {
            Name = name;
            Description = desc;
            Id = id;
            Owner = mod;
        }
    }
}
