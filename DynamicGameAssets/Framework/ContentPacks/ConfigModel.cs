using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynamicGameAssets.Framework.ContentPacks
{
    internal class ConfigModel
    {
        [JsonExtensionData]
        public Dictionary<string, JToken> Values = new();
    }
}
