using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures.Game.Projectiles;
using Netcode;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile;

namespace MoonMisadventures.Game.Locations
{
    [XmlType( "Mods_spacechase0_MoonMisadventuress_LunarLocation" )]
    public class LunarLocation : GameLocation
    {
        public class PickEdgeEventArgs : NetEventArg
        {
            public Point PickPoint { get; set; }
            public int ToolTier { get; set; }

            public PickEdgeEventArgs()
            {
            }

            public PickEdgeEventArgs( Point p, int tt )
            {
                PickPoint = p;
                ToolTier = tt;
            }

            public void Read( BinaryReader reader )
            {
                PickPoint = new Point( reader.ReadInt32(), reader.ReadInt32() );
                ToolTier = reader.ReadInt32();
            }

            public void Write( BinaryWriter writer )
            {
                writer.Write( PickPoint.X );
                writer.Write( PickPoint.Y );
                writer.Write( ToolTier );
            }
        }

        public const int SpaceTileIndex = 608;

        public NetFloat asteroidChance = new();
        [XmlIgnore]
        public NetEvent1<PickEdgeEventArgs> pickEdgeEvent = new();

        public LunarLocation() { }
        public LunarLocation( IContentHelper content, string mapPath, string mapName )
        :   base( content.GetActualAssetKey( "assets/maps/" + mapPath + ".tmx" ), "Custom_MM_" + mapName )
        {
            PlaceSpaceTiles();
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields( asteroidChance, pickEdgeEvent );

            pickEdgeEvent.onEvent += OnPickEdgeEvent;

            terrainFeatures.OnValueAdded += ( sender, added ) =>
            {
                if ( added is Grass grass )
                {
                    grass.grassType.Value = Grass.lavaGrass;
                    grass.loadSprite();
                }
                else if ( added is HoeDirt hd )
                {
                    Mod.instance.Helper.Reflection.GetField< Texture2D >( hd, "texture" ).SetValue( Assets.HoeDirt );
                }
            };
        }

        private void OnPickEdgeEvent( PickEdgeEventArgs args )
        {
            int tile = getTileIndexAt( args.PickPoint.X, args.PickPoint.Y, "Buildings" );

            Vector2 dir = Vector2.Zero;
            switch ( tile )
            {
                case 208: dir = new Vector2( 0, -1 ); break;
                case 236: dir = new Vector2( -1, 0 ); break;
                case 238: dir = new Vector2( 1, 0 ); break;
                case 266: dir = new Vector2( 0, 1 ); break;
            }
            //Log.Debug( "meow!? " + dir +" "+ tile + " " + args.PickPoint.X+ " " + args.PickPoint.Y + " " + args.ToolTier );

            if ( dir != Vector2.Zero )
            {
                int debrisTs = Map.TileSheets.IndexOf( Map.GetTileSheet( "flying_debris" ) );
                if ( debrisTs == -1 )
                {
                    var ts = new xTile.Tiles.TileSheet( Map, Mod.instance.Helper.Content.GetActualAssetKey( "assets/maps/flying_debris.png" ), new xTile.Dimensions.Size( 4, 6 ), new xTile.Dimensions.Size( 16, 16 ) );
                    Map.AddTileSheet( ts );
                    Map.LoadTileSheets( Game1.mapDisplayDevice );
                    debrisTs = Map.TileSheets.IndexOf( ts );
                }

                int i = 1;
                bool placedTiles = false;
                for ( ; i <= Math.Max( args.ToolTier, 3 ); ++i )
                {
                    int ii = i;
                    var newTile = new Vector2( args.PickPoint.X, args.PickPoint.Y ) + dir * i;
                    if ( /*getTileIndexAt( ( int ) newTile.X, ( int ) newTile.Y, "Back" ) == -1 &&
                             */getTileIndexAt( ( int ) newTile.X, ( int ) newTile.Y, "Buildings" ) == SpaceTileIndex )
                    {
                        placedTiles = true;
                        DelayedAction.functionAfterDelay( () =>
                        {
                            int tileIndex = Game1.random.Next( 3 ) * 4 + ( dir.Y != 0 ? 0 : 12 );
                            List< int > tiles = new List<int> { tileIndex, tileIndex + 1, tileIndex + 2, tileIndex + 3 };
                            int shift = Game1.random.Next( tiles.Count );
                            int[] tilesArr = tiles.OrderBy( x => ( ( tiles.IndexOf( x ) + shift ) % tiles.Count ) ).ToArray(); // Randomize where it starts animating from

                            Game1.playSound( "boulderBreak" );
                            removeTile( ( int ) newTile.X, ( int ) newTile.Y, "Buildings" );
                            setAnimatedMapTile( ( int ) newTile.X, ( int ) newTile.Y, tilesArr, 450, "Back1", null, debrisTs );
                        }, 100 * ii );
                    }
                    else break;
                }
                //SpaceShared.Log.Debug( "meow? " + tileX + " " + tileY + " " + t + " " + dir+ " " + i + " " + placedTiles );

                if ( placedTiles )
                {
                    setMapTileIndex( args.PickPoint.X, args.PickPoint.Y, tile, "Back", Map.TileSheets.IndexOf( Map.GetTileSheet( getTileSheetIDAt( args.PickPoint.X, args.PickPoint.Y, "Buildings" ) ) ) );
                    removeTile( args.PickPoint.X, args.PickPoint.Y, "Buildings" );

                    var newTile = new Vector2( args.PickPoint.X, args.PickPoint.Y ) + dir * i;

                    int newBtile = getTileIndexAt( ( int ) newTile.X, ( int ) newTile.Y, "Buildings" );
                    if ( newBtile != -1 && newBtile != SpaceTileIndex )
                    {
                        DelayedAction.functionAfterDelay( () =>
                        {
                            setMapTileIndex( ( int ) newTile.X, ( int ) newTile.Y, getTileIndexAt( ( int ) newTile.X, ( int ) newTile.Y, "Buildings" ), "Back", Map.TileSheets.IndexOf( Map.GetTileSheet( getTileSheetIDAt( ( int ) newTile.X, ( int ) newTile.Y, "Buildings" ) ) ) );
                            removeTile( ( int ) newTile.X, ( int ) newTile.Y, "Buildings" );
                        }, 100 * ( i - 1 ) );
                    }
                }
            }
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            Game1.changeMusicTrack( "into-the-spaceship" );

            if ( IsOutdoors )
            {
                Game1.drawLighting = true;
                int colValue = ( 14 - Game1.dayOfMonth % 14 ) * 7;
                if ( Game1.dayOfMonth > 14 )
                    colValue = ( Game1.dayOfMonth % 14 - 14 ) * 7;
                colValue = 175 - colValue;
                Game1.ambientLight = Game1.outdoorLight = new Color( colValue, colValue, colValue );// new Color( 100, 120, 30 );
            }

            foreach ( var tf in terrainFeatures.Values )
            {
                if ( tf is HoeDirt hd )
                {
                    Mod.instance.Helper.Reflection.GetField<Texture2D>( hd, "texture" ).SetValue( Assets.HoeDirt );
                }
            }

            Game1.background = new SpaceBackground( this.NameOrUniqueName == "Custom_MM_MoonPlanetOverlook" );
        }
        public override void cleanupBeforePlayerExit()
        {
            base.cleanupBeforePlayerExit();
            Game1.ambientLight = Game1.outdoorLight = Color.Black;
            Game1.background = null;
        }

        public override void checkForMusic( GameTime time )
        {
            // get rid of those stupid birds
        }

        public override bool SeedsIgnoreSeasonsHere()
        {
            return true;
        }

        public override bool CanPlantSeedsHere( int crop_index, int tile_x, int tile_y )
        {
            // No normal crops - note that moon crops get an override with a harmony patch anyways
            return false;
        }

        public override bool CanPlantTreesHere( int sapling_index, int tile_x, int tile_y )
        {
            return false;
        }

        public override void UpdateWhenCurrentLocation( GameTime time )
        {
            base.UpdateWhenCurrentLocation( time );
            pickEdgeEvent.Poll();
        }

        public override void updateEvenIfFarmerIsntHere( GameTime time, bool ignoreWasUpdatedFlush = false )
        {
            base.updateEvenIfFarmerIsntHere( time, ignoreWasUpdatedFlush );

            if ( !Context.CanPlayerMove )
                return;

            if ( asteroidChance.Value > 0 && Game1.recentMultiplayerRandom.NextDouble() < asteroidChance.Value )
            {
                int mw = map.Layers[ 0 ].DisplayWidth;
                int mh = map.Layers[ 0 ].DisplayHeight;

                float spot = ( float ) Game1.recentMultiplayerRandom.NextDouble() * ( mw * 2 + mh * 2 );
                Vector2 pos = Vector2.Zero;
                if ( spot > mw * 2 + mh )
                    pos = new Vector2( 0, spot - ( mw * 2 + mh ) );
                else if ( spot > mw + mh )
                    pos = new Vector2( spot - ( mw + mh ), mh );
                else if ( spot > mw )
                    pos = new Vector2( mw, spot - mw );
                else
                    pos = new Vector2( spot, 0 );

                int angle = 0;
                if ( new Rectangle( 0, 0, mw / 2, mh / 2 ).Contains( pos ) )
                    angle = Game1.recentMultiplayerRandom.Next( 270, 360 );
                else if ( new Rectangle( mw / 2, 0, mw / 2, mh / 2 ).Contains( pos ) )
                    angle = Game1.recentMultiplayerRandom.Next( 180, 270 );
                else if ( new Rectangle( 0, mh / 2, mw / 2, mh / 2 ).Contains( pos ) )
                    angle = Game1.recentMultiplayerRandom.Next( 0, 90 );
                else if ( new Rectangle( mw / 2, mh / 2, mw / 2, mh / 2 ).Contains( pos ) )
                    angle = Game1.recentMultiplayerRandom.Next( 90, 180 );

                float rad = MathHelper.ToRadians( angle );
                Vector2 velAngle = new Vector2( ( float ) Math.Cos( angle ), ( float ) Math.Sin( angle ) );
                projectiles.Add( new AsteroidProjectile( pos, velAngle ) );
            }
        }

        public override string checkForBuriedItem( int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who )
        {
            Random r = new Random( xLocation * 3000 + yLocation + ( int ) Game1.uniqueIDForThisGame + ( int ) Game1.stats.DaysPlayed + Name.GetDeterministicHashCode() );
            if ( r.NextDouble() < 0.03 )
            {
                Game1.createObjectDebris( 424 /* cheese */, xLocation, yLocation, this );
            }
            return base.checkForBuriedItem( xLocation, yLocation, explosion, detectOnly, who );
        }

        public override bool performToolAction( Tool t, int tileX, int tileY )
        {
            if ( t is Pickaxe )
            {
                int tile = getTileIndexAt( tileX, tileY, "Buildings" );

                Vector2 dir = Vector2.Zero;
                switch ( tile )
                {
                    case 208: dir = new Vector2( 0, -1 ); break;
                    case 236: dir = new Vector2( -1, 0 ); break;
                    case 238: dir = new Vector2( 1, 0 ); break;
                    case 266: dir = new Vector2( 0, 1 ); break;
                }

                if ( dir != Vector2.Zero )
                {
                    pickEdgeEvent.Fire( new PickEdgeEventArgs( new Point( tileX, tileY ), t.UpgradeLevel ) );
                    return true;
                }
            }

            return base.performToolAction( t, tileX, tileY );
        }

        public override StardewValley.Object getFish( float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null )
        {
            return base.getFish( millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName );
        }

        public override void tryToAddCritters( bool onlyIfOnScreen = false )
        {
        }

        public override void drawWater( SpriteBatch b )
        {
            // TODO: Stencil effects?
        }

        public void PlaceSpaceTiles()
        {
            int ts = Map.TileSheets.IndexOf( Map.GetTileSheet( "tf_darkdimension_sheet" ) );
            if ( map.GetLayer( "Back1" ) == null )
            {
                map.AddLayer( new xTile.Layers.Layer( "Back1", Map, Map.Layers[ 0 ].LayerSize, Map.Layers[ 0 ].TileSize ) );
            }
            for ( int ix = 0; ix < Map.Layers[ 0 ].LayerWidth; ++ix )
            {
                for ( int iy = 0; iy < Map.Layers[ 0 ].LayerHeight; ++iy )
                {
                    if ( ( getTileIndexAt( ix, iy, "Back" ) is -1 or 294 or 295 or 296 or 381 or 382 or 383 || doesTileHaveProperty( ix, iy, "CanSpace", "Back" ) == "T" ) && ( getTileIndexAt( ix, iy, "Buildings" ) == -1 ) )
                    {
                        setMapTileIndex( ix, iy, SpaceTileIndex, "Buildings", ts );
                        //Log.Debug( this.Name + " placed space tile @ " + ix + ", " + iy );
                    }
                }
            }
        }
    }
}
