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
using SpaceShared.Attributes;
using Stardew3D.Models;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Stardew3D.Data
{
    [CustomDictionaryAsset("Interactions")]
    public partial class InteractionData
    {
        public List<InteractionArea> Areas { get; set; } = new();

        static partial void AfterRefreshData()
        {
            Mod.State.ActiveHandler?.SwitchOff(Mod.State.ActiveHandler);
            Mod.State.ActiveHandler?.SwitchOn(Mod.State.ActiveHandler);
        }
    }
}
