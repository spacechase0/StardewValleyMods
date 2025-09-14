using System;
using Microsoft.Xna.Framework;
using SpaceShared;

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
            return Mod.Instance.ReserveNextMachineSpot();
        }

        public event EventHandler OnRoomSetup;
        internal void InvokeOnRoomSetup()
        {
            Log.Trace("Event: OnRoomSetup");
            if (this.OnRoomSetup == null)
                return;
            Util.InvokeEvent("ArcadeRoom.Api.OnRoomSetup", this.OnRoomSetup.GetInvocationList(), null);
        }
    }
}
