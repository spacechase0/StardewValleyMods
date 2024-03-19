using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Tools;

namespace MoonMisadventures.Game
{
    public class InfuserMenu : MenuWithInventory
    {
        private static readonly Rectangle starRect1 = new Rectangle(14, 1474, 11, 11);
        private static readonly Rectangle starRect2 = new Rectangle(27, 1579, 9, 9);

        private List<TemporaryAnimatedSprite> stars = new();

        private ClickableTextureComponent mainSpot;
        private ClickableTextureComponent secondarySpot;

        private Item essenceItem;

        private float doingStars = -1;

        public InfuserMenu()
        :   base( null, true, true, 0, 0 )
        {
            for ( int i = 0; i < ( width * height ) / ( 100 * 100 ); ++i )
            {
                int ix = Game1.random.Next(0, 400);
                int iy = Game1.random.Next(0, 400);

                TemporaryAnimatedSprite star = null;
                if (Game1.random.Next(3) == 0)
                    stars.Add(star = new TemporaryAnimatedSprite("LooseSprites\\cursors", starRect1, new Vector2(ix, iy), false, 0, Color.White) { scale = 4 });
                else
                    stars.Add(star = new TemporaryAnimatedSprite("LooseSprites\\cursors", starRect2, new Vector2(ix, iy), false, 0, Color.White) { scale = 4 });

                star.motion = new Vector2(-1, 0);
            }

            base.inventory.highlightMethod = Highlight;

            mainSpot = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 - 32, yPositionOnScreen / 2 + 128+ 64, 64, 64), Game1.menuTexture, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), 1);
            secondarySpot = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 - 32, yPositionOnScreen / 2 + 32, 64, 64), Game1.menuTexture, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), 1);

            mainSpot.scale = secondarySpot.scale = 4;

            essenceItem = ( Item ) new StardewValley.Object(ItemIds.StellarEssence, 1);

            // todo - gamepad controls
        }

        private bool Highlight(Item i)
        {
            if ( mainSpot.item == null )
            {
                return true;
            }
            else
            {
                if (mainSpot.item is MeleeWeapon mw)
                {
                    if (Utility.IsNormalObjectAtParentSheetIndex(i, StardewValley.Object.prismaticShardID))
                        return true;
                    else if (mw.InitialParentTileIndex == 62 || mw.InitialParentTileIndex == 63 || mw.InitialParentTileIndex == 64)
                        return (i is StardewValley.Object dgai && dgai.ItemId == ItemIds.SoulSapphire);
                }
                else if (mainSpot.item is Tool)
                    return Utility.IsNormalObjectAtParentSheetIndex(i, StardewValley.Object.prismaticShardID);
                else
                    return (i is StardewValley.Object dgai && dgai.ItemId == ItemIds.PersistiumDust);
            }

            return false;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if ( !okButton.containsPoint( x, y ) )
                base.receiveLeftClick(x, y, playSound);

            Item held = heldItem;
            if ( mainSpot.containsPoint( x, y ) )
            {
                if ( secondarySpot.item == null )
                {
                    var tmp = mainSpot.item;
                    mainSpot.item = heldItem;
                    heldItem = tmp;
                }
                else
                {
                    Game1.playSound("cancel");
                }
            }
            else if ( secondarySpot.containsPoint( x, y ) && mainSpot.item != null )
            {
                bool doIt = false;
                if (mainSpot.item is MeleeWeapon mw)
                {
                    if (Utility.IsNormalObjectAtParentSheetIndex(heldItem, StardewValley.Object.prismaticShardID))
                        doIt = true;
                    else if (heldItem is StardewValley.Object dgai && dgai.ItemId == ItemIds.SoulSapphire && (mw.InitialParentTileIndex == 62 || mw.InitialParentTileIndex == 63 || mw.InitialParentTileIndex == 64))
                        doIt = true;
                }
                else if (mainSpot.item is Tool)
                {
                    if (Utility.IsNormalObjectAtParentSheetIndex(heldItem, StardewValley.Object.prismaticShardID))
                        doIt = true;
                }
                else
                {
                    if (heldItem is StardewValley.Object dgai && dgai.ItemId == ItemIds.PersistiumDust)
                        doIt = true;
                }

                if ( doIt )
                {
                    var tmp = secondarySpot.item;
                    secondarySpot.item = heldItem;
                    heldItem = tmp;
                }
                else
                {
                    Game1.playSound("cancel");
                }
            }
            else if ( okButton.containsPoint( x, y ) )
            {
                if ( mainSpot.item != null && secondarySpot.item != null && Game1.player.Items.CountId( ItemIds.StellarEssence ) >= 25 )
                {
                    doingStars = 0;
                }
                else
                {
                    Game1.playSound("cancel");
                }
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);

            if ( doingStars != -1 )
            {
                foreach ( var star in stars.ToList() )
                {
                    var offset = new Vector2((Game1.uiViewport.Width - 400) / 2 - 32, yPositionOnScreen - 100 - 32);
                    var dir = (mainSpot.bounds.Location.ToVector2() - ( star.position + offset ));
                    if (dir.Length() < doingStars)
                        stars.Remove(star);
                    else
                    {
                        dir.Normalize();
                        star.position += dir * doingStars;
                    }
                }
                doingStars += ( float ) time.ElapsedGameTime.TotalSeconds * 15;

                if ( stars.Count <= 0 )
                {
                    var item = DoCraft();
                    Game1.player.addItemByMenuIfNecessary(item);
                    Game1.player.holdUpItemThenMessage(item);
                    exitThisMenu();
                }
            }
            else
            {
                foreach (var star in stars)
                {
                    if (star.position.X + star.sourceRect.Width < 0)
                        star.position = new Vector2(400, star.position.Y);

                    star.update(time);
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, (Game1.uiViewport.Width - 400) / 2, yPositionOnScreen - 100, 400, 400, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle((Game1.uiViewport.Width - 400) / 2 + 12, yPositionOnScreen + 12 - 100, 400 - 24, 400 - 24), Color.Black );

            base.draw(b, false, false );
            b.Draw(Game1.menuTexture, mainSpot.bounds.Location.ToVector2(), mainSpot.sourceRect, Color.White);
            mainSpot.drawItem(b);
            if (mainSpot.item != null)
            {
                b.Draw(Game1.menuTexture, secondarySpot.bounds.Location.ToVector2(), secondarySpot.sourceRect, Color.White);
                secondarySpot.drawItem(b);

                var epos = secondarySpot.bounds.Location.ToVector2() + new Vector2(80, 0);
                essenceItem.drawInMenu(b, epos, 1);
                b.DrawString(Game1.dialogueFont, "x25", epos + new Vector2(64, 8), new Color(136, 255, 3));
            }

            b.End();
            using RasterizerState rs = new() { ScissorTestEnable = true };
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: rs);
            Game1.graphics.GraphicsDevice.ScissorRectangle = new Rectangle(( Game1.uiViewport.Width - 400 ) / 2 + 12, yPositionOnScreen + 12 - 100, 400 - 24, 400 - 24);

            foreach ( var star in stars )
            {
                var x = Game1.uiViewport;
                star.draw(b, xOffset: (Game1.uiViewport.Width - 400) / 2 + Game1.uiViewport.X, yOffset: yPositionOnScreen - 100 + Game1.uiViewport.Y);
            }

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);


            if (!base.hoverText.Equals(""))
            {
                IClickableMenu.drawToolTip(b, hoverText, hoveredItem.DisplayName, hoveredItem);
            }

            if (base.heldItem != null)
            {
                base.heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
            }
            drawMouse(b);
        }
        
        private Item DoCraft()
        {
            var result = mainSpot.item;
            if ( Utility.IsNormalObjectAtParentSheetIndex( secondarySpot.item, StardewValley.Object.prismaticShardID) )
            {
                EnchantAgain(result);
                if (--secondarySpot.item.Stack <= 0)
                    secondarySpot.item = null;
            }
            else if (secondarySpot.item is StardewValley.Object dgai2 && dgai2.ItemId == ItemIds.SoulSapphire && result is MeleeWeapon mw && ( mw.InitialParentTileIndex == 62 || mw.InitialParentTileIndex == 63 || mw.InitialParentTileIndex == 64 ) )
            {
                var oldResult = result;
                switch ( mw.InitialParentTileIndex)
                {
                    case 62:
                        result = new MeleeWeapon(ItemIds.CosmosSword);
                        break;
                    case 63:
                        result = new MeleeWeapon(ItemIds.CosmosClub);
                        break;
                    case 64:
                        result = new MeleeWeapon(ItemIds.CosmosDagger);
                        break;
                }

                (result as MeleeWeapon).enchantments.AddRange((oldResult as MeleeWeapon).enchantments);
                foreach (var kvp in oldResult.modData.Pairs)
                    result.modData.Add(kvp.Key, kvp.Value);
                foreach (var oldEnch in (result as MeleeWeapon).enchantments)
                    oldEnch.ApplyTo(result, (result as Tool).getLastFarmerToUse());

            }
            if ( secondarySpot.item is MeleeWeapon dgai && dgai.ItemId == ItemIds.PersistiumDust )
            {
                result.modData.Add("persists", "true");
                if (--secondarySpot.item.Stack <= 0)
                    secondarySpot.item = null;
            }

            Game1.player.removeFirstOfThisItemFromInventory(ItemIds.StellarEssence, 25);

            mainSpot.item = null;

            return result;
        }

        public static void EnchantAgain( Item item )
        {
            var t = item as Tool;
            int amtPrimary = t.enchantments.Count(e => !e.IsForge() && !e.IsSecondaryEnchantment());
            if (amtPrimary >= 2)
            {
                foreach (var ench in t.enchantments.Reverse())
                {
                    if ( !ench.IsForge() && !ench.IsSecondaryEnchantment() )
                    {
                        t.enchantments.Remove(ench);
                        break;
                    }
                }
            }
            var newEnch = Game1.random.ChooseFrom(BaseEnchantment.GetAvailableEnchantmentsForItem(item as Tool));
            t.enchantments.Add(newEnch);
            newEnch.ApplyTo(t, t.getLastFarmerToUse());
        }

        protected override void cleanupBeforeExit()
        {
            OnClose();
        }

        private void OnClose()
        {
            Utility.CollectOrDrop(heldItem);
            Utility.CollectOrDrop(mainSpot.item);
            Utility.CollectOrDrop(secondarySpot.item);

            heldItem = null;
            mainSpot.item = null;
            secondarySpot.item = null;
        }
    }
}
