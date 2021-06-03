using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SpaceShared;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;

namespace SpaceCore.Utilities
{
    // https://stackoverflow.com/a/21923200
    public class ItemListResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract( Type type )
        {
            if (type.IsSubclassOf(typeof(Item)))
            {
                JsonContract contract = base.CreateObjectContract(type);
                contract.Converter = new ItemConverter();
                return contract;
            }
            else if ( type == typeof( Rectangle ) )
            {
                JsonContract contract = base.CreateObjectContract(type);
                contract.Converter = new MyRectangleConverter();
                return contract;
            }

            return base.CreateContract( type );
        }
    }

    public class ItemListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var oldResolver = serializer.ContractResolver;
            serializer.ContractResolver = new ItemListResolver();
            try
            {
                JArray obj = JArray.Load(reader);
                List<Item> ret = new List<Item>();
                foreach ( var elem in obj )
                {
                    string id = (string) elem["$type"];

                    // Cross-platform fix, since for some reason Windows has a space but Mono doesn't.
                    if (Util.UsingMono)
                        id = id.Replace(", Stardew Valley", ", StardewValley");
                    else
                        id = id.Replace(", StardewValley", ", Stardew Valley");
                    
                    ret.Add(( Item ) elem.ToObject(Type.GetType(id), serializer));
                }
                return ret;
            }
            finally
            {
                serializer.ContractResolver = oldResolver;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var oldResolver = serializer.ContractResolver;
            serializer.ContractResolver = new ItemListResolver();
            try
            {
                // Couldn't get JArray to work correctly, and this was simpler
                // than figuring that out.
                writer.WriteStartArray();
                foreach (var elem in (System.Collections.IEnumerable)value)
                {
                    itemConverter.WriteJson(writer, elem, serializer);
                }
                writer.WriteEndArray();
            }
            finally
            {
                serializer.ContractResolver = oldResolver;
            }
        }

        private JsonConverter itemConverter = new ItemConverter();
    }
    
    public class ItemConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // JObject doesn't seem to use the custom converters.
            // I read somewhere that it creates its own serializer or something?
            // Anyways, that's why this doesn't use JObject, but everything else does
            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            writer.WriteValue($"{value.GetType().FullName}, {value.GetType().Assembly.GetName().Name}");
            foreach (FieldInfo field in value.GetType().GetFields())
            {
                // We check XmlIgnore because this is what the vanilla classes use
                // (And likely any custom ones as well, for vanilla save file compat.)
                if (field.GetCustomAttribute(typeof(XmlIgnoreAttribute)) != null)
                    continue;
                if (field.IsStatic)
                    continue;

                var val = field.GetValue(value);
                writer.WritePropertyName(field.Name);
                if (val == null)
                    writer.WriteNull();
                else
                    serializer.Serialize(writer, val);
            }
            foreach (PropertyInfo prop in value.GetType().GetProperties())
            {
                if (!prop.CanRead || prop.GetCustomAttribute(typeof(XmlIgnoreAttribute)) != null)
                    continue;
                if (prop.GetGetMethod().IsStatic)
                    continue;

                var val = prop.GetValue(value);
                //if (prop.GetValue(prop.Name) != null)
                //    continue;

                writer.WritePropertyName(prop.Name);
                if (val == null)
                    writer.WriteNull();
                else
                    serializer.Serialize(writer, val);
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            var o = JObject.Load(reader);

            
            string id = (string)o["$type"];

            // Cross-platform fix, since for some reason Windows has a space but Mono doesn't.
            if (Util.UsingMono)
                id = id.Replace(", Stardew Valley", ", StardewValley");
            else
                id = id.Replace(", StardewValley", ", Stardew Valley");

            var data = Type.GetType(id).GetConstructor(new Type[0]).Invoke(null);
            serializer.Populate(o.CreateReader(), data);

            return data;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsSubclassOf(typeof(Item));
        }

        private static int? GetTokenValue(JObject o, string tokenName)
        {
            JToken t;
            return o.TryGetValue(tokenName, StringComparison.InvariantCultureIgnoreCase, out t) ? (int)t : (int?)null;
        }
    }

    public class MyRectangleConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Rectangle? rect = value as Rectangle?;

            var o = new JObject();
            o.Add("X", rect.Value.X);
            o.Add("Y", rect.Value.Y);
            o.Add("Width", rect.Value.Width);
            o.Add("Height", rect.Value.Height);
            o.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            Rectangle rect = new Rectangle();
            rect.X = obj["X"].ToObject<Int32>();
            rect.Y = obj["Y"].ToObject<Int32>();
            rect.Width = obj["Width"].ToObject<Int32>();
            rect.Height = obj["Height"].ToObject<Int32>();
            return rect;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Rectangle);
        }

        private static int? GetTokenValue(JObject o, string tokenName)
        {
            JToken t;
            return o.TryGetValue(tokenName, StringComparison.InvariantCultureIgnoreCase, out t) ? (int)t : (int?)null;
        }
    }
}
