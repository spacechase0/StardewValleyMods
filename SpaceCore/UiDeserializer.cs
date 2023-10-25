using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.UI;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace SpaceCore
{
    public class UiExtraData
    {
        public string Id { get; set; }

        public Dictionary<string, string> ExtraFields { get; set; } = new();

        public object UserData { get; set; }
    }

    public class UiDeserializer
    {
        private Func<string, string> textLoader;
        private Func<string, Texture2D> textureLoader;
        private Func<string, string> tokenSubstituter;
        private Func<string, bool> conditionChecker;

        public Dictionary<string, Type> Types { get; } = new();


        public UiDeserializer(Func<string, string> textLoader, Func<string, Texture2D> textureLoader, Func<string, string> tokenSubstituter = null, Func<string, bool> conditionChecker = null)
        {
            this.textLoader = textLoader;
            this.textureLoader = textureLoader;
            this.tokenSubstituter = tokenSubstituter ?? ((s) => s);
            this.conditionChecker = conditionChecker ?? ((s) => true);

            Type[] theTypes = new Type[]
            {
                typeof( Button ),
                typeof( Checkbox ),
                typeof( Container ),
                typeof( Dropdown ),
                typeof( Element ),
                typeof( Floatbox ),
                typeof( Image ),
                typeof( Intbox ),
                typeof( ItemSlot ),
                typeof( ItemWithBorder ),
                typeof( Label ),
                typeof( RootElement ),
                typeof( Scrollbar ),
                typeof( Slider ),
                typeof( StaticContainer ),
                typeof( Table ),
                typeof( Textbox )
            };
            foreach (var type in theTypes)
                Types.Add(type.Name, type);
        }

        public Element LoadFromFile(string path)
        {
            return LoadFromFile(path, out var _);
        }

        public Element LoadFromFile(string path, out List<Element> allElements)
        {
            string markup = tokenSubstituter(textLoader(path));
            using TextReader tr = new StringReader(markup);
            using var xr = XmlReader.Create(tr);
            xr.Read();
            return Deserialize(xr, out allElements);
        }

        public Element Deserialize(XmlReader reader, out List<Element> allElements)
        {
            allElements = new();
            return ReadElement(reader, allElements);
        }

        private Element ReadElement(XmlReader reader, List<Element> allElements)
        {
            Element elem;

            if (reader.Name == "Include")
            {
                Dictionary<string, string> attrs = new();
                reader.MoveToFirstAttribute();
                for (int i = 0; i < reader.AttributeCount; ++i, reader.MoveToNextAttribute())
                {
                    string name = reader.Name;
                    reader.ReadAttributeValue();
                    string val = reader.Value;
                    attrs.Add(name.ToLower(), val);
                }

                bool doStuff = true;
                if (attrs.ContainsKey("when") && !conditionChecker(attrs["when"]))
                {
                    reader.MoveToElement();
                    reader.ReadOuterXml();
                    return null;
                }

                string markup = tokenSubstituter(textLoader(attrs["file"]));
                using TextReader tr = new StringReader(markup);
                using var xr = XmlReader.Create(tr);
                xr.Read();
                elem = Deserialize(xr, out List<Element> allElements2);
                allElements.AddRange(allElements2);

                goto AfterParsingAttributes;
            }

            if (!Types.ContainsKey(reader.Name))
            {
                return null;
            }

            Type t = Types[reader.Name];
            elem = (Element)t.GetConstructor(new Type[0]).Invoke(new object[0]);
            elem.UserData = new UiExtraData();
            reader.MoveToFirstAttribute();
            for (int i = 0; i < reader.AttributeCount; ++i, reader.MoveToNextAttribute())
            {
                string name = reader.Name;
                reader.ReadAttributeValue();
                string val = reader.Value;

                if (!LoadPropertyToElement(elem, name, val))
                {
                    elem = null;
                    goto AfterParsingAttributes;
                }
            }

        AfterParsingAttributes:
            if (elem != null)
                allElements.Add(elem);
            reader.MoveToElement();
            //var children = reader.ReadSubtree();

            if (!reader.IsEmptyElement)
            {
                reader.Read();
                reader.MoveToContent();
                if (reader.NodeType == XmlNodeType.EndElement)
                    return elem;

                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    var child = ReadElement(reader, allElements);
                    if (child != null && elem is Container container)
                    {
                        container.AddChild(child);
                        if ((child.UserData as UiExtraData).ExtraFields.ContainsKey("CenterH"))
                            child.LocalPosition += new Vector2((container.Bounds.Size.ToVector2() - child.Bounds.Size.ToVector2()).X / 2, 0);
                        if ((child.UserData as UiExtraData).ExtraFields.ContainsKey("CenterV"))
                            child.LocalPosition += new Vector2(0, (container.Bounds.Size.ToVector2() - child.Bounds.Size.ToVector2()).Y / 2);
                    }
                    reader.MoveToContent();
                }
                reader.ReadEndElement();
            }
            else reader.Read();

            return elem;
        }

        internal bool LoadPropertyToElement(Element elem, string name, string val)
        {
            var prop = elem.GetType().GetProperty(name);
            if (prop != null && name != "UserData")
            {
                object obj = val;

                if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                    obj = int.Parse(val);
                else if (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(float?))
                    obj = float.Parse(val);
                else
                {
                    string[] parts = val.Split(',').Select(x => x.Trim()).ToArray();
                    if (parts.Length == 2 && (prop.PropertyType == typeof(Vector2) || prop.PropertyType == typeof(Vector2?)))
                        obj = new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
                    else if (parts.Length == 4 && (prop.PropertyType == typeof(Rectangle) || prop.PropertyType == typeof(Rectangle?)))
                        obj = new Rectangle(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                    else if (parts.Length == 3 && (prop.PropertyType == typeof(Color) || prop.PropertyType == typeof(Color?)))
                        obj = new Color(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
                    else if (parts.Length == 4 && (prop.PropertyType == typeof(Color) || prop.PropertyType == typeof(Color?)))
                        obj = new Color(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                }

                prop.SetMethod.Invoke(elem, new object[] { obj });
            }
            else
            {
                if (name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    elem.UserData ??= new UiExtraData();
                    (elem.UserData as UiExtraData).Id = val;
                }
                else if (name.Equals("when", StringComparison.OrdinalIgnoreCase))
                {
                    if (!conditionChecker(val))
                    {
                        elem = null;
                        return false;
                    }
                }
                // TODO - abstract into functions section or something
                else if (name == "LoadFromImage" && elem is ISingleTexture image1)
                {
                    image1.Texture = textureLoader(val);
                }
                else if (name == "LoadFromGame" && elem is ISingleTexture image2)
                {
                    image2.Texture = Game1.content.Load<Texture2D>(val);
                }
                else
                {
                    elem.UserData ??= new UiExtraData();
                    (elem.UserData as UiExtraData).ExtraFields.Add(name, val);
                }
            }

            return true;
        }
    }
}
