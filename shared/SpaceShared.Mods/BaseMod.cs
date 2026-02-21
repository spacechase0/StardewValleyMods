using System;
using System.Reflection;
using StardewModdingAPI;

namespace SpaceShared;

// Has to be public because of C# visibility rules
public abstract class BaseMod< ActualType > : StardewModdingAPI.Mod where ActualType : BaseMod< ActualType >
{
    public static ActualType Instance { get; private set; }

    public override void Entry(IModHelper helper)
    {
        //Helper.Reflection.GetProperty< ActualType >( typeof( ActualType ), nameof( Instance ) ).SetValue( this as ActualType );
        Instance = this as ActualType;
        Log.Monitor = Monitor;

        SetupConfig();
        SetupHarmony();
        SetupContent();
        ModEntry();
    }

    protected abstract void ModEntry();

    protected virtual void SetupConfig() { }
    protected virtual void SetupHarmony() { }
    protected virtual void SetupContent() { }
}
