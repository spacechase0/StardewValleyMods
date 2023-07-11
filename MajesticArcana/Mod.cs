using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace MajesticArcana
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            AlchemyRecipes.Init();

            Helper.ConsoleCommands.Add("magik_alchemy", "...", OnAlchemyCommand);
        }

        private void OnAlchemyCommand(string arg1, string[] arg2)
        {
            if (!Context.IsPlayerFree)
                return;

            Game1.activeClickableMenu = new AlchemyMenu();
        }
    }
}
