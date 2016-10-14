using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Inheritance;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;
using Microsoft.Xna.Framework.Input;

namespace RushOrders
{
    public class RushOrdersMod : Mod
    {
        public override void Entry(params object[] objects)
        {
            GameEvents.UpdateTick += onUpdate;
        }

        public static void onUpdate(object sender, EventArgs args)
        {
        }
    }
}
