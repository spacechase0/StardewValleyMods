using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;
using SpaceCore.UI;
using SpaceShared;

namespace SpaceCore.Framework.ExtEngine
{
    internal class UiDeserializer
    {
        private Dictionary<string, Type> types = new();

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

        public Element Deserialize(XmlReader reader, out Dictionary<string, List<Element>> elemsById)
        {
            elemsById = new();
            return ReadElement(reader, elemsById);
        }

        private Element ReadElement(XmlReader reader, Dictionary<string, List<Element>> elemsById)
        {
            if (!types.ContainsKey(reader.Name))
            {
                return null;
            }

            Type t = types[reader.Name];
            Element elem = ( Element ) t.GetConstructor(new Type[0]).Invoke( new object[ 0 ] );
            reader.MoveToFirstAttribute();
            for (int i = 0; i < reader.AttributeCount; ++i, reader.MoveToNextAttribute())
            {
                string name = reader.Name;
                reader.ReadAttributeValue();
                var prop = t.GetProperty(name);
                string val = reader.Value;

                if (prop != null)
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
                            obj = new Vector2( float.Parse(parts[0]), float.Parse(parts[1]) );
                        else if (parts.Length == 4 && ( prop.PropertyType == typeof(Rectangle) || prop.PropertyType == typeof(Rectangle?)))
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
                        if (!elemsById.ContainsKey(val))
                            elemsById.Add(val, new());
                        elemsById[val].Add(elem);
                    }
                    // TODO - abstract into functions section or something
                    else if (name == "LoadFromImage" && elem is Image image)
                    {
                        image.Texture = Util.FetchTexture(SpaceCore.Instance.Helper.ModRegistry, val);
                        Console.WriteLine("meow! " + val + " " + image.Texture + "!");
                    }
                }
            }

            reader.MoveToElement();
            var children = reader.ReadSubtree();
            reader.Read();
            reader.MoveToContent();
            if (reader.NodeType == XmlNodeType.EndElement)
                return elem;

            if (!reader.IsEmptyElement)
            {
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    var child = ReadElement(reader, elemsById);
                    if (elem is Container container)
                        container.AddChild(child);
                    reader.MoveToContent();
                }
                reader.ReadEndElement();
            }

            return elem;
        }
    }
}
