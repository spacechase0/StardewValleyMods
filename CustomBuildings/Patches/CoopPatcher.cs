using System;
using System.Diagnostics.CodeAnalysis;
using CustomBuildings.Framework;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;

namespace CustomBuildings.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Coop"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class CoopPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Coop>("getIndoors"),
                prefix: this.GetHarmonyMethod(nameof(CoopPatcher.Before_GetIndoors))
            );

            harmony.Patch(
                original: this.RequireMethod<Coop>(nameof(Coop.performActionOnConstruction)),
                postfix: this.GetHarmonyMethod(nameof(CoopPatcher.After_PerformActionOnConstruction))
            );

            harmony.Patch(
                original: this.RequireMethod<Coop>(nameof(Coop.performActionOnUpgrade)),
                prefix: this.GetHarmonyMethod(nameof(CoopPatcher.Before_PerformActionOnUpgrade))
            );

            harmony.Patch(
                original: this.RequireMethod<Coop>(nameof(Coop.dayUpdate)),
                postfix: this.GetHarmonyMethod(nameof(CoopPatcher.After_DayUpdate))
            );

            harmony.Patch(
                original: this.RequireMethod<Coop>(nameof(Coop.upgrade)),
                prefix: this.GetHarmonyMethod(nameof(CoopPatcher.Before_Upgrade))
            );

            //harmony.Patch(
            //    original: this.RequireMethod<Coop>(nameof(Coop.getUpgradeSignLocation)),
            //    postfix: this.GetHarmonyMethod(nameof(CoopPatcher.After_GetUpgradeSignLocation))
            //);

            harmony.Patch(
                original: this.RequireMethod<Coop>(nameof(Coop.draw)),
                prefix: this.GetHarmonyMethod(nameof(CoopPatcher.Before_Draw))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Coop.getIndoors"/>.</summary>
        private static bool Before_GetIndoors(Coop __instance, string nameOfIndoorsWithoutUnique, ref GameLocation __result)
        {
            if (!Mod.Instance.Buildings.TryGetValue(nameOfIndoorsWithoutUnique, out BuildingData buildingData))
                return true;

            GameLocation loc = new AnimalHouse($"Maps\\{buildingData.Id}", __instance.buildingType);
            loc.IsFarm = true;
            loc.isStructure.Value = true;
            loc.uniqueName.Value = nameOfIndoorsWithoutUnique + Guid.NewGuid().ToString();
            (loc as AnimalHouse).animalLimit.Value = buildingData.MaxOccupants;
            foreach (var warp in loc.warps)
            {
                warp.TargetX = __instance.humanDoor.X + __instance.tileX;
                warp.TargetY = __instance.humanDoor.Y + __instance.tileY + 1;
            }
            __result = loc;
            return false;
        }

        /// <summary>The method to call after <see cref="Coop.performActionOnConstruction"/>.</summary>
        private static void After_PerformActionOnConstruction(Coop __instance, GameLocation location)
        {
            if (!Mod.Instance.Buildings.TryGetValue(__instance.buildingType, out BuildingData buildingData))
                return;

            __instance.indoors.Value.objects.Remove(new Vector2(3, 3));
            StardewValley.Object @object = new StardewValley.Object(new Vector2(buildingData.FeedHopperX, buildingData.FeedHopperY), 99);
            @object.fragility.Value = 2;
            __instance.indoors.Value.objects.Add(new Vector2(buildingData.FeedHopperX, buildingData.FeedHopperY), @object);
            __instance.daysOfConstructionLeft.Value = buildingData.DaysToConstruct;
        }

        /// <summary>The method to call before <see cref="Coop.performActionOnUpgrade"/>.</summary>
        private static bool Before_PerformActionOnUpgrade(Coop __instance, GameLocation location)
        {
            return !Mod.Instance.Buildings.ContainsKey(__instance.buildingType);
        }

        /// <summary>The method to call after <see cref="Coop.dayUpdate"/>.</summary>
        private static void After_DayUpdate(Coop __instance, int dayOfMonth)
        {
            if (!Mod.Instance.Buildings.TryGetValue(__instance.buildingType, out BuildingData buildingData))
                return;

            if (buildingData.AutoFeedsAnimals)
            {
                int num = Math.Min((__instance.indoors.Value as AnimalHouse).animals.Count() - __instance.indoors.Value.numberOfObjectsWithName("Hay"), (Game1.getLocationFromName("Farm") as Farm).piecesOfHay);
                (Game1.getLocationFromName("Farm") as Farm).piecesOfHay.Value -= num;
                for (int ix = 0; ix < __instance.indoors.Value.map.Layers[0].LayerWidth && num > 0; ++ix)
                {
                    for (int iy = 0; iy < __instance.indoors.Value.map.Layers[0].LayerHeight && num > 0; ++iy)
                    {
                        if (__instance.indoors.Value.doesTileHaveProperty(ix, iy, "Trough", "Back") != null)
                        {
                            __instance.indoors.Value.objects.Add(new Vector2(ix, iy), new StardewValley.Object(178, 1));
                            --num;
                        }
                    }
                }
            }
        }

        /// <summary>The method to call before <see cref="Coop.upgrade"/>.</summary>
        private static bool Before_Upgrade(Coop __instance)
        {
            if (!Mod.Instance.Buildings.TryGetValue(__instance.buildingType, out BuildingData buildingData))
                return true;

            (__instance.indoors.Value as AnimalHouse).animalLimit.Value = buildingData.MaxOccupants;
            if (buildingData.IncubatorX != -1)
            {
                StardewValley.Object @object = new StardewValley.Object(new Vector2(buildingData.IncubatorX, buildingData.IncubatorY), 104);
                @object.fragility.Value = 2;
                __instance.indoors.Value.objects.Add(new Vector2(buildingData.IncubatorX, buildingData.IncubatorY), @object);
            }

            foreach (var rect in buildingData.MoveObjectsWhenUpgradedTo)
            {
                for (int ix = 0; ix < rect.Key.Width; ++ix)
                {
                    for (int iy = 0; iy < rect.Key.Height; ++iy)
                    {
                        __instance.indoors.Value.moveObject(rect.Key.X + ix, rect.Key.Y + iy, rect.Value.X + ix, rect.Value.Y + iy);
                    }
                }
            }

            __instance.humanDoor.X = buildingData.HumanDoorX;
            __instance.humanDoor.Y = buildingData.HumanDoorY;
            __instance.animalDoor.X = buildingData.AnimalDoorX;
            __instance.animalDoor.Y = buildingData.AnimalDoorY;

            foreach (var warp in __instance.indoors.Value.warps)
            {
                warp.TargetX = __instance.humanDoor.X + __instance.tileX;
                warp.TargetY = __instance.humanDoor.Y + __instance.tileY + 1;
            }

            return false;
        }

        // this patch doesn't work - crashes the game from __instance being null (your guess is as good as mine)
        /*
        /// <summary>The method to call after <see cref="Coop.getUpgradeSignLocation"/>.</summary>
        private static bool After_GetUpgradeSignLocation(Coop __instance, ref Vector2 __result)
        {
            if (!Mod.instance.buildings.TryGetValue(__instance.buildingType, out BuildingData buildingData))
                return true;

            __result = new Vector2( __instance.tileX.Value + buildingData.UpgradeSignX, __instance.tileY.Value + buildingData.UpgradeSignY ) * Game1.tileSize

            return false;
        }
        //*/

        /// <summary>The method to call before <see cref="Coop.draw"/>.</summary>
        private static bool Before_Draw(Coop __instance, SpriteBatch b)
        {
            if (__instance.isMoving || !Mod.Instance.Buildings.TryGetValue(__instance.buildingType, out BuildingData buildingData))
                return false;

            if (__instance.daysOfConstructionLeft.Value > 0)
                __instance.drawInConstruction(b);
            else
            {
                __instance.drawShadow(b);
                Vector2 animalDoorBase = new Vector2(__instance.tileX.Value + __instance.animalDoor.X, __instance.tileY.Value + __instance.animalDoor.Y - buildingData.AnimalDoorHeight + 1);
                int animalDoorY = Mod.Instance.Helper.Reflection.GetField<NetInt>(__instance, "yPositionOfAnimalDoor").GetValue().Value;
                float alpha = Mod.Instance.Helper.Reflection.GetField<NetFloat>(__instance, "alpha").GetValue().Value;
                for (int ix = 0; ix < buildingData.AnimalDoorWidth; ++ix)
                {
                    for (int iy = 0; iy < buildingData.AnimalDoorHeight; ++iy)
                    {
                        var pos = Game1.GlobalToLocal(Game1.viewport, (animalDoorBase + new Vector2(ix, iy)) * Game1.tileSize);
                        var rect = new Rectangle(ix * 16, buildingData.BuildingHeight + iy * 16, 16, 16);
                        b.Draw(buildingData.Texture, pos, new Rectangle(rect.X + buildingData.AnimalDoorWidth * 16, rect.Y, rect.Width, rect.Height), Color.White * alpha, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1e-06f);
                    }
                }
                float depth = (float)((__instance.tileY.Value + __instance.tilesHigh.Value) * 64 / 10000.0 + 9.99999974737875E-05);
                for (int ix = 0; ix < buildingData.AnimalDoorWidth; ++ix)
                {
                    for (int iy = 0; iy < buildingData.AnimalDoorHeight; ++iy)
                    {
                        var pos = Game1.GlobalToLocal(Game1.viewport, (animalDoorBase + new Vector2(ix, iy)) * Game1.tileSize) + new Vector2(0, animalDoorY);
                        var rect = new Rectangle(ix * 16, buildingData.BuildingHeight + iy * 16, 16, 16);
                        b.Draw(buildingData.Texture, pos, rect, Color.White * alpha, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth);
                    }
                }
                b.Draw(buildingData.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.tileX.Value, __instance.tileY.Value + __instance.tilesHigh.Value) * Game1.tileSize), new Rectangle(0, 0, buildingData.TileWidth * 16, buildingData.BuildingHeight), Color.White * alpha, 0, new Vector2(0, buildingData.BuildingHeight), Game1.pixelZoom, SpriteEffects.None, depth + 0.00001f);
                if (__instance.daysUntilUpgrade.Value <= 0)
                    return false;
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.tileX.Value + buildingData.UpgradeSignX, __instance.tileY.Value + buildingData.UpgradeSignY) * Game1.tileSize), new Rectangle(367, 309, 16, 15), Color.White * alpha, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth + 0.00001f);
            }

            return false;
        }
    }
}
