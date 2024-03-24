using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace NewGamePlus
{
    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.performUseAction))]
    public static class LegacyTokenUsePatch
    {
        public static bool Prefix(StardewValley.Object __instance, ref bool __result)
        {
            if (__instance.ItemId != $"{Mod.instance.ModManifest.UniqueID}_LegacyToken")
                return true;

            Game1.activeClickableMenu = new LegacyMenu();
            __result = false;
            return false;
        }
    }
}
