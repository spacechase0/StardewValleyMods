using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets
{
    class MyUtility
    {
        public static void iterateAllItems( Func<Item, Item> action )
        {
            foreach ( GameLocation location in Game1.locations )
            {
                MyUtility._recursiveIterateLocation( location, action );
            }
            foreach ( Farmer farmer in Game1.getAllFarmers() )
            {
                IList<Item> list = farmer.Items;
                for ( int i = 0; i < list.Count; ++i )
                {
                    list[ i ] = MyUtility._recursiveIterateItem( list[ i ], action );
                }
                //farmer.Items = list;
                farmer.shirtItem.Value = ( Clothing ) MyUtility._recursiveIterateItem( farmer.shirtItem.Value, action );
                farmer.pantsItem.Value = ( Clothing ) MyUtility._recursiveIterateItem( farmer.pantsItem.Value, action );
                farmer.boots.Value = ( Boots ) MyUtility._recursiveIterateItem( farmer.boots.Value, action );
                farmer.hat.Value = ( Hat ) MyUtility._recursiveIterateItem( farmer.hat.Value, action );
                farmer.leftRing.Value = ( Ring ) MyUtility._recursiveIterateItem( farmer.leftRing.Value, action );
                farmer.rightRing.Value = ( Ring ) MyUtility._recursiveIterateItem( farmer.rightRing.Value, action );
                list = farmer.itemsLostLastDeath;
                for ( int i = 0; i < list.Count; ++i )
                {
                    list[ i ] = MyUtility._recursiveIterateItem( list[ i ], action );
                }
                //farmer.itemsLostLastDeath.CopyFrom( list );
            }
            IList<Item> list2 = Game1.player.team.returnedDonations;
            for ( int i = 0; i < list2.Count; ++i )
            {
                if ( list2[ i ] != null )
                {
                    list2[ i ] = action( list2[ i ] );
                }
            }
            //Game1.player.team.returnedDonations.Set( list2 );
            list2 = Game1.player.team.junimoChest;
            for ( int i = 0; i < list2.Count; ++i )
            {
                if ( list2[ i ] != null )
                {
                    list2[ i ] = action( list2[ i ] );
                }
            }
            //Game1.player.team.junimoChest.CopyFrom( list2 );
            foreach ( SpecialOrder specialOrder in Game1.player.team.specialOrders )
            {
                list2 = specialOrder.donatedItems;
                for ( int i = 0; i < list2.Count; ++i )
                {
                    if ( list2[ i ] != null )
                    {
                        list2[ i ] = action( list2[ i ] );
                    }
                }
                //specialOrder.donatedItems.CopyFrom( list2 );
            }
        }
        protected static void _recursiveIterateLocation( GameLocation l, Func<Item, Item> action )
        {
            if ( l == null )
            {
                return;
            }
            if ( l != null )
            {
                IList<Furniture> list = l.furniture;
                for ( int i = 0; i < list.Count; ++i )
                {
                    list[i] = (Furniture) MyUtility._recursiveIterateItem( list[i], action );
                }
            }
            if ( l is IslandFarmHouse )
            {
                IList<Item> list = ( l as IslandFarmHouse ).fridge.Value.items;
                for ( int i = 0; i < list.Count; ++i )
                {
                    if ( list[ i ] != null )
                    {
                        list[ i ] = MyUtility._recursiveIterateItem( list[ i ], action );
                    }
                }
            }
            if ( l is FarmHouse )
            {
                IList<Item> list = ( l as FarmHouse ).fridge.Value.items;
                for ( int i = 0; i < list.Count; ++i )
                {
                    if ( list[ i ] != null )
                    {
                        list[ i ] = MyUtility._recursiveIterateItem( list[ i ], action );
                    }
                }
            }
            foreach ( NPC character in l.characters )
            {
                if ( character is Child && ( character as Child ).hat.Value != null )
                {
                    ( character as Child ).hat.Value = ( Hat ) MyUtility._recursiveIterateItem( ( character as Child ).hat.Value, action );
                }
                if ( character is Horse && ( character as Horse ).hat.Value != null )
                {
                    ( character as Horse ).hat.Value = ( Hat ) MyUtility._recursiveIterateItem( ( character as Horse ).hat.Value, action );
                }
            }
            if ( l is BuildableGameLocation )
            {
                foreach ( Building b in ( l as BuildableGameLocation ).buildings )
                {
                    if ( b.indoors.Value != null )
                    {
                        MyUtility._recursiveIterateLocation( b.indoors.Value, action );
                    }
                    if ( b is Mill )
                    {
                        IList<Item> list = ( b as Mill ).output.Value.items;
                        for ( int i = 0; i < list.Count; ++i )
                        {
                            if ( list[ i ] != null )
                            {
                                list[ i ] = MyUtility._recursiveIterateItem( list[ i ], action );
                            }
                        }
                    }
                    else
                    {
                        if ( !( b is JunimoHut ) )
                        {
                            continue;
                        }
                        IList<Item> list = ( b as JunimoHut ).output.Value.items;
                        for ( int i = 0; i < list.Count; ++i )
                        {
                            if ( list[ i ] != null )
                            {
                                list[ i ] = MyUtility._recursiveIterateItem( list[ i ], action );
                            }
                        }
                    }
                }
            }
            var toRemove = new List<Vector2>();
            foreach ( var key in l.objects.Keys )
            {
                var ret = (StardewValley.Object ) MyUtility._recursiveIterateItem( l.objects[ key ], action );
                if ( ret == null )
                    toRemove.Add( key );
                else
                    l.objects[ key ] = ret;
            }
            foreach ( var r in toRemove )
                l.objects.Remove( r );
            foreach ( Debris d in l.debris )
            {
                if ( d.item != null )
                {
                    d.item = MyUtility._recursiveIterateItem( d.item, action );
                }
            }
        }
        private static Item _recursiveIterateItem( Item i, Func<Item, Item> action )
        {
            if ( i == null )
            {
                return null;
            }
            if ( i is StardewValley.Object )
            {
                StardewValley.Object o = i as StardewValley.Object;
                if ( o is StorageFurniture )
                {
                    IList<Item> list = ( o as StorageFurniture ).heldItems;
                    for ( int ii = 0; ii < list.Count; ++ii )
                    {
                        if ( list[ ii ] != null )
                        {
                            list[ ii ] = MyUtility._recursiveIterateItem( list[ ii ], action );
                        }
                    }
                }
                if ( o is Chest )
                {
                    IList<Item> list = ( o as Chest ).items;
                    for ( int ii = 0; ii < list.Count; ++ii )
                    {
                        if ( list[ ii ] != null )
                        {
                            list[ ii ] = MyUtility._recursiveIterateItem( list[ ii ], action );
                        }
                    }
                }
                if ( o.heldObject.Value != null )
                {
                    o.heldObject.Value = ( StardewValley.Object ) MyUtility._recursiveIterateItem( o.heldObject.Value, action );
                }
            }
            if ( i is Tool t )
            {
                IList<StardewValley.Object> list = t.attachments;
                for ( int ii = 0; ii < list.Count; ++ii )
                {
                    if ( list[ ii ] != null )
                    {
                        list[ ii ] = (StardewValley.Object) MyUtility._recursiveIterateItem( list[ ii ], action );
                    }
                }
            }
            return action( i );
        }
    }
}
