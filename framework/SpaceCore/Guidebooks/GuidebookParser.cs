using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SpaceShared;
using StardewValley;
using StardewValley.Delegates;

namespace SpaceCore.Guidebooks;
internal class GuidebookParser
{
    public class ClickData
    {
        public enum ClickType
        {
            PageLink,
            Action,
            OnceAction,
        }
        public ClickType Type { get; set; }
        public string Value { get; set; }
    }

    public class HoverData
    {
        public enum HoverType
        {
            Text,
            Item,
            Image,
        }
        public HoverType Type { get; set; }
        public string HoverTitle { get; set; }
        public string HoverValue { get; set; }
    }

    public class Element
    {
        public enum ElementType
        {
            Text,
            Image,
            InlineImage,
            MetaCommand,
        }
        public ElementType Type { get; set; }
        public string TagName { get; set; } // Not valid for Text
        public string Value { get; set; }
        public ClickData OnClick { get; set; } = null;
        public HoverData Hover { get; set; } = null;

        public Dictionary<string, string> Tags { get; set; } = new();
    }

    public static List<Element> Parse(string text, GameStateQueryContext ctx)
    {
        List<Element> ret = new();

        Stack<ClickData> clicks = new();
        Stack<HoverData> hovers = new();
        Stack<(string id, string val)> tags = new();
        string buffer = "";
        Stack<bool> ifSucceeded = new();
        bool lastIf = false;
        Dictionary<string, string> GetActiveTags()
        {
            Dictionary<string, string> ret = new();
            foreach (var entry in tags)
            {
                if (ret.ContainsKey(entry.id))
                    continue;
                ret.Add(entry.id, entry.val);
            }
            return ret;
        }
        void FlushBuffer()
        {
            if (buffer == "")
                return;

            if (!ifSucceeded.Any(b => !b))
            {
                ret.Add(new Element()
                {
                    Type = Element.ElementType.Text,
                    Value = buffer,
                    OnClick = clicks.Count > 0 ? clicks.Peek() : null,
                    Hover = hovers.Count > 0 ? hovers.Peek() : null,
                    Tags = GetActiveTags(),
                });
            }
            buffer = "";
        }
        for (int i = 0; i < text.Length; ++i)
        {
            char c = text[i];

            if (c == '[')
            {
                FlushBuffer();
                int end = text.IndexOf(']', i + 1);
                if (end == -1)
                {
                    buffer += c;
                    continue;
                }

                string tag = text.Substring(i + 1, end - i - 1);
                string tagName = tag;
                string tagVal = null;
                int eqInd = tag.IndexOf('=');
                if (eqInd != -1)
                {
                    tagName = tag.Substring(0, eqInd);
                    tagVal = tag.Substring(eqInd + 1);
                }

                bool isEnd = false;
                if (tagName.StartsWith('/'))
                {
                    isEnd = true;
                    tagName = tagName.Substring(1);
                }

                switch (tagName.ToLower())
                {
                    case "image":
                        if (!ifSucceeded.Any(b => !b))
                        {
                            Rectangle? rect = null;
                            ret.Add(new Element()
                            {
                                Type = Element.ElementType.Image,
                                TagName = tagName,
                                Value = tagVal,
                                OnClick = clicks.Count > 0 ? clicks.Peek() : null,
                                Hover = hovers.Count > 0 ? hovers.Peek() : null,
                                Tags = GetActiveTags(),
                            });
                        }
                        break;
                    case "icon":
                        if (!ifSucceeded.Any(b => !b))
                        {
                            Rectangle? rect = null;
                            ret.Add(new Element()
                            {
                                Type = Element.ElementType.InlineImage,
                                TagName = tagName,
                                Value = tagVal,
                                OnClick = clicks.Count > 0 ? clicks.Peek() : null,
                                Hover = hovers.Count > 0 ? hovers.Peek() : null,
                                Tags = GetActiveTags(),
                            });
                        }
                        break;
                    case "nextposition":
                    case "offset":
                        if (!ifSucceeded.Any(b => !b))
                        {
                            ret.Add(new Element()
                            {
                                Type = Element.ElementType.MetaCommand,
                                TagName = tagName,
                                Value = tagVal,
                                Tags = GetActiveTags(),
                            });
                        }
                        break;
                    case "if":
                        if (isEnd)
                        {
                            lastIf = ifSucceeded.Pop();
                        }
                        else
                        {
                            ifSucceeded.Push(tagVal == null || GameStateQuery.CheckConditions(tagVal));
                        }
                        break;
                    case "else":
                        if (isEnd)
                        {
                            ifSucceeded.Pop();
                        }
                        else
                        {
                            ifSucceeded.Push(!lastIf);
                        }
                        break;
                    case "link":
                        if (isEnd)
                        {
                            clicks.Pop();
                        }
                        else
                        {
                            clicks.Push(new()
                            {
                                Type = ClickData.ClickType.PageLink,
                                Value = tagVal,
                            });
                        }
                        break;
                    case "action":
                    case "onceaction":
                        if (isEnd)
                        {
                            clicks.Pop();
                        }
                        else
                        {
                            clicks.Push(new()
                            {
                                Type = tagName == "action" ? ClickData.ClickType.Action : ClickData.ClickType.OnceAction,
                                Value = tagVal,
                            });
                        }
                        break;
                    case "texttooltip":
                        if (isEnd)
                        {
                            hovers.Pop();
                        }
                        else
                        {
                            int colon = tagVal.IndexOf(':');
                            string a = tagVal.Substring(0, colon);
                            string b = tagVal.Substring(colon + 1);
                            hovers.Push(new()
                            {
                                Type = HoverData.HoverType.Text,
                                HoverTitle = a,
                                HoverValue = b,
                            });
                        }
                        break;
                    case "imagetooltip":
                        if (isEnd)
                        {
                            hovers.Pop();
                        }
                        else
                        {
                            int colon = tagVal.IndexOf(':');
                            string a = tagVal.Substring(0, colon);
                            string b = tagVal.Substring(colon + 1);
                            hovers.Push(new()
                            {
                                Type = HoverData.HoverType.Image,
                                HoverTitle = a,
                                HoverValue = b,
                            });
                        }
                        break;
                    case "itemtooltip":
                        if (isEnd)
                        {
                            hovers.Pop();
                        }
                        else
                        {
                            hovers.Push(new()
                            {
                                Type = HoverData.HoverType.Item,
                                HoverValue = tagVal,
                            });
                        }
                        break;
                    default:
                        if (isEnd)
                        {
                            tags.Pop();
                        }
                        else
                        {
                            tags.Push(new(tagName, tagVal));
                        }
                        break;
                }

                i = end;
            }
            else if (c != '\r')
                buffer += c;
        }
        FlushBuffer();

        for (int i = 1; i < ret.Count; ++i)
        {
            Element a = ret[i - 1];
            Element b = ret[i];

            if (a.Type == Element.ElementType.Text && b.Type == Element.ElementType.Text &&
                a.OnClick == null && b.OnClick == null && a.Hover == null && b.Hover == null)
            {
                List<string> atags = a.Tags.Select(t => $"{t.Key}={t.Value}").ToList();
                atags.Sort();
                List<string> btags = b.Tags.Select(t => $"{t.Key}={t.Value}").ToList();
                btags.Sort();

                if (Enumerable.SequenceEqual(atags, btags))
                {
                    a.Value += b.Value;
                    ret.RemoveAt(i);
                    --i;
                }
            }
        }

        return ret;
    }
}
