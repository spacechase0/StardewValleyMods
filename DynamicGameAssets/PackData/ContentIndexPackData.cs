using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DynamicGameAssets.PackData
{
    public class ContentIndexPackData : BasePackData
    {
        public string ContentType { get; set; } // Only for format version 1
        public string FilePath { get; set; }
    }
}
