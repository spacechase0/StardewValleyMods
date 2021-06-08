using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Buildings;

namespace CustomBuildings.Overrides
{
    public static class CoopPatches
    {
        public static bool getIndoors_Prefix(Coop __instance, string nameOfIndoorsWithoutUnique, ref GameLocation __result)
        {
            if (!Mod.instance.buildings.ContainsKey(nameOfIndoorsWithoutUnique))
            {
                return true;
            }

            var bdata = Mod.instance.buildings[nameOfIndoorsWithoutUnique];

            GameLocation loc = new AnimalHouse("Maps\\" + bdata.Id, __instance.buildingType);
            loc.IsFarm = true;
            loc.isStructure.Value = true;
            loc.uniqueName.Value = nameOfIndoorsWithoutUnique + Guid.NewGuid().ToString();
            (loc as AnimalHouse).animalLimit.Value = bdata.MaxOccupants;
            foreach (var warp in loc.warps)
            {
                warp.TargetX = __instance.humanDoor.X + __instance.tileX;
                warp.TargetY = __instance.humanDoor.Y + __instance.tileY + 1;
            }
            __result = loc;
            return false;
        }

        public static void performActionOnConstruction_Postfix(Coop __instance, GameLocation location)
        {
            if (!Mod.instance.buildings.ContainsKey(__instance.buildingType))
            {
                return;
            }

            var bdata = Mod.instance.buildings[__instance.buildingType];
            __instance.indoors.Value.objects.Remove(new Microsoft.Xna.Framework.Vector2(3, 3));
            StardewValley.Object @object = new StardewValley.Object(new Vector2(bdata.FeedHopperX, bdata.FeedHopperY), 99, false);
            @object.fragility.Value = 2;
            __instance.indoors.Value.objects.Add(new Vector2(bdata.FeedHopperX, bdata.FeedHopperY), @object);
            __instance.daysOfConstructionLeft.Value = bdata.DaysToConstruct;
        }

        public static bool performActionOnUpgrade_Prefix(Coop __instance, GameLocation location)
        {
            if (!Mod.instance.buildings.ContainsKey(__instance.buildingType))
            {
                return true;
            }

            var bdata = Mod.instance.buildings[__instance.buildingType];

            return false;
        }

        public static void dayUpdate_Postfix(Coop __instance, int dayOfMonth)
        {
            if (!Mod.instance.buildings.ContainsKey(__instance.buildingType))
            {
                return;
            }

            var bdata = Mod.instance.buildings[__instance.buildingType];

            if (bdata.AutoFeedsAnimals)
            {
                int num = Math.Min((__instance.indoors.Value as AnimalHouse).animals.Count() - __instance.indoors.Value.numberOfObjectsWithName("Hay"), (Game1.getLocationFromName("Farm") as Farm).piecesOfHay);
                (Game1.getLocationFromName("Farm") as Farm).piecesOfHay.Value -= num;
                for (int ix = 0; ix < __instance.indoors.Value.map.Layers[0].LayerWidth && num > 0; ++ix)
                {
                    for (int iy = 0; iy < __instance.indoors.Value.map.Layers[0].LayerHeight && num > 0; ++iy)
                    {
                        if (__instance.indoors.Value.doesTileHaveProperty(ix, iy, "Trough", "Back") != null)
                        {
                            __instance.indoors.Value.objects.Add(new Vector2(ix, iy), new StardewValley.Object(178, 1, false, -1, 0));
                            --num;
                        }
                    }
                }
            }
        }

        public static bool upgrade_Prefix(Coop __instance)
        {
            if (!Mod.instance.buildings.ContainsKey(__instance.buildingType))
            {
                return true;
            }

            var bdata = Mod.instance.buildings[__instance.buildingType];

            (__instance.indoors.Value as AnimalHouse).animalLimit.Value = bdata.MaxOccupants;
            if (bdata.IncubatorX != -1)
            {
                StardewValley.Object @object = new StardewValley.Object(new Vector2(bdata.IncubatorX, bdata.IncubatorY), 104, false);
                @object.fragility.Value = 2;
                __instance.indoors.Value.objects.Add(new Vector2(bdata.IncubatorX, bdata.IncubatorY), @object);
            }

            foreach (var rect in bdata.MoveObjectsWhenUpgradedTo)
            {
                for (int ix = 0; ix < rect.Key.Width; ++ix)
                {
                    for (int iy = 0; iy < rect.Key.Height; ++iy)
                    {
                        __instance.indoors.Value.moveObject(rect.Key.X + ix, rect.Key.Y + iy, rect.Value.X + ix, rect.Value.Y + iy);
                    }
                }
            }

            __instance.humanDoor.X = bdata.HumanDoorX;
            __instance.humanDoor.Y = bdata.HumanDoorY;
            __instance.animalDoor.X = (int)bdata.AnimalDoorX;
            __instance.animalDoor.Y = (int)bdata.AnimalDoorY;

            foreach (var warp in __instance.indoors.Value.warps)
            {
                warp.TargetX = __instance.humanDoor.X + __instance.tileX;
                warp.TargetY = __instance.humanDoor.Y + __instance.tileY + 1;
            }

            return false;
        }

        // this patch doesn't work - crashes the game from __instance being null (your guess is as good as mine)
        /*
        public static bool getUpgradeSignLocation_Postfix(Coop __instance, ref Vector2 __result)
        {
            if (!Mod.instance.buildings.ContainsKey(__instance.buildingType))
            {
                return true;
            }

            var bdata = Mod.instance.buildings[__instance.buildingType];
            
            __result = new Vector2( __instance.tileX.Value + bdata.UpgradeSignX, __instance.tileY.Value + bdata.UpgradeSignY ) * Game1.tileSize

            return false;
        }
        //*/

        public static bool draw_Prefix(Coop __instance, SpriteBatch b)
        {
            if (!Mod.instance.buildings.ContainsKey(__instance.buildingType))
            {
                return false;
            }
            var bdata = Mod.instance.buildings[__instance.buildingType];

            if (__instance.isMoving)
                return false;

            if (__instance.daysOfConstructionLeft.Value > 0)
            {
                __instance.drawInConstruction(b);
            }
            else
            {
                __instance.drawShadow(b, -1, -1);
                Vector2 animalDoorBase = new Vector2(__instance.tileX.Value + __instance.animalDoor.X, __instance.tileY.Value + __instance.animalDoor.Y - bdata.AnimalDoorHeight + 1);
                int animalDoorY = Mod.instance.Helper.Reflection.GetField<NetInt>(__instance, "yPositionOfAnimalDoor").GetValue().Value;
                float alpha = Mod.instance.Helper.Reflection.GetField<NetFloat>(__instance, "alpha").GetValue().Value;
                for (int ix = 0; ix < bdata.AnimalDoorWidth; ++ix)
                {
                    for (int iy = 0; iy < bdata.AnimalDoorHeight; ++iy)
                    {
                        var pos = Game1.GlobalToLocal(Game1.viewport, (animalDoorBase + new Vector2(ix, iy)) * Game1.tileSize);
                        var rect = new Rectangle(ix * 16, bdata.BuildingHeight + iy * 16, 16, 16);
                        b.Draw(bdata.texture, pos, new Rectangle(rect.X + bdata.AnimalDoorWidth * 16, rect.Y, rect.Width, rect.Height), Color.White * alpha, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1e-06f);
                    }
                }
                float depth = (float)((__instance.tileY.Value + __instance.tilesHigh.Value) * 64 / 10000.0 + 9.99999974737875E-05);
                for (int ix = 0; ix < bdata.AnimalDoorWidth; ++ix)
                {
                    for (int iy = 0; iy < bdata.AnimalDoorHeight; ++iy)
                    {
                        var pos = Game1.GlobalToLocal(Game1.viewport, (animalDoorBase + new Vector2(ix, iy)) * Game1.tileSize) + new Vector2(0, animalDoorY);
                        var rect = new Rectangle(ix * 16, bdata.BuildingHeight + iy * 16, 16, 16);
                        b.Draw(bdata.texture, pos, rect, Color.White * alpha, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth);
                    }
                }
                b.Draw(bdata.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.tileX.Value, __instance.tileY.Value + __instance.tilesHigh.Value) * Game1.tileSize), new Rectangle(0, 0, bdata.TileWidth * 16, bdata.BuildingHeight), Color.White * alpha, 0, new Vector2(0, bdata.BuildingHeight), Game1.pixelZoom, SpriteEffects.None, depth + 0.00001f);
                if (__instance.daysUntilUpgrade.Value <= 0)
                    return false;
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.tileX.Value + bdata.UpgradeSignX, __instance.tileY.Value + bdata.UpgradeSignY) * Game1.tileSize), new Rectangle(367, 309, 16, 15), Color.White * alpha, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth + 0.00001f);
            }

            return false;
        }
    }
}
