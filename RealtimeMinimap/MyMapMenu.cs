using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;

namespace RealtimeMinimap
{
    internal class Region
    {
        public class RegionLocation
        {
            internal Region Parent { get; set; }
            internal string Name { get; set; }

            public class InternalWarp
            {
                public Vector2 Spot { get; set; }
                public string WarpTo { get; set; }
            }

            public Vector2 Offset { get; set; }
            public Vector2 Size { get; set; }
            public List<InternalWarp> InternalWarps { get; set; } = new();

            public Rectangle Rect => new Rectangle( (int) Offset.X, (int) Offset.Y, (int) Size.X, (int) Size.Y );
        }

        public class LocationWarp
        {
            public string A { get; set; }
            public Vector2 OffsetA { get; set; }
            public string B { get; set; }
            public Vector2 OffsetB { get; set; }
        }

        public string RootLocation { get; set; }
        public Dictionary<string, RegionLocation> Locations { get; set; } = new();
        public Dictionary<long, LocationWarp> Warps { get; set; } = new();
    }

    public class MyMapMenu : IClickableMenu
    {
        private Region ActiveRegion { get; set; } = null;
        private List<Region> Regions { get; set; } = new();
        private Dictionary<string, Region.RegionLocation> LocationToRegions { get; set; } = new();
        private Vector2 offset { get; set; }
        private Vector2? click { get; set; }

        public MyMapMenu()
        {
            PlaceMaps();
            ActiveRegion = Regions[ 0 ];
        }

        public override void receiveLeftClick( int x, int y, bool playSound = true )
        {
            click = new Vector2( x, y );
        }

        public override void releaseLeftClick( int x, int y )
        {
            click = null;
        }

        public override void update( GameTime time )
        {
            if ( click.HasValue && ( Game1.getMouseX() != click.Value.X || Game1.getMouseY() != click.Value.Y ) )
            {
                Vector2 newClick = new Vector2( Game1.getMouseX(), Game1.getMouseY() );
                offset = new Vector2( offset.X + ( newClick.X - click.Value.X ), offset.Y + ( newClick.Y - click.Value.Y ) );
                click = newClick;
            }
        }

        public override void draw( SpriteBatch b )
        {
            b.Draw( Game1.staminaRect, new Rectangle( 0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height ), Color.Black );

            float scale = 2f;
            foreach ( var loc in ActiveRegion.Locations )
            {
                var locObj = Game1.getLocationFromName( loc.Key );
                var r = new Rectangle( (int)(loc.Value.Offset.X * scale+offset.X), (int)(loc.Value.Offset.Y * scale+offset.Y), (int)(loc.Value.Size.X * scale), (int)(loc.Value.Size.Y * scale) );
                if ( !Mod.State.Locations.ContainsKey( loc.Key ) )
                {
                    Mod.State.RenderQueue.Add( loc.Key );
                    var col = new Color( ( uint ) loc.Key.GetHashCode() );
                    col.A = 255;
                    b.Draw( Game1.staminaRect, r, col );
                    continue;
                }

                var tex = Mod.State.Locations[ loc.Key ];
                b.Draw( tex, r, Color.White );
            }
            drawMouse( b );
        }

        private void PlaceMaps()
        {
            Explore( "Town" );
            //Explore( "Desert" );
            // todo - island
        }

        private void Explore( string root )
        {
            var loc = Game1.getLocationFromName( root );
            var region = new Region();
            region.RootLocation = root;
            /*region.Locations.Add( root, new Region.RegionLocation()
            {
                Parent = region,
                Offset = Vector2.Zero,
            } );*/

            HashSet< string > explored = new();
            Queue< string > toExplore = new();

            void MoveMin( string aroundName, int depth )
            {
                if ( depth > 1000 ) // Sanity check
                {
                    Log.Warn( "Reached max depth moving things around!" );
                    return;
                }

                Region.RegionLocation around = region.Locations[ aroundName ];

                foreach ( var loc in region.Locations )
                {
                    if ( loc.Value == around )
                        continue;

                    var l = loc.Value.Rect;
                    var a = around.Rect;
                    if ( l.Intersects( a ) )
                    {
                        // Please refactor me
                        Region.RegionLocation outX = null;
                        int amountX = 0;
                        bool posOutX = false;
                        if ( l.X > 0 && a.X > 0 )
                        {
                            outX = l.X < a.X ? around : loc.Value;
                            amountX = Math.Abs( l.X - a.X );
                            posOutX = true;
                        }
                        else if ( l.X < 0 && a.X < 0 )
                        {
                            outX = l.X > a.X ? around : loc.Value;
                            amountX = Math.Abs( l.X - a.X );
                            posOutX = false;
                        }
                        else if ( l.X < 0 && a.X > 0 )
                        {
                            outX = -l.X < a.X ? around : loc.Value;
                            amountX = Math.Abs( a.X - l.X );
                            posOutX = -l.X < a.X;
                        }
                        else if ( l.X > 0 && a.X < 0 )
                        {
                            outX = l.X < -a.X ? around : loc.Value;
                            amountX = Math.Abs( l.X - a.X );
                            posOutX = !(l.X < -a.X);
                        }
                        else if ( l.X == 0 )
                        {
                            outX = around;
                            amountX = Math.Abs( a.X );
                            posOutX = a.X > 0;
                        }
                        else if ( a.X == 0 )
                        {
                            outX = loc.Value;
                            amountX = Math.Abs( l.X );
                            posOutX = l.X > 0;
                        }

                        // Please refactor me
                        Region.RegionLocation outY = null;
                        int amountY = 0;
                        bool posOutY = false;
                        if ( l.Y > 0 && a.Y > 0 )
                        {
                            outY = l.Y < a.Y ? around : loc.Value;
                            amountY = Math.Abs( l.Y - a.Y );
                            posOutY = true;
                        }
                        else if ( l.Y < 0 && a.Y < 0 )
                        {
                            outY = l.Y > a.Y ? around : loc.Value;
                            amountY = Math.Abs( l.Y - a.Y );
                            posOutY = false;
                        }
                        else if ( l.Y < 0 && a.Y > 0 )
                        {
                            outY = -l.Y < a.Y ? around : loc.Value;
                            amountY = Math.Abs( a.Y - l.Y );
                            posOutY = -l.Y < a.Y;
                        }
                        else if ( l.Y > 0 && a.Y < 0 )
                        {
                            outY = l.Y < -a.Y ? around : loc.Value;
                            amountY = Math.Abs( l.Y - a.Y );
                            posOutY = !( l.Y < -a.Y );
                        }
                        else if ( l.Y == 0 )
                        {
                            outY = around;
                            amountY = Math.Abs( a.Y );
                            posOutY = a.Y > 0;
                        }
                        else if ( a.Y == 0 )
                        {
                            outY = loc.Value;
                            amountY = Math.Abs( l.Y );
                            posOutY = l.Y > 0;
                        }

                        if ( amountY < amountX )
                        {
                            outY.Offset = new Vector2( outY.Offset.X, outY.Offset.Y + (posOutY ? amountY+1 : -amountY-1) );
                            MoveMin( outY.Name, depth + 1 );
                        }
                        else
                        {
                            outX.Offset = new Vector2( outX.Offset.X + (posOutX ? amountX+1 : -amountX-1), outX.Offset.Y );
                            MoveMin( outX.Name, depth + 1 );
                        }

                        return;
                    }
                }
            }

            void ExploreRegion( GameLocation loc )
            {
                Vector2 offset = region.Locations[ loc.NameOrUniqueName ].Offset;

                foreach ( var warp in loc.warps )
                {
                    var newLoc = Game1.getLocationFromName( warp.TargetName );
                    if ( warp.npcOnly.Value || explored.Contains( newLoc.NameOrUniqueName ) || !newLoc.IsOutdoors )
                        continue;

                    if ( newLoc.Name == "Backwoods" && loc.Name != "Mountain" )
                        continue;
                    
                    Vector2 newOffset;
                    newOffset.X = offset.X + warp.X - warp.TargetX;
                    newOffset.Y = offset.Y + warp.Y - warp.TargetY;

                    if ( newLoc.Name == "Railroad" )
                        newOffset.Y -= 2;

                    /*List<int[]> sides = new();
                    sides.Add( new int[] { warp.X, Game1.left } );
                    sides.Add( new int[] { warp.Y, Game1.up } );
                    sides.Add( new int[] { newLoc.Map.Layers[ 0 ].LayerWidth - warp.X, Game1.right } );
                    sides.Add( new int[] { newLoc.Map.Layers[ 0 ].LayerHeight - warp.Y, Game1.down } );

                    int min = 0;
                    for ( int i = 1; i < sides.Count; ++i )
                    {
                        if ( sides[ i ][ 0 ] < sides[ min ][ 0 ] )
                            min = i;
                    }

                    while ( FoundOverlap( new Rectangle( ( int ) newOffset.X, ( int ) newOffset.Y, newLoc.Map.Layers[ 0 ].LayerWidth, newLoc.Map.Layers[ 0 ].LayerHeight ) ) )
                    {
                        switch ( sides[ min ][ 1 ] )
                        {
                            case Game1.left:
                                newOffset = new Vector2( newOffset.X - 1, newOffset.Y );
                                break;
                            case Game1.up:
                                newOffset = new Vector2( newOffset.X, newOffset.Y - 1 );
                                break;
                            case Game1.right:
                                newOffset = new Vector2( newOffset.X + 1, newOffset.Y );
                                break;
                            case Game1.down:
                                newOffset = new Vector2( newOffset.X, newOffset.Y + 1 );
                                break;
                        }
                    }*/

                    region.Locations.Add( newLoc.NameOrUniqueName, new()
                    {
                        Name = newLoc.NameOrUniqueName,
                        Parent = region,
                        Offset = newOffset,
                        Size = new Vector2( newLoc.Map.Layers[ 0 ].LayerWidth, newLoc.Map.Layers[ 0 ].LayerHeight )
                    } );

                    MoveMin( newLoc.NameOrUniqueName, 0 );

                    explored.Add( newLoc.NameOrUniqueName );
                    toExplore.Enqueue( newLoc.NameOrUniqueName );
                }

                foreach ( var door in loc.doors )
                {
                    // todo
                }
            }

            region.Locations.Add( root, new()
            {
                Name = root,
                Parent = region,
                Offset = Vector2.Zero,
                Size = new Vector2( loc.Map.Layers[ 0 ].LayerWidth, loc.Map.Layers[ 0 ].LayerHeight )
            } );
            explored.Add( root );
            toExplore.Enqueue( root );
            for ( int i = 0; i < 4 && toExplore.Count > 0; ++i )
            {
                ExploreRegion( Game1.getLocationFromName( toExplore.Dequeue() ) );
            }
            Regions.Add( region );
        }
    }
}
