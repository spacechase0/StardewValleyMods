using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace HybridCropEngine
{
    public class Mod : StardewModdingAPI.Mod, IAssetLoader
    {
        public static Mod instance;
        public static Configuration config;

        public override void Entry( IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;
            config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.GameLoop.DayEnding += onDayEnding;
        }

        public bool CanLoad<T>( IAssetInfo asset )
        {
            return asset.AssetNameEquals( "Data/HybridCrops" );
        }

        public T Load<T>( IAssetInfo asset )
        {
            return (T) (object) new Dictionary<int, HybridCropData>();
        }

        private void onGameLaunched( object sender, GameLaunchedEventArgs e )
        {
            var gmcm = Helper.ModRegistry.GetApi< GenericModConfigMenuAPI >( "spacechase0.GenericModConfigMenu" );
            if ( gmcm != null )
            {
                gmcm.RegisterModConfig( ModManifest, () => config = new Configuration(), () => Helper.WriteConfig( config ) );
                gmcm.RegisterSimpleOption( ModManifest, "Scan Everywhere", "Scan everywhere for hybrid creation.\nFalse means only scan the Farm and Greenhouse.", () => config.ScanEverywhere, ( b ) => config.ScanEverywhere = b );
            }
        }

        private void onDayEnding( object sender, DayEndingEventArgs e )
        {
            if ( !Game1.IsMasterGame )
                return;

            var hybrids = Game1.content.Load< Dictionary< int, HybridCropData > >( "Data/HybridCrops" );
            var hybridIndexByCrop = makeHybridIndex( hybrids );
            var cropsByIndex = makeCropIndex();

            foreach ( var hybrid in hybrids )
            {
                Log.verbose( "Hybrids: " + hybrid.Key + " " + hybrid.Value.BaseCropA + " " + hybrid.Value.BaseCropB + " " + hybrid.Value.Chance );
            }
            foreach ( var index in hybridIndexByCrop )
            {
                Log.verbose( "Hybrid Index: " + index.Key + " " + index.Value );
            }
            foreach ( var index in cropsByIndex )
            {
                Log.verbose( "Crop Index: " + index.Key + " " + index.Value );
            }

            if ( config.ScanEverywhere )
            {
                var locs = new List<GameLocation>( Game1.locations );
                var moreLocs = new List<GameLocation>();
                foreach ( var loc in locs )
                {
                    if ( loc is BuildableGameLocation buildLoc )
                    {
                        foreach ( var building in buildLoc.buildings )
                        {
                            if ( building.indoors.Value != null )
                                moreLocs.Add( building.indoors.Value );
                        }
                    }

                    growHybrids( loc, hybrids, hybridIndexByCrop, cropsByIndex );
                }
                foreach ( var loc in moreLocs )
                    growHybrids( loc, hybrids, hybridIndexByCrop, cropsByIndex );
            }
            else
            {
                growHybrids( Game1.getFarm(), hybrids, hybridIndexByCrop, cropsByIndex );
                growHybrids( Game1.getLocationFromName( "Greenhouse" ), hybrids, hybridIndexByCrop, cropsByIndex );
            }
        }

        private Dictionary<ulong, int> makeHybridIndex( Dictionary< int, HybridCropData > data )
        {
            var ret = new Dictionary< ulong, int >();
            foreach ( var entry in data )
            {
                if ( entry.Value.BaseCropA == entry.Value.BaseCropB )
                    continue;

                ulong la = (ulong) entry.Value.BaseCropA;
                ulong lb = (ulong) entry.Value.BaseCropB;
                ret.Add( ( la << 32 ) | lb, entry.Key );
                ret.Add( ( lb << 32 ) | la, entry.Key );
            }
            return ret;
        }

        private Dictionary<int, int> makeCropIndex()
        {
            var ret = new Dictionary< int, int >();
            var crops = Game1.content.Load<Dictionary<int, string>>( "Data/Crops" );
            foreach ( var crop in crops )
            {
                if ( crop.Key >= 495 && crop.Key <= 498 )
                    continue;

                string[] fields = crop.Value.Split( '/' );
                ret.Add( int.Parse( fields[ 2 ] ), crop.Key );
            }
            return ret;
        }

        private void growHybrids( GameLocation loc, Dictionary< int, HybridCropData > hybrids, Dictionary<ulong, int> hybridIndex, Dictionary< int, int > cropSeedIndex )
        {
            int baseSeed = loc.NameOrUniqueName.GetHashCode();
            baseSeed ^= (int) Game1.uniqueIDForThisGame;
            baseSeed += (int) Game1.stats.DaysPlayed;

            for ( int ix = 0; ix < loc.Map.Layers[ 0 ].LayerSize.Width; ++ix )
            {
                for ( int iy = 0; iy < loc.Map.Layers[ 0 ].LayerSize.Height; ++iy )
                {
                    Func<int, int, HoeDirt> getHoedirt = (x, y) => (loc.terrainFeatures.ContainsKey( new Vector2( ix + x, iy + y ) ) ? ( loc.terrainFeatures[ new Vector2( ix + x, iy + y ) ] as HoeDirt ) : null);

                    HoeDirt[] dirts = new HoeDirt[]
                    {
                        getHoedirt( -1, -1 ), getHoedirt( 0, -1 ), getHoedirt( 1, -1 ),
                        getHoedirt( -1, 0 ), getHoedirt( 0, 0 ), getHoedirt( 1, 0 ),
                        getHoedirt( -1, 1 ), getHoedirt( 0, 1 ), getHoedirt( 1, 1 ),
                    };

                    if ( dirts[ 4 ] == null || dirts[ 4 ].crop != null )
                        continue;

                    // Make only hoe dirts with fully grown crops remain in the dirts array
                    for ( int h = 0; h < dirts.Length; ++h )
                    {
                        if ( h == 4 )
                            continue;
                        var hd = dirts[ h ];
                        if ( hd != null && hd.crop == null )
                            dirts[ h ] = null;
                        else if ( hd != null )
                        {
                            if ( hd.crop.currentPhase.Value == hd.crop.phaseDays.Count - 1 && hd.crop.dayOfCurrentPhase.Value == 0 )
                            {
                            }
                            else
                            {
                                dirts[ h ] = null;
                            }
                        }
                    }

                    var combos = new List<HoeDirt[]>();
                    Action< int, int > addIfCombo = (a, b) =>
                    {
                        if ( dirts[ a ] != null && dirts[ b ] != null )
                            combos.Add( new HoeDirt[] { dirts[ a ], dirts[ b ] } );
                    };
                    addIfCombo( 0, 1 );
                    addIfCombo( 1, 2 );
                    addIfCombo( 0, 3 );
                    addIfCombo( 2, 5 );
                    addIfCombo( 3, 6 );
                    addIfCombo( 5, 8 );
                    addIfCombo( 6, 7 );
                    addIfCombo( 7, 8 );
                    addIfCombo( 1, 3 );
                    addIfCombo( 1, 5 );
                    addIfCombo( 3, 7 );
                    addIfCombo( 5, 7 );

                    Random r = new Random( baseSeed + ix * loc.Map.Layers[ 0 ].LayerSize.Height + iy );
                    foreach ( var combo in combos )
                    {
                        ulong ca = ( ulong ) combo[ 0 ].crop.rowInSpriteSheet.Value;
                        ulong cb = ( ulong ) combo[ 1 ].crop.rowInSpriteSheet.Value;
                        ulong code = ( ca << 32 ) | cb;

                        if ( !hybridIndex.ContainsKey( code ) )
                            continue;

                        var hybridData = hybrids[ hybridIndex[ code ] ];
                        if ( r.NextDouble() < hybridData.Chance )
                        {
                            dirts[ 4 ].crop = new Crop( cropSeedIndex[ hybridIndex[ code ] ], ix, iy );
                            break;
                        }
                    }
                }
            }
        }
    }
}
