using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Miniscript;
using SpaceCore.UI;
using SpaceShared;
using StardewValley;

namespace SpaceCore.Framework.ExtEngine
{

    internal class UiDeserializer
    {
        internal Dictionary<string, Type> types = new();

        public UiDeserializer()
        {
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
                types.Add(type.Name, type);
        }

        public Element Deserialize(string pack, XmlReader reader, out List<Element> allElements, out List<string> extra)
        {
            allElements = new();
            return ReadElement(pack, reader, allElements, out extra);
        }

        private Element ReadElement(string pack, XmlReader reader, List<Element> allElements, out List<string> extra)
        {
            Element elem;
            extra = new();

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
                if (attrs.ContainsKey("when") && !ExtensionEngine.CheckWhen(pack, attrs["when"]))
                {
                    reader.MoveToElement();
                    reader.ReadOuterXml();
                    return null;
                }

                string markup = ExtensionEngine.SubstituteTokens(pack, File.ReadAllText(Util.FetchFullPath(SpaceCore.Instance.Helper.ModRegistry, attrs[ "file" ])));
                using TextReader tr = new StringReader(markup);
                using var xr = XmlReader.Create(tr);
                xr.Read();
                elem = new UiDeserializer().Deserialize(pack, xr, out List<Element> allElements2, out extra);
                allElements.AddRange(allElements2);

                goto AfterParsingAttributes;
            }

            if (!types.ContainsKey(reader.Name))
            {
                return null;
            }

            Type t = types[reader.Name];
            elem = ( Element ) t.GetConstructor(new Type[0]).Invoke( new object[ 0 ] );
            elem.UserData = new UiExtraData();
            reader.MoveToFirstAttribute();
            for (int i = 0; i < reader.AttributeCount; ++i, reader.MoveToNextAttribute())
            {
                string name = reader.Name;
                reader.ReadAttributeValue();
                string val = reader.Value;

                if (!LoadPropertyToElement(pack, elem, name, val, extra))
                {
                    elem = null;
                    goto AfterParsingAttributes;
                }
            }

        AfterParsingAttributes:
            if ( elem != null )
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
                    var child = ReadElement(pack, reader, allElements, out List<string> extraTmp);
                    if (child != null && elem is Container container)
                    {
                        container.AddChild(child);
                        if (extraTmp.Contains("CenterH"))
                            child.LocalPosition += new Vector2((container.Bounds.Size.ToVector2() - child.Bounds.Size.ToVector2()).X / 2, 0);
                        if (extraTmp.Contains("CenterV"))
                            child.LocalPosition += new Vector2(0, (container.Bounds.Size.ToVector2() - child.Bounds.Size.ToVector2()).Y / 2);
                    }
                    reader.MoveToContent();
                }
                reader.ReadEndElement();
            }
            else reader.Read();

            return elem;
        }

        internal bool LoadPropertyToElement(string pack, Element elem, string name, string val, List<string> extra)
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
                    (elem.UserData as UiExtraData).Id = val;
                }
                else if (name.Equals("when", StringComparison.OrdinalIgnoreCase))
                {
                    if (!ExtensionEngine.CheckWhen(pack, val))
                    {
                        elem = null;
                        return false;
                    }
                }
                // TODO - abstract into functions section or something
                else if (name == "LoadFromImage" && elem is Image image1)
                {
                    image1.Texture = Util.FetchTexture(SpaceCore.Instance.Helper.ModRegistry, val);
                }
                else if (name == "LoadFromGame" && elem is Image image2)
                {
                    image2.Texture = Game1.content.Load< Texture2D >(val);
                }
                else if (name == "OnClickFunction")
                {
                    (elem.UserData as UiExtraData).OnClickFunction = val;
                }
                else if (name == "ScriptData")
                {
                    (elem.UserData as UiExtraData).ScriptData = new ValString(val);
                }
                else if (name == "TooltipTitle")
                {
                    (elem.UserData as UiExtraData).TooltipTitle = val;
                }
                else if (name == "TooltipText")
                {
                    (elem.UserData as UiExtraData).TooltipText = val;
                }
                else if (name == "CenterH" && !val.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    extra.Add("CenterH");
                }
                else if (name == "CenterV" && !val.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    extra.Add("CenterV");
                }
            }

            return true;
        }
    }
}
