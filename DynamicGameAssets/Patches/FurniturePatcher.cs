using System;
using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Game;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Furniture"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class FurniturePatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Furniture>(nameof(Furniture.rotate)),
                prefix: this.GetHarmonyMethod(nameof(Before_Rotate))
            );
            harmony.Patch(
                original: this.RequireMethod<Furniture>(nameof(Furniture.updateRotation)),
                prefix: this.GetHarmonyMethod(nameof(Before_UpdateRotation))
            );
            harmony.Patch(
                original: this.RequireMethod<Furniture>(nameof(Furniture.placementAction)),
                postfix: this.GetHarmonyMethod(nameof(After_PlacementAction))
            );
            harmony.Patch(
                 original: this.RequireMethod<Furniture>(nameof(Furniture.drawAtNonTileSpot)),
                 prefix: this.GetHarmonyMethod(nameof(Before_drawAtNonTileSpot))
            );
            harmony.Patch(
                original: this.RequireMethod<Furniture>(nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                postfix: this.GetHarmonyMethod(nameof(After_draw))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Furniture.rotate"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_Rotate(Furniture __instance)
        {
            if (__instance is CustomBasicFurniture furniture)
            {
                if (__instance.rotations.Value > 1)
                {
                    __instance.currentRotation.Value = (__instance.currentRotation.Value + 1) % __instance.rotations.Value;
                    furniture.UpdateRotation();
                }
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Furniture.updateRotation"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_UpdateRotation(Furniture __instance)
        {
            if (__instance is CustomBasicFurniture furniture)
            {
                furniture.UpdateRotation();
                return false;
            }

            return true;
        }

        /// <summary>The method to call after <see cref="Furniture.placementAction"/>.</summary>
        private static void After_PlacementAction(Furniture __instance, GameLocation location)
        {
            // correct lamp/sconce light position
            if (__instance is CustomBasicFurniture furniture)
            {
                if (furniture.furniture_type.Value is Furniture.lamp or Furniture.sconce)
                    furniture.resetOnPlayerEntry(location);
            }
        }

        /// <summary>The method to call before <see cref="Furniture.drawAtNonTileSpot"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_drawAtNonTileSpot(Furniture __instance, SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha)
        {
            if (__instance is CustomBasicFurniture furniture)
            {
                furniture.drawAtNonTileSpot(spriteBatch, location, layerDepth, alpha);
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Furniture.draw"/>.</summary>
        private static void After_draw(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            // If this is a table, and not a DGA table, and there is a DGA object on the table
            if ((__instance.furniture_type == Furniture.table || __instance.furniture_type == Furniture.longTable) &&
                __instance is not CustomBasicFurniture && __instance.heldObject.Value is CustomObject heldObject)
            {
                int xLocation = __instance.boundingBox.Center.X - 32;
                int yLocation = __instance.boundingBox.Center.Y - (__instance.drawHeldObjectLow ? 32 : 85);
                float heldLayerDepth = (float)(__instance.boundingBox.Bottom + 1) / 10000f;
                heldObject.drawWithoutShadow(spriteBatch, xLocation, yLocation, heldLayerDepth, alpha);
            }
        }
    }
}
