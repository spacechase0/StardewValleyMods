using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace SpaceCore.Guidebooks;
public class GuidebookData
{
    public class PageData
    {
        public string Id { get; set; }
        public string Contents { get; set; }
        public string Condition { get; set; } = "TRUE";
    }

    public class ChapterData
    {
        public string Name { get; set; }
        public string TabIconTexture { get; set; }
        public Rectangle TabIconRect { get; set; }
        public int TabIconScale { get; set; } = Game1.pixelZoom;
        public string Condition { get; set; } = "TRUE";

        public List<PageData> Pages { get; set; } = new();
    }

    public string Title { get; set; }
    public string PageTexture { get; set; } // If null, use IClickableMenu.drawTextureBox
    public Vector2 PagePadding { get; set; } = new(28, 28);
    public Vector2 PageSize { get; set; } = new(600, 500); // Only used if PageTexture is null
    public string DefaultChapter { get; set; }
    public Dictionary<string, ChapterData> Chapters { get; set; } = new();
}
