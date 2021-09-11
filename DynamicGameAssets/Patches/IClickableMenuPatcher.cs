using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using DynamicGameAssets.Framework;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="IClickableMenu"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class IClickableMenuPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<IClickableMenu>(nameof(IClickableMenu.drawHoverText), new[] { typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(CraftingRecipe), typeof(IList<Item>) }),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_DrawHoverText))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="IClickableMenu.drawHoverText(SpriteBatch,string,SpriteFont,int,int,int,string,int,string[],Item,int,int,int,int,int,float,CraftingRecipe,IList{Item})"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_DrawHoverText(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            return PatchCommon.RedirectForFakeObjectInformationTranspiler(gen, original, instructions);
        }
    }
}
