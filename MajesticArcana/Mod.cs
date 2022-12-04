using System;
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
            Helper.ConsoleCommands.Add("magik_spellcrafting", "...", OnSpellcraftingCommand);
        }

        private void OnAlchemyCommand(string arg1, string[] arg2)
        {
            if (!Context.IsPlayerFree)
                return;

            Game1.activeClickableMenu = new AlchemyMenu();
        }

        private void OnSpellcraftingCommand(string arg1, string[] arg2)
        {
            if (!Context.IsPlayerFree)
                return;

            Game1.activeClickableMenu = new SpellcraftingMenu();
        }
    }
}
