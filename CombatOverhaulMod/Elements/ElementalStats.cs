using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
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

namespace CombatOverhaulMod.Elements
{
    public static class Item_ElementalStatOverrides
    {
        internal class Holder { public readonly NetStringDictionary<double, NetDouble> Value = new(); }

        internal static ConditionalWeakTable< Item, Holder > values = new();

        internal static void Register()
        {
            Mod.instance.SpaceCore.RegisterCustomProperty(
                typeof( Item ), "ElementalStatOverrides",
                typeof( NetStringDictionary<double, NetDouble> ),
                AccessTools.Method( typeof( Item_ElementalStatOverrides ), nameof( get_ElementalStatOverrides ) ),
                AccessTools.Method( typeof( Item_ElementalStatOverrides ), nameof( set_ElementalStatOverrides ) ) );
        }

        public static void set_ElementalStatOverrides( this Item item, NetStringDictionary<double, NetDouble> newVal )
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetStringDictionary<double, NetDouble> get_ElementalStatOverrides( this Item item )
        {
            var holder = values.GetOrCreateValue( item );
            return holder.Value;
        }
    }

    [HarmonyPatch( typeof( Item ), MethodType.Constructor )]
    public static class ItemAddElementStatOverridesPatch
    {
        public static void Postfix( Item __instance )
        {
            __instance.NetFields.AddField( __instance.get_ElementalStatOverrides() );
        }
    }

    [HarmonyPatch]
    public static class ItemRoomForElementsTooltipPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var subclasses = from asm in AppDomain.CurrentDomain.GetAssemblies().Where( a => !a.FullName.Contains( "Steamworks.NET" ) && !a.IsDynamic )
                             from type in asm.GetExportedTypes()
                             where type.IsSubclassOf( typeof( Item ) )
                             select type;

            yield return AccessTools.Method( typeof( Item ), nameof( Item.getExtraSpaceNeededForTooltipSpecialIcons ) );
            foreach ( var subclass in subclasses )
            {
                if ( subclass == typeof( Tool ) ) // this calls base.___()
                    continue;

                var meth = subclass.GetMethod( nameof( Item.getExtraSpaceNeededForTooltipSpecialIcons ) );
                if ( meth != null && meth.DeclaringType == subclass )
                    yield return meth;
            }
        }

        public static void Postfix( Item __instance, SpriteFont font, int startingHeight, ref Point __result )
        {
            if ( __result.Y == 0 )
                __result.Y = startingHeight;

            var stats = __instance.GetElementalStats();
            foreach ( var stat in stats )
            {
                if ( stat.Value == 0 )
                    continue;
                __result.Y += 40;
            }
        }
    }

    [HarmonyPatch( typeof( IClickableMenu ), nameof( IClickableMenu.drawHoverText ), new Type[] { typeof( SpriteBatch ), typeof( StringBuilder ), typeof( SpriteFont ), typeof( int ), typeof( int ), typeof( int ), typeof( string ), typeof( int ), typeof( string[] ), typeof( Item ), typeof( int ), typeof( string ), typeof( int ), typeof( int ), typeof( int ), typeof( float ), typeof( CraftingRecipe ), typeof( IList<Item> ), typeof( Texture2D ), typeof( Rectangle? ), typeof( Color? ), typeof( Color? ) } )]
    public static class IClickableMenuDrawElementalHoverTextPatch
    {
        public static void DrawElementalStats( Item item, SpriteBatch b, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText )
        {
            item.drawTooltip( b, ref x, ref y, font, alpha, overrideText );

            var elements = Game1.content.Load<Dictionary<string, ElementData>>( "spacechase0.CombatOverhaulMod\\Elements" );

            y += 16;
            var stats = item.GetElementalStats();
            foreach ( var stat in stats )
            {
                if ( stat.Value == 0 )
                    continue;

                var elem = elements[ stat.Key ];
                b.Draw( /* TODO: cache the following */ Game1.content.Load< Texture2D >( elem.IconTexture ), new Vector2( x + 20, y ), null, Color.White * alpha, 0, Vector2.Zero, 2, SpriteEffects.None, 1 );
                b.DrawString( font, ( stat.Value > 0 ? "+" : "-" ) + stat.Value * 100, new Vector2( x + 40 + 20, y ), Game1.textColor );
                y += 40;
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> insns, ILGenerator ilgen )
        {
            List<CodeInstruction> ret = new();

            foreach ( var insn in insns )
            {
                if ( insn.Calls( AccessTools.Method( typeof( Item ), nameof( Item.drawTooltip ) ) ) )
                {
                    var tmp = CodeInstruction.Call( typeof( IClickableMenuDrawElementalHoverTextPatch ), nameof( DrawElementalStats ) );
                    insn.opcode = tmp.opcode;
                    insn.operand = tmp.operand;
                }

                ret.Add( insn );
            }

            return ret;
        }
    }
    
    [HarmonyPatch( typeof( Item ), nameof( Item.canStackWith ) )]
    public static class ItemCanStackElementsPatch
    {
        public static bool Prefix( Item __instance, ISalable other, ref bool __result )
        {
            if ( __instance != null && other is Item otherItem )
            {
                var eoverrideA = __instance.get_ElementalStatOverrides();
                var eoverrideB = otherItem.get_ElementalStatOverrides();

                if ( eoverrideA.Count() != eoverrideB.Count() )
                {
                    __result = false;
                    return false;
                }

                // https://stackoverflow.com/a/2913348/17827276
                if ( eoverrideA.Keys.Intersect( eoverrideB.Keys ).Count() != eoverrideA.Count() )
                {
                    __result = false;
                    return false;
                }

                foreach ( string key in eoverrideA.Keys )
                {
                    if ( eoverrideA[ key ] != eoverrideB[ key ] )
                    {
                        __result = false;
                        return false;
                    }    
                }
            }

            return true;
        }
    }
}
