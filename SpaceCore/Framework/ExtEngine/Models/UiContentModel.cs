using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SpaceCore.UI;
using SpaceShared;

namespace SpaceCore.Framework.ExtEngine.Models
{
    internal class UiContentModel
    {
        [JsonIgnore]
        public string UiMarkup { get; set; }

        [JsonIgnore]
        public string Script { get; set; }

        public Element CreateUi( out Dictionary<string, List<Element>> elemsById )
        {
            using TextReader tr = new StringReader(UiMarkup);
            using var xr = XmlReader.Create(tr);
            xr.Read();
            return new UiDeserializer().Deserialize(xr, out elemsById);
        }

        public string UiFile { get; set; }
        public string ScriptFile { get; set; }

        public bool DefaultClosable { get; set; } = true;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (UiFile != null)
            {
                UiMarkup = File.ReadAllText(Util.FetchFullPath(SpaceCore.Instance.Helper.ModRegistry, UiFile));
            }
            if (ScriptFile != null)
            {
                Script = File.ReadAllText(Util.FetchFullPath(SpaceCore.Instance.Helper.ModRegistry, ScriptFile));
            }
        }
    }
}
