using System.Collections.Generic;
using System.Linq;
using JsonAssets.Data;
using SpaceShared.APIs;

namespace JsonAssets.Framework.ContentPatcher
{
    internal class ContentPatcherIntegration
    {
        private static IContentPatcherApi Cp;
        private static IApi Ja;

        internal static bool IdsAssigned;
        internal static int IdsAssignedGen = -1;

        private static List<BaseToken> Tokens;

        public static void Initialize()
        {
            ContentPatcherIntegration.Cp = Mod.instance.Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");
            ContentPatcherIntegration.Ja = Mod.instance.Helper.ModRegistry.GetApi<IApi>("spacechase0.JsonAssets");
            if (ContentPatcherIntegration.Cp == null)
                return;

            ContentPatcherIntegration.Ja.IdsAssigned += (s, e) => ContentPatcherIntegration.IdsAssigned = true;
            ContentPatcherIntegration.Ja.IdsAssigned += (s, e) => ContentPatcherIntegration.IdsAssignedGen++;
            Mod.instance.Helper.Events.GameLoop.ReturnedToTitle += (s, e) => ContentPatcherIntegration.IdsAssigned = false;

            ContentPatcherIntegration.Tokens = new List<BaseToken>
            {
                new IdToken("Object", ContentPatcherIntegration.Ja.GetAllObjectIds),
                new IdToken("Crop", ContentPatcherIntegration.Ja.GetAllCropIds),
                new IdToken("FruitTree", ContentPatcherIntegration.Ja.GetAllFruitTreeIds),
                new IdToken("BigCraftable", ContentPatcherIntegration.Ja.GetAllBigCraftableIds),
                new IdToken("Hat", ContentPatcherIntegration.Ja.GetAllHatIds),
                new IdToken("Weapon", ContentPatcherIntegration.Ja.GetAllWeaponIds),
                new IdToken("Pants", ContentPatcherIntegration.Ja.GetAllPantsIds),
                new IdToken("Shirts", ContentPatcherIntegration.Ja.GetAllShirtIds),
                new IdToken("Boots", ContentPatcherIntegration.Ja.GetAllBootsIds),
                new SpriteTilesheetToken("Object", ContentPatcherIntegration.Ja.GetAllObjectIds),
                new SpriteCoordinateToken("Object", true, ContentPatcherIntegration.Ja.GetAllObjectIds),
                new SpriteCoordinateToken("Object", false, ContentPatcherIntegration.Ja.GetAllObjectIds),
                new SpriteTilesheetToken("Crop", ContentPatcherIntegration.Ja.GetAllCropIds),
                new SpriteCoordinateToken("Crop", true, ContentPatcherIntegration.Ja.GetAllCropIds),
                new SpriteCoordinateToken("Crop", false, ContentPatcherIntegration.Ja.GetAllCropIds),
                new SpriteTilesheetToken("FruitTree", ContentPatcherIntegration.Ja.GetAllFruitTreeIds),
                new SpriteCoordinateToken("FruitTree", true, ContentPatcherIntegration.Ja.GetAllFruitTreeIds),
                new SpriteCoordinateToken("FruitTree", false, ContentPatcherIntegration.Ja.GetAllFruitTreeIds),
                new SpriteTilesheetToken("BigCraftable", ContentPatcherIntegration.Ja.GetAllBigCraftableIds),
                new SpriteCoordinateToken("BigCraftable", true, ContentPatcherIntegration.Ja.GetAllBigCraftableIds),
                new SpriteCoordinateToken("BigCraftable", false, ContentPatcherIntegration.Ja.GetAllBigCraftableIds),
                new SpriteTilesheetToken("Hat", ContentPatcherIntegration.Ja.GetAllHatIds),
                new SpriteCoordinateToken("Hat", true, ContentPatcherIntegration.Ja.GetAllHatIds),
                new SpriteCoordinateToken("Hat", false, ContentPatcherIntegration.Ja.GetAllHatIds),
                new SpriteTilesheetToken("Weapon", ContentPatcherIntegration.Ja.GetAllWeaponIds),
                new SpriteCoordinateToken("Weapon", true, ContentPatcherIntegration.Ja.GetAllWeaponIds),
                new SpriteCoordinateToken("Weapon", false, ContentPatcherIntegration.Ja.GetAllWeaponIds),
                new SpriteTilesheetToken("Shirts", ContentPatcherIntegration.Ja.GetAllShirtIds),
                new SpriteCoordinateToken("Shirts", true, ContentPatcherIntegration.Ja.GetAllShirtIds),
                new SpriteCoordinateToken("Shirts", false, ContentPatcherIntegration.Ja.GetAllShirtIds),
                new SpriteTilesheetToken("Pants", ContentPatcherIntegration.Ja.GetAllPantsIds),
                new SpriteCoordinateToken("Pants", true, ContentPatcherIntegration.Ja.GetAllPantsIds),
                new SpriteCoordinateToken("Pants", false, ContentPatcherIntegration.Ja.GetAllPantsIds),
                new SpriteTilesheetToken("Boots", ContentPatcherIntegration.Ja.GetAllBootsIds),
                new SpriteCoordinateToken("Boots", true, ContentPatcherIntegration.Ja.GetAllBootsIds),
                new SpriteCoordinateToken("Boots", false, ContentPatcherIntegration.Ja.GetAllBootsIds)
            };
            // TODO: Shirt tilesheet
            // TODO: Shirt x
            // TODO: Shirt y
            // TODO: Pants tilesheet
            // TODO: Pants x
            // TODO: Pants y

            foreach (var token in ContentPatcherIntegration.Tokens)
            {
                //cp.RegisterToken(Mod.instance.ModManifest, token.TokenName, token.UpdateContext, token.IsReady, token.GetValue, true, true);
                ContentPatcherIntegration.Cp.RegisterToken(Mod.instance.ModManifest, token.TokenName, token);
            }
        }
    }
}
