using System;
using System.Collections.Generic;
using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace BuildableLocationsFramework.Patches
{
    [HarmonyPatch( typeof( FarmAnimal ), nameof( FarmAnimal.updateWhenNotCurrentLocation ) )]
    public static class FarmAnimalUpdateWhenNotCurrentLocationPatch
    {
        public static bool Prefix( FarmAnimal __instance, Building currentBuilding, GameTime time, GameLocation environment )
        {
            Mod.instance.Helper.Reflection.GetField<NetEvent1Field<int, NetInt>>(__instance, "doFarmerPushEvent" ).GetValue().Poll();
            Mod.instance.Helper.Reflection.GetField<NetEvent0>( __instance, "doBuildingPokeEvent" ).GetValue().Poll();
            if ( !Game1.shouldTimePass() )
                return false;
            __instance.update( time, environment, ( long ) __instance.myID, false );
            if ( !Game1.IsMasterGame )
                return false;
            if ( currentBuilding != null && Game1.random.NextDouble() < 0.002 && ( ( bool ) ( NetFieldBase<bool, NetBool> ) currentBuilding.animalDoorOpen && Game1.timeOfDay < 1630 ) && ( !Game1.isRaining && !Game1.currentSeason.Equals( "winter" ) && environment.farmers.Count == 0 ) )
            {
                GameLocation locationFromName = Mod.findOutdoorsOf( currentBuilding );
                IAnimalLocation locationFromName_animals = (IAnimalLocation)locationFromName;
                if ( locationFromName.isCollidingPosition( new Microsoft.Xna.Framework.Rectangle( ( ( int ) ( NetFieldBase<int, NetInt> ) currentBuilding.tileX + currentBuilding.animalDoor.X ) * 64 + 2, ( ( int ) ( NetFieldBase<int, NetInt> ) currentBuilding.tileY + currentBuilding.animalDoor.Y ) * 64 + 2, ( __instance.isCoopDweller() ? 64 : 128 ) - 4, 60 ), Game1.viewport, false, 0, false, ( Character ) __instance, false, false, false ) || locationFromName.isCollidingPosition( new Microsoft.Xna.Framework.Rectangle( ( ( int ) ( NetFieldBase<int, NetInt> ) currentBuilding.tileX + currentBuilding.animalDoor.X ) * 64 + 2, ( ( int ) ( NetFieldBase<int, NetInt> ) currentBuilding.tileY + currentBuilding.animalDoor.Y + 1 ) * 64 + 2, ( __instance.isCoopDweller() ? 64 : 128 ) - 4, 60 ), Game1.viewport, false, 0, false, ( Character ) __instance, false, false, false ) )
                    return false;
                if ( locationFromName_animals.Animals.ContainsKey( ( long ) __instance.myID ) )
                {
                    for ( int index = locationFromName_animals.Animals.Count() - 1; index >= 0; --index )
                    {
                        if ( locationFromName_animals.Animals.Pairs.ElementAt( index ).Key.Equals( ( long ) __instance.myID ) )
                        {
                            locationFromName_animals.Animals.Remove( ( long ) __instance.myID );
                            break;
                        }
                    }
                }
                ( currentBuilding.indoors.Value as AnimalHouse ).animals.Remove( ( long ) __instance.myID );
                locationFromName_animals.Animals.Add( ( long ) __instance.myID, __instance );
                __instance.faceDirection( 2 );
                __instance.SetMovingDown( true );
                __instance.Position = new Vector2( ( float ) currentBuilding.getRectForAnimalDoor().X, ( float ) ( ( ( int ) ( NetFieldBase<int, NetInt> ) currentBuilding.tileY + currentBuilding.animalDoor.Y ) * 64 - ( __instance.Sprite.getHeight() * 4 - __instance.GetBoundingBox().Height ) + 32 ) );
                if ( FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick )
                {
                    ++FarmAnimal.NumPathfindingThisTick;
                    __instance.controller = new PathFindController( ( Character ) __instance, ( GameLocation ) locationFromName, new PathFindController.isAtEnd( FarmAnimal.grassEndPointFunction ), Game1.random.Next( 4 ), false, new PathFindController.endBehavior( FarmAnimal.behaviorAfterFindingGrassPatch ), 200, Point.Zero, true );
                }
                if ( __instance.controller == null || __instance.controller.pathToEndPoint == null || __instance.controller.pathToEndPoint.Count < 3 )
                {
                    __instance.SetMovingDown( true );
                    __instance.controller = ( PathFindController ) null;
                }
                else
                {
                    __instance.faceDirection( 2 );
                    __instance.Position = new Vector2( ( float ) ( __instance.controller.pathToEndPoint.Peek().X * 64 ), ( float ) ( __instance.controller.pathToEndPoint.Peek().Y * 64 - ( __instance.Sprite.getHeight() * 4 - __instance.GetBoundingBox().Height ) + 16 ) );
                    if ( !__instance.isCoopDweller() )
                        __instance.position.X -= 32f;
                }
                __instance.noWarpTimer = 3000;
                --currentBuilding.currentOccupants.Value;
                if ( Utility.isOnScreen( __instance.getTileLocationPoint(), 192, ( GameLocation ) locationFromName ) )
                    locationFromName.localSound( "sandyStep" );
                if ( environment.isTileOccupiedByFarmer( __instance.getTileLocation() ) != null )
                    environment.isTileOccupiedByFarmer( __instance.getTileLocation() ).TemporaryPassableTiles.Add( __instance.GetBoundingBox() );
            }
            Mod.instance.Helper.Reflection.GetMethod( __instance, "behaviors" ).Invoke( time, environment );
            return false;
        }
    }

    [HarmonyPatch( typeof( FarmAnimal ), nameof( FarmAnimal.updateWhenCurrentLocation ) )]
    public static class FarmAnimalUpdateWhenCurrentLocationPatch
    {
        public static bool Prefix( FarmAnimal __instance, GameTime time, GameLocation location, ref bool __result )
        {
            if ( !Game1.shouldTimePass() )
                return false;
            if ( __instance.health.Value <= 0 )
                return true;
            Mod.instance.Helper.Reflection.GetField< NetEvent0>( __instance, "doBuildingPokeEvent" ).GetValue().Poll();
            if ( __instance.hitGlowTimer > 0 )
                __instance.hitGlowTimer -= time.ElapsedGameTime.Milliseconds;
            if ( __instance.Sprite.CurrentAnimation != null )
            {
                if ( __instance.Sprite.animateOnce( time ) )
                    __instance.Sprite.CurrentAnimation = ( List<FarmerSprite.AnimationFrame> ) null;
                return false;
            }
            __instance.update( time, location, ( long ) __instance.myID, false );
            if ( Game1.IsMasterGame && Mod.instance.Helper.Reflection.GetMethod( __instance, "behaviors" ).Invoke< bool >( time, location ) || __instance.Sprite.CurrentAnimation != null )
                return false;
            if ( __instance.controller != null && __instance.controller.timerSinceLastCheckPoint > 10000 )
            {
                __instance.controller = ( PathFindController ) null;
                __instance.Halt();
            }
            if ( location is BuildableGameLocation && location is IAnimalLocation && __instance.noWarpTimer <= 0 )
            {
                Building building = __instance.home;
                if ( building != null && Game1.IsMasterGame && building.getRectForAnimalDoor().Contains( __instance.GetBoundingBox().Center.X, __instance.GetBoundingBox().Top ) )
                {
                    if ( Utility.isOnScreen( __instance.getTileLocationPoint(), 192, location ) )
                        location.localSound( "dwoop" );
                    ( ( IAnimalLocation ) location ).Animals.Remove( ( long ) __instance.myID );
                    ( building.indoors.Value as AnimalHouse ).animals[ ( long ) __instance.myID ] = __instance;
                    __instance.setRandomPosition( ( GameLocation ) ( NetFieldBase<GameLocation, NetRef<GameLocation>> ) building.indoors );
                    __instance.faceDirection( Game1.random.Next( 4 ) );
                    __instance.controller = ( PathFindController ) null;
                    return true;
                }
            }
            __instance.noWarpTimer = Math.Max( 0, __instance.noWarpTimer - time.ElapsedGameTime.Milliseconds );
            if ( __instance.pauseTimer > 0 )
                __instance.pauseTimer -= time.ElapsedGameTime.Milliseconds;
            if ( Game1.timeOfDay >= 2000 )
            {
                __instance.Sprite.currentFrame = __instance.buildingTypeILiveIn.Contains( "Coop" ) ? 16 : 12;
                __instance.Sprite.UpdateSourceRect();
                __instance.FacingDirection = 2;
                if ( !__instance.isEmoting && Game1.random.NextDouble() < 0.002 )
                    __instance.doEmote( 24, true );
            }
            else if ( __instance.pauseTimer <= 0 )
            {
                if ( Game1.random.NextDouble() < 0.001 && ( int ) ( NetFieldBase<int, NetInt> ) __instance.age >= ( int ) ( byte ) ( NetFieldBase<byte, NetByte> ) __instance.ageWhenMature && ( Game1.gameMode == ( byte ) 3 && __instance.sound.Value != null ) && Utility.isOnScreen( __instance.Position, 192 ) )
                    __instance.makeSound();
                if ( !Game1.IsClient && Game1.random.NextDouble() < 0.007 && __instance.uniqueFrameAccumulator == -1 )
                {
                    int direction = Game1.random.Next(5);
                    if ( direction != ( __instance.FacingDirection + 2 ) % 4 )
                    {
                        if ( direction < 4 )
                        {
                            int facingDirection = __instance.FacingDirection;
                            __instance.faceDirection( direction );
                            if ( !( bool ) ( NetFieldBase<bool, NetBool> ) location.isOutdoors && location.isCollidingPosition( __instance.nextPosition( direction ), Game1.viewport, ( Character ) __instance ) )
                            {
                                __instance.faceDirection( facingDirection );
                                return false;
                            }
                        }
                        switch ( direction )
                        {
                            case 0:
                                __instance.SetMovingUp( true );
                                break;
                            case 1:
                                __instance.SetMovingRight( true );
                                break;
                            case 2:
                                __instance.SetMovingDown( true );
                                break;
                            case 3:
                                __instance.SetMovingLeft( true );
                                break;
                            default:
                                __instance.Halt();
                                __instance.Sprite.StopAnimation();
                                break;
                        }
                    }
                    else if ( __instance.noWarpTimer <= 0 )
                    {
                        __instance.Halt();
                        __instance.Sprite.StopAnimation();
                    }
                }
                if ( !Game1.IsClient && __instance.isMoving() && ( Game1.random.NextDouble() < 0.014 && __instance.uniqueFrameAccumulator == -1 ) )
                {
                    __instance.Halt();
                    __instance.Sprite.StopAnimation();
                    if ( Game1.random.NextDouble() < 0.75 )
                    {
                        __instance.uniqueFrameAccumulator = 0;
                        if ( __instance.buildingTypeILiveIn.Contains( "Coop" ) )
                        {
                            switch ( __instance.FacingDirection )
                            {
                                case 0:
                                    __instance.Sprite.currentFrame = 20;
                                    break;
                                case 1:
                                    __instance.Sprite.currentFrame = 18;
                                    break;
                                case 2:
                                    __instance.Sprite.currentFrame = 16;
                                    break;
                                case 3:
                                    __instance.Sprite.currentFrame = 22;
                                    break;
                            }
                        }
                        else if ( __instance.buildingTypeILiveIn.Contains( "Barn" ) )
                        {
                            switch ( __instance.FacingDirection )
                            {
                                case 0:
                                    __instance.Sprite.currentFrame = 15;
                                    break;
                                case 1:
                                    __instance.Sprite.currentFrame = 14;
                                    break;
                                case 2:
                                    __instance.Sprite.currentFrame = 13;
                                    break;
                                case 3:
                                    __instance.Sprite.currentFrame = 14;
                                    break;
                            }
                        }
                    }
                    __instance.Sprite.UpdateSourceRect();
                }
                if ( __instance.uniqueFrameAccumulator != -1 && !Game1.IsClient )
                {
                    __instance.uniqueFrameAccumulator += time.ElapsedGameTime.Milliseconds;
                    if ( __instance.uniqueFrameAccumulator > 500 )
                    {
                        if ( __instance.buildingTypeILiveIn.Contains( "Coop" ) )
                            __instance.Sprite.currentFrame = __instance.Sprite.currentFrame + 1 - __instance.Sprite.currentFrame % 2 * 2;
                        else if ( __instance.Sprite.currentFrame > 12 )
                        {
                            __instance.Sprite.currentFrame = ( __instance.Sprite.currentFrame - 13 ) * 4;
                        }
                        else
                        {
                            switch ( __instance.FacingDirection )
                            {
                                case 0:
                                    __instance.Sprite.currentFrame = 15;
                                    break;
                                case 1:
                                    __instance.Sprite.currentFrame = 14;
                                    break;
                                case 2:
                                    __instance.Sprite.currentFrame = 13;
                                    break;
                                case 3:
                                    __instance.Sprite.currentFrame = 14;
                                    break;
                            }
                        }
                        __instance.uniqueFrameAccumulator = 0;
                        if ( Game1.random.NextDouble() < 0.4 )
                            __instance.uniqueFrameAccumulator = -1;
                    }
                }
                else if ( !Game1.IsClient )
                    __instance.MovePosition( time, Game1.viewport, location );
            }
            return false;
        }
    }

    [HarmonyPatch( typeof( FarmAnimal ), "behaviors" )]
    public static class FarmAnimalBehaviorsPrefix
    {
        public static bool Prefix( FarmAnimal __instance, GameTime time, GameLocation location, ref bool __result )
        {
            NetBool isEating = Mod.instance.Helper.Reflection.GetField<NetBool>( __instance, "isEating" ).GetValue();
            if ( __instance.home == null )
            {
                __result = false;
                return false;
            }
            if ( ( bool ) ( NetFieldBase<bool, NetBool> ) isEating )
            {
                if ( __instance.home != null && __instance.home.getRectForAnimalDoor().Intersects( __instance.GetBoundingBox() ) )
                {
                    FarmAnimal.behaviorAfterFindingGrassPatch( ( Character ) __instance, location );
                    isEating.Value = false;
                    __instance.Halt();
                    __result = false;
                    return false;
                }
                if ( __instance.buildingTypeILiveIn.Contains( "Barn" ) )
                {
                    __instance.Sprite.Animate( time, 16, 4, 100f );
                    if ( __instance.Sprite.currentFrame >= 20 )
                    {
                        isEating.Value = false;
                        __instance.Sprite.loop = true;
                        __instance.Sprite.currentFrame = 0;
                        __instance.faceDirection( 2 );
                    }
                }
                else
                {
                    __instance.Sprite.Animate( time, 24, 4, 100f );
                    if ( __instance.Sprite.currentFrame >= 28 )
                    {
                        isEating.Value = false;
                        __instance.Sprite.loop = true;
                        __instance.Sprite.currentFrame = 0;
                        __instance.faceDirection( 2 );
                    }
                }
                __result = true;
                return false;
            }
            if ( !Game1.IsClient )
            {
                if ( __instance.controller != null )
                {
                    __result = true;
                    return false;
                }
                if ( location.IsOutdoors && ( byte ) ( NetFieldBase<byte, NetByte> ) __instance.fullness < ( byte ) 195 && ( Game1.random.NextDouble() < 0.002 && FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick ) )
                {
                    ++FarmAnimal.NumPathfindingThisTick;
                    __instance.controller = new PathFindController( ( Character ) __instance, location, new PathFindController.isAtEnd( FarmAnimal.grassEndPointFunction ), -1, false, new PathFindController.endBehavior( FarmAnimal.behaviorAfterFindingGrassPatch ), 200, Point.Zero, true );
                }
                if ( Game1.timeOfDay >= 1700 && location.IsOutdoors && ( __instance.controller == null && Game1.random.NextDouble() < 0.002 ) )
                {
                    if ( location.farmers.Count == 0 )
                    {
                        ( location as Farm ).animals.Remove( ( long ) __instance.myID );
                        ( __instance.home.indoors.Value as AnimalHouse ).animals.Add( ( long ) __instance.myID, __instance );
                        __instance.setRandomPosition( ( GameLocation ) ( NetFieldBase<GameLocation, NetRef<GameLocation>> ) __instance.home.indoors );
                        __instance.faceDirection( Game1.random.Next( 4 ) );
                        __instance.controller = ( PathFindController ) null;
                        __result = true;
                        return false;
                    }
                    if ( FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick )
                    {
                        ++FarmAnimal.NumPathfindingThisTick;
                        __instance.controller = new PathFindController( ( Character ) __instance, location, new PathFindController.isAtEnd( PathFindController.isAtEndPoint ), 0, false, ( PathFindController.endBehavior ) null, 200, new Point( ( int ) ( NetFieldBase<int, NetInt> ) __instance.home.tileX + __instance.home.animalDoor.X, ( int ) ( NetFieldBase<int, NetInt> ) __instance.home.tileY + __instance.home.animalDoor.Y ), true );
                    }
                }
                if ( location.IsOutdoors && !Game1.isRaining && ( !Game1.currentSeason.Equals( "winter" ) && ( int ) ( NetFieldBase<int, NetInt> ) __instance.currentProduce != -1 ) && ( ( int ) ( NetFieldBase<int, NetInt> ) __instance.age >= ( int ) ( byte ) ( NetFieldBase<byte, NetByte> ) __instance.ageWhenMature && __instance.type.Value.Contains( "Pig" ) && Game1.random.NextDouble() < 0.0002 ) )
                {
                    Microsoft.Xna.Framework.Rectangle boundingBox = __instance.GetBoundingBox();
                    for ( int corner = 0; corner < 4; ++corner )
                    {
                        Vector2 cornersOfThisRectangle = Utility.getCornersOfThisRectangle(ref boundingBox, corner);
                        Vector2 key = new Vector2((float)(int)((double)cornersOfThisRectangle.X / 64.0), (float)(int)((double)cornersOfThisRectangle.Y / 64.0));
                        if ( location.terrainFeatures.ContainsKey( key ) || location.objects.ContainsKey( key ) )
                        {
                            __result = false;
                            return false;
                        }
                    }
                    if ( Game1.player.currentLocation.Equals( location ) )
                    {
                        DelayedAction.playSoundAfterDelay( "dirtyHit", 450, ( GameLocation ) null, -1 );
                        DelayedAction.playSoundAfterDelay( "dirtyHit", 900, ( GameLocation ) null, -1 );
                        DelayedAction.playSoundAfterDelay( "dirtyHit", 1350, ( GameLocation ) null, -1 );
                    }
                    if ( location.Equals( Game1.currentLocation ) )
                    {
                        var findTruffleDelegate = (AnimatedSprite.endOfAnimationBehavior) Delegate.CreateDelegate( typeof( AnimatedSprite.endOfAnimationBehavior ), Mod.instance.Helper.Reflection.GetMethod( __instance, "findTruffle" ).MethodInfo );

                        switch ( __instance.FacingDirection )
                        {
                            case 0:
                                __instance.Sprite.setCurrentAnimation( new List<FarmerSprite.AnimationFrame>()
                                {
                                    new FarmerSprite.AnimationFrame(9, 250),
                                    new FarmerSprite.AnimationFrame(11, 250),
                                    new FarmerSprite.AnimationFrame(9, 250),
                                    new FarmerSprite.AnimationFrame(11, 250),
                                    new FarmerSprite.AnimationFrame(9, 250),
                                    new FarmerSprite.AnimationFrame(11, 250, false, false, findTruffleDelegate, false)
                                } );
                                break;
                            case 1:
                                __instance.Sprite.setCurrentAnimation( new List<FarmerSprite.AnimationFrame>()
                                {
                                    new FarmerSprite.AnimationFrame(5, 250),
                                    new FarmerSprite.AnimationFrame(7, 250),
                                    new FarmerSprite.AnimationFrame(5, 250),
                                    new FarmerSprite.AnimationFrame(7, 250),
                                    new FarmerSprite.AnimationFrame(5, 250),
                                    new FarmerSprite.AnimationFrame(7, 250, false, false, findTruffleDelegate, false)
                                } );
                                break;
                            case 2:
                                __instance.Sprite.setCurrentAnimation( new List<FarmerSprite.AnimationFrame>()
                                {
                                    new FarmerSprite.AnimationFrame(1, 250),
                                    new FarmerSprite.AnimationFrame(3, 250),
                                    new FarmerSprite.AnimationFrame(1, 250),
                                    new FarmerSprite.AnimationFrame(3, 250),
                                    new FarmerSprite.AnimationFrame(1, 250),
                                    new FarmerSprite.AnimationFrame(3, 250, false, false, findTruffleDelegate, false)
                                } );
                                break;
                            case 3:
                                __instance.Sprite.setCurrentAnimation( new List<FarmerSprite.AnimationFrame>()
                                {
                                    new FarmerSprite.AnimationFrame(5, 250, false, true, (AnimatedSprite.endOfAnimationBehavior)null, false),
                                    new FarmerSprite.AnimationFrame(7, 250, false, true, (AnimatedSprite.endOfAnimationBehavior)null, false),
                                    new FarmerSprite.AnimationFrame(5, 250, false, true, (AnimatedSprite.endOfAnimationBehavior)null, false),
                                    new FarmerSprite.AnimationFrame(7, 250, false, true, (AnimatedSprite.endOfAnimationBehavior)null, false),
                                    new FarmerSprite.AnimationFrame(5, 250, false, true, (AnimatedSprite.endOfAnimationBehavior)null, false),
                                    new FarmerSprite.AnimationFrame(7, 250, false, true, findTruffleDelegate, false)
                                } );
                                break;
                        }
                        __instance.Sprite.loop = false;
                    }
                    else
                        Mod.instance.Helper.Reflection.GetMethod( __instance, "findTruffle" ).Invoke( Game1.player );
                }
            }
            __result = false;
            return false;
        }
    }
}
