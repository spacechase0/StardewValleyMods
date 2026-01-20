using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.Models;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Stardew3D.Data
{
    public class InteractionData
    {
        public List<InteractionArea> Areas { get; set; } = new();

        public static InteractionData Get(string id)
        {
            id = id.Replace('\\', '/');
            if (!Mod.Instance.InteractionDataDict.TryGetValue(id, out var data))
                return null;
            return data;
        }
    }
}
