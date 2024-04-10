using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Force.DeepCloner;
using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace SpaceCore.VanillaAssetExpansion
{
    public class FruitTreeExtensionData
    {
        public List<Vector2> FruitLocations { get; set; } = new();
    }

    [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.draw))]
    public static class FruitTreeDrawFruitPatch
    {
        public static Vector2 GetFruitOffset(Vector2 tilePos, FruitTree __instance, int index)
        {
            var dict = Game1.content.Load<Dictionary<string, FruitTreeExtensionData>>("spacechase0.SpaceCore/FruitTreeExtensionData");

            Vector2 offset = GetFruitOffset(index);

            if (dict.TryGetValue(__instance.treeId.Value, out var ftData) && ftData.FruitLocations != null)
            {
                offset = ftData.FruitLocations[index];
            }

            return Game1.GlobalToLocal(Game1.viewport, tilePos * Game1.tileSize + offset + new Vector2(0, -192));
        }

        public static Vector2 GetFruitOffset(int index)
        {
            Vector2[] fruits = new[]
            {
                new Vector2( 0, -64 ),
                new Vector2( 64, -32 ),
                new Vector2( 0, 32 )
            };
            return index < fruits.Length ? fruits[index] : fruits[0];
        }

        public static void Prefix(FruitTree __instance, out List<Item> __state)
        {
            __state = new List<Item>();

            var dict = Game1.content.Load<Dictionary<string, FruitTreeExtensionData>>("spacechase0.SpaceCore/FruitTreeExtensionData");

            // Remove the fruits from the tree temporarily if we want to draw them ourselves
            if (dict.TryGetValue(__instance.treeId.Value, out var ftData) && ftData.FruitLocations != null)
            {
                for (int i = 0; i < __instance.fruit.Count; i++)
                {
                    __state.Add(__instance.fruit[i]);
                }
                __instance.fruit.Clear();
            }
        }

        public static void Postfix(FruitTree __instance, SpriteBatch spriteBatch, List<Item> __state)
        {
            var dict = Game1.content.Load<Dictionary<string, FruitTreeExtensionData>>("spacechase0.SpaceCore/FruitTreeExtensionData");

            if (dict.TryGetValue(__instance.treeId.Value, out var ftData) && ftData.FruitLocations != null)
            {
                // Put the fruit back on if we need to
                for (int i = 0; i < __state.Count; i++)
                {
                    __instance.fruit.Add(__state[i]);
                }

                Vector2 tileLocation = __instance.Tile;
                // Draw the fruit if we need to
                for (int i = 0; i < __instance.fruit.Count; i++)
                {
                    ParsedItemData obj = (__instance.struckByLightningCountdown.Value > 0) ? ItemRegistry.GetDataOrErrorItem("(O)382") : ItemRegistry.GetDataOrErrorItem(__instance.fruit[i].QualifiedItemId);
                    Texture2D texture = obj.GetTexture();
                    Rectangle sourceRect = obj.GetSourceRect();
                    spriteBatch.Draw(texture, GetFruitOffset(tileLocation, __instance, i), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)__instance.getBoundingBox().Bottom / 10000f + 0.002f - tileLocation.X / 1000000f);
                }
            }
        }
    }
}