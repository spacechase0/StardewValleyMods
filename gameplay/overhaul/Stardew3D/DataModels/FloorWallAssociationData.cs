using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceShared.Attributes;

namespace Stardew3D.DataModels;

[CustomDictionaryAsset("FloorWallAssociations")]
public partial class FloorWallAssociationData
{
    public string WallDefinitionId { get; set; }
}
