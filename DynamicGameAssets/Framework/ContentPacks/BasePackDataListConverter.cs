using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DynamicGameAssets.PackData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceShared;

namespace DynamicGameAssets.Framework.ContentPacks
{
    internal class BasePackDataListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<BasePackData>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jtoken = JToken.Load(reader);
            if (jtoken is JArray array)
            {
                List<BasePackData> ret = new List<BasePackData>();

                int i = 0;
                foreach (var entry in array)
                {
                    var obj = (JObject)entry;

                    var typeProp = obj.Properties().FirstOrDefault(prop => prop.Name == "$ItemType");
                    if (typeProp == null)
                    {
                        Log.Error("No $ItemType prop @ " + reader.Path + "/" + i + "!");
                        continue;
                    }

                    var actualType = Type.GetType("DynamicGameAssets.PackData." + typeProp.Value + "PackData");
                    if (actualType == null)
                    {
                        Log.Error("Invalid $ItemType prop @ " + reader.Path + "/" + i + "! (" + typeProp.Value + ")");
                        continue;
                    }

                    ret.Add((BasePackData)entry.ToObject(actualType, serializer));
                    ++i;
                }

                return ret;
            }
            else
            {
                Log.Error("Must have array here! " + reader.Path);
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JArray jarray = new JArray();
            foreach (var val in (List<BasePackData>)value)
            {
                var toAdd = (JObject)JToken.FromObject(val);
                List<string> toRemove = new();
                foreach (var prop in toAdd)
                {
                    var cprop = val.GetType().GetProperty(prop.Key);
                    var defAttr = cprop.GetCustomAttribute(typeof(DefaultValueAttribute));
                    if (defAttr != null)
                    {
                        if ((defAttr as DefaultValueAttribute).Value == null)
                        {
                            if (prop.Value.Type == JTokenType.Null)
                                toRemove.Add(prop.Key);
                            continue;
                        }

                        bool same = (defAttr as DefaultValueAttribute).Value.Equals(prop.Value.ToObject((defAttr as DefaultValueAttribute).Value.GetType()));
                        if (same)
                        {
                            toRemove.Add(prop.Key);
                            continue;
                        }
                    }

                    var ignAttr = cprop.GetCustomAttribute(typeof(JsonIgnoreAttribute));
                    if (ignAttr != null)
                    {
                        toRemove.Add(prop.Key);
                        continue;
                    }

                    var cmeth = val.GetType().GetMethod("ShouldSerialize" + prop.Key);
                    if (cmeth != null && !(cmeth.Invoke(val, Array.Empty<object>()) as bool?).Value)
                    {
                        toRemove.Add(prop.Key);
                        continue;
                    }
                }
                foreach (string prop in toRemove)
                    toAdd.Remove(prop);

                toAdd.AddFirst(new JProperty("$ItemType", val.GetType().Name.ToString().Substring(0, val.GetType().Name.IndexOf("PackData"))));
                jarray.Add(toAdd);
            }
            serializer.Serialize(writer, jarray);
        }
    }
}
