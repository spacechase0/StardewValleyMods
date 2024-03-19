using HarmonyLib;
using MageDelve.Skill;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MageDelve.Alchemy
{
    public class AlchemyEngine
    {
        public AlchemyEngine()
        {
            SoundEffect alchemyParticlize = SoundEffect.FromFile(Path.Combine(Mod.instance.Helper.DirectoryPath, "assets", "alchemy-particlize.wav"));
            Game1.soundBank.AddCue(new CueDefinition("spacechase0.MageDelve_alchemy_particlize", alchemyParticlize, 3));
            SoundEffect alchemySynthesize = SoundEffect.FromFile(Path.Combine(Mod.instance.Helper.DirectoryPath, "assets", "alchemy-synthesize.wav"));
            Game1.soundBank.AddCue(new CueDefinition("spacechase0.MageDelve_alchemy_synthesize", alchemySynthesize, 3));

            Mod.instance.Helper.Events.Display.MenuChanged += this.Display_MenuChanged;

            Mod.instance.Helper.ConsoleCommands.Add("magedelve_alchemy", "...", OnAlchemyCommand);
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is GameMenu gm)
            {
                (gm.pages[GameMenu.inventoryTab] as InventoryPage).trashCan.texture = Mod.instance.Helper.ModContent.Load<Texture2D>("assets/philosophers_stone.png");
                (gm.pages[GameMenu.inventoryTab] as InventoryPage).trashCan.sourceRect = new(0, 0, 16, 32);
            }
        }

        private void OnAlchemyCommand(string arg1, string[] arg2)
        {
            if (!Context.IsPlayerFree)
                return;

            Game1.activeClickableMenu = new FancyAlchemyMenu();
        }
    }

    [HarmonyPatch(typeof(Utility), nameof(Utility.getTrashReclamationPrice))]
    public class PhilosophersStoneFunctionalityPatch
    {
        public static void Postfix(Item i, Farmer f, ref int __result)
        {
            if (!f.professions.Contains( ArcanaSkill.TrashCanProfession.GetVanillaId() ) )
            {
                return;
            }

            float sellPercentage = 1;
            if (i.canBeTrashed())
            {
                if (i is Wallpaper || i is Furniture)
                {
                    __result = -1;
                    return;
                }
                StardewValley.Object obj = i as StardewValley.Object;
                if ((obj != null && !obj.bigCraftable) || i is MeleeWeapon || i is Ring || i is Boots)
                {
                    __result = (int)((float)i.Stack * ((float)i.sellToStorePrice(-1L) * sellPercentage));
                    return;
                }
            }
            __result = -1;
        }
    }
}
