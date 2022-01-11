using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MisappliedPhysicalities
{
    public enum ConnectorType
    {
        Power,
        Logic,
        Storage,
    }

    public static class ConnectorTypeExtensions
    {
        public static Color GetConnectorWireColor( this ConnectorType type )
        {
            switch ( type )
            {
                case ConnectorType.Power: return new Color( 255, 159, 42 );
                case ConnectorType.Logic: return new Color( 194, 229, 255 );
                case ConnectorType.Storage: return new Color( 255, 213, 0 );
            }

            return Color.Magenta;
        }

        public static int GetConnectorWireItem( this ConnectorType type )
        {
            switch ( type )
            {
                case ConnectorType.Power: return StardewValley.Object.copperBar;
                case ConnectorType.Logic: return StardewValley.Object.ironBar;
                case ConnectorType.Storage: return StardewValley.Object.goldBar;
            }

            return -1;
        }

        public static ConnectorType? GetConnectorTypeFromWireItemId( this int parentSheetIndex )
        {
            switch ( parentSheetIndex )
            {
                case StardewValley.Object.copperBar: return ConnectorType.Power;
                case StardewValley.Object.ironBar: return ConnectorType.Logic;
                case StardewValley.Object.goldBar: return ConnectorType.Storage;
            }

            return null;
        }
    }
}
