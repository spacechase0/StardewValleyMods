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
using StardewValley.Buildings;
using StardewValley.Locations;

namespace NewGamePlus
{
    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction))]
    public static class StablePlacementPatch1
    {
        public static bool Prefix(StardewValley.Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            if (__instance.ItemId != $"{Mod.instance.ModManifest.UniqueID}_StableToken")
                return true;
            bool ret = location.buildStructure(new Stable(new Vector2(x / 64 - 3, y / 64 - 1)), new Vector2(x / 64 - 3, y / 64 - 1), who);
            if (ret)
            {
                var b = location.getBuildingAt(new Vector2(x / 64 - 3, y / 64 - 1));
                while (b.daysOfConstructionLeft.Value > 0)
                    b.dayUpdate(0);
            }
            __result = ret;
            return false;
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.drawPlacementBounds))]
    public static class StablePlacementBoundsPatch
    {
        public static bool Prefix(StardewValley.Object __instance, SpriteBatch spriteBatch, GameLocation location)
        {
            if (__instance.ItemId != $"{Mod.instance.ModManifest.UniqueID}_StableToken")
                return true;

            int x = (int)Game1.GetPlacementGrabTile().X * 64;
            int y = (int)Game1.GetPlacementGrabTile().Y * 64;

            Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
            if (Game1.isCheckingNonMousePlacement)
            {
                Vector2 placementPosition = Utility.GetNearbyValidPlacementPosition(Game1.player, location, __instance, x, y);
                x = (int)placementPosition.X;
                y = (int)placementPosition.Y;
            }

            Game1.isCheckingNonMousePlacement = false;

            var CurrentBlueprint = new Stable();

            Vector2 mousePositionTile2 = new Vector2(x / 64 - 3, y / 64 - 1);
            for (int y4 = 0; y4 < CurrentBlueprint.GetData().Size.Y; y4++)
            {
                for (int x3 = 0; x3 < CurrentBlueprint.GetData().Size.X; x3++)
                {
                    int sheetIndex3 = CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(x3, y4);
                    Vector2 currentGlobalTilePosition3 = new Vector2(mousePositionTile2.X + (float)x3, mousePositionTile2.Y + (float)y4);
                    if (!(Game1.currentLocation as GameLocation).isBuildable(currentGlobalTilePosition3))
                    {
                        sheetIndex3++;
                    }
                    spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition3 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex3 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
                }
            }
            /*
            foreach (Point additionalPlacementTile in CurrentBlueprint.GetAdditionalPlacementTiles())
            {
                int x4 = additionalPlacementTile.X;
                int y3 = additionalPlacementTile.Y;
                int sheetIndex4 = CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(x4, y3);
                Vector2 currentGlobalTilePosition4 = new Vector2(mousePositionTile2.X + (float)x4, mousePositionTile2.Y + (float)y3);
                if (!(Game1.currentLocation as GameLocation).isBuildable(currentGlobalTilePosition4))
                {
                    sheetIndex4++;
                }
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition4 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex4 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
            }
            */

            return false;
        }
    }
}
