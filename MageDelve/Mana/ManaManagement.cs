using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MageDelve.Mana
{
    public static class Farmer_Mana
    {
        internal class Holder { public readonly NetFloat currMana = new(); public readonly NetFloat maxMana = new(); }

        internal static ConditionalWeakTable< Farmer, Holder > values = new();

        internal static void Register()
        {
            Mod.SpaceCore.RegisterCustomProperty(
                typeof( Farmer ), "Mana",
                typeof( NetFloat ),
                AccessTools.Method( typeof( Farmer_Mana ), nameof( get_Mana ) ),
                AccessTools.Method( typeof( Farmer_Mana ), nameof( set_Mana ) ) );
            Mod.SpaceCore.RegisterCustomProperty(
                typeof( Farmer ), "MaxMana",
                typeof( NetFloat ),
                AccessTools.Method( typeof( Farmer_Mana ), nameof( get_MaxMana ) ),
                AccessTools.Method( typeof( Farmer_Mana ), nameof( set_MaxMana ) ) );
        }

        public static void set_Mana( this Farmer farmer, NetFloat newVal )
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetFloat get_Mana( this Farmer farmer )
        {
            var holder = values.GetOrCreateValue( farmer );
            return holder.currMana;
        }

        public static void set_MaxMana( this Farmer farmer, NetFloat newVal )
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetFloat get_MaxMana( this Farmer farmer )
        {
            var holder = values.GetOrCreateValue( farmer );
            return holder.maxMana;
        }
    }

    [HarmonyPatch( typeof( Farmer ), "farmerInit" )]
    public static class FarmerAddManaNetPatch
    {
        public static void Postfix( Farmer __instance )
        {
            __instance.NetFields.AddField(__instance.get_Mana(), "Mana");
            __instance.NetFields.AddField(__instance.get_MaxMana(), "MaxMana");
        }
    }

    // This is a patch instead of using the RenderedHud event so that it can go underneath 
    // the text from hovering over the health/stamina bar.
    [HarmonyPatch( typeof( Game1 ), "drawHUD" )]
    public static class Game1DrawManaBarPatch
    {
        public static void Prefix()
        {
            // Derived from the code I wrote for Mana Bar: https://github.com/spacechase0/StardewValleyMods/blob/develop/ManaBar/Mod.cs#L66
            // Which was derived from vanilla code

            // TODO: SpaceCore API for placing bars on the right side (to reduce mod conflicts)

            if ( !Context.IsPlayerFree )
                return;

            SpriteBatch b = Game1.spriteBatch;
            float curr = Game1.player.GetCurrentMana();
            float max = Game1.player.GetMaxMana();

            if ( max <= 0 )
                return;

            float perc = curr / max;

            Rectangle area = new(
                Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Right - 48 - 8 - ManaEngine.manaBg.Width * Game1.pixelZoom - 8,
                Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - ManaEngine.manaBg.Height * Game1.pixelZoom - 16,
                ManaEngine.manaBg.Width * Game1.pixelZoom,
                ManaEngine.manaBg.Height * Game1.pixelZoom );
            if ( Game1.showingHealthBar || Game1.showingHealth )
                area.X -= 56;

            b.Draw(ManaEngine.manaBg, area, null, Color.White );

            float height = 41;
            int filled = ( int )( 41 * perc );

            if ( filled > 0 )
            {
                Rectangle fill = new(
                    area.X + 3 * Game1.pixelZoom,
                    area.Y + ( int )( 13 + height - filled ) * Game1.pixelZoom,
                    6 * Game1.pixelZoom,
                    filled * Game1.pixelZoom );

                b.Draw( Game1.staminaRect, fill, null, ManaEngine.manaCol );
            }

            if ( area.Contains( Game1.getMouseX(), Game1.getMouseY() ) )
                Game1.drawWithBorder( $"{( int ) curr}/{( int ) max}", Color.Black * 0, Color.White, new Vector2( area.Left - Game1.dialogueFont.MeasureString( "999/999" ).X - 16, area.Top + 64 ) );
        }
    }
}
