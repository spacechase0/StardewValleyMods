using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared.UI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

using MagicElement = MajesticArcana.Elements.Element;

namespace MajesticArcana
{
    internal class Spell
    {
        public class Slot
        {
            public string ManifestationElement { get; set; }
            public float ManifestationModifier { get; set; } = 1;
            public List<string> AttributeElements { get; set; } = new( new string[] { null } );
            public float AttributeStrength { get; set; } = 1;
        }

        public class Chain
        {
            // These are sorta different per manifestation...
            public enum Type
            {
                OnSpellFinish,
                OnSpellTrigger,
            }

            public Type ChainType { get; set; }
            public float WithDelay { get; set; } = 0;
        }

        public string Name { get; set; } = "Spell";
        public Slot Primary { get; set; } = new();
        public List<Tuple<Chain, Spell>> Secondaries { get; set; } = new();
    }

    // TODO: Confirmation box on close?
    internal class SpellcraftingMenu : IClickableMenu
    {
        private RootElement ui;

        private Textbox nameBox;

        private List<Image> uiElementsForMagicElements = new();
        private MagicElement currElem = null;

        private Spell editing;
        private Spell.Chain editingChain;

        public Action<Spell> OnSave { get; set; }

        private const int ElementScale = 6;

        public void Trash(Spell.Chain chain)
        {
            editing.Secondaries.RemoveAll(t => t.Item1 == chain);
        }

        public SpellcraftingMenu( Spell theEditing = null, Spell.Chain theChain = null )
        :   base( Game1.viewport.Width / 2 - 400, Game1.viewport.Height / 2 - 300, 800, 600, true )
        {
            editing = theEditing ?? new Spell();
            editingChain = theChain;

            ui = new();
            ui.LocalPosition = new(xPositionOnScreen, yPositionOnScreen);

            Image spellsButton = new()
            {
                Texture = Game1.objectSpriteSheet,
                TexturePixelArea = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 102, 16, 16),
                Scale = Game1.pixelZoom,
                Callback = (e) => { }, // TODO
                LocalPosition = new(32, 32),
            };
            ui.AddChild(spellsButton);

            Label helpButton = new()
            {
                NonBoldShadow = false,
                IdleTextColor = Color.White,
                HoverTextColor = Color.Gray,
                String = "?",
                Callback = (e) => { }, // TODO
            };
            helpButton.LocalPosition = new(800 - helpButton.Width - 32, 32);
            ui.AddChild(helpButton);

            if (editingChain != null)
            {
                Image trashButton = new()
                {
                    Texture = Game1.toolSpriteSheet,
                    TexturePixelArea = new( 224, 0, 16, 16 ),
                    Scale = Game1.pixelZoom,
                    Callback = (e) =>
                    {
                        (GetParentMenu() as SpellcraftingMenu).Trash(editingChain);
                        GetParentMenu().SetChildMenu(null);
                    },
                };
                trashButton.LocalPosition = new(800 - trashButton.Width - 32, 600 - trashButton.Height - 32);
                ui.AddChild(trashButton);
            }

            nameBox = new()
            {
                String = editing.Name,
                LocalPosition = new( ((spellsButton.LocalPosition.X + spellsButton.Width + 32) + (helpButton.LocalPosition.X - 32)) / 2, 40 ),
                Callback = (e) => { editing.Name = nameBox.String; },
            };
            nameBox.LocalPosition = new(nameBox.LocalPosition.X - nameBox.Width / 2, nameBox.LocalPosition.Y);
            ui.AddChild(nameBox);

            var elems = MagicElement.Elements;
            StaticContainer elementsContainer = new()
            {
                Size = new( 3 * ( 7 * ElementScale) + 2 * 8, ( elems.Count + 2 ) / 3 * ( (7 * ElementScale) + 8) - 8 ),
                LocalPosition = new( 800 + IClickableMenu.spaceToClearSideBorder, 0 ),
                OutlineColor = Color.White,
            };
            int ie = 0;
            foreach (var elem in elems)
            {
                Image elemButton = new()
                {
                    Texture = elem.Value.Tilesheet,
                    TexturePixelArea = elem.Value.TextureRect,
                    Scale = ElementScale,
                    Callback = (e) =>
                    {
                        if (currElem != elem.Value)
                            currElem = elem.Value;
                        else
                            currElem = null;
                    },
                    LocalPosition = new( ( ie % 3 ) * ( 7 * ElementScale + 8 ), ( ie / 3 ) * ( 7 * ElementScale + 8 ) ),
                    UserData = elem.Value,
                };
                uiElementsForMagicElements.Add(elemButton);
                elementsContainer.AddChild(elemButton);
                ++ie;
            }
            ui.AddChild(elementsContainer);
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape) exitThisMenu();
            // E was closing the menu, even with a textbox selected
        }

        private int justClickedX = -1, justClickedY = -1;
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height).Contains(x, y))
            {
                // Bad practice probably to do things this way, but its easy
                justClickedX = x;
                justClickedY = y;
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            ui.Update();
        }

        public override void draw(SpriteBatch b)
        {
            drawTextureBox(b, xPositionOnScreen - IClickableMenu.spaceToClearSideBorder, yPositionOnScreen - IClickableMenu.spaceToClearSideBorder, 800 + IClickableMenu.spaceToClearSideBorder * 2, height + IClickableMenu.spaceToClearSideBorder * 2, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), Color.Black);
            ui.Draw(b);

            // TODO: for each parent also a spellcrafting menu, draw a < (which is @ in the spritetext I think?)
            // When any clicked, saves this branch and closes this menu (going to previous one)

            int fourthX = xPositionOnScreen + width / 4;
            int centerX = xPositionOnScreen + width / 2;
            int threeFourthX = xPositionOnScreen + width / 4 * 3;
            int topY = yPositionOnScreen + height / 3;

            // cheating a bit, should rename variables...
            int moveLeft = 75;
            fourthX -= moveLeft;
            centerX -= moveLeft;
            threeFourthX -= moveLeft;

            if (editingChain != null)
            {
                // TODO: draw those fields
            }

            int firstBoxX = fourthX - 7 * ElementScale / 2 - IClickableMenu.spaceToClearSideBorder;
            int firstBoxY = topY - 7 * ElementScale / 2 - IClickableMenu.spaceToClearSideBorder;
            int boxSize = 7 * ElementScale + IClickableMenu.spaceToClearSideBorder * 2;
            drawTextureBox(b, firstBoxX, firstBoxY, boxSize, boxSize, Color.White);
            if (new Rectangle(firstBoxX, firstBoxY, boxSize, boxSize).Contains(justClickedX, justClickedY))
            {
                editing.Primary.ManifestationElement = currElem?.Id;
            }
            if (editing.Primary.ManifestationElement != null)
            {
                var elem = MagicElement.Elements[editing.Primary.ManifestationElement];
                b.Draw(elem.Tilesheet, new Vector2( firstBoxX + IClickableMenu.spaceToClearSideBorder, firstBoxY + IClickableMenu.spaceToClearSideBorder ), elem.TextureRect, Color.White, 0, Vector2.Zero, ElementScale, SpriteEffects.None, 1);
                // TODO: draw manifestation modifier
            }

            // TODO: Draw attribute strength
            for ( int i = 0; i < editing.Primary.AttributeElements.Count; ++i)
            {
                int boxX = centerX - 7 * ElementScale / 2 - IClickableMenu.spaceToClearSideBorder;
                int boxY = topY + -boxSize / 2 + boxSize * i;

                if (new Rectangle(boxX, boxY, boxSize, boxSize).Contains(justClickedX, justClickedY))
                {
                    editing.Primary.AttributeElements[i] = currElem?.Id;
                    if (i < editing.Primary.AttributeElements.Count - 1)
                    {
                        editing.Primary.AttributeElements.RemoveAt(i);
                        justClickedX = justClickedY = -1; // Needed so this doesn't trigger next round
                        --i;
                        continue;
                    }
                    else if (i == editing.Primary.AttributeElements.Count - 1)
                    {
                        editing.Primary.AttributeElements.Add(null);
                    }
                }

                string attrElem = editing.Primary.AttributeElements[i];
                drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                               boxX, boxY, boxSize, boxSize, Color.White, drawShadow: false);

                if (attrElem != null)
                {
                    var elem = MagicElement.Elements[attrElem];
                    b.Draw(elem.Tilesheet, new Vector2(boxX + IClickableMenu.spaceToClearSideBorder, boxY + IClickableMenu.spaceToClearSideBorder), elem.TextureRect, Color.White, 0, Vector2.Zero, ElementScale, SpriteEffects.None, 1);
                }
            }

            for (int i = 0; i < editing.Secondaries.Count; ++i)
            {
                int chainY = topY + i * 80 - 32;
                drawTextureBox(b, threeFourthX - boxSize, chainY, 300, 64, Color.White);
                b.DrawString(Game1.smallFont, editing.Secondaries[i].Item2.Name, new Vector2(threeFourthX - boxSize + IClickableMenu.spaceToClearSideBorder, chainY + IClickableMenu.spaceToClearSideBorder), Color.Black);

                if (new Rectangle(threeFourthX - boxSize, chainY, 300, 64).Contains(justClickedX, justClickedY))
                {
                    SetChildMenu(new SpellcraftingMenu(editing.Secondaries[ i ].Item2, editing.Secondaries[i].Item1));
                }
            }

            int createChainY = topY + editing.Secondaries.Count * 80 - 32;
            drawTextureBox(b, threeFourthX - boxSize, createChainY, 300, 64, Color.White);
            b.DrawString(Game1.smallFont, I18n.Spellcrafting_CreateChain(), new Vector2( threeFourthX - boxSize + IClickableMenu.spaceToClearSideBorder, createChainY + IClickableMenu.spaceToClearSideBorder ), Color.Black);
            if (new Rectangle(threeFourthX - boxSize, createChainY, 300, 64).Contains(justClickedX, justClickedY))
            {
                Spell.Chain newChain = new();
                Spell newSpell = new();
                newSpell.Name = "Chain";
                editing.Secondaries.Add(new(newChain, newSpell));
                SetChildMenu(new SpellcraftingMenu(newSpell, newChain));
            }

            SpriteText.drawString(b, ">", firstBoxX + boxSize + 56, topY - 24, color: SpriteText.color_White);
            SpriteText.drawString(b, ">", threeFourthX - boxSize - 56, topY - 24, color: SpriteText.color_White);

            if (currElem != null)
            {
                b.Draw(currElem.Tilesheet, new Vector2(Game1.getMouseX(), Game1.getMouseY()), currElem.TextureRect, Color.White, 0, currElem.TextureRect.Size.ToVector2() / 2, ElementScale, SpriteEffects.None, 1);
            }
            drawMouse(b);

            foreach (var uiElem in uiElementsForMagicElements)
            {
                if (uiElem.Hover)
                {
                    var magicElem = uiElem.UserData as MagicElement;
                    drawToolTip(b, magicElem.Description + "\n\n" + I18n.Element_Manifestation(magicElem.Manifestation) + "\n\n" + I18n.Element_Attribute(magicElem.Attribute), magicElem.Name, null);
                }
            }

            if (justClickedX != -1 && justClickedY != -1)
                currElem = null;
            justClickedX = justClickedY = -1;
        }
    }
}
