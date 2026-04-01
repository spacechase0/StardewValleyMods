using System.Collections;
using System.ComponentModel.Design;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceShared;
using SpaceShared.Attributes;
using StardewValley.Extensions;

namespace XlsxSupport;

[HasHarmony]
public partial class Mod : BaseMod<Mod>
{
    protected override void ModEntry()
    {
    }
}

[HarmonyPatch]
public static class XlsxReaderPatch
{
    public static IEnumerable<MethodInfo> TargetMethods()
    {
        var type = AccessTools.TypeByName("StardewModdingAPI.Toolkit.Serialization.JsonHelper");
        var method = type.GetMethod("ReadJsonFileIfExists");
        yield return method.MakeGenericMethod(typeof(Dictionary<string, string>)); // Harmony should let this apply to them all? Hopefully?
    }

    public static void Postfix(object __instance, MethodInfo __originalMethod, string fullPath, ref object result, ref bool __result)
    {
        if (__result == true)
            return;

        fullPath += ".xlsx";
        if (!File.Exists(fullPath))
            return;

        JsonSerializerSettings ___JsonSettings = (JsonSerializerSettings) AccessTools.Property("StardewModdingAPI.Toolkit.Serialization.JsonHelper:JsonSettings").GetValue(__instance);

        using FileStream fs = File.OpenRead(fullPath);
        MakeMiniExcelRedirectWhenNecessaryPatch.settings = ___JsonSettings;

        Type type = __originalMethod.GetGenericArguments()[0];
        if (type.IsAssignableTo(typeof(IDictionary)) && type.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.IsConstructedGenericType &&
                i.GetGenericTypeDefinition() == typeof(IDictionary<,>) &&
                i.GenericTypeArguments[0] == typeof(string)) is Type dictInterface)
        {
            MethodInfo rawQueryMethod = typeof(MiniExcel).GetMethod("Query", BindingFlags.Static | BindingFlags.Public, [typeof(Stream), typeof(string), typeof(ExcelType), typeof(string), typeof(IConfiguration), typeof(bool)]);
            MethodInfo queryMethod = rawQueryMethod.MakeGenericMethod(dictInterface.GenericTypeArguments[1]);

            IDictionary dict = (IDictionary) type.GetConstructor([]).Invoke([]);
            foreach (var sheet in fs.GetSheetNames())
            {
                dict.Add(sheet, queryMethod.Invoke(null, [fs, sheet, ExcelType.XLSX, "A1", null, true]));
            }

            result = dict;
            __result = true;
            return;
        }
        else if (type.IsAssignableTo(typeof(IList)) && type.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.IsConstructedGenericType &&
                i.GetGenericTypeDefinition() == typeof(IList<>)) is Type listInterface)
        {
            MethodInfo rawQueryMethod = typeof(MiniExcel).GetMethod("Query", BindingFlags.Static | BindingFlags.Public, [typeof(Stream), typeof(string), typeof(ExcelType), typeof(string), typeof(IConfiguration), typeof(bool)]);
            MethodInfo queryMethod = rawQueryMethod.MakeGenericMethod(listInterface.GenericTypeArguments[0]);

            IList list = (IList) type.GetConstructor([]).Invoke([]);
            foreach (var sheet in fs.GetSheetNames())
            {
                list.Add(queryMethod.Invoke(null, [fs, sheet, ExcelType.XLSX, "A1", null, true]));
            }

            result = list;
            __result = true;
            return;
        }
        else
        {
            result = type.GetConstructor([]).Invoke([]);
            foreach (var sheet in fs.GetSheetNames())
            {
                FieldInfo f = type.GetField(sheet);
                PropertyInfo p = type.GetProperty(sheet);
                Type t = f?.FieldType ?? p?.PropertyType;
                if (t == null)
                    continue;

                object val;
                if (!t.IsAssignableTo(typeof(IDictionary)) && !t.IsAssignableTo(typeof(IList)))
                {
                    val = Convert.ChangeType(fs.Query<Holder<string>>(sheet).FirstOrDefault()?.Value, t);
                }
                else
                {
                    MethodInfo rawQueryMethod = typeof(MiniExcel).GetMethod("Query", BindingFlags.Static | BindingFlags.Public, [typeof(Stream), typeof(string), typeof(ExcelType), typeof(string), typeof(IConfiguration), typeof(bool)]);
                    MethodInfo queryMethod = rawQueryMethod.MakeGenericMethod(t);

                    val = (queryMethod.Invoke(null, [fs, sheet, ExcelType.XLSX, "A1", null, true]) as IEnumerable).GetEnumerator().Current;
                }

                if (f != null) f.SetValue(result, val);
                if (p != null) p.SetValue(result, val);
            }

            __result = true;
            return;
        }

    }
}

[HarmonyPatch(typeof(TypeHelper), nameof(TypeHelper.TypeMappingImpl))]
public static class MakeMiniExcelRedirectWhenNecessaryPatch
{
    internal static JsonSerializerSettings settings;
    public static Exception Finalizer(MethodInfo __originalMethod, Exception __exception, object v, ExcelColumnInfo pInfo, object itemValue, ref object __result)
    {
        if (__exception is not InvalidCastException)
            return __exception;

        try
        {
            var tok = JToken.Parse(itemValue.ToString());
            object newVal = null;
            if (tok is JValue jval)
                newVal = jval.Value;
            else
                newVal = JsonConvert.DeserializeObject(itemValue.ToString(), pInfo.ExcludeNullableType, settings);
            pInfo.Property.SetValue(v, newVal);
            __result = newVal;
        }
        catch (Exception e)
        {
            return e;
        }

        return null;
    }
}
