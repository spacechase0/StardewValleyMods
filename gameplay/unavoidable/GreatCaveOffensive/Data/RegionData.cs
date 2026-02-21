using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceShared.Attributes;

namespace GreatCaveOffensive.Data;

[CustomDictionaryAsset("Regions")]
public partial class RegionData
{
    public Dictionary<string, BiomeData> Biomes { get; set; } = new();

    public string ParentRegion { get; set; }
}
