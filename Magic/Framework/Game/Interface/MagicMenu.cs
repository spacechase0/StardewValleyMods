using System.Linq;
using Magic.Framework.Schools;
using Magic.Framework.Skills;
using Magic.Framework.Spells;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using StardewValley;
using StardewValley.Menus;

namespace Magic.Framework.Game.Interface
{
    internal class MagicMenu : IClickableMenu
    {
        /*********
        ** Fields
        *********/
        private const int WindowWidth = 800;
        private const int WindowHeight = 600;
        private const int SchoolIconSize = 32;
        private const int SpellIconSize = 64;
        private const int SelIconSize = 192;
        private const int HotbarIconSize = 48;

        private readonly School School;
        private School Active;
        private Spell Sel;
        private PreparedSpell Dragging;

        private bool JustLeftClicked;
        private bool JustRightClicked;


        /*********
        ** Public methods
        *********/
        public MagicMenu(School school = null)
            : base((Game1.viewport.Size.Width - MagicMenu.WindowWidth) / 2, (Game1.viewport.Size.Height - MagicMenu.WindowHeight) / 2, MagicMenu.WindowWidth, MagicMenu.WindowHeight, true)
        {
            this.School = school;
            this.Active = school;
        }

        /// <inheritdoc />
        public override bool overrideSnappyMenuCursorMovementBan()
        {
            return true;
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            // get info
            SpellBook spellBook = Game1.player.GetSpellBook();
            bool hasFifthSpellSlot = Game1.player.HasCustomProfession(Skill.MemoryProfession);
            int hotbarHeight = 12 + 48 * (hasFifthSpellSlot ? 5 : 4) + 12 * (hasFifthSpellSlot ? 4 : 3) + 12;
            int gap = (MagicMenu.WindowHeight - hotbarHeight * 2) / 3 + (hasFifthSpellSlot ? 25 : 0);

            // draw main window
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, MagicMenu.WindowWidth, MagicMenu.WindowHeight, Color.White);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, MagicMenu.WindowWidth / 2, MagicMenu.WindowHeight, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + 12, this.yPositionOnScreen + 12, MagicMenu.WindowWidth / 2 - 24, MagicMenu.WindowHeight - 24), Color.Black);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen - MagicMenu.SchoolIconSize - 12, this.yPositionOnScreen, MagicMenu.SchoolIconSize + 24, MagicMenu.WindowHeight, Color.White);

            // draw school icons
            {
                int x = this.xPositionOnScreen - MagicMenu.SchoolIconSize - 12;
                int y = this.yPositionOnScreen;
                foreach (string schoolId in School.GetSchoolList())
                {
                    School school = School.GetSchool(schoolId);
                    Rectangle iconBounds = new(x + 12, y + 12, MagicMenu.SchoolIconSize, MagicMenu.SchoolIconSize);

                    IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, MagicMenu.SchoolIconSize + 24, MagicMenu.SchoolIconSize + 24, this.Active == school ? Color.Green : Color.White, 1f, false);
                    b.Draw(school.Icon, iconBounds, Color.White);

                    if (iconBounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                    {
                        if (this.JustLeftClicked)
                        {
                            if (this.School == null)
                                this.Active = School.GetSchool(schoolId);
                            this.JustLeftClicked = false;
                        }
                    }

                    y += MagicMenu.SchoolIconSize + 12;
                }
            }

            // draw spell icon area
            if (this.Active != null)
            {
                Spell[][] spells = new[] { this.Active.GetSpellsTier1(), this.Active.GetSpellsTier2(), this.Active.GetSpellsTier3() }.Where(p => p != null).ToArray();

                int sy = spells.Length + 1;
                for (int t = 0; t < spells.Length; ++t)
                {
                    Spell[] spellGroup = spells[t];
                    if (spellGroup == null)
                        continue;

                    int y = this.yPositionOnScreen + (MagicMenu.WindowHeight - 24) / sy * (t + 1);
                    int sx = spellGroup.Length + 1;
                    for (int s = 0; s < spellGroup.Length; ++s)
                    {
                        Spell spell = spellGroup[s];
                        if (spell == null || !spellBook.KnowsSpell(spell, 0))
                            continue;

                        int x = this.xPositionOnScreen + (MagicMenu.WindowWidth / 2 - 24) / sx * (s + 1);
                        Rectangle iconBounds = new Rectangle(x - MagicMenu.SpellIconSize / 2, y - MagicMenu.SpellIconSize / 2, MagicMenu.SpellIconSize, MagicMenu.SpellIconSize);

                        if (iconBounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                        {
                            if (this.JustLeftClicked)
                            {
                                this.Sel = spell;
                                this.JustLeftClicked = false;
                            }
                        }

                        IClickableMenu.drawTextureBox(b, x - MagicMenu.SpellIconSize / 2 - 12, y - MagicMenu.SpellIconSize / 2 - 12, MagicMenu.SpellIconSize + 24, MagicMenu.SpellIconSize + 24, spell == this.Sel ? Color.Green : Color.White);

                        Texture2D icon = spell.Icons[spell.Icons.Length - 1];
                        b.Draw(icon, iconBounds, Color.White);
                    }
                }
            }

            // draw selected spell area
            if (this.Sel != null)
            {
                // draw title
                string title = this.Sel.GetTranslatedName();
                b.DrawString(Game1.dialogueFont, title, new Vector2(this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + (MagicMenu.WindowWidth / 2 - Game1.dialogueFont.MeasureString(title).X) / 2, this.yPositionOnScreen + 30), Color.Black);

                // draw icon
                var icon = this.Sel.Icons[this.Sel.Icons.Length - 1];
                b.Draw(icon, new Rectangle(this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + (MagicMenu.WindowWidth / 2 - MagicMenu.SelIconSize) / 2, this.yPositionOnScreen + 85, MagicMenu.SelIconSize, MagicMenu.SelIconSize), Color.White);

                // draw description
                string desc = this.WrapText(this.Sel.GetTranslatedDescription(), (int)((MagicMenu.WindowWidth / 2) / 0.75f));
                b.DrawString(Game1.dialogueFont, desc, new Vector2(this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + 12, this.yPositionOnScreen + 280), Color.Black, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);

                // draw level icons
                int sx = this.Sel.Icons.Length + 1;
                for (int i = 0; i < this.Sel.Icons.Length; ++i)
                {
                    // get icon position
                    int x = this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + (MagicMenu.WindowWidth / 2) / sx * (i + 1);
                    int y = this.yPositionOnScreen + MagicMenu.WindowHeight - 12 - MagicMenu.SpellIconSize - 32 - 40;
                    var bounds = new Rectangle(x - MagicMenu.SpellIconSize / 2, y, MagicMenu.SpellIconSize, MagicMenu.SpellIconSize);
                    bool isHovered = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());

                    // get border color
                    Color stateCol;
                    if (spellBook.KnowsSpell(this.Sel, i))
                        stateCol = Color.Green;
                    else if (i == 0 || spellBook.KnowsSpell(this.Sel, i - 1))
                        stateCol = Color.White;
                    else
                        stateCol = Color.Gray;

                    // draw icon
                    IClickableMenu.drawTextureBox(b, bounds.Left - 12, bounds.Top - 12, bounds.Width + 24, bounds.Height + 24, stateCol);
                    b.Draw(this.Sel.Icons[i], bounds, Color.White);

                    // handle click
                    if (isHovered && (this.JustLeftClicked || this.JustRightClicked))
                    {
                        if (this.JustLeftClicked && spellBook.KnowsSpell(this.Sel, i))
                        {
                            this.Dragging = new PreparedSpell(this.Sel.FullId, i);
                            this.JustLeftClicked = false;
                        }
                        else if (i == 0 || spellBook.KnowsSpell(this.Sel, i - 1))
                        {
                            if (this.JustLeftClicked && spellBook.FreePoints > 0)
                                spellBook.Mutate(_ => spellBook.LearnSpell(this.Sel, i));
                            else if (this.JustRightClicked && i != 0)
                                spellBook.Mutate(_ => spellBook.ForgetSpell(this.Sel, i));
                        }
                    }
                }

                // draw free points count
                b.DrawString(Game1.dialogueFont, $"Free points: {spellBook.FreePoints}", new Vector2(this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + 12 + 24, this.yPositionOnScreen + MagicMenu.WindowHeight - 12 - 32 - 20), Color.Black);
            }

            // draw spell bars
            {
                int y = this.yPositionOnScreen + gap + 12 + (hasFifthSpellSlot ? -32 : 0);
                foreach (var spellBar in spellBook.Prepared)
                {
                    for (int i = 0; i < (hasFifthSpellSlot ? 5 : 4); ++i)
                    {
                        var prep = spellBar.GetSlot(i);

                        var r = new Rectangle(this.xPositionOnScreen + MagicMenu.WindowWidth + 12, y, MagicMenu.HotbarIconSize, MagicMenu.HotbarIconSize);
                        if (r.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                        {
                            if (this.JustRightClicked)
                                spellBook.Mutate(_ => spellBar.SetSlot(i, prep = null));
                            else if (this.JustLeftClicked)
                            {
                                spellBook.Mutate(_ => spellBar.SetSlot(i, prep = this.Dragging));
                                this.Dragging = null;
                                this.JustLeftClicked = false;
                            }
                        }

                        IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), r.X - 12, y - 12, MagicMenu.HotbarIconSize + 24, MagicMenu.HotbarIconSize + 24, Color.White, 1f, false);

                        if (prep != null)
                        {
                            Spell spell = SpellManager.Get(prep.SpellId);
                            Texture2D[] icons = spell?.Icons;
                            if (icons != null && icons.Length > prep.Level && icons[prep.Level] != null)
                            {
                                Texture2D icon = icons[prep.Level];
                                b.Draw(icon, r, Color.White);
                            }
                        }
                        y += MagicMenu.HotbarIconSize + 12;
                    }
                    y += gap + 12;
                }
            }

            // reset dragging
            if (this.JustLeftClicked)
            {
                this.Dragging = null;
                this.JustLeftClicked = false;
            }
            this.JustRightClicked = false;

            // draw base menu
            base.draw(b);

            // draw dragged spell
            if (this.Dragging != null)
            {
                Spell spell = SpellManager.Get(this.Dragging.SpellId);
                Texture2D[] icons = spell?.Icons;
                if (icons != null && icons.Length > this.Dragging.Level && icons[this.Dragging.Level] != null)
                {
                    Texture2D icon = icons[this.Dragging.Level];

                    b.Draw(icon, new Rectangle(Game1.getOldMouseX(), Game1.getOldMouseY(), MagicMenu.HotbarIconSize, MagicMenu.HotbarIconSize), Color.White);
                }
            }

            // draw cursor
            this.drawMouse(b);
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            this.JustLeftClicked = true;
        }

        /// <inheritdoc />
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this.JustRightClicked = true;
        }


        /*********
        ** Private methods
        *********/
        // https://gist.github.com/Sankra/5585584
        // TODO: A better version that handles me doing newlines correctly
        private string WrapText(string text, int maxLineWidth)
        {
            if (Game1.dialogueFont.MeasureString(text).X < maxLineWidth)
            {
                return text;
            }

            string[] words = text.Split(' ', '\n');
            var wrappedText = new System.Text.StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = Game1.dialogueFont.MeasureString(" ").X;
            foreach (string word in words)
            {
                Vector2 size = Game1.dialogueFont.MeasureString(word);
                if (lineWidth + size.X < maxLineWidth)
                {
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    wrappedText.Append("\n");
                    lineWidth = size.X + spaceWidth;
                }
                wrappedText.Append(word);
                wrappedText.Append(" ");
            }

            return wrappedText.ToString();
        }
    }
}
