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
        
        public abstract void SyncToMod();
        public abstract void Save();

        public BaseModOption( string name, string desc )
        {
            Name = name;
            Description = desc;
        }
    }
}
