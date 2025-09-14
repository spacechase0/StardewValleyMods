using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Netcode;
using SpaceCore.UI;
using SpaceShared;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.HomeRenovations;
using StardewValley.Menus;
using StardewValley.Triggers;
using static StardewValley.Menus.CoopMenu;

namespace SpaceCore.Guidebooks;
internal class GuidebookMenu : IClickableMenu
{
    private GuidebookData Data { get; }

    private Texture2D PageTexture { get; }
    private Vector2 PageSize { get; }

    private RootElement Ui { get; }
    private Label ChapterTitle { get; }
    private Dictionary<string, Image> ChapterTabs { get; } = new();
    private ScrollContainer PageContainer { get; set; }
    private Image PreviousPageButton { get; set; }
    private Image NextPageButton { get; set; }
    private Label PageLabel { get; set; }

    private List<string> ValidChapters { get; } = new();
    private List<(string id, string contents)> CurrentChapterPages { get; } = new();

    private string CurrentChapter { get; set; }
    private int CurrentPage { get; set; }
    private string PendingGoto { get; set; }
    private int? PendingPage { get; set; }

    public GuidebookMenu(GuidebookData data)
    {
        Data = data;

        if (Data.PageTexture == null)
        {
            width = (int)Data.PageSize.X;
            height = (int)Data.PageSize.Y;
        }
        else
        {
            PageTexture = Game1.content.Load<Texture2D>(Data.PageTexture);
            width = PageTexture.Width;
            height = PageTexture.Height;
        }
        PageSize = new Vector2(width, height);
        height += 200;
        xPositionOnScreen = (Game1.uiViewport.Width - width) / 2;
        yPositionOnScreen = (Game1.uiViewport.Height - height) / 2;
        Ui = new RootElement()
        {
            LocalPosition = new(xPositionOnScreen, yPositionOnScreen),
        };
        width += 64;
        xPositionOnScreen -= 64;

        /*
        Label bookTitle = new()
        {
            String = Data.Title,
            Bold = true,
        };
        bookTitle.LocalPosition = new((width - 64) / 2 - bookTitle.Width / 2, 50);
        Ui.AddChild(bookTitle);
        */

        ChapterTitle = new()
        {
            LocalPosition = new(0, 80)
        };
        Ui.AddChild(ChapterTitle);

        PageContainer = new()
        {
            LocalPosition = Data.PagePadding - new Vector2(12, 12) + new Vector2( 0, 125 ), // 12 is already included in ScrollContainer calculations
            Size = PageSize,
            OutlineColor = Data.PageTexture == null ? Color.White : null,
        };

        PreviousPageButton = new()
        {
            LocalPosition = new Vector2( 16, height - ( 50 - 11*Game1.pixelZoom / 2)),
            Texture = Game1.mouseCursors,
            TexturePixelArea = new Rectangle( 352, 495, 12, 11 ),
            Scale = Game1.pixelZoom,
            Callback = (_) =>
            {
                if (CurrentPage > 0)
                {
                    CurrentPage -= 1;
                    RefreshPage();
                }
            }
        };
        Ui.AddChild(PreviousPageButton);
        NextPageButton = new()
        {
            LocalPosition = new Vector2(PageContainer.Width - 12*Game1.pixelZoom, height - (50 - 11 * Game1.pixelZoom / 2)),
            Texture = Game1.mouseCursors,
            TexturePixelArea = new Rectangle(365, 495, 12, 11),
            Scale = Game1.pixelZoom,
            Callback = (_) =>
            {
                if (CurrentPage < CurrentChapterPages.Count - 1)
                {
                    CurrentPage += 1;
                    RefreshPage();
                }
            }
        };
        Ui.AddChild(NextPageButton);

        PageLabel = new()
        {
        };
        Ui.AddChild(PageLabel);

        foreach (var chapter in Data.Chapters)
        {
            if (GameStateQuery.CheckConditions(chapter.Value.Condition, location: Game1.currentLocation, player: Game1.player))
                ValidChapters.Add(chapter.Key);
        }

        float y = 0;
        foreach (string chapter in ValidChapters)
        {
            var chapData = Data.Chapters[chapter];
            Image tab = new()
            {
                Texture = Game1.content.Load<Texture2D>(chapData.TabIconTexture),
                TexturePixelArea = chapData.TabIconRect,
                Scale = chapData.TabIconScale,
                Callback = (_) => GotoChapter(chapter, null),
            };
            tab.LocalPosition = new Vector2(-tab.Width, PageContainer.LocalPosition.Y + y);
            Ui.AddChild(tab);
            ChapterTabs.Add(chapter, tab);
            y += tab.Height;
        }

        Ui.AddChild(PageContainer); // This needs to be later so the tabs draw underneath

        GotoChapter(Data.DefaultChapter, null);
    }

    public void GotoChapter(string chapterId, string pageId)
    {
        if (chapterId != CurrentChapter)
        {
            CurrentChapterPages.Clear();

            CurrentChapter = chapterId;
            foreach (var page in Data.Chapters[CurrentChapter].Pages)
            {
                if (!GameStateQuery.CheckConditions(page.Condition, location: Game1.currentLocation, player: Game1.player))
                    continue;
                CurrentChapterPages.Add(new(page.Id, page.Contents));
            }

            ChapterTitle.String = Data.Chapters[CurrentChapter].Name;
            ChapterTitle.LocalPosition = new((width - 64) / 2 - ChapterTitle.Width / 2 + 20, ChapterTitle.LocalPosition.Y);

            foreach (var tab in ChapterTabs.Values)
            {
                tab.LocalPosition = new(-tab.Width + 4 + 16, tab.LocalPosition.Y);
            }
            if (ChapterTabs.TryGetValue(CurrentChapter, out var currentTab))
            {
                currentTab.LocalPosition = new(-currentTab.Width + 4, currentTab.LocalPosition.Y);
            }

            CurrentPage = 0;
        }

        CurrentPage = pageId != null ? CurrentChapterPages.FindIndex(0, CurrentChapterPages.Count, p => p.id == pageId) : -1;
        if (CurrentPage == -1)
            CurrentPage = 0;
        RefreshPage();
    }

    private void RefreshPage()
    {
        foreach (var child in PageContainer.Children.ToList())
        {
            if (child == PageContainer.Scrollbar)
                continue;
            PageContainer.RemoveChild(child);
        }

        var elems = GuidebookParser.Parse(CurrentChapterPages[CurrentPage].contents, new(Game1.currentLocation, Game1.player, null, null, null));

        float x = 0, y = 0;
        SpriteFont lastFont = GuidebookFont.Fonts["default"];
        for (int ie = 0; ie < elems.Count; ++ie)
        {
            var elem = elems[ie];
            GuidebookParser.Element nextElem = ie + 1 < elems.Count ? elems[ie + 1] : null;
            if (elem.Type == GuidebookParser.Element.ElementType.Text &&
                elem.Value.Replace("\n", "") == "" &&
                ( nextElem == null || ( nextElem.Type != GuidebookParser.Element.ElementType.Text && nextElem.Type != GuidebookParser.Element.ElementType.InlineImage ) ) )
                continue;

            switch (elem.Type)
            {
                case GuidebookParser.Element.ElementType.Text:
                    {
                        string elemVal = elem.Value;

                        string strWithoutEmptyLines = "";
                        char prevC = '\0';
                        foreach (char c in elemVal)
                        {
                            if (c == '\n' && prevC == '\n')
                                continue;
                            strWithoutEmptyLines += c;
                            prevC = c;
                        }

                        if (!elem.Tags.ContainsKey("nospacing"))
                        {
                            int i = 0;
                            for (; i < strWithoutEmptyLines.Length; ++i)
                            {
                                if (strWithoutEmptyLines[i] == '\n')
                                {
                                    x = 0;
                                    y += lastFont.LineSpacing;
                                }
                                else
                                    break;
                            }
                            strWithoutEmptyLines = strWithoutEmptyLines.Substring(i);
                        }

                        string usedFontId = elem.Tags.TryGetValue("font", out string fontId) ? fontId : "default";
                        lastFont = GuidebookFont.Fonts[usedFontId];
                        Color textCol = elem.Tags.TryGetValue("color", out string colStr) ? ( Utility.StringToColor( colStr ) ?? Game1.textColor ) : Game1.textColor;
                        if (textCol == Game1.textColor && elem.OnClick != null)
                        {
                            textCol = new Color(textCol.R * 3, textCol.G * 3, textCol.B * 3);
                        }
                        (string str, Vector2 offset) = GuidebookFont.SplitAtWidth(strWithoutEmptyLines, lastFont, x, PageSize.X);

                        void AddLabel(string str, Vector2 pos)
                        {
                            // Because of how the bounds of UI elements work, we want to split each line on their own.
                            // Otherwise, the last line could have hover/click effects to the right of it because the previous line extended further to the right
                            if (str.Contains('\n'))
                            {
                                string[] stuff = str.Split('\n');
                                for (int i = 0; i < stuff.Length; ++i)
                                {
                                    AddLabel(stuff[i], pos + new Vector2(0, lastFont.LineSpacing * i));
                                }
                                return;
                            }
                            
                            Label label = new Label()
                            {
                                IdleTextColor = textCol,
                                Font = lastFont,
                                String = str,
                                LocalPosition = pos,
                                UserData = elem.Hover,
                            };

                            if (elem.Tags.TryGetValue("center", out string centerStr))
                            {
                                float w = PageSize.X;
                                if (!string.IsNullOrEmpty(centerStr))
                                    float.TryParse(centerStr, out w);
                                label.LocalPosition = new(pos.X + (w - label.Width) / 2, pos.Y);
                            }

                            if (elem.OnClick != null)
                            {
                                var clickData = elem.OnClick;
                                label.Callback = (_) => HandleClick(label, clickData);

                                // We want users to always be aware when they are clicking a browser link
                                if (elem.OnClick.Type == GuidebookParser.ClickData.ClickType.PageLink &&
                                    (elem.OnClick.Value.StartsWith("http://") || elem.OnClick.Value.StartsWith("https://")))
                                {
                                    label.UserData = new GuidebookParser.HoverData()
                                    {
                                        Type = GuidebookParser.HoverData.HoverType.Text,
                                        HoverValue = elem.OnClick.Value,
                                    };
                                }
                            }

                            PageContainer.AddChild(label);
                        }

                        // If the label has a new line but doesn't start at x=0,
                        // new lines will continue at the base position instead of the next line
                        // So we need two lines
                        if (offset.Y > 0 && x > 0)
                        {
                            int nl = str.IndexOf('\n');
                            string beforeFirstBreak = str.Substring(0, nl);
                            string afterFirstBreak = str.Substring(nl + 1);
                            AddLabel(beforeFirstBreak, new(x, y));
                            AddLabel(afterFirstBreak, new(0, y + lastFont.LineSpacing));
                            /*if (!elem.Tags.ContainsKey("nospacing"))
                            {
                                y += lastFont.LineSpacing;
                            }
                            */
                        }
                        else
                        {
                            AddLabel(str, new( x, y ));
                        }

                        if (!elem.Tags.ContainsKey("nospacing"))
                        {
                            x = offset.X;
                            y += offset.Y;
                        }
                        break;
                    };
                case GuidebookParser.Element.ElementType.Image:
                case GuidebookParser.Element.ElementType.InlineImage:
                    {
                        if (elem.Type == GuidebookParser.Element.ElementType.Image && x > 0 && !elem.Tags.ContainsKey("nospacing"))
                        {
                            x = 0;
                            y += lastFont.LineSpacing;
                        }
                        string[] parts = elem.Value.Split(':');
                        string imagePath = parts[0];
                        Rectangle? rect = null;
                        int scale = elem.Type == GuidebookParser.Element.ElementType.InlineImage ? 2 : 4;
                        if (parts.Length >= 2 && parts[1] != "null")
                        {
                            string[] rectParts = parts[1].Split(',');
                            if (rectParts.Length == 4)
                            {
                                try
                                {
                                    int[] nums = rectParts.Select(s => int.Parse(s)).ToArray();
                                    rect = new Rectangle(nums[0], nums[1], nums[2], nums[3]);
                                }
                                catch (FormatException e)
                                {
                                    Log.Warn($"Failed to parse \"{elem.Value}\" image subrect: One of the components wasn't an integer");
                                }
                            }
                            else
                            {
                                Log.Warn($"Failed to parse \"{elem.Value}\" image subrect: Exactly four integers must be specified (x,y,width,height)");
                            }
                        }
                        if (parts.Length >= 3)
                        {
                            if (!int.TryParse(parts[2], out scale))
                            {
                                Log.Warn($"Failed to parse \"{elem.Value}\" image scale: Must be an integer");
                            }
                        }

                        Image img = new Image()
                        {
                            Texture = Game1.content.DoesAssetExist<Texture2D>(imagePath) ? Game1.content.Load<Texture2D>(imagePath) : Game1.staminaRect,
                            TexturePixelArea = rect,
                            Scale = scale,
                            LocalPosition = new(x, y),
                            UserData = elem.Hover,
                        };
                        if (!elem.Tags.ContainsKey("nospacing"))
                        {
                            if (elem.Type == GuidebookParser.Element.ElementType.Image)
                            {
                                img.LocalPosition = new((Data.PageSize.X - img.Width) / 2, img.LocalPosition.Y);
                            }
                            else if (elem.Type == GuidebookParser.Element.ElementType.InlineImage)
                            {
                                img.LocalPosition = new(img.LocalPosition.X, img.LocalPosition.Y + (lastFont.LineSpacing - (rect?.Height ?? img.Texture.Height) * scale) / 2);
                            }
                        }
                        if (elem.OnClick != null)
                        {
                            var clickData = elem.OnClick;
                            img.Callback = (_) => HandleClick(img, clickData);
                        }

                        PageContainer.AddChild(img);
                        if (!elem.Tags.ContainsKey("nospacing"))
                        {
                            if (elem.Type == GuidebookParser.Element.ElementType.Image)
                            {
                                x = 0;
                                y += (rect?.Height ?? img.Texture.Height) * scale;
                            }
                            else if (elem.Type == GuidebookParser.Element.ElementType.InlineImage)
                            {
                                x += (rect?.Width ?? img.Texture.Width) * scale;
                                if (x >= PageSize.X)
                                {
                                    x = 0;
                                    y += lastFont.LineSpacing;
                                }
                            }
                        }
                    }
                    break;
                case GuidebookParser.Element.ElementType.MetaCommand:
                    switch (elem.TagName)
                    {
                        case "offset":
                            {
                                string[] parts = elem.Value.Split(',');
                                int[] partsI = parts.Select(s => int.Parse(s)).ToArray();
                                x += partsI[0];
                                y += partsI[1];
                            }
                            break;
                        case "nextposition":
                            {
                                string[] parts = elem.Value.Split(',');
                                x = parts[0] == "null" ? x : int.Parse(parts[0]);
                                y = parts[1] == "null" ? y : int.Parse(parts[1]);
                            }
                            break;
                    }
                    break;
            }
        }

        PageLabel.String = $"{CurrentPage + 1}/{CurrentChapterPages.Count}";
        PageLabel.LocalPosition = new((width - 64) / 2 - PageLabel.Width / 2 + 20, height - 50 + 16);
        PageContainer.Scrollbar.ScrollTo(0);
        PageContainer.lastScroll = 0;
    }

    private List<Element> RemoveBecauseClicked = new();
    private void HandleClick(Element elem, GuidebookParser.ClickData clickData)
    {
        switch (clickData.Type)
        {
            case GuidebookParser.ClickData.ClickType.PageLink:
                PendingGoto = clickData.Value;
                break;
            case GuidebookParser.ClickData.ClickType.Action:
            case GuidebookParser.ClickData.ClickType.OnceAction:
                TriggerActionManager.TryRunAction(clickData.Value, out string error, out Exception exception);
                if (exception != null)
                {
                    Log.Error($"Exception while running clicked action chapter \"{CurrentChapter}\" page {CurrentPage}: {exception}");
                }
                else if (error != null)
                {
                    Log.Error($"Error while running clicked action on chapter \"{CurrentChapter}\" page {CurrentPage}: {error}");
                }
                if (clickData.Type == GuidebookParser.ClickData.ClickType.OnceAction)
                    RemoveBecauseClicked.Add(elem);
                break;
        }
    }
    public override void receiveScrollWheelAction(int direction)
    {
        PageContainer.Scrollbar.ScrollBy(direction / -120);
    }

    private int scrollCounter = 0;

    public override void update(GameTime time)
    {
        foreach (var elem in RemoveBecauseClicked)
        {
            PageContainer.RemoveChild(elem);
        }
        RemoveBecauseClicked.Clear();

        if (PendingGoto != null)
        {
            if (PendingGoto.StartsWith("http://") || PendingGoto.StartsWith("https://"))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(PendingGoto) { UseShellExecute = true });
                }
                catch (Exception)
                {
                }
            }
            else
            {
                int slash = PendingGoto.IndexOf('/');
                string chapter = slash == -1 ? PendingGoto : PendingGoto.Substring(0, slash);
                string page = slash == -1 ? null : PendingGoto.Substring(slash + 1);
                GotoChapter(chapter, page);
            }
            PendingGoto = null;
        }

        if (PendingPage != null)
        {
            CurrentPage = PendingPage.Value;
            RefreshPage();
            PendingPage = null;
        }

        Ui.Update();

        if (Game1.input.GetGamePadState().ThumbSticks.Right.Y != 0)
        {
            if (++scrollCounter == 5)
            {
                scrollCounter = 0;
                PageContainer.Scrollbar.ScrollBy(Math.Sign(Game1.input.GetGamePadState().ThumbSticks.Right.Y) * 120 / -120);
            }
        }
        else scrollCounter = 0;
    }

    public override void draw(SpriteBatch b)
    {
        base.draw(b);

        IClickableMenu.drawTextureBox(b, PageLabel.Bounds.X - 20, PageLabel.Bounds.Y - 20, PageLabel.Bounds.Width + 40, PageLabel.Bounds.Height + 40, Color.White);
        IClickableMenu.drawTextureBox(b, ChapterTitle.Bounds.X - 24, ChapterTitle.Bounds.Y - 24, ChapterTitle.Bounds.Width + 48, ChapterTitle.Bounds.Height + 48, Color.White);
        SpriteText.drawStringWithScrollCenteredAt(b, Data.Title, (int)Ui.Position.X + (int)PageSize.X / 2 + 16, (int)Ui.Position.Y + 10, (int)PageSize.X - 64);

        if (PageTexture != null)
        {
            b.Draw(PageTexture, PageContainer.Position, Color.White);
        }
        Ui.Draw(b);

        foreach (var child in PageContainer.Children)
        {
            if (child.Hover && child.UserData is GuidebookParser.HoverData hover)
            {
                switch (hover.Type)
                {
                    case GuidebookParser.HoverData.HoverType.Text:
                        drawHoverText(b, hover.HoverValue, Game1.smallFont, boldTitleText: string.IsNullOrEmpty(hover.HoverTitle) ? null : hover.HoverTitle);
                        break;
                    case GuidebookParser.HoverData.HoverType.Item:
                        var item = ItemRegistry.Create(hover.HoverValue);
                        drawHoverText(b, item.getDescription(), Game1.smallFont, boldTitleText: item.DisplayName, hoveredItem: item);
                        break;
                    case GuidebookParser.HoverData.HoverType.Image:
                        {
                            int x = Game1.getOldMouseX() + 32;
                            int y = Game1.getOldMouseY() + 32;

                            string[] parts = hover.HoverValue.Split(':');
                            string imagePath = parts[0];
                            Rectangle? rect = null;
                            int scale = 4;
                            if (parts.Length >= 2 && parts[1] != "null")
                            {
                                string[] rectParts = parts[1].Split(',');
                                if (rectParts.Length == 4)
                                {
                                    try
                                    {
                                        int[] nums = rectParts.Select(s => int.Parse(s)).ToArray();
                                        rect = new Rectangle(nums[0], nums[1], nums[2], nums[3]);
                                    }
                                    catch (FormatException e)
                                    {
                                        Log.Warn($"Failed to parse \"{hover.HoverValue}\" image subrect: One of the components wasn't an integer");
                                    }
                                }
                                else
                                {
                                    Log.Warn($"Failed to parse \"{hover.HoverValue}\" image subrect: Exactly four integers must be specified (x,y,width,height)");
                                }
                            }
                            if (parts.Length >= 3)
                            {
                                if (!int.TryParse(parts[2], out scale))
                                {
                                    Log.Warn($"Failed to parse \"{hover.HoverValue}\" image scale: Must be an integer");
                                }
                            }
                            Texture2D tex = Game1.content.DoesAssetExist<Texture2D>(imagePath) ? Game1.content.Load<Texture2D>(imagePath) : Game1.staminaRect;

                            int width = (rect?.Width ?? tex.Width) * scale;
                            int height = (rect?.Height ?? tex.Height) * scale;

                            if (!string.IsNullOrEmpty(hover.HoverTitle))
                            {
                                width = Math.Max(width, (int)Game1.dialogueFont.MeasureString(hover.HoverTitle).X + 32);
                                height += (int)Game1.dialogueFont.MeasureString(hover.HoverTitle).Y + 32;
                            }

                            if (x + width > Utility.getSafeArea().Right)
                            {
                                x = Utility.getSafeArea().Right - width;
                                y += 16;
                            }

                            if (y + height > Utility.getSafeArea().Bottom)
                            {
                                x += 16;
                                if (x + width > Utility.getSafeArea().Right)
                                {
                                    x = Utility.getSafeArea().Right - width;
                                }

                                y = Utility.getSafeArea().Bottom - height;
                            }

                            drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, height, Color.White);
                            if (!string.IsNullOrEmpty(hover.HoverTitle))
                            {
                                drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, (int)Game1.dialogueFont.MeasureString(hover.HoverTitle).Y + 32 - 4, Color.White, 1, drawShadow: false);
                                b.Draw(Game1.menuTexture, new Rectangle(x + 12, y + (int)Game1.dialogueFont.MeasureString(hover.HoverTitle).Y + 32 - 4, width - 4 * 6, 4), new Rectangle(44, 300, 4, 4), Color.White);
                                b.DrawString(Game1.dialogueFont, hover.HoverTitle, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
                                b.DrawString(Game1.dialogueFont, hover.HoverTitle, new Vector2(x + 16, y + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
                                b.DrawString(Game1.dialogueFont, hover.HoverTitle, new Vector2(x + 16, y + 16 + 4), Game1.textColor);

                                y += (int)Game1.dialogueFont.MeasureString(hover.HoverTitle).Y;
                            }
                            b.Draw(tex, new Vector2(x, y), rect, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1);
                        }
                        break;
                }
            }
        }

        foreach (var entry in ChapterTabs)
        {
            if (entry.Value.Hover)
            {
                drawHoverText(b, Data.Chapters[entry.Key].Name, Game1.smallFont);
            }
        }

        drawMouse(b);
    }
    public override bool overrideSnappyMenuCursorMovementBan()
    {
        return true;
    }
}
