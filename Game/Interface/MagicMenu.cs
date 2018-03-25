using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Magic;
using Magic.Schools;
using Magic.Spells;
using StardewValley;
using StardewValley.Menus;
using System.Linq;
//using StardewMountain.Combat.Magic;

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
        private School active = null;
        private Spell sel = null;
        private PreparedSpell dragging = null;

        private bool justLeftClicked = false;
        private bool justRightClicked = false;

        public MagicMenu(School theSchool = null)
        :   base( ( Game1.viewport.Size.Width - WINDOW_WIDTH ) / 2, (Game1.viewport.Size.Height - WINDOW_HEIGHT) / 2, WINDOW_WIDTH, WINDOW_HEIGHT, true )
        {
            school = theSchool;
            if (school != null)
                active = school;
        }

        public override void draw(SpriteBatch b)
        {
            var spellbook = Game1.player.getSpellBook();

            var hotbarH = 12 + 48 * 4 + 12 * 3 + 12;
            var gap = ( WINDOW_HEIGHT - hotbarH * 2 ) / 3;
            drawTextureBox(b, xPositionOnScreen + WINDOW_WIDTH, yPositionOnScreen + gap, 48 + 24, hotbarH, Color.White);
            drawTextureBox(b, xPositionOnScreen + WINDOW_WIDTH, yPositionOnScreen + WINDOW_HEIGHT - hotbarH - gap, 48 + 24, hotbarH, Color.White);
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, WINDOW_WIDTH, WINDOW_HEIGHT, Color.White);
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, WINDOW_WIDTH / 2, WINDOW_HEIGHT, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + 12, yPositionOnScreen + 12, WINDOW_WIDTH / 2 - 24, WINDOW_HEIGHT - 24), Color.Black);
            drawTextureBox(b, xPositionOnScreen - SCHOOL_ICON_SIZE - 12, yPositionOnScreen, SCHOOL_ICON_SIZE + 24, WINDOW_HEIGHT, Color.White);

            {
                int ix = xPositionOnScreen - SCHOOL_ICON_SIZE - 12, iy = yPositionOnScreen;
                foreach (string schoolId in School.getSchoolList())
                {
                    if (!spellbook.knownSchools.Contains(schoolId))
                        continue;

                    var school = School.getSchool(schoolId);
                    drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), ix, iy, SCHOOL_ICON_SIZE + 24, SCHOOL_ICON_SIZE + 24, Color.White, 1f, false);
                    //drawTextureBox(b, ix, iy, 64 + 24, 64 + 24, Color.White);
                    b.Draw(Game1.staminaRect, new Rectangle(ix + 12, iy + 12, SCHOOL_ICON_SIZE, SCHOOL_ICON_SIZE), Color.Aqua);

                    if ( justLeftClicked && new Rectangle( ix + 12, iy + 12, SCHOOL_ICON_SIZE, SCHOOL_ICON_SIZE ).Contains( Game1.getOldMouseX(), Game1.getOldMouseY() ) )
                    {
                        if ( this.school == null )
                            active = School.getSchool(schoolId);
                        justLeftClicked = false;
                    }

                    iy += SCHOOL_ICON_SIZE + 12;
                }
            }
            
            if ( active != null )
            {
                Spell[] t1 = active.GetSpellsTier1();
                Spell[] t2 = active.GetSpellsTier2();
                Spell[] t3 = active.GetSpellsTier3();

                Spell[][] spells = new Spell[][] { t1, t2, t3 };

                int sy = spells.Length + 1;
                for ( int t = 0; t < spells.Length; ++t )
                {
                    int y = yPositionOnScreen + (WINDOW_HEIGHT - 24) / sy * (t + 1);
                    int sx = spells[t].Length + 1;
                    for ( int s = 0; s < spells[ t ].Length; ++s )
                    {
                        int x = xPositionOnScreen + (WINDOW_WIDTH / 2 - 24) / sx * (s + 1);

                        var spell = spells[t][s];
                        if ( justLeftClicked && new Rectangle( x - SPELL_ICON_SIZE / 2, y - SPELL_ICON_SIZE / 2, SPELL_ICON_SIZE, SPELL_ICON_SIZE ).Contains( Game1.getOldMouseX(), Game1.getOldMouseY() ) )
                        {
                            sel = spell;
                            justLeftClicked = false;
                        }

                        drawTextureBox(b, x - SPELL_ICON_SIZE / 2 - 12, y - SPELL_ICON_SIZE / 2 - 12, SPELL_ICON_SIZE + 24, SPELL_ICON_SIZE + 24, spell == sel ? Color.Green : Color.White);

                        if (spell == null)
                            continue;
                        var icon = spell.Icons != null ? spell.Icons[ spell.Icons.Length - 1 ] : Game1.staminaRect;
                        if (icon == null)
                        {
                            icon = spell.Icons[0];
                            if ( icon == null )
                                continue;
                        }

                        b.Draw(icon, new Rectangle(x - SPELL_ICON_SIZE / 2, y - SPELL_ICON_SIZE / 2, SPELL_ICON_SIZE, SPELL_ICON_SIZE), Color.White);
                    }
                }
            }

            if ( sel != null )
            {
                var title = Mod.instance.Helper.Translation.Get( "spell." + sel.FullId + ".name" );
                var desc = WrapText( Mod.instance.Helper.Translation.Get( "spell." + sel.FullId + ".desc" ), (int)((WINDOW_WIDTH / 2) / 0.75f) );

                b.DrawString(Game1.dialogueFont, title, new Vector2(xPositionOnScreen + WINDOW_WIDTH / 2 + (WINDOW_WIDTH / 2 - Game1.dialogueFont.MeasureString(title).X) / 2, yPositionOnScreen + 30), Color.Black);

                var icon = sel.Icons != null ? sel.Icons[sel.Icons.Length - 1] : Game1.staminaRect;
                if (icon == null)
                {
                    icon = sel.Icons[0];
                }
                if ( icon != null )
                {
                    b.Draw(icon, new Rectangle(xPositionOnScreen + WINDOW_WIDTH / 2 + (WINDOW_WIDTH / 2 - SEL_ICON_SIZE) / 2, yPositionOnScreen + 85, SEL_ICON_SIZE, SEL_ICON_SIZE), Color.White);
                }
                b.DrawString(Game1.dialogueFont, desc, new Vector2(xPositionOnScreen + WINDOW_WIDTH / 2 + 12, yPositionOnScreen + 280), Color.Black, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);

                int sx = sel.Icons.Length + 1;
                for ( int i = 0; i < sel.Icons.Length; ++i )
                {
                    int x = xPositionOnScreen + WINDOW_WIDTH / 2 + (WINDOW_WIDTH / 2) / sx * (i + 1);
                    int y = yPositionOnScreen + WINDOW_HEIGHT - 12 - SPELL_ICON_SIZE - 32 - 40;

                    Color stateCol = Color.Gray;
                    if (Game1.player.knowsSpell(sel, i))
                        stateCol = Color.Green;
                    else if (i == 0 || Game1.player.knowsSpell(sel, i - 1))
                        stateCol = Color.White;

                    var r = new Rectangle(x - SPELL_ICON_SIZE / 2, y, SPELL_ICON_SIZE, SPELL_ICON_SIZE);
                    drawTextureBox(b, r.Left - 12, r.Top - 12, r.Width + 24, r.Height + 24, stateCol);
                    if (sel.Icons[i] != null)
                        b.Draw(sel.Icons[i], r, Color.White);
                    if ( r.Contains( Game1.getOldMouseX(), Game1.getOldMouseY() ) )
                    {
                        if ( justLeftClicked && Game1.player.knowsSpell( sel, i ) )
                        {
                            dragging = new PreparedSpell(sel.FullId, i);
                            justLeftClicked = false;
                        }
                        else if (i == 0 || Game1.player.knowsSpell(sel, i - 1))
                        {
                            if ( justLeftClicked )
                                Game1.player.learnSpell(sel, i);
                            else if ( justRightClicked )
                                Game1.player.forgetSpell(sel, i);
                        }
                    }
                }

                b.DrawString(Game1.dialogueFont, "Free points: " + Game1.player.getFreeSpellPoints(), new Vector2(xPositionOnScreen + WINDOW_WIDTH / 2 + 12 + 24, yPositionOnScreen + WINDOW_HEIGHT - 12 - 32 - 20), Color.Black);
            }

            {
                int y = yPositionOnScreen + gap + 12;
                foreach (var preps in spellbook.prepared)
                {
                    for ( int i = 0; i < preps.Length; ++i )
                    {
                        var prep = preps[i];

                        var r = new Rectangle(xPositionOnScreen + WINDOW_WIDTH + 12, y, HOTBAR_ICON_SIZE, HOTBAR_ICON_SIZE);
                        if (r.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                        {
                            if (justRightClicked)
                                preps[i] = prep = null;
                            else if (justLeftClicked)
                            {
                                preps[i] = prep = dragging;
                                dragging = null;
                                justLeftClicked = false;
                            }
                        }

                        drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), r.X - 12, y - 12, HOTBAR_ICON_SIZE + 24, HOTBAR_ICON_SIZE + 24, Color.White, 1f, false);

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
                        y += HOTBAR_ICON_SIZE + 12;
                    }
                    y += gap + 12;
                }
            }

            if ( justLeftClicked )
            {
                dragging = null;
                justLeftClicked = false;
            }
            justRightClicked = false;

            base.draw(b);
            if ( dragging != null )
            {
                Spell spell = SpellBook.get(dragging.SpellId);
                if (spell != null)
                {
                    Texture2D[] icons = spell.Icons;
                    if (icons != null && icons.Length > dragging.Level && icons[dragging.Level] != null)
                    {
                        Texture2D icon = icons[dragging.Level];

                        b.Draw(icon, new Rectangle(Game1.getOldMouseX(), Game1.getOldMouseY(), HOTBAR_ICON_SIZE, HOTBAR_ICON_SIZE), Color.White);
                    }
                }
            }
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            justLeftClicked = true;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            justRightClicked = true;
        }

        // https://gist.github.com/Sankra/5585584
        // TODO: A better version that handles me doing newlines correctly
        private string WrapText(string text, int MaxLineWidth)
        {
            if (Game1.dialogueFont.MeasureString(text).X < MaxLineWidth)
            {
                return text;
            }

            string[] words = text.Split(new char[] { ' ', '\n' });
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
