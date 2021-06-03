using Microsoft.Xna.Framework;
using SpaceShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadeRoom
{
    public interface IApi
    {
        Vector2 ReserveMachineSpot();

        event EventHandler OnRoomSetup; 
    }

    public class Api : IApi
    {
        public Vector2 ReserveMachineSpot()
        {
            return Mod.instance.ReserveNextMachineSpot();
        }
        
        public event EventHandler OnRoomSetup;
        internal void InvokeOnRoomSetup()
        {
            Log.trace( "Event: OnRoomSetup" );
            if ( OnRoomSetup == null )
                return;
            Util.invokeEvent( "ArcadeRoom.Api.OnRoomSetup", OnRoomSetup.GetInvocationList(), null );
        }
    }
}
