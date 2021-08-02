using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace BuildableLocationsFramework.Patches
{
    /// <summary>Applies Harmony patches to <see cref="FarmAnimal"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class FarmAnimalPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<FarmAnimal>(nameof(FarmAnimal.updateWhenNotCurrentLocation)),
                prefix: this.GetHarmonyMethod(nameof(Before_UpdateWhenNotCurrentLocation))
            );

            harmony.Patch(
                original: this.RequireMethod<FarmAnimal>(nameof(FarmAnimal.updateWhenCurrentLocation)),
                prefix: this.GetHarmonyMethod(nameof(Before_UpdateWhenCurrentLocation))
            );

            harmony.Patch(
                original: this.RequireMethod<FarmAnimal>("behaviors"),
                prefix: this.GetHarmonyMethod(nameof(Before_Behaviors))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="FarmAnimal.updateWhenNotCurrentLocation"/>.</summary>
        private static bool Before_UpdateWhenNotCurrentLocation(FarmAnimal __instance, Building currentBuilding, GameTime time, GameLocation environment)
        {
            Mod.Instance.Helper.Reflection.GetField<NetEvent1Field<int, NetInt>>(__instance, "doFarmerPushEvent").GetValue().Poll();
            Mod.Instance.Helper.Reflection.GetField<NetEvent0>(__instance, "doBuildingPokeEvent").GetValue().Poll();
            if (!Game1.shouldTimePass())
                return false;
            __instance.update(time, environment, __instance.myID.Value, false);
            if (!Game1.IsMasterGame)
                return false;
            if (currentBuilding != null && Game1.random.NextDouble() < 0.002 && (currentBuilding.animalDoorOpen.Value && Game1.timeOfDay < 1630) && (!Game1.isRaining && !Game1.currentSeason.Equals("winter") && environment.farmers.Count == 0))
            {
                GameLocation locationFromName = Mod.FindOutdoorsOf(currentBuilding);
                IAnimalLocation locationFromName_animals = (IAnimalLocation)locationFromName;
                if (locationFromName.isCollidingPosition(new Rectangle((currentBuilding.tileX.Value + currentBuilding.animalDoor.X) * 64 + 2, (currentBuilding.tileY.Value + currentBuilding.animalDoor.Y) * 64 + 2, (__instance.isCoopDweller() ? 64 : 128) - 4, 60), Game1.viewport, false, 0, false, __instance, false) || locationFromName.isCollidingPosition(new Rectangle((currentBuilding.tileX.Value + currentBuilding.animalDoor.X) * 64 + 2, (currentBuilding.tileY.Value + currentBuilding.animalDoor.Y + 1) * 64 + 2, (__instance.isCoopDweller() ? 64 : 128) - 4, 60), Game1.viewport, false, 0, false, __instance, false))
                    return false;
                if (locationFromName_animals.Animals.ContainsKey(__instance.myID.Value))
                {
                    for (int index = locationFromName_animals.Animals.Count() - 1; index >= 0; --index)
                    {
                        if (locationFromName_animals.Animals.Pairs.ElementAt(index).Key.Equals(__instance.myID.Value))
                        {
                            locationFromName_animals.Animals.Remove(__instance.myID.Value);
                            break;
                        }
                    }
                }
                (currentBuilding.indoors.Value as AnimalHouse).animals.Remove(__instance.myID.Value);
                locationFromName_animals.Animals.Add(__instance.myID.Value, __instance);
                __instance.faceDirection(2);
                __instance.SetMovingDown(true);
                __instance.Position = new Vector2(currentBuilding.getRectForAnimalDoor().X, (currentBuilding.tileY.Value + currentBuilding.animalDoor.Y) * 64 - (__instance.Sprite.getHeight() * 4 - __instance.GetBoundingBox().Height) + 32);
                if (FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick)
                {
                    ++FarmAnimal.NumPathfindingThisTick;
                    __instance.controller = new PathFindController(__instance, locationFromName, FarmAnimal.grassEndPointFunction, Game1.random.Next(4), false, FarmAnimal.behaviorAfterFindingGrassPatch, 200, Point.Zero);
                }
                if (__instance.controller?.pathToEndPoint == null || __instance.controller.pathToEndPoint.Count < 3)
                {
                    __instance.SetMovingDown(true);
                    __instance.controller = null;
                }
                else
                {
                    __instance.faceDirection(2);
                    __instance.Position = new Vector2(__instance.controller.pathToEndPoint.Peek().X * 64, __instance.controller.pathToEndPoint.Peek().Y * 64 - (__instance.Sprite.getHeight() * 4 - __instance.GetBoundingBox().Height) + 16);
                    if (!__instance.isCoopDweller())
                        __instance.position.X -= 32f;
                }
                __instance.noWarpTimer = 3000;
                --currentBuilding.currentOccupants.Value;
                if (Utility.isOnScreen(__instance.getTileLocationPoint(), 192, locationFromName))
                    locationFromName.localSound("sandyStep");
                if (environment.isTileOccupiedByFarmer(__instance.getTileLocation()) != null)
                    environment.isTileOccupiedByFarmer(__instance.getTileLocation()).TemporaryPassableTiles.Add(__instance.GetBoundingBox());
            }
            Mod.Instance.Helper.Reflection.GetMethod(__instance, "behaviors").Invoke(time, environment);
            return false;
        }

        /// <summary>The method to call before <see cref="FarmAnimal.updateWhenCurrentLocation"/>.</summary>
        private static bool Before_UpdateWhenCurrentLocation(FarmAnimal __instance, GameTime time, GameLocation location, ref bool __result)
        {
            if (!Game1.shouldTimePass())
                return false;
            if (__instance.health.Value <= 0)
                return true;
            Mod.Instance.Helper.Reflection.GetField<NetEvent0>(__instance, "doBuildingPokeEvent").GetValue().Poll();
            if (__instance.hitGlowTimer > 0)
                __instance.hitGlowTimer -= time.ElapsedGameTime.Milliseconds;
            if (__instance.Sprite.CurrentAnimation != null)
            {
                if (__instance.Sprite.animateOnce(time))
                    __instance.Sprite.CurrentAnimation = null;
                return false;
            }
            __instance.update(time, location, __instance.myID.Value, false);
            if (Game1.IsMasterGame && Mod.Instance.Helper.Reflection.GetMethod(__instance, "behaviors").Invoke<bool>(time, location) || __instance.Sprite.CurrentAnimation != null)
                return false;
            if (__instance.controller?.timerSinceLastCheckPoint > 10000)
            {
                __instance.controller = null;
                __instance.Halt();
            }
            if (location is BuildableGameLocation && location is IAnimalLocation animalLocation && __instance.noWarpTimer <= 0)
            {
                Building building = __instance.home;
                if (building != null && Game1.IsMasterGame && building.getRectForAnimalDoor().Contains(__instance.GetBoundingBox().Center.X, __instance.GetBoundingBox().Top))
                {
                    if (Utility.isOnScreen(__instance.getTileLocationPoint(), 192, location))
                        location.localSound("dwoop");
                    animalLocation.Animals.Remove(__instance.myID.Value);
                    (building.indoors.Value as AnimalHouse).animals[__instance.myID.Value] = __instance;
                    __instance.setRandomPosition(building.indoors.Value);
                    __instance.faceDirection(Game1.random.Next(4));
                    __instance.controller = null;
                    return true;
                }
            }
            __instance.noWarpTimer = Math.Max(0, __instance.noWarpTimer - time.ElapsedGameTime.Milliseconds);
            if (__instance.pauseTimer > 0)
                __instance.pauseTimer -= time.ElapsedGameTime.Milliseconds;
            if (Game1.timeOfDay >= 2000)
            {
                __instance.Sprite.currentFrame = __instance.buildingTypeILiveIn.Contains("Coop") ? 16 : 12;
                __instance.Sprite.UpdateSourceRect();
                __instance.FacingDirection = 2;
                if (!__instance.isEmoting && Game1.random.NextDouble() < 0.002)
                    __instance.doEmote(24);
            }
            else if (__instance.pauseTimer <= 0)
            {
                if (Game1.random.NextDouble() < 0.001 && __instance.age.Value >= __instance.ageWhenMature.Value && (Game1.gameMode == 3 && __instance.sound.Value != null) && Utility.isOnScreen(__instance.Position, 192))
                    __instance.makeSound();
                if (!Game1.IsClient && Game1.random.NextDouble() < 0.007 && __instance.uniqueFrameAccumulator == -1)
                {
                    int direction = Game1.random.Next(5);
                    if (direction != (__instance.FacingDirection + 2) % 4)
                    {
                        if (direction < 4)
                        {
                            int facingDirection = __instance.FacingDirection;
                            __instance.faceDirection(direction);
                            if (!location.IsOutdoors && location.isCollidingPosition(__instance.nextPosition(direction), Game1.viewport, __instance))
                            {
                                __instance.faceDirection(facingDirection);
                                return false;
                            }
                        }
                        switch (direction)
                        {
                            case 0:
                                __instance.SetMovingUp(true);
                                break;
                            case 1:
                                __instance.SetMovingRight(true);
                                break;
                            case 2:
                                __instance.SetMovingDown(true);
                                break;
                            case 3:
                                __instance.SetMovingLeft(true);
                                break;
                            default:
                                __instance.Halt();
                                __instance.Sprite.StopAnimation();
                                break;
                        }
                    }
                    else if (__instance.noWarpTimer <= 0)
                    {
                        __instance.Halt();
                        __instance.Sprite.StopAnimation();
                    }
                }
                if (!Game1.IsClient && __instance.isMoving() && (Game1.random.NextDouble() < 0.014 && __instance.uniqueFrameAccumulator == -1))
                {
                    __instance.Halt();
                    __instance.Sprite.StopAnimation();
                    if (Game1.random.NextDouble() < 0.75)
                    {
                        __instance.uniqueFrameAccumulator = 0;
                        if (__instance.buildingTypeILiveIn.Contains("Coop"))
                        {
                            __instance.Sprite.currentFrame = __instance.FacingDirection switch
                            {
                                0 => 20,
                                1 => 18,
                                2 => 16,
                                3 => 22,
                                _ => __instance.Sprite.currentFrame
                            };
                        }
                        else if (__instance.buildingTypeILiveIn.Contains("Barn"))
                        {
                            __instance.Sprite.currentFrame = __instance.FacingDirection switch
                            {
                                0 => 15,
                                1 => 14,
                                2 => 13,
                                3 => 14,
                                _ => __instance.Sprite.currentFrame
                            };
                        }
                    }
                    __instance.Sprite.UpdateSourceRect();
                }
                if (__instance.uniqueFrameAccumulator != -1 && !Game1.IsClient)
                {
                    __instance.uniqueFrameAccumulator += time.ElapsedGameTime.Milliseconds;
                    if (__instance.uniqueFrameAccumulator > 500)
                    {
                        if (__instance.buildingTypeILiveIn.Contains("Coop"))
                            __instance.Sprite.currentFrame = __instance.Sprite.currentFrame + 1 - __instance.Sprite.currentFrame % 2 * 2;
                        else if (__instance.Sprite.currentFrame > 12)
                        {
                            __instance.Sprite.currentFrame = (__instance.Sprite.currentFrame - 13) * 4;
                        }
                        else
                        {
                            __instance.Sprite.currentFrame = __instance.FacingDirection switch
                            {
                                0 => 15,
                                1 => 14,
                                2 => 13,
                                3 => 14,
                                _ => __instance.Sprite.currentFrame
                            };
                        }
                        __instance.uniqueFrameAccumulator = 0;
                        if (Game1.random.NextDouble() < 0.4)
                            __instance.uniqueFrameAccumulator = -1;
                    }
                }
                else if (!Game1.IsClient)
                    __instance.MovePosition(time, Game1.viewport, location);
            }
            return false;
        }

        /// <summary>The method to call before <see cref="FarmAnimal.behaviors"/>.</summary>
        public static bool Before_Behaviors(FarmAnimal __instance, GameTime time, GameLocation location, ref bool __result)
        {
            if (__instance.home == null)
            {
                __result = false;
                return false;
            }
            if (__instance.isEating.Value)
            {
                if (__instance.home != null && __instance.home.getRectForAnimalDoor().Intersects(__instance.GetBoundingBox()))
                {
                    FarmAnimal.behaviorAfterFindingGrassPatch(__instance, location);
                    __instance.isEating.Value = false;
                    __instance.Halt();
                    __result = false;
                    return false;
                }
                if (__instance.buildingTypeILiveIn.Contains("Barn"))
                {
                    __instance.Sprite.Animate(time, 16, 4, 100f);
                    if (__instance.Sprite.currentFrame >= 20)
                    {
                        __instance.isEating.Value = false;
                        __instance.Sprite.loop = true;
                        __instance.Sprite.currentFrame = 0;
                        __instance.faceDirection(2);
                    }
                }
                else
                {
                    __instance.Sprite.Animate(time, 24, 4, 100f);
                    if (__instance.Sprite.currentFrame >= 28)
                    {
                        __instance.isEating.Value = false;
                        __instance.Sprite.loop = true;
                        __instance.Sprite.currentFrame = 0;
                        __instance.faceDirection(2);
                    }
                }
                __result = true;
                return false;
            }
            if (!Game1.IsClient)
            {
                if (__instance.controller != null)
                {
                    __result = true;
                    return false;
                }
                if (location.IsOutdoors && __instance.fullness.Value < 195 && (Game1.random.NextDouble() < 0.002 && FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick))
                {
                    ++FarmAnimal.NumPathfindingThisTick;
                    __instance.controller = new PathFindController(__instance, location, FarmAnimal.grassEndPointFunction, -1, false, FarmAnimal.behaviorAfterFindingGrassPatch, 200, Point.Zero);
                }
                if (Game1.timeOfDay >= 1700 && location.IsOutdoors && (__instance.controller == null && Game1.random.NextDouble() < 0.002))
                {
                    if (location.farmers.Count == 0)
                    {
                        (location as Farm).animals.Remove(__instance.myID.Value);
                        (__instance.home.indoors.Value as AnimalHouse).animals.Add(__instance.myID.Value, __instance);
                        __instance.setRandomPosition(__instance.home.indoors.Value);
                        __instance.faceDirection(Game1.random.Next(4));
                        __instance.controller = null;
                        __result = true;
                        return false;
                    }
                    if (FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick)
                    {
                        ++FarmAnimal.NumPathfindingThisTick;
                        __instance.controller = new PathFindController(__instance, location, PathFindController.isAtEndPoint, 0, false, null, 200, new Point(__instance.home.tileX.Value + __instance.home.animalDoor.X, __instance.home.tileY.Value + __instance.home.animalDoor.Y));
                    }
                }
                if (location.IsOutdoors && !Game1.isRaining && (!Game1.currentSeason.Equals("winter") && __instance.currentProduce.Value != -1) && (__instance.age.Value >= __instance.ageWhenMature.Value && __instance.type.Value.Contains("Pig") && Game1.random.NextDouble() < 0.0002))
                {
                    Rectangle boundingBox = __instance.GetBoundingBox();
                    for (int corner = 0; corner < 4; ++corner)
                    {
                        Vector2 cornersOfThisRectangle = Utility.getCornersOfThisRectangle(ref boundingBox, corner);
                        Vector2 key = new Vector2((int)(cornersOfThisRectangle.X / 64.0), (int)(cornersOfThisRectangle.Y / 64.0));
                        if (location.terrainFeatures.ContainsKey(key) || location.objects.ContainsKey(key))
                        {
                            __result = false;
                            return false;
                        }
                    }
                    if (Game1.player.currentLocation.Equals(location))
                    {
                        DelayedAction.playSoundAfterDelay("dirtyHit", 450);
                        DelayedAction.playSoundAfterDelay("dirtyHit", 900);
                        DelayedAction.playSoundAfterDelay("dirtyHit", 1350);
                    }
                    if (location.Equals(Game1.currentLocation))
                    {
                        var findTruffleDelegate = (AnimatedSprite.endOfAnimationBehavior)Delegate.CreateDelegate(typeof(AnimatedSprite.endOfAnimationBehavior), Mod.Instance.Helper.Reflection.GetMethod(__instance, "findTruffle").MethodInfo);

                        switch (__instance.FacingDirection)
                        {
                            case 0:
                                __instance.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                                {
                                    new(9, 250),
                                    new(11, 250),
                                    new(9, 250),
                                    new(11, 250),
                                    new(9, 250),
                                    new(11, 250, false, false, findTruffleDelegate)
                                });
                                break;
                            case 1:
                                __instance.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                                {
                                    new(5, 250),
                                    new(7, 250),
                                    new(5, 250),
                                    new(7, 250),
                                    new(5, 250),
                                    new(7, 250, false, false, findTruffleDelegate)
                                });
                                break;
                            case 2:
                                __instance.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                                {
                                    new(1, 250),
                                    new(3, 250),
                                    new(1, 250),
                                    new(3, 250),
                                    new(1, 250),
                                    new(3, 250, false, false, findTruffleDelegate)
                                });
                                break;
                            case 3:
                                __instance.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                                {
                                    new(5, 250, false, true),
                                    new(7, 250, false, true),
                                    new(5, 250, false, true),
                                    new(7, 250, false, true),
                                    new(5, 250, false, true),
                                    new(7, 250, false, true, findTruffleDelegate)
                                });
                                break;
                        }
                        __instance.Sprite.loop = false;
                    }
                    else
                        Mod.Instance.Helper.Reflection.GetMethod(__instance, "findTruffle").Invoke(Game1.player);
                }
            }
            __result = false;
            return false;
        }
    }
}
