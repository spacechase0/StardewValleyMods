using System.Collections.Generic;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;

namespace BuildableLocationsFramework.Patches
{
    [HarmonyPatch(typeof(Utility), nameof(Utility.areThereAnyOtherAnimalsWithThisName))]
    public static class UtilityAnimalsWithNamePatch
    {
        public static bool Prefix( string name, ref bool __result )
        {
            var locs = Mod.GetAllLocations();
            foreach ( var loc in locs )
            {
                if ( loc is IAnimalLocation aloc )
                {
                    foreach ( var animal in aloc.Animals.Values )
                    {
                        if ( animal.displayName == name )
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
            }
            __result = false;
            return false;
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getAnimal ) )]
    public static class UtilityGetAnimalPatch
    {
        public static bool Prefix( long id, ref FarmAnimal __result )
        {
            var locs = Mod.GetAllLocations();
            foreach ( var loc in locs )
            {
                if ( loc is IAnimalLocation aloc )
                {
                    if ( aloc.Animals.ContainsKey( id ) )
                    {
                        __result = aloc.Animals[ id ];
                        return false;
                    }
                }
            }
            __result = null;
            return false;
        }
    }
    [HarmonyPatch( typeof( Utility ), nameof( Utility.fixAllAnimals ) )]
    public static class UtilityFixAnimalsPatch
    {
        public static bool Prefix()
        {
            if ( !Game1.IsMasterGame )
                return false;

            foreach (var loc in Mod.GetAllLocations() )
            {
                if ( loc is BuildableGameLocation farm && loc is IAnimalLocation farm_animals )
                {
                    foreach ( Building building in farm.buildings )
                    {
                        if ( building.indoors.Value != null && building.indoors.Value is AnimalHouse )
                        {
                            foreach ( long id in ( NetList<long, NetLong> ) ( building.indoors.Value as AnimalHouse ).animalsThatLiveHere )
                            {
                                FarmAnimal animal = Utility.getAnimal(id);
                                if ( animal != null )
                                {
                                    animal.home = building;
                                    animal.homeLocation.Value = new Vector2( ( float ) ( int ) ( NetFieldBase<int, NetInt> ) building.tileX, ( float ) ( int ) ( NetFieldBase<int, NetInt> ) building.tileY );
                                }
                            }
                        }
                    }
                    List<FarmAnimal> farmAnimalList1 = new List<FarmAnimal>();
                    foreach ( FarmAnimal allFarmAnimal in getAllFarmAnimals(farm) )
                    {
                        if ( allFarmAnimal.home == null )
                            farmAnimalList1.Add( allFarmAnimal );
                    }
                    foreach ( FarmAnimal farmAnimal in farmAnimalList1 )
                    {
                        NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>.PairsCollection pairs;
                        KeyValuePair<long, FarmAnimal> keyValuePair;
                        foreach ( Building building in farm.buildings )
                        {
                            if ( building.indoors.Value != null && building.indoors.Value is AnimalHouse )
                            {
                                for ( int index = ( building.indoors.Value as AnimalHouse ).animals.Count() - 1; index >= 0; --index )
                                {
                                    pairs = ( building.indoors.Value as AnimalHouse ).animals.Pairs;
                                    keyValuePair = pairs.ElementAt( index );
                                    if ( keyValuePair.Value.Equals( ( object ) farmAnimal ) )
                                    {
                                        NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = (building.indoors.Value as AnimalHouse).animals;
                                        pairs = ( building.indoors.Value as AnimalHouse ).animals.Pairs;
                                        keyValuePair = pairs.ElementAt( index );
                                        long key = keyValuePair.Key;
                                        animals.Remove( key );
                                    }
                                }
                            }
                        }
                        for ( int index = farm_animals.Animals.Count() - 1; index >= 0; --index )
                        {
                            pairs = farm_animals.Animals.Pairs;
                            keyValuePair = pairs.ElementAt( index );
                            if ( keyValuePair.Value.Equals( ( object ) farmAnimal ) )
                            {
                                NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = farm_animals.Animals;
                                pairs = farm_animals.Animals.Pairs;
                                keyValuePair = pairs.ElementAt( index );
                                long key = keyValuePair.Key;
                                animals.Remove( key );
                            }
                        }
                    }
                    foreach ( Building building in farm.buildings )
                    {
                        if ( building.indoors.Value != null && building.indoors.Value is AnimalHouse )
                        {
                            for ( int index = ( building.indoors.Value as AnimalHouse ).animalsThatLiveHere.Count - 1; index >= 0; --index )
                            {
                                if ( Utility.getAnimal( ( building.indoors.Value as AnimalHouse ).animalsThatLiveHere[ index ] ).home != building )
                                    ( building.indoors.Value as AnimalHouse ).animalsThatLiveHere.RemoveAt( index );
                            }
                        }
                    }
                    foreach ( FarmAnimal farmAnimal in farmAnimalList1 )
                    {
                        foreach ( Building building in farm.buildings )
                        {
                            if ( building.buildingType.Contains( ( string ) ( NetFieldBase<string, NetString> ) farmAnimal.buildingTypeILiveIn ) && building.indoors.Value != null && ( building.indoors.Value is AnimalHouse && !( building.indoors.Value as AnimalHouse ).isFull() ) )
                            {
                                farmAnimal.home = building;
                                farmAnimal.homeLocation.Value = new Vector2( ( float ) ( int ) ( NetFieldBase<int, NetInt> ) building.tileX, ( float ) ( int ) ( NetFieldBase<int, NetInt> ) building.tileY );
                                farmAnimal.setRandomPosition( ( GameLocation ) ( NetFieldBase<GameLocation, NetRef<GameLocation>> ) farmAnimal.home.indoors );
                                ( farmAnimal.home.indoors.Value as AnimalHouse ).animals.Add( ( long ) farmAnimal.myID, farmAnimal );
                                ( farmAnimal.home.indoors.Value as AnimalHouse ).animalsThatLiveHere.Add( ( long ) farmAnimal.myID );
                                break;
                            }
                        }
                    }
                    List<FarmAnimal> farmAnimalList2 = new List<FarmAnimal>();
                    foreach ( FarmAnimal farmAnimal in farmAnimalList1 )
                    {
                        if ( farmAnimal.home == null )
                            farmAnimalList2.Add( farmAnimal );
                    }
                    foreach ( FarmAnimal farmAnimal in farmAnimalList2 )
                    {
                        farmAnimal.Position = Utility.recursiveFindOpenTileForCharacter( ( Character ) farmAnimal, ( GameLocation ) farm, new Vector2( 40f, 40f ), 200 ) * 64f;
                        if ( !farm_animals.Animals.ContainsKey( ( long ) farmAnimal.myID ) )
                            farm_animals.Animals.Add( ( long ) farmAnimal.myID, farmAnimal );
                    }
                }
            }
            return false;
        }

        private static List<FarmAnimal> getAllFarmAnimals( BuildableGameLocation loc )
        {
            List<FarmAnimal> list = new List<FarmAnimal>();
            if ( loc is IAnimalLocation loca )
                list = loca.Animals.Values.ToList<FarmAnimal>();
            foreach ( Building building in loc.buildings )
            {
                if ( building.indoors.Value != null && building.indoors.Value is AnimalHouse )
                    list.AddRange( ( IEnumerable<FarmAnimal> ) ( ( AnimalHouse ) ( GameLocation ) ( NetFieldBase<GameLocation, NetRef<GameLocation>> ) building.indoors ).animals.Values.ToList<FarmAnimal>() );
            }
            return list;
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.numSilos ) )]
    public static class UtilityNumSilosPatch
    {
        public static void Postfix( ref int __result )
        {
            foreach ( var loc in Mod.GetAllLocations() )
            {
                if ( loc != Game1.getFarm() && loc is BuildableGameLocation bgl )
                {
                    foreach ( var building in bgl.buildings )
                    {
                        if ( building.buildingType.Value == "Silo" && building.daysOfConstructionLeft.Value <= 0 )
                            ++__result;
                    }
                }
            }
        }
    }
}
