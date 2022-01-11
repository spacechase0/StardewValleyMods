using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Netcode;

namespace MisappliedPhysicalities.Game.Network
{
    public class ConnectorConnection : INetObject<NetFields>
    {
        private readonly NetEnum<Layer> otherLayer = new();
        private readonly NetPoint otherPoint = new( new Point( -1, -1 ) );

        public Layer OtherLayer { get { return otherLayer.Value; } set { otherLayer.Value = value; } }
        public Point OtherPoint { get { return otherPoint.Value; } set { otherPoint.Value = value; } }

        public NetFields NetFields { get; } = new NetFields();

        public ConnectorConnection()
        {
            NetFields.AddFields( otherLayer, otherPoint );
        }

        public override bool Equals( object obj )
        {
            if ( obj is not ConnectorConnection conn )
                return false;

            return OtherLayer == conn.OtherLayer && OtherPoint == conn.OtherPoint;
        }
    }
}
