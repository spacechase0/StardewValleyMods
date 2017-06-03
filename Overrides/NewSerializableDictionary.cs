using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SpaceCore.Overrides
{
    //[XmlType(TypeName = "NewSerializableDictionary", IncludeInSchema = true)]
    public class NewSerializableDictionary< TKey, TValue > : SerializableDictionary< TKey, TValue >, IXmlSerializable
    {
        private Type[] types = new Type[]
    {
      typeof (Tool),
      typeof (StardewValley.Monsters.Crow),
      typeof (Duggy),
      typeof (Fireball),
      typeof (Ghost),
      typeof (GreenSlime),
      typeof (LavaCrab),
      typeof (RockCrab),
      typeof (ShadowGuy),
      typeof (SkeletonWarrior),
      typeof (Child),
      typeof (Pet),
      typeof (Dog),
      typeof (StardewValley.Characters.Cat),
      typeof (Horse),
      typeof (SquidKid),
      typeof (Grub),
      typeof (Fly),
      typeof (DustSpirit),
      typeof (Bug),
      typeof (BigSlime),
      typeof (BreakableContainer),
      typeof (MetalHead),
      typeof (ShadowGirl),
      typeof (Monster),
      typeof (JunimoHarvester),
      typeof (TerrainFeature)
    };

        public NewSerializableDictionary()
        {
        }

        public new void ReadXml(XmlReader reader)
        {
            XmlSerializer xmlSerializer1 = new XmlSerializer(typeof(TKey));
            XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(TValue), types.Concat(Entoarox.Framework.EntoFramework.GetTypeRegistry().GetInjectedTypes()).ToArray());
            int num = reader.IsEmptyElement ? 1 : 0;
            reader.Read();
            if (num != 0)
                return;
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");
                TKey key = (TKey)xmlSerializer1.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("value");
                TValue obj = (TValue)xmlSerializer2.Deserialize(reader);
                reader.ReadEndElement();
                this.Add(key, obj);
                reader.ReadEndElement();
                int content = (int)reader.MoveToContent();
            }
            reader.ReadEndElement();
        }



        public new void WriteXml(XmlWriter writer)
        {
            XmlSerializer xmlSerializer1 = new XmlSerializer(typeof(TKey));
            XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(TValue), types.Concat( Entoarox.Framework.EntoFramework.GetTypeRegistry().GetInjectedTypes()).ToArray());
            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                xmlSerializer1.Serialize(writer, (object)key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                TValue obj = this[key];
                xmlSerializer2.Serialize(writer, (object)obj);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }
}
