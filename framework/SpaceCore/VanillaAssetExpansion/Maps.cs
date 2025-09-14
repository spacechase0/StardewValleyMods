using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;

namespace SpaceCore.VanillaAssetExpansion
{
    [HarmonyPatch(typeof(GameLocation), "resetLocalState")]
    public static class MapResetLocalStateWithColoredLightsPatch
    {
        public static void Postfix(GameLocation __instance)
        {
            if (__instance.ignoreLights.Value)
                return;

            string[] colLights = __instance.GetMapPropertySplitBySpaces("spacechase0.SpaceCore_ColoredLights");
            for (int i = 0; i < colLights.Length; i += 7)
            {
                if (!ArgUtility.TryGetPoint(colLights, i, out var tile, out string error) ||
                     !ArgUtility.TryGet(colLights, i + 2, out string texture, out error) ||
                     !ArgUtility.TryGetInt(colLights, i + 3, out int radiusMultiplier, out error) ||
                     !ArgUtility.TryGetInt(colLights, i + 4, out int colorR, out error) ||
                     !ArgUtility.TryGetInt(colLights, i + 5, out int colorG, out error) ||
                     !ArgUtility.TryGetInt(colLights, i + 6, out int colorB, out error))
                {
                    __instance.LogMapPropertyError("spacechase0.SpaceCore_ColoredLights", colLights, error);
                }
                else
                {
                    int texInd = 0;
                    int.TryParse(texture, out texInd);

                    string id = $"{__instance.NameOrUniqueName}_{tile.X}_{tile.Y}_Colored";
                    Game1.currentLightSources.Add(new LightSource(id, 0, tile.ToVector2() * Game1.tileSize + new Vector2(32, 32), radiusMultiplier, LightSource.LightContext.MapLight)
                    {
                        color = { new Color(255 - colorR, 255 - colorG, 255 - colorB) },
                        lightTexture = texInd == 0 ? Game1.content.Load<Texture2D>(texture) : null,
                    });
                    Game1.currentLightSources[id].textureIndex.Value = texInd; // Set this later so that the texture refreshes if we null-ed it
                }
            }
        }
    }
}
