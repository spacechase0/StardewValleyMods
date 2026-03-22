using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stardew3D.DataModels;

internal class ModelDataCreationConverter : JsonConverter
{
    internal static Dictionary<string, Func<ModelData>> creationFuncs = new()
    {
        { new MenuModelData().Type, () => new MenuModelData() },
    };

    public override bool CanWrite => false;
    public override bool CanRead => true;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var data = JObject.Load(reader);
        string type = data.TryGetValue("Type", out var value) ? value.ToString() : "";

        var ret = creationFuncs.TryGetValue(type, out var creator) ? creator() : new ModelData();
        serializer.Populate(reader, ret);

        return ret;
    }

    public override bool CanConvert(Type type)
    {
        return type.IsAssignableTo(typeof(ModelData));
    }
}
