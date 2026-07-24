using System;
using System.Reflection;
using StardewModdingAPI;

namespace SpaceShared;

#nullable enable

// Has to be public because of C# visibility rules
#pragma warning disable CS0436
public abstract class BaseMod< ActualType > : StardewModdingAPI.Mod where ActualType : BaseMod< ActualType >
{
    public static ActualType Instance { get; private set; } = null!;

    public static string ID => Instance.ModManifest.UniqueID;

    public override void Entry(IModHelper helper)
    {
        //Helper.Reflection.GetProperty< ActualType >( typeof( ActualType ), nameof( Instance ) ).SetValue( this as ActualType );
        Instance = (this as ActualType)!;
        Log.Monitor = Monitor;

        SetupTranslations();
        SetupConfig();
        SetupHarmony();
        SetupContent();
        SetupAssets();
        ModEntry();
    }

    protected abstract void ModEntry();

    protected virtual void SetupTranslations() { }
    protected virtual void SetupConfig() { }
    protected virtual void SetupHarmony() { }
    protected virtual void SetupContent() { }
    protected virtual void SetupAssets() { }
}
