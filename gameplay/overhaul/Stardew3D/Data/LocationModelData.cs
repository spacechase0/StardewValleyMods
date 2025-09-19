using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Stardew3D.Data;
public class LocationModelData : ModelData
{
    public override string Type => $"{Mod.Instance.ModManifest.UniqueID}/Location";

    public class Portal
    {
        public string OtherLocation { get; set; }
        public string MatchingPortal { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Facing { get; set; }
    }
    public Dictionary<string, Portal> Portals { get; set; } = new();
}
