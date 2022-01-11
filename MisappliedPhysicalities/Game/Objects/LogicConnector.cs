using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MisappliedPhysicalities.Game.Network;
using MisappliedPhysicalities.VirtualProperties;
using Netcode;
using StardewValley;

namespace MisappliedPhysicalities.Game.Objects
{
    [XmlType( "Mods_spacechase0_MisappliedPhysicalities_LogicConnector" )]
    public class LogicConnector : ConnectorBase, IUpdatesEvenWithoutFarmer
    {
        public readonly NetDouble currSignal = new();

        public LogicConnector() : base() { }
        public LogicConnector( Vector2 tile, Layer layer, Side connect ) : base( tile, layer, connect ) { }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField( currSignal );
        }

        public override ConnectorType GetConnectorType()
        {
            return ConnectorType.Logic;
        }
        public override InOutType GetInOutType( GameLocation loc )
        {
            if ( GetAttachedMachine( loc ) is not ILogicObject logic )
                return InOutType.None;

            return logic.GetLogicTypeForSide( connectionSide.Value.GetOpposite() );
        }

        public override Texture2D GetTexture()
        {
            return Assets.LogicConnector;
        }

        public override ConnectorBase MakeMe( Vector2 tile, Layer layer, Side connect )
        {
            return new LogicConnector( tile, layer, connect );
        }

        protected override string loadDisplayName()
        {
            return Mod.instance.Helper.Translation.Get( "item.logic-connector.name" );
        }

        public override string getDescription()
        {
            return Mod.instance.Helper.Translation.Get( "item.logic-connector.description" );
        }

        public override bool canStackWith( ISalable other )
        {
            return other is LogicConnector;
        }

        public override Item getOne()
        {
            var ret = new LogicConnector();
            this._GetOneFrom( ret );
            return ret;
        }

        public void UpdateEvenWithoutFarmer( GameLocation loc, GameTime time )
        {
            var io = GetInOutType( loc );
            var machine = GetAttachedMachine( loc ) as ILogicObject;
            if ( machine == null )
                return;

            if ( io == InOutType.Input )
            {
                machine.SendLogicTo( connectionSide.Value.GetOpposite(), currSignal.Value );
                //SpaceShared.Log.Debug( "Sending " + currSignal.Value + " to " + machine );
            }
            else if ( io == InOutType.Output )
            {
                currSignal.Value = machine.GetLogicFrom( connectionSide.Value.GetOpposite() );
                //SpaceShared.Log.Debug( "got " + currSignal.Value + " from " + machine );

                foreach ( var conn in connections )
                {
                    var container = loc.GetContainerForLayer( conn.OtherLayer );
                    if ( container.TryGetValue( new Vector2( conn.OtherPoint.X, conn.OtherPoint.Y ), out StardewValley.Object obj ) &&
                         obj is LogicConnector logicConn )
                    {
                        logicConn.currSignal.Value = currSignal.Value;
                    }
                }
            }
        }
    }
}
