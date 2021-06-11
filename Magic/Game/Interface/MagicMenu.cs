using Magic.Schools;
using Magic.Spells;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using StardewValley;
using StardewValley.Menus;

namespace Magic.Game.Interface
{
    public class MagicMenu : IClickableMenu
    {
        public const int WINDOW_WIDTH = 800;
        public const int WINDOW_HEIGHT = 600;
        public const int SCHOOL_ICON_SIZE = 32;
        public const int SPELL_ICON_SIZE = 64;
        public const int SEL_ICON_SIZE = 192;
        public const int HOTBAR_ICON_SIZE = 48;

        private School school;
        private School active;
        private Spell sel;
        private PreparedSpell dragging;

        private bool justLeftClicked;
        private bool justRightClicked;

        public MagicMenu(School theSchool = null)
            : base((Game1.viewport.Size.Width - MagicMenu.WINDOW_WIDTH) / 2, (Game1.viewport.Size.Height - MagicMenu.WINDOW_HEIGHT) / 2, MagicMenu.WINDOW_WIDTH, MagicMenu.WINDOW_HEIGHT, true)
        {
            this.school = theSchool;
            if (this.school != null)
                this.active = this.school;
        }

        public override void draw(SpriteBatch b)
        {
            var spellbook = Game1.player.getSpellBook();
            bool hasFifthSpellSlot = Game1.player.HasCustomProfession(Skill.ProfessionFifthSpellSlot);

            var hotbarH = 12 + 48 * (hasFifthSpellSlot ? 5 : 4) + 12 * (hasFifthSpellSlot ? 4 : 3) + 12;
            var gap = (MagicMenu.WINDOW_HEIGHT - hotbarH * 2) / 3 + (hasFifthSpellSlot ? 25 : 0);
            //drawTextureBox(b, xPositionOnScreen + WINDOW_WIDTH, yPositionOnScreen + gap, 48 + 24, hotbarH, Color.White);
            //drawTextureBox(b, xPositionOnScreen + WINDOW_WIDTH, yPositionOnScreen + WINDOW_HEIGHT - hotbarH - gap, 48 + 24, hotbarH, Color.White);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, MagicMenu.WINDOW_WIDTH, MagicMenu.WINDOW_HEIGHT, Color.White);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, MagicMenu.WINDOW_WIDTH / 2, MagicMenu.WINDOW_HEIGHT, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + 12, this.yPositionOnScreen + 12, MagicMenu.WINDOW_WIDTH / 2 - 24, MagicMenu.WINDOW_HEIGHT - 24), Color.Black);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen - MagicMenu.SCHOOL_ICON_SIZE - 12, this.yPositionOnScreen, MagicMenu.SCHOOL_ICON_SIZE + 24, MagicMenu.WINDOW_HEIGHT, Color.White);

            {
                int ix = this.xPositionOnScreen - MagicMenu.SCHOOL_ICON_SIZE - 12, iy = this.yPositionOnScreen;
                foreach (string schoolId in School.getSchoolList())
                {
                    var school = School.getSchool(schoolId);
                    IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), ix, iy, MagicMenu.SCHOOL_ICON_SIZE + 24, MagicMenu.SCHOOL_ICON_SIZE + 24, this.active == school ? Color.Green : Color.White, 1f, false);
                    //drawTextureBox(b, ix, iy, 64 + 24, 64 + 24, Color.White);
                    b.Draw(Game1.staminaRect, new Rectangle(ix + 12, iy + 12, MagicMenu.SCHOOL_ICON_SIZE, MagicMenu.SCHOOL_ICON_SIZE), Color.Aqua);

                    if (this.justLeftClicked && new Rectangle(ix + 12, iy + 12, MagicMenu.SCHOOL_ICON_SIZE, MagicMenu.SCHOOL_ICON_SIZE).Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                    {
                        if (this.school == null)
                            this.active = School.getSchool(schoolId);
                        this.justLeftClicked = false;
                    }

                    iy += MagicMenu.SCHOOL_ICON_SIZE + 12;
                }
            }

            if (this.active != null)
            {
                Spell[] t1 = this.active.GetSpellsTier1();
                Spell[] t2 = this.active.GetSpellsTier2();
                Spell[] t3 = this.active.GetSpellsTier3();

                Spell[][] spells = new[] { t1, t2, t3 };

                int sy = spells.Length + 1;
                for (int t = 0; t < spells.Length; ++t)
                {
                    int y = this.yPositionOnScreen + (MagicMenu.WINDOW_HEIGHT - 24) / sy * (t + 1);
                    int sx = spells[t].Length + 1;
                    for (int s = 0; s < spells[t].Length; ++s)
                    {
                        int x = this.xPositionOnScreen + (MagicMenu.WINDOW_WIDTH / 2 - 24) / sx * (s + 1);

                        var spell = spells[t][s];
                        if (!Game1.player.knowsSpell(spell, 0))
                            continue;
                        if (this.justLeftClicked && new Rectangle(x - MagicMenu.SPELL_ICON_SIZE / 2, y - MagicMenu.SPELL_ICON_SIZE / 2, MagicMenu.SPELL_ICON_SIZE, MagicMenu.SPELL_ICON_SIZE).Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                        {
                            this.sel = spell;
                            this.justLeftClicked = false;
                        }

                        IClickableMenu.drawTextureBox(b, x - MagicMenu.SPELL_ICON_SIZE / 2 - 12, y - MagicMenu.SPELL_ICON_SIZE / 2 - 12, MagicMenu.SPELL_ICON_SIZE + 24, MagicMenu.SPELL_ICON_SIZE + 24, spell == this.sel ? Color.Green : Color.White);

                        if (spell == null)
                            continue;
                        var icon = spell.Icons != null ? spell.Icons[spell.Icons.Length - 1] : Game1.staminaRect;
                        if (icon == null)
                        {
                            icon = spell.Icons[0];
                            if (icon == null)
                                continue;
                        }

                        b.Draw(icon, new Rectangle(x - MagicMenu.SPELL_ICON_SIZE / 2, y - MagicMenu.SPELL_ICON_SIZE / 2, MagicMenu.SPELL_ICON_SIZE, MagicMenu.SPELL_ICON_SIZE), Color.White);
                    }
                }
            }

            if (this.sel != null)
            {
                var title = this.sel.getTranslatedName();
                var desc = this.WrapText(this.sel.getTranslatedDescription(), (int)((MagicMenu.WINDOW_WIDTH / 2) / 0.75f));

                b.DrawString(Game1.dialogueFont, title, new Vector2(this.xPositionOnScreen + MagicMenu.WINDOW_WIDTH / 2 + (MagicMenu.WINDOW_WIDTH / 2 - Game1.dialogueFont.MeasureString(title).X) / 2, this.yPositionOnScreen + 30), Color.Black);

                var icon = this.sel.Icons != null ? this.sel.Icons[this.sel.Icons.Length - 1] : Game1.staminaRect;
                if (icon == null)
                {
                    icon = this.sel.Icons[0];
                }
                if (icon != null)
                {
                    b.Draw(icon, new Rectangle(this.xPositionOnScreen + MagicMenu.WINDOW_WIDTH / 2 + (MagicMenu.WINDOW_WIDTH / 2 - MagicMenu.SEL_ICON_SIZE) / 2, this.yPositionOnScreen + 85, MagicMenu.SEL_ICON_SIZE, MagicMenu.SEL_ICON_SIZE), Color.White);
                }
                b.DrawString(Game1.dialogueFont, desc, new Vector2(this.xPositionOnScreen + MagicMenu.WINDOW_WIDTH / 2 + 12, this.yPositionOnScreen + 280), Color.Black, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);

                int sx = this.sel.Icons.Length + 1;
                for (int i = 0; i < this.sel.Icons.Length; ++i)
                {
                    int x = this.xPositionOnScreen + MagicMenu.WINDOW_WIDTH / 2 + (MagicMenu.WINDOW_WIDTH / 2) / sx * (i + 1);
                    int y = this.yPositionOnScreen + MagicMenu.WINDOW_HEIGHT - 12 - MagicMenu.SPELL_ICON_SIZE - 32 - 40;

                    Color stateCol = Color.Gray;
                    if (Game1.player.knowsSpell(this.sel, i))
                        stateCol = Color.Green;
                    else if (i == 0 || Game1.player.knowsSpell(this.sel, i - 1))
                        stateCol = Color.White;

                    var r = new Rectangle(x - MagicMenu.SPELL_ICON_SIZE / 2, y, MagicMenu.SPELL_ICON_SIZE, MagicMenu.SPELL_ICON_SIZE);
                    IClickableMenu.drawTextureBox(b, r.Left - 12, r.Top - 12, r.Width + 24, r.Height + 24, stateCol);
                    if (this.sel.Icons[i] != null)
                        b.Draw(this.sel.Icons[i], r, Color.White);
                    if (r.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                    {
                        if (this.justLeftClicked && Game1.player.knowsSpell(this.sel, i))
                        {
                            this.dragging = new PreparedSpell(this.sel.FullId, i);
                            this.justLeftClicked = false;
                        }
                        else if (i == 0 || Game1.player.knowsSpell(this.sel, i - 1))
                        {
                            if (this.justLeftClicked)
                                Game1.player.learnSpell(this.sel, i);
                            else if (this.justRightClicked && i != 0)
                                Game1.player.forgetSpell(this.sel, i);
                        }
                    }
                }

                b.DrawString(Game1.dialogueFont, "Free points: " + Game1.player.getFreeSpellPoints(), new Vector2(this.xPositionOnScreen + MagicMenu.WINDOW_WIDTH / 2 + 12 + 24, this.yPositionOnScreen + MagicMenu.WINDOW_HEIGHT - 12 - 32 - 20), Color.Black);
            }
            //*
            {
                int y = this.yPositionOnScreen + gap + 12 + (hasFifthSpellSlot ? -32 : 0);
                foreach (var preps in spellbook.prepared)
                {
                    for (int i = 0; i < (hasFifthSpellSlot ? 5 : 4); ++i)
                    {
                        var prep = preps[i];

                        var r = new Rectangle(this.xPositionOnScreen + MagicMenu.WINDOW_WIDTH + 12, y, MagicMenu.HOTBAR_ICON_SIZE, MagicMenu.HOTBAR_ICON_SIZE);
                        if (r.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                        {
                            if (this.justRightClicked)
                                preps[i] = prep = null;
                            else if (this.justLeftClicked)
                            {
                                preps[i] = prep = this.dragging;
                                this.dragging = null;
                                this.justLeftClicked = false;
                            }
                        }

                        IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), r.X - 12, y - 12, MagicMenu.HOTBAR_ICON_SIZE + 24, MagicMenu.HOTBAR_ICON_SIZE + 24, Color.White, 1f, false);

                        if (prep != null)
                        {
                            Spell spell = SpellBook.get(prep.SpellId);
                            if (spell != null)
                            {
                                Texture2D[] icons = spell.Icons;
                                if (icons != null && icons.Length > prep.Level && icons[prep.Level] != null)
                                {
                                    Texture2D icon = icons[prep.Level];
                                    b.Draw(icon, r, Color.White);
                                }
                            }
                        }
                        y += MagicMenu.HOTBAR_ICON_SIZE + 12;
                    }
                    y += gap + 12;
                }
            }
            //*/

            if (this.justLeftClicked)
            {
                this.dragging = null;
                this.justLeftClicked = false;
            }
            this.justRightClicked = false;

            base.draw(b);
            if (this.dragging != null)
            {
                Spell spell = SpellBook.get(this.dragging.SpellId);
                if (spell != null)
                {
                    Texture2D[] icons = spell.Icons;
                    if (icons != null && icons.Length > this.dragging.Level && icons[this.dragging.Level] != null)
                    {
                        Texture2D icon = icons[this.dragging.Level];

                        b.Draw(icon, new Rectangle(Game1.getOldMouseX(), Game1.getOldMouseY(), MagicMenu.HOTBAR_ICON_SIZE, MagicMenu.HOTBAR_ICON_SIZE), Color.White);
                    }
                }
            }
            this.drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            this.justLeftClicked = true;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this.justRightClicked = true;
        }

        // https://gist.github.com/Sankra/5585584
        // TODO: A better version that handles me doing newlines correctly
        private string WrapText(string text, int MaxLineWidth)
        {
            if (Game1.dialogueFont.MeasureString(text).X < MaxLineWidth)
            {
                return text;
            }

            string[] words = text.Split(new[] { ' ', '\n' });
            var wrappedText = new System.Text.StringBuilder();
            float linewidth = 0f;
            float spaceWidth = Game1.dialogueFont.MeasureString(" ").X;
            for (int i = 0; i < words.Length; ++i)
            {
                Vector2 size = Game1.dialogueFont.MeasureString(words[i]);
                if (linewidth + size.X < MaxLineWidth)
                {
                    linewidth += size.X + spaceWidth;
                }
                else
                {
                    wrappedText.Append("\n");
                    linewidth = size.X + spaceWidth;
                }
                wrappedText.Append(words[i]);
                wrappedText.Append(" ");
            }

            return wrappedText.ToString();
        }
    }
}
