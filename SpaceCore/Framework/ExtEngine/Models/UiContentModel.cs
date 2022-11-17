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
        public string UiMarkup
        {
            get
            {
                if (UiFile == null)
                    return null;
                return File.ReadAllText(Util.FetchFullPath(SpaceCore.Instance.Helper.ModRegistry, UiFile));
            }
        }

        [JsonIgnore]
        public string Script
        {
            get
            {
                if (ScriptFile == null)
                    return "";
                return File.ReadAllText(Util.FetchFullPath(SpaceCore.Instance.Helper.ModRegistry, ScriptFile));
            }
        }

        public Element CreateUi( out List<Element> allElements )
        {
            string pack = UiFile.Substring(0, UiFile.IndexOf('/'));

            string actualMarkup = ExtensionEngine.SubstituteTokens(pack, UiMarkup);
            using TextReader tr = new StringReader(actualMarkup);
            using var xr = XmlReader.Create(tr);
            xr.Read();
            return new UiDeserializer().Deserialize(pack, xr, out allElements);
        }

        public string UiFile { get; set; }
        public string ScriptFile { get; set; }

        public bool DefaultClosable { get; set; } = true;
    }
}
