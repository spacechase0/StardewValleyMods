using System;
using SpaceShared;
using StardewValley;

namespace Magic
{
    public interface IApi
    {
        event EventHandler OnAnalyzeCast;
    }

    public class Api : IApi
    {
        public event EventHandler OnAnalyzeCast;
        internal void InvokeOnAnalyzeCast(Farmer farmer)
        {
            Log.trace("Event: OnAnalyzeCast");
            if (OnAnalyzeCast == null)
                return;
            Util.invokeEvent("Magic.Api.OnAnalyzeCast", OnAnalyzeCast.GetInvocationList(), farmer);
        }
    }
}
