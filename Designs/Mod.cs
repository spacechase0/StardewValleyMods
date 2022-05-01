using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Designs
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            Helper.ConsoleCommands.Add("design_add", "...", OnDesignAdd);
        }

        private void OnDesignAdd(string cmd, string[] args)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            Game1.activeClickableMenu = new DesignMenu();
        }
    }
}
