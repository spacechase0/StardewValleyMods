using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;

namespace FancyLighting
{
    public static class Extensions
    {
        public static string GetLogSummary(this Exception exception)
        {
            return (string)AccessTools.Method("StardewModdingAPI.Internal.ExceptionHelper:GetLogSummary").Invoke(null, new object[] { exception });
        }
        public static string GetMenuChainLabel(this IClickableMenu menu)
        {
            return (string)AccessTools.Method("StardewModdingAPI.Framework.InternalExtensions:GetMenuChainLabel").Invoke(null, new object[] { menu });
        }
    }
}
