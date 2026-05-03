using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stardew3D.DataModels;

internal class InteractionAreaCreationConverter : JsonConverter
{
    internal static Dictionary<string, Func<InteractionArea>> creationFuncs = new()
    {
        { new BoxInteractionArea().Type, () => new BoxInteractionArea() },
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

        var ret = creationFuncs.TryGetValue(type, out var creator) ? creator() : null;
        serializer.Populate(reader, ret);

        return ret;
    }

    public override bool CanConvert(Type type)
    {
        return type.IsAssignableTo(typeof(InteractionArea));
    }
}
