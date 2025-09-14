using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraformingHoe
{
    public enum HoeMode
    {
        Normal,
        Water,
        Grass,
    }

    public class ScreenState
    {
        public HoeMode Mode { get; set; } = HoeMode.Normal;
        public bool LocationDirty { get; set; } = false;
    }
}
