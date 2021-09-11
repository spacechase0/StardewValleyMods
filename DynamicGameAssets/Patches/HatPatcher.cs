namespace DynamicGameAssets.Patches
{
    /*
    [HarmonyPatch( typeof( Hat ), nameof( Hat.draw ) )]
    public static class HatDrawPatch
    {
        public static bool Prefix( Hat  __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction )
        {
            if ( __instance is CustomHat ch )
            {
                ch.Draw( spriteBatch, location, scaleSize, transparency, layerDepth, direction );
                return false;
            }

            return true;
        }
    }
    */
}
