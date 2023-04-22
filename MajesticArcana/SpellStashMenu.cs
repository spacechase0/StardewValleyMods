using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace MajesticArcana
{
    public class SpellStashMenu : IClickableMenu
    {
        private const int SpellsPerPage = 8;

        private List<Spell> defaultSpells;
        private List<Spell> spells = new();
        private int page = 0;

        public SpellStashMenu()
        : base(Game1.uiViewport.Width / 2 - 300, Game1.uiViewport.Height / 2 - 200, 600, 400, true)
        {
            defaultSpells = GetDefaultSpells();
            //for (int i = 0; i < 15; ++i)
                spells.AddRange(defaultSpells);
            if (Game1.player.modData.TryGetValue(Mod.SpellStashKey, out string spellStash))
                spells.AddRange(JsonConvert.DeserializeObject<List<Spell>>(spellStash));
        }

        private int? leftClickX = null, leftClickY = null;
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            leftClickX = x;
            leftClickY = y;
        }

        public override void draw(SpriteBatch b)
        {
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

            for (int i = page * SpellsPerPage; i < Math.Min(spells.Count, (page + 1) * SpellsPerPage); ++i)
            {
                Spell spell = spells[i];
                int num = i % SpellsPerPage;
                int x = xPositionOnScreen + 50 + (num % 4) * 100 + 32 * (num % 4);
                int y = yPositionOnScreen + 32 + (num / 4) * 130 + 32 * (num / 4);

                Texture2D tex = Game1.staminaRect;
                Color col = Color.Magenta;
                if (Mod.SpellIcons.ContainsKey(spell.Icon))
                {
                    tex = Mod.SpellIcons[spell.Icon];
                    col = Color.White;
                }
                b.Draw(tex, new Rectangle(x, y, 100, 100), col);

                b.DrawString(Game1.smallFont, spell.Name, new Vector2(x + 50 - Game1.smallFont.MeasureString(spell.Name).X / 2, y + 110), Color.Black);

                if (!defaultSpells.Contains(spell))
                {
                    b.Draw(Game1.toolSpriteSheet, new Rectangle(x - 8, y + 76, 32, 32), new(224, 0, 16, 16), Color.White);

                    if (leftClickX.HasValue && new Rectangle(x - 8, y + 76, 32, 32).Contains(leftClickX.Value, leftClickY.Value))
                    {
                        spells.Remove(spell);
                        if (page >= (int)Math.Ceiling((float)spells.Count / SpellsPerPage))
                            page--;
                        leftClickX = leftClickY = null;
                    }
                }

                if (true)
                {
                    b.Draw(Game1.mouseCursors, new Rectangle(x + 76, y + 76, 32, 32), new(128, 256, 64, 64), Color.White);

                    if (leftClickX.HasValue && new Rectangle(x + 76, y + 76, 32, 32).Contains(leftClickX.Value, leftClickY.Value))
                    {
                        if (GetParentMenu() is SpellcraftingMenu spellcrafting)
                        {
                            spellcrafting.Load(spell);
                            GetParentMenu().SetChildMenu(null);
                        }
                        // TODO: else put on hotbar or item or whatever?
                        leftClickX = leftClickY = null;
                    }
                }
            }

            int maxPages = (int)Math.Ceiling((float) spells.Count / SpellsPerPage);
            string pageStr = "Page " + (page+1) + "/" + maxPages; // I18n.SpellStash_Page(page + 1, maxPages);
            b.DrawString(Game1.smallFont, pageStr, new Vector2(xPositionOnScreen + (width - Game1.smallFont.MeasureString( pageStr ).X) / 2, yPositionOnScreen + height - 50), Color.Black);

            if (page > 0)
            {
                SpriteText.drawString(b, "@", xPositionOnScreen + 32, yPositionOnScreen + height - 64);
                if (leftClickX.HasValue && new Rectangle(xPositionOnScreen + 32 - 8, yPositionOnScreen + height - 64 - 8, 48, 48).Contains(leftClickX.Value, leftClickY.Value))
                {
                    --page;
                    leftClickX = leftClickY = null;
                }
            }
            if (page + 1 < maxPages)
            {
                SpriteText.drawString(b, ">", xPositionOnScreen + width - 64, yPositionOnScreen + height - 64);
                if (leftClickX.HasValue && new Rectangle(xPositionOnScreen + width - 64 - 8, yPositionOnScreen + height - 64 - 8, 48, 48).Contains(leftClickX.Value, leftClickY.Value))
                {
                    ++page;
                    leftClickX = leftClickY = null;
                }
            }

            drawMouse(b);
        }

        private static List<Spell> GetDefaultSpells()
        {
            List<Spell> ret = new();

            ret.Add(new()
            {
                Name = "Fireball",
                Icon = "fireball-red-1.png",
                Primary = new()
                {
                    ManifestationElement = "fire",
                    AttributeElements = new( new[] { "fire" } ),
                    AttributeStrength = 5,
                }
            });

            ret.Add(new()
            {
                Name = "Fireballs",
                Icon = "fire-arrows-2.png",
                Primary = new()
                {
                    ManifestationElement = "fire",
                    ManifestationModifier = 5,
                    AttributeElements = new(new[] { "fire" }),
                    AttributeStrength = 3,
                }
            });

            ret.Add(new()
            {
                Name = "Stream",
                Icon = "beam-blue-3.png",
                Primary = new()
                {
                    ManifestationElement = "water",
                    AttributeElements = new(new[] { "fire" }),
                    AttributeStrength = 2,
                }
            });

            ret.Add(new()
            {
                Name = "Fireboom",
                Icon = "fireball-red-3.png",
                Primary = new()
                {
                    ManifestationElement = "fire",
                    AttributeElements = new(new[] { "fire" }),
                    AttributeStrength = 1,
                },
                Secondaries = new(new[] {
                    new Tuple<Spell.Chain, Spell>( new()
                    {
                        ChainType = Spell.Chain.Type.OnSpellTrigger,
                    },
                    new()
                    {
                        Name = "Explosion",
                        Icon = "explosion-orange-3.png",
                        Primary = new()
                        {
                            ManifestationElement = "solar",
                            ManifestationModifier = 4,
                            AttributeElements = new( new[] { "fire" } ),
                            AttributeStrength = 6,
                        }
                    } )
                })
            });

            ret.Add(new()
            {
                Name = "Self-Heal",
                Icon = "heal-royal-2.png",
                Primary = new()
                {
                    ManifestationElement = "light",
                    AttributeElements = new(new[] { "healing" }),
                    AttributeStrength = 5,
                }
            });

            ret.Add(new()
            {
                Name = "Haste",
                Icon = "haste-sky-3.png",
                Primary = new()
                {
                    ManifestationElement = "light",
                    AttributeElements = new(new[] { "air" }),
                    AttributeStrength = 3,
                }
            });

            ret.Add(new()
            {
                Name = "Defense",
                Icon = "protect-sky-1.png",
                Primary = new()
                {
                    ManifestationElement = "light",
                    AttributeElements = new(new[] { "water" }),
                    AttributeStrength = 3,
                }
            });

            ret.Add(new()
            {
                Name = "Blink",
                Icon = "evil-eye-eerie-1.png",
                Primary = new()
                {
                    ManifestationElement = "space",
                }
            });

            return ret;
        }
    }
}
