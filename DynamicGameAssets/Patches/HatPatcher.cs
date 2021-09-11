namespace DynamicGameAssets.Patches
{
    ///// <summary>Applies Harmony patches to <see cref="Hat"/>.</summary>
    //[SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    //internal class HatPatcher : BasePatcher
    //{
    //    /*********
    //    ** Public methods
    //    *********/
    //    /// <inheritdoc />
    //    public override void Apply(Harmony harmony, IMonitor monitor)
    //    {
    //        harmony.Patch(
    //            original: this.RequireMethod<Hat>(nameof(Hat.draw)),
    //            prefix: this.GetHarmonyMethod(nameof(Before_Draw))
    //        );
    //    }


    //    /*********
    //    ** Private methods
    //    *********/
    //    /// <summary>The method to call before <see cref="Hat.draw"/>.</summary>
    //    /// <returns>Returns whether to run the original method.</returns>
    //    private static bool Before_Draw(Hat __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction)
    //    {
    //        if (__instance is CustomHat hat)
    //        {
    //            hat.Draw(spriteBatch, location, scaleSize, transparency, layerDepth, direction);
    //            return false;
    //        }

    //        return true;
    //    }
    //}
}
