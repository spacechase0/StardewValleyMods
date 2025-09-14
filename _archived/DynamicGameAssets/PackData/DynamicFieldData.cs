using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceShared;

namespace DynamicGameAssets.PackData
{
    public class DynamicFieldData
    {
        public Dictionary<string, string> Conditions { get; set; } = new();

        internal ContentPatcher.IManagedConditions ConditionsObject;

        [JsonExtensionData]
        public Dictionary<string, JToken> Fields { get; set; } = new();

        public bool Check(BasePackData parent)
        {
            //if ( ConditionsObject == null )
            {
                var conds = new Dictionary<string, string>();
                foreach (var cond in this.Conditions)
                {
                    (string key, string value) = cond;
                    foreach (var opt in parent.pack.configIndex)
                    {
                        string val = parent.pack.currConfig.Values[opt.Key].ToString();

                        if (key == opt.Key)
                        { // this is one we should handle ourselves.
                            if (val != value)
                                return false;
                            goto DontAdd;
                        }

                        if (parent.pack.configIndex[opt.Key].ValueType == ConfigPackData.ConfigValueType.String)
                            val = "'" + val + "'";

                        key = key.Replace("{{" + opt.Key + "}}", val);
                        value = value.Replace("{{" + opt.Key + "}}", val);
                    }
                    conds.Add(key, value);
DontAdd:;
                }

                this.ConditionsObject = Mod.instance.cp.ParseConditions(
                    Mod.instance.ModManifest,
                    conds,
                    parent.pack.conditionVersion,
                    parent.pack.smapiPack.Manifest.Dependencies?.Select((d) => d.UniqueID)?.ToArray() ?? Array.Empty<string>()
                );
            }
            if (!this.ConditionsObject.IsValid)
            {
                string id = parent.ToString();
                if (parent is CommonPackData cpd)
                    id += $"[{cpd.ID}]";
                Log.Error("Invalid conditions object for " + id + "! Error from CP: " + this.ConditionsObject.ValidationError);
            }
            if (this.ConditionsObject.IsMatch)
                return true;
            return false;
        }

        public void Apply(BasePackData obj_)
        {
            foreach (var singleField in this.Fields)
            {
                string Field = singleField.Key;
                JToken Data = singleField.Value;
                Log.Trace( "parsing:"+Field+" and "+Data );
                if (Data.ToString().Contains("{{"))
                {
                    string strData = Data.ToString();
                    foreach (var opt in obj_.pack.configIndex)
                    {
                        string val = obj_.pack.currConfig.Values[opt.Key].ToString();
                        strData = strData.Replace("{{" + opt.Key + "}}", val);
                    }
                    Data = JToken.Parse(strData);
                }
                object obj = obj_;

                string[] fields = Field.Split('.');

                // Find the place to apply it to.
                object lastObj = null;
                PropertyInfo lastProp = null;
                object lastInd = null;
                int fCount = 0;
                foreach (string field_ in fields)
                {
                    string field = field_;

                    loopBeginning:
                    // Prepare index value
                    object ind = null;
                    if (field.Contains('['))
                    {
                        int indStart = field.IndexOf('[') + 1;
                        int indEnd = field.IndexOf(']');
                        string indStr = field.Substring(indStart, indEnd - indStart);
                        if (int.TryParse(indStr, out int result))
                            ind = result; // For arrays
                        else
                            ind = indStr; // For dictionaries

                        field = field.Substring(0, indStart - 1);
                    }

                    // Get the property the field refers to
                    var prop = obj.GetType().GetProperty(field);
                    if (prop == null)
                    {
                        // This hack is to let you use dynamic fields on extension data fields (such as the PortraitFor field in PicturePortraits).
                        if (fCount == 0)
                        {
                            field = $"ExtensionData[{field}]";
                            goto loopBeginning; // Evil, I know. Whatever
                        }

                        throw new ArgumentException($"No such property '{field}' on {obj}");
                    }
                    else
                    {
                        lastObj = obj;
                        obj = prop.GetValue(obj);
                    }

                    // Direct indices to next field
                    if (ind is int indI && obj is Array arr)
                    {
                        if (arr.Length <= indI)
                            throw new ArgumentException($"No such index '{indI}' in array '{field}'");
                        obj = arr.Cast<object>().ElementAt(indI);
                    }
                    if (ind is int indI2 && obj is System.Collections.IList list)
                    {
                        if (list.Count <= indI2)
                            throw new ArgumentException($"No such index '{indI2}' in array '{field}'");
                        obj = list[indI2];
                    }
                    if (fCount < fields.Length - 1)
                    {
                        if (ind != null && obj is System.Collections.IDictionary dict)
                        {
                            if (!dict.Contains(ind))
                                throw new ArgumentException($"No such key '{ind}' in dictionary '{field}'");
                            obj = dict[ind];
                        }
                    }

                    lastProp = prop;
                    lastInd = ind;
                    ++fCount;
                }

                // Apply it
                if (lastInd == null)
                {
                    if (lastProp.PropertyType == typeof(int))
                        lastProp.SetValue(lastObj, int.Parse((string)Data));
                    else if (lastProp.PropertyType == typeof(float))
                        lastProp.SetValue(lastObj, float.Parse((string)Data));
                    else if (lastProp.PropertyType == typeof(bool))
                        lastProp.SetValue(lastObj, bool.Parse((string)Data));
                    else if (lastProp.PropertyType == typeof(string))
                        lastProp.SetValue(lastObj, (string)Data);
                    else if (Nullable.GetUnderlyingType(lastProp.PropertyType) != null)
                    {
                        if (lastProp.PropertyType == typeof(int?))
                            lastProp.SetValue(lastObj, int.Parse((string)Data));
                        else if (lastProp.PropertyType == typeof(float?))
                            lastProp.SetValue(lastObj, float.Parse((string)Data));
                        else
                            lastProp.SetValue(lastObj, Data);
                    }
                    else if (!lastProp.PropertyType.IsPrimitive && Data == null)
                        lastProp.SetValue(lastObj, null);
                    else if (!lastProp.PropertyType.IsPrimitive && lastProp.PropertyType.IsArray)
                        lastProp.SetValue(lastObj, (Data as JArray).ToObject(lastProp.PropertyType));
                    else if (!lastProp.PropertyType.IsPrimitive)
                        lastProp.SetValue(lastObj, (Data as JObject).ToObject(lastProp.PropertyType));
                    else
                        throw new ArgumentException($"Unsupported type {lastProp.PropertyType} {Data.GetType()}");
                }
                else
                {
                    object setVal = null;
                    if (Data.Type == JTokenType.Integer)
                        setVal = int.Parse((string)Data);
                    else if (Data.Type == JTokenType.Float)
                        setVal = float.Parse((string)Data);
                    else if (Data.Type == JTokenType.Boolean)
                        setVal = bool.Parse((string)Data);
                    else if (Data.Type == JTokenType.String)
                        setVal = (string)Data;
                    else
                    {
                        Type t = null;
                        if (obj is Array arr2)
                            t = arr2.GetType().GetElementType();
                        else if (obj is System.Collections.IList list2)
                            t = list2.GetType().GenericTypeArguments[0];
                        else if (obj is System.Collections.IDictionary dict2)
                            t = dict2.GetType().GenericTypeArguments[1];
                        else
                            throw new ArgumentException($"Unsupported type {obj} for data object w/ index");
                        setVal = (Data as JObject).ToObject(t);
                    }

                    if (lastInd is int indI && obj is Array arr)
                    {
                        if (arr.Length <= indI)
                            throw new ArgumentException($"No such index '{indI}' in array {arr}");
                        arr.SetValue(setVal, indI);
                    }
                    else if (lastInd is int indI2 && obj is System.Collections.IList list)
                    {
                        if (list.Count <= indI2)
                            throw new ArgumentException($"No such index '{indI2}' in list '{list}'");
                        list[indI2] = setVal;
                    }
                    else if (lastInd != null && obj is System.Collections.IDictionary dict)
                    {
                        if (setVal == null)
                            dict.Remove(lastInd);
                        else
                            dict[lastInd] = setVal;
                    }
                    else
                        throw new NotImplementedException($"Not implemented: Setting index {lastProp} {lastInd} {lastObj} {obj} {Data} {Data.GetType()}");
                }
            }
        }
    }
}
