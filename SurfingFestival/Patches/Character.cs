using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace SurfingFestival.Patches
{
    [HarmonyPatch(typeof(Character), nameof(Character.draw), typeof( SpriteBatch ))]
    public static class CharacterDrawPatch
    {
        public static void Prefix(Character __instance, SpriteBatch b)
        {
            if ( Game1.CurrentEvent?.FestivalName != Mod.festivalName || Game1.CurrentEvent?.playerControlSequenceID != "surfingRace" )
                return;

            Mod.DrawSurfboard( __instance, b );
        }

        public static void Postfix( Character __instance, SpriteBatch b )
        {
            if ( Game1.CurrentEvent?.FestivalName != Mod.festivalName || Game1.CurrentEvent?.playerControlSequenceID != "surfingRace" )
                return;

            Mod.DrawSurfingStatuses( __instance, b );
        }
    }
}
