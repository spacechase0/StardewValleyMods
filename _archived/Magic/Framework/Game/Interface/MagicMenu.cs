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

        private School SelectedSchool;
        private Spell SelectedSpell;
        private PreparedSpell Dragging;

        private bool JustLeftClicked;
        private bool JustRightClicked;


        /*********
        ** Public methods
        *********/
        public MagicMenu()
            : base((Game1.viewport.Size.Width - MagicMenu.WindowWidth) / 2, (Game1.viewport.Size.Height - MagicMenu.WindowHeight) / 2, MagicMenu.WindowWidth, MagicMenu.WindowHeight, true)
        {
            this.SelectDefaultSchool();
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
            string hoverText = null;

            // draw main window
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, MagicMenu.WindowWidth, MagicMenu.WindowHeight, Color.White);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, MagicMenu.WindowWidth / 2, MagicMenu.WindowHeight, Color.White);

            // draw school icons
            {
                int x = this.xPositionOnScreen - MagicMenu.SchoolIconSize - 12;
                int y = this.yPositionOnScreen;
                foreach (string schoolId in School.GetSchoolList())
                {
                    School school = School.GetSchool(schoolId);
                    bool knowsSchool = spellBook.KnowsSchool(school);

                    float alpha = knowsSchool ? 1f : 0.2f;
                    Rectangle iconBounds = new(x + 12, y + 12, MagicMenu.SchoolIconSize, MagicMenu.SchoolIconSize);

                    IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, MagicMenu.SchoolIconSize + 24, MagicMenu.SchoolIconSize + 24, (this.SelectedSchool == school ? Color.Green : Color.White), 1f, false);
                    b.Draw(school.Icon, iconBounds, Color.White * alpha);

                    if (iconBounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                    {
                        if (knowsSchool)
                        {
                            hoverText = school.DisplayName;

                            if (this.JustLeftClicked)
                            {
                                this.SelectSchool(schoolId, spellBook);
                                this.JustLeftClicked = false;
                            }
                        }
                        else
                            hoverText = "???";
                    }

                    y += MagicMenu.SchoolIconSize + 12;
                }
            }

            // draw spell icon area
            if (this.SelectedSchool != null)
            {
                Spell[][] spells = this.SelectedSchool.GetAllSpellTiers().ToArray();

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
                            hoverText = spell.GetTooltip();

                            if (this.JustLeftClicked)
                            {
                                this.SelectedSpell = spell;
                                this.JustLeftClicked = false;
                            }
                        }

                        if (spell == this.SelectedSpell)
                        {
                            IClickableMenu.drawTextureBox(b, x - MagicMenu.SpellIconSize / 2 - 12, y - MagicMenu.SpellIconSize / 2 - 12, MagicMenu.SpellIconSize + 24, MagicMenu.SpellIconSize + 24, Color.Green);
                        }

                        Texture2D icon = spell.Icons[spell.Icons.Length - 1];
                        b.Draw(icon, iconBounds, Color.White);
                    }
                }
            }

            // draw selected spell area
            if (this.SelectedSpell != null)
            {
                // draw title
                string title = this.SelectedSpell.GetTranslatedName();
                b.DrawString(Game1.dialogueFont, title, new Vector2(this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + (MagicMenu.WindowWidth / 2 - Game1.dialogueFont.MeasureString(title).X) / 2, this.yPositionOnScreen + 30), Color.Black);

                // draw icon
                var icon = this.SelectedSpell.Icons[this.SelectedSpell.Icons.Length - 1];
                b.Draw(icon, new Rectangle(this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + (MagicMenu.WindowWidth / 2 - MagicMenu.SelIconSize) / 2, this.yPositionOnScreen + 85, MagicMenu.SelIconSize, MagicMenu.SelIconSize), Color.White);

                // draw description
                string desc = this.WrapText(this.SelectedSpell.GetTranslatedDescription(), (int)((MagicMenu.WindowWidth / 2) / 0.75f));
                b.DrawString(Game1.dialogueFont, desc, new Vector2(this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + 12, this.yPositionOnScreen + 280), Color.Black, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);

                // draw level icons
                int sx = this.SelectedSpell.Icons.Length + 1;
                for (int i = 0; i < this.SelectedSpell.Icons.Length; ++i)
                {
                    // get icon position
                    int x = this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + (MagicMenu.WindowWidth / 2) / sx * (i + 1);
                    int y = this.yPositionOnScreen + MagicMenu.WindowHeight - 12 - MagicMenu.SpellIconSize - 32 - 40;
                    var bounds = new Rectangle(x - MagicMenu.SpellIconSize / 2, y, MagicMenu.SpellIconSize, MagicMenu.SpellIconSize);
                    bool isHovered = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());

                    // get state
                    bool isKnown = spellBook.KnowsSpell(this.SelectedSpell, i);
                    bool hasPreviousLevels = isKnown || i == 0 || spellBook.KnowsSpell(this.SelectedSpell, i - 1);

                    // get border color
                    Color stateCol;
                    if (isKnown)
                    {
                        if (isHovered)
                            hoverText = I18n.Tooltip_Spell_Known(spell: I18n.Tooltip_Spell_NameAndLevel(title, level: i + 1));
                        stateCol = Color.Green;
                    }
                    else if (hasPreviousLevels)
                    {
                        if (isHovered)
                        {
                            hoverText = spellBook.FreePoints > 0
                                ? I18n.Tooltip_Spell_CanLearn(spell: I18n.Tooltip_Spell_NameAndLevel(title, level: i + 1))
                                : I18n.Tooltip_Spell_NeedFreePoints(spell: I18n.Tooltip_Spell_NameAndLevel(title, level: i + 1));
                        }
                        stateCol = Color.White;
                    }
                    else
                    {
                        if (isHovered)
                            hoverText = I18n.Tooltip_Spell_NeedPreviousLevels();
                        stateCol = Color.Gray;
                    }

                    // draw border
                    if (isKnown)
                    {
                        IClickableMenu.drawTextureBox(b, bounds.Left - 12, bounds.Top - 12, bounds.Width + 24, bounds.Height + 24, Color.Green);
                    }

                    // draw icon
                    float alpha = hasPreviousLevels ? 1f : 0.5f;
                    b.Draw(this.SelectedSpell.Icons[i], bounds, Color.White * alpha);

                    // handle click
                    if (isHovered && (this.JustLeftClicked || this.JustRightClicked))
                    {
                        if (this.JustLeftClicked && isKnown)
                        {
                            this.Dragging = new PreparedSpell(this.SelectedSpell.FullId, i);
                            this.JustLeftClicked = false;
                        }
                        else if (hasPreviousLevels)
                        {
                            if (this.JustLeftClicked && spellBook.FreePoints > 0)
                                spellBook.Mutate(_ => spellBook.LearnSpell(this.SelectedSpell, i));
                            else if (this.JustRightClicked && i != 0)
                                spellBook.Mutate(_ => spellBook.ForgetSpell(this.SelectedSpell, i));
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
                        PreparedSpell prep = spellBar.GetSlot(i);
                        Rectangle bounds = new(this.xPositionOnScreen + MagicMenu.WindowWidth + 12, y, MagicMenu.HotbarIconSize, MagicMenu.HotbarIconSize);
                        bool isHovered = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());

                        if (isHovered)
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

                        IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), bounds.X - 12, y - 12, MagicMenu.HotbarIconSize + 24, MagicMenu.HotbarIconSize + 24, Color.White, 1f, false);

                        if (prep != null)
                        {
                            Spell spell = SpellManager.Get(prep.SpellId);

                            Texture2D[] icons = spell?.Icons;
                            if (icons?.Length > prep.Level && icons[prep.Level] != null)
                            {
                                Texture2D icon = icons[prep.Level];
                                b.Draw(icon, bounds, Color.White);
                            }

                            if (isHovered)
                                hoverText = spell.GetTooltip(level: prep.Level);
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

            // draw hover text
            if (hoverText != null)
                drawHoverText(b, hoverText, Game1.smallFont);

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
        /// <summary>Set the selected school to the first one the player knows spells for.</summary>
        private void SelectDefaultSchool()
        {
            SpellBook spellBook = Game1.player.GetSpellBook();
            School school = School.GetSchoolList().Select(School.GetSchool).FirstOrDefault(spellBook.KnowsSchool);
            if (school != null)
                this.SelectSchool(School.GetSchoolList().First(), spellBook);
        }

        /// <summary>Set the selected school for which to show spells.</summary>
        /// <param name="id">The school ID.</param>
        private void SelectSchool(string id, SpellBook spellbook)
        {
            var school = School.GetSchool(id);

            this.SelectedSchool = school;
            this.SelectedSpell = school.GetAllSpellTiers().SelectMany(p => p).FirstOrDefault(id => spellbook.KnowsSpell(id, 0));
        }

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
