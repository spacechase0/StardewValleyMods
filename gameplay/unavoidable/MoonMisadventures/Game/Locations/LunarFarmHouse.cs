using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;

namespace MoonMisadventures.Game.Locations
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_LunarFarmHouse" )]
    public class LunarFarmHouse : LunarLocation
    {
        public readonly NetBool visited = new();

        public LunarFarmHouse()
        {
        }

        public LunarFarmHouse( IModContentHelper content )
        : base( content, "MoonFarmHouse", "MoonFarmHouse" )
        {
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField( visited, "visited" );

            visited.InterpolationEnabled = false;
            visited.fieldChangeVisibleEvent += delegate { SpawnBeds(); };
        }

        public override void TransferDataFromSavedLocation( GameLocation l )
        {
            visited.Value = ( l as LunarFarmHouse ).visited.Value;
            base.TransferDataFromSavedLocation( l );
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();
            visited.Value = true;
            Game1.background = null;
        }

        private void SpawnBeds()
        {
            if ( !visited.Value )
                return;

            int players = 0;
            foreach ( var farmer in Game1.getAllFarmers() )
                ++players;

            furniture.Add( new BedFurniture( "2048", new Vector2( 4, 4 ) ) );
            if ( --players > 0 )
            {
                furniture.Add( new BedFurniture("2048", new Vector2( 4, 8 ) ) );
                if ( --players > 0 )
                {
                    furniture.Add( new BedFurniture("2048", new Vector2( 12, 4 ) ) );
                    if ( --players > 0 )
                        furniture.Add( new BedFurniture("2048", new Vector2( 12, 8 ) ) );
                }
            }
        }
    }
}
