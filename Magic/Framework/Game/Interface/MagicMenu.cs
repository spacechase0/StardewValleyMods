using System.Linq;
using Magic.Framework.Schools;
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
        public const int WindowWidth = 800;
        public const int WindowHeight = 600;
        public const int SchoolIconSize = 32;
        public const int SpellIconSize = 64;
        public const int SelIconSize = 192;
        public const int HotbarIconSize = 48;

        private readonly School School;
        private School Active;
        private Spell Sel;
        private PreparedSpell Dragging;

        private bool JustLeftClicked;
        private bool JustRightClicked;

        public MagicMenu(School theSchool = null)
            : base((Game1.viewport.Size.Width - MagicMenu.WindowWidth) / 2, (Game1.viewport.Size.Height - MagicMenu.WindowHeight) / 2, MagicMenu.WindowWidth, MagicMenu.WindowHeight, true)
        {
            this.School = theSchool;
            if (this.School != null)
                this.Active = this.School;
        }

        public override void draw(SpriteBatch b)
        {
            var spellBook = Game1.player.GetSpellBook();
            if (!spellBook.Prepared.Any())
            {
                spellBook.Mutate(data =>
                {
                    data.Prepared.Add(new PreparedSpellBar());
                    data.SelectedPrepared = 0;
                });
            }

            bool hasFifthSpellSlot = Game1.player.HasCustomProfession(Skill.MemoryProfession);

            int hotbarH = 12 + 48 * (hasFifthSpellSlot ? 5 : 4) + 12 * (hasFifthSpellSlot ? 4 : 3) + 12;
            int gap = (MagicMenu.WindowHeight - hotbarH * 2) / 3 + (hasFifthSpellSlot ? 25 : 0);
            //drawTextureBox(b, xPositionOnScreen + WINDOW_WIDTH, yPositionOnScreen + gap, 48 + 24, hotbarH, Color.White);
            //drawTextureBox(b, xPositionOnScreen + WINDOW_WIDTH, yPositionOnScreen + WINDOW_HEIGHT - hotbarH - gap, 48 + 24, hotbarH, Color.White);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, MagicMenu.WindowWidth, MagicMenu.WindowHeight, Color.White);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, MagicMenu.WindowWidth / 2, MagicMenu.WindowHeight, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + 12, this.yPositionOnScreen + 12, MagicMenu.WindowWidth / 2 - 24, MagicMenu.WindowHeight - 24), Color.Black);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen - MagicMenu.SchoolIconSize - 12, this.yPositionOnScreen, MagicMenu.SchoolIconSize + 24, MagicMenu.WindowHeight, Color.White);

            {
                int ix = this.xPositionOnScreen - MagicMenu.SchoolIconSize - 12, iy = this.yPositionOnScreen;
                foreach (string schoolId in School.GetSchoolList())
                {
                    var school = School.GetSchool(schoolId);
                    IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), ix, iy, MagicMenu.SchoolIconSize + 24, MagicMenu.SchoolIconSize + 24, this.Active == school ? Color.Green : Color.White, 1f, false);
                    //drawTextureBox(b, ix, iy, 64 + 24, 64 + 24, Color.White);
                    b.Draw(Game1.staminaRect, new Rectangle(ix + 12, iy + 12, MagicMenu.SchoolIconSize, MagicMenu.SchoolIconSize), Color.Aqua);

                    if (this.JustLeftClicked && new Rectangle(ix + 12, iy + 12, MagicMenu.SchoolIconSize, MagicMenu.SchoolIconSize).Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                    {
                        if (this.School == null)
                            this.Active = School.GetSchool(schoolId);
                        this.JustLeftClicked = false;
                    }

                    iy += MagicMenu.SchoolIconSize + 12;
                }
            }

            if (this.Active != null)
            {
                Spell[] t1 = this.Active.GetSpellsTier1();
                Spell[] t2 = this.Active.GetSpellsTier2();
                Spell[] t3 = this.Active.GetSpellsTier3();

                Spell[][] spells = new[] { t1, t2, t3 };

                int sy = spells.Length + 1;
                for (int t = 0; t < spells.Length; ++t)
                {
                    int y = this.yPositionOnScreen + (MagicMenu.WindowHeight - 24) / sy * (t + 1);
                    int sx = spells[t].Length + 1;
                    for (int s = 0; s < spells[t].Length; ++s)
                    {
                        int x = this.xPositionOnScreen + (MagicMenu.WindowWidth / 2 - 24) / sx * (s + 1);

                        var spell = spells[t][s];
                        if (!spellBook.KnowsSpell(spell, 0))
                            continue;
                        if (this.JustLeftClicked && new Rectangle(x - MagicMenu.SpellIconSize / 2, y - MagicMenu.SpellIconSize / 2, MagicMenu.SpellIconSize, MagicMenu.SpellIconSize).Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                        {
                            this.Sel = spell;
                            this.JustLeftClicked = false;
                        }

                        IClickableMenu.drawTextureBox(b, x - MagicMenu.SpellIconSize / 2 - 12, y - MagicMenu.SpellIconSize / 2 - 12, MagicMenu.SpellIconSize + 24, MagicMenu.SpellIconSize + 24, spell == this.Sel ? Color.Green : Color.White);

                        if (spell == null)
                            continue;
                        var icon = spell.Icons != null ? spell.Icons[spell.Icons.Length - 1] : Game1.staminaRect;
                        if (icon == null)
                        {
                            icon = spell.Icons[0];
                            if (icon == null)
                                continue;
                        }

                        b.Draw(icon, new Rectangle(x - MagicMenu.SpellIconSize / 2, y - MagicMenu.SpellIconSize / 2, MagicMenu.SpellIconSize, MagicMenu.SpellIconSize), Color.White);
                    }
                }
            }

            if (this.Sel != null)
            {
                string title = this.Sel.GetTranslatedName();
                string desc = this.WrapText(this.Sel.GetTranslatedDescription(), (int)((MagicMenu.WindowWidth / 2) / 0.75f));

                b.DrawString(Game1.dialogueFont, title, new Vector2(this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + (MagicMenu.WindowWidth / 2 - Game1.dialogueFont.MeasureString(title).X) / 2, this.yPositionOnScreen + 30), Color.Black);

                var icon =
                    (this.Sel.Icons != null ? this.Sel.Icons[this.Sel.Icons.Length - 1] : Game1.staminaRect)
                    ?? this.Sel.Icons[0];
                if (icon != null)
                {
                    b.Draw(icon, new Rectangle(this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + (MagicMenu.WindowWidth / 2 - MagicMenu.SelIconSize) / 2, this.yPositionOnScreen + 85, MagicMenu.SelIconSize, MagicMenu.SelIconSize), Color.White);
                }
                b.DrawString(Game1.dialogueFont, desc, new Vector2(this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + 12, this.yPositionOnScreen + 280), Color.Black, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);

                int sx = this.Sel.Icons.Length + 1;
                for (int i = 0; i < this.Sel.Icons.Length; ++i)
                {
                    int x = this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + (MagicMenu.WindowWidth / 2) / sx * (i + 1);
                    int y = this.yPositionOnScreen + MagicMenu.WindowHeight - 12 - MagicMenu.SpellIconSize - 32 - 40;

                    Color stateCol = Color.Gray;
                    if (spellBook.KnowsSpell(this.Sel, i))
                        stateCol = Color.Green;
                    else if (i == 0 || spellBook.KnowsSpell(this.Sel, i - 1))
                        stateCol = Color.White;

                    var r = new Rectangle(x - MagicMenu.SpellIconSize / 2, y, MagicMenu.SpellIconSize, MagicMenu.SpellIconSize);
                    IClickableMenu.drawTextureBox(b, r.Left - 12, r.Top - 12, r.Width + 24, r.Height + 24, stateCol);
                    if (this.Sel.Icons[i] != null)
                        b.Draw(this.Sel.Icons[i], r, Color.White);
                    if (r.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                    {
                        if (this.JustLeftClicked && spellBook.KnowsSpell(this.Sel, i))
                        {
                            this.Dragging = new PreparedSpell(this.Sel.FullId, i);
                            this.JustLeftClicked = false;
                        }
                        else if (i == 0 || spellBook.KnowsSpell(this.Sel, i - 1))
                        {
                            if (this.JustLeftClicked)
                                spellBook.Mutate(_ => spellBook.LearnSpell(this.Sel, i));
                            else if (this.JustRightClicked && i != 0)
                                spellBook.Mutate(_ => spellBook.ForgetSpell(this.Sel, i));
                        }
                    }
                }

                b.DrawString(Game1.dialogueFont, "Free points: " + spellBook.FreePoints, new Vector2(this.xPositionOnScreen + MagicMenu.WindowWidth / 2 + 12 + 24, this.yPositionOnScreen + MagicMenu.WindowHeight - 12 - 32 - 20), Color.Black);
            }
            //*
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
            //*/

            if (this.JustLeftClicked)
            {
                this.Dragging = null;
                this.JustLeftClicked = false;
            }
            this.JustRightClicked = false;

            base.draw(b);
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
            this.drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            this.JustLeftClicked = true;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this.JustRightClicked = true;
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
