using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures.Game.Items;
using MoonMisadventures.VirtualProperties;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace MoonMisadventures.Patches
{
    [HarmonyPatch( typeof( InventoryPage ), MethodType.Constructor, new Type[] { typeof( int ), typeof( int ), typeof( int ), typeof( int ) } )]
    public static class InventoryPageNecklaceConstructorPatch
    {
        public static void Postfix( InventoryPage __instance )
        {
            __instance.equipmentIcons.Add(
                new ClickableComponent(
                    new Rectangle( __instance.xPositionOnScreen + 48 + 208 - 80 - ( Mod.instance.Helper.ModRegistry.IsLoaded("bcmpinc.WearMoreRings") ? 208 : -144 ),
                        __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 256 - 12,
                        64, 64 ),
                    "Necklace" )
                {
                    myID = 123450101, // TODO: Replace with Nexus mod id prefix
                    rightNeighborID = 101,
                    fullyImmutable = true,
                } );
        }
    }

    [HarmonyPatch( typeof( InventoryPage ), nameof( InventoryPage.performHoverAction ) )]
    public static class InventoryPageNecklaceHoverPatch
    {
        public static void Postfix( InventoryPage __instance, int x, int y, ref Item ___hoveredItem, ref string ___hoverText, ref string ___hoverTitle )
        {
            var necklaceSlot = __instance.equipmentIcons.First( cc => cc.myID == 123450101 );
            if ( necklaceSlot.containsPoint( x, y ) && Game1.player.get_necklaceItem().Value != null )
            {
                var necklaceItem = Game1.player.get_necklaceItem().Value;
                ___hoveredItem = necklaceItem;
                ___hoverText = necklaceItem.getDescription();
                ___hoverTitle = necklaceItem.DisplayName;
            }
        }
    }
    [HarmonyPatch( typeof( InventoryPage ), nameof( InventoryPage.receiveLeftClick ) )]
    public static class InventoryPageNecklaceLeftClickPatch
    {
        public static bool Prefix( InventoryPage __instance, int x, int y )
        {
            var necklaceSlot = __instance.equipmentIcons.First( cc => cc.myID == 123450101 );
            if ( necklaceSlot.containsPoint( x, y ) )
            {
                var necklaceItem = Game1.player.get_necklaceItem();
                if ( Game1.player.CursorSlotItem == null || Game1.player.CursorSlotItem is Necklace )
                {
                    Item tmp = Mod.instance.Helper.Reflection.GetMethod( __instance, "takeHeldItem" ).Invoke<Item>();
                    Item held = necklaceItem.Value;
                    if ( held != null )
                        ( held as Necklace ).OnUnequip( Game1.player );
                    held = Utility.PerformSpecialItemGrabReplacement( held );
                    Mod.instance.Helper.Reflection.GetMethod( __instance, "setHeldItem" ).Invoke( held );
                    necklaceItem.Value = tmp;

                    LevelUpMenu.RevalidateHealth( Game1.player );

                    if ( necklaceItem.Value != null )
                    {
                        ( necklaceItem.Value as Necklace ).OnEquip( Game1.player );
                        Game1.playSound( "crit" );
                    }
                    else if ( Game1.player.CursorSlotItem != null )
                        Game1.playSound( "dwop" );
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch( typeof( InventoryPage ), nameof( InventoryPage.draw ) )]
    public static class InventoryPageNecklaceDrawPatch
    {
        public static void Postfix( InventoryPage __instance, SpriteBatch b, Item ___hoveredItem)
        {
            if (___hoveredItem != null && ___hoveredItem != Game1.player.get_necklaceItem().Value)
                return;

            var necklaceSlot = __instance.equipmentIcons.First( cc => cc.myID == 123450101 );
            if ( Game1.player.get_necklaceItem().Value != null )
            {
                b.Draw( Game1.menuTexture, necklaceSlot.bounds, Game1.getSourceRectForStandardTileSheet( Game1.menuTexture, 10 ), Color.White );
                Game1.player.get_necklaceItem().Value.drawInMenu( b, new Vector2( necklaceSlot.bounds.X, necklaceSlot.bounds.Y ), necklaceSlot.scale, 1f, 0.866f, StackDrawType.Hide );
            }
            else
            {
                b.Draw( Assets.NecklaceBg, necklaceSlot.bounds, null, Color.White );
            }
        }
    }

    [HarmonyPatch( typeof( Hoe ), nameof( Hoe.DoFunction ) )]
    public static class HoeWaterTilledWithNecklacePatch
    {
        private static void OnFeatureAdded( Vector2 key, TerrainFeature value )
        {
            if ( value is HoeDirt hd )
                hd.state.Value = HoeDirt.watered;
        }

        public static void Prefix( Hoe __instance, GameLocation location, Farmer who )
        {
            if ( who.HasNecklace( Necklace.Type.Water ) )
                location.terrainFeatures.OnValueAdded += OnFeatureAdded;
        }

        public static void Postfix( Hoe __instance, GameLocation location, Farmer who )
        {
            if ( who.HasNecklace( Necklace.Type.Water ) )
                location.terrainFeatures.OnValueAdded -= OnFeatureAdded;
        }
    }

    [HarmonyPatch]
    public static class MonsterTakeDamagePatch
    {
        internal static bool applyingShock = false;

        public static IEnumerable<MethodBase> TargetMethods()
        {
            var subclasses = from asm in AppDomain.CurrentDomain.GetAssemblies().Where( a => !a.FullName.Contains( "Steamworks.NET" ) )
                             from type in asm.GetTypes()
                             where type.IsSubclassOf( typeof( Monster ) )
                             select type;

            var ps = new Type[] { typeof( int ), typeof( int ), typeof( int ), typeof( bool ), typeof( double ), typeof( Farmer ) };

            yield return AccessTools.Method( typeof( Monster ), nameof( Monster.takeDamage ), ps );
            foreach ( var subclass in subclasses )
            {
                var meth = subclass.GetMethod( nameof( Monster.takeDamage ), ps );
                if ( meth != null && meth.DeclaringType == subclass )
                    yield return meth;
            }
        }

        public static void Postfix( Monster __instance, Farmer who )
        {
            if ( __instance.Health <= 0 )
            {
                if ( who.HasNecklace( Necklace.Type.Looting ) )
                    who.Money += __instance.MaxHealth / 8;
            }
            else
            {
                if ( who.HasNecklace( Necklace.Type.Shocking ) && !applyingShock )
                {
                    __instance.get_shocked().Value = 1000 + Game1.random.Next( 3 ) * 500;
                    __instance.set_shocker( who );
                }
            }
        }
    }

    [HarmonyPatch( typeof( Monster ), nameof( Monster.update ) )]
    public static class MonsterShockDamagePatch
    {
        public static void Postfix( Monster __instance, GameTime time, GameLocation location )
        {
            var shock = __instance.get_shocked();
            if ( shock.Value >= 0 )
            {
                int shockBefore = shock.Value;
                shock.Value -= time.ElapsedGameTime.Milliseconds;
                //SpaceShared.Log.Debug( "shocking " + shock.Value + " " + shockBefore + " " + ( shock.Value % 500 ) + " " + ( shockBefore % 500 ) );
                if ( shock.Value % 500 > shockBefore % 500 )
                {
                    if ( __instance.get_shocker() != null ) // Only happens for the player who shocked them since it isn't a net var, meaning this won't trigger for everyone
                    {
                        MonsterTakeDamagePatch.applyingShock = true;
                        int amt = __instance.takeDamage( ( int ) ( __instance.MaxHealth * 0.15 ), 0, 0, false, 0, __instance.get_shocker() );
                        if ( amt != -1 )
                        {
                            location.removeDamageDebris( __instance );
                            var monsterBox = __instance.GetBoundingBox();
                            location.debris.Add( new Debris( amt, new Vector2( monsterBox.Center.X + 16, monsterBox.Center.Y ), Color.Purple, 1, __instance ) );
                            if ( __instance.get_shocker() != null )
                            {
                                foreach ( BaseEnchantment enchantment2 in __instance.get_shocker().enchantments )
                                {
                                    enchantment2.OnDealDamage( __instance, location, __instance.get_shocker(), ref amt );
                                }
                            }
                        }
                        MonsterTakeDamagePatch.applyingShock = false;
                    }
                }
            }
        }
    }

    [HarmonyPatch]
    public static class MonsterDrawPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var subclasses = from asm in AppDomain.CurrentDomain.GetAssemblies().Where( a => !a.FullName.Contains( "Steamworks.NET" ) )
                             from type in asm.GetTypes()
                             where type.IsSubclassOf( typeof( Monster ) )
                             select type;

            var ps = new Type[] { typeof( SpriteBatch ) };

            yield return AccessTools.Method( typeof( Monster ), nameof( Monster.draw ), ps );
            foreach ( var subclass in subclasses )
            {
                var meth = subclass.GetMethod( nameof( Monster.draw ), ps );
                if ( meth != null && meth.DeclaringType == subclass )
                    yield return meth;
            }
        }

        public static void Postfix( Monster __instance, SpriteBatch b )
        {
            int shocked = __instance.get_shocked().Value;
            if ( shocked >= 0 )
            {
                int shocking = 500 - shocked % 500;
                var src = new Rectangle( 647, 1103, 16, 16 );
                b.Draw( Game1.mouseCursors, __instance.getLocalPosition(Game1.viewport) - ( __instance.Position - __instance.GetBoundingBox().Center.ToVector2() ), src, Color.White * (shocking / 250f), ( float )( Game1.ticks * 15 * Math.PI / 180 ), new Vector2( 8, 8 ), Game1.pixelZoom, SpriteEffects.None, (__instance.getStandingY() + 8) / 10000f );
            }
        }
    }
}
