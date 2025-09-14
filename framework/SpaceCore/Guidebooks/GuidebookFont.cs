using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;

namespace SpaceCore.Guidebooks;
public static class GuidebookFont
{
    public static Dictionary<string, SpriteFont> Fonts { get; } = new();

    public static (string newStr, Vector2 endingAt) SplitAtWidth(string str, SpriteFont font, float startAtX, float maxWidth)
    {
        Vector2 ret = new Vector2(startAtX, 0);

        var glyphs = font.GetGlyphs();

        string used = "";
        string currentWord = "";
        float currentWordSize = 0;

        bool startedNewLine = startAtX == 0;
        for (int i = 0; i < str.Length; ++i)
        {
            char c = str[i];
            if (c == '\n')
            {
                ret.X = 0;
                ret.Y += font.LineSpacing;
                currentWord = "";
                currentWordSize = 0;
                startedNewLine = true;
            }
            else if (c == '\r')
                continue;
            else
            {
                if (!glyphs.TryGetValue(c, out var glyph))
                    glyph = glyphs[font.DefaultCharacter.Value];

                float individualWidth = 0;
                if (startedNewLine)
                {
                    individualWidth = Math.Max(glyph.LeftSideBearing, 0);
                    startedNewLine = false;
                }
                else
                {
                    individualWidth += font.Spacing + glyph.LeftSideBearing;
                }

                individualWidth += glyph.Width;
                individualWidth += glyph.RightSideBearing;

                // There's also glyph.Cropping but not necessary here

                if (char.IsWhiteSpace(c))
                {
                    currentWord = "";
                    currentWordSize = 0;
                    ret.X += individualWidth;
                }
                else
                {
                    if (ret.X + individualWidth > maxWidth)
                    {
                        if (currentWordSize + individualWidth > maxWidth)
                        {
                            // Word is too long for a single line anyways
                            used += '\n';
                            ret.X = 0;
                            ret.Y += font.LineSpacing;
                            currentWord = "";
                            currentWordSize = 0;

                            // Fix for next glyph being on new line
                            individualWidth -= font.Spacing + glyph.LeftSideBearing;
                            individualWidth += Math.Max(glyph.LeftSideBearing, 0);
                        }
                        else
                        {
                            // Move whole word to new line
                            used = used.Substring(0, used.Length - currentWord.Length) + '\n' + currentWord;
                            ret.X = currentWordSize;
                            ret.Y += font.LineSpacing;

                            if (currentWord.Length > 0)
                            {
                                // Fix for first glyph being on new line
                                if (!glyphs.TryGetValue(currentWord[0], out var glyphForFirst))
                                    glyphForFirst = glyphs[font.DefaultCharacter.Value];
                                ret.X -= font.Spacing + glyphForFirst.LeftSideBearing;
                                ret.X += Math.Max(glyph.LeftSideBearing, 0);
                            }
                        }
                    }

                    currentWord += c;
                    currentWordSize += individualWidth;
                    ret.X += individualWidth;
                }
            }

            used += c;
        }

        return new(used, ret);
    }
}
