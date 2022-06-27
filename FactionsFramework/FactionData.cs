using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace FactionsFramework
{
    public class FactionData
    {
        [JsonIgnore]
        public Texture2D Icon { get; set; }
        [JsonIgnore]
        public Texture2D NeutralRankIcon { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public bool Visible { get; set; } = true;

        public bool TrackedPerPlayer { get; set; } = true;

        public int LowerLimit { get; set; } = -10000;
        public int UpperLimit { get; set; } = 10000;
        public int DefaultValue { get; set; } = 0;
        public int TrendTowardsDefaultPerDay { get; set; } = 0;

        public string NeutralRankName { get; set; }
        public string NeutralRankDescription { get; set; }
        public string NeutralRankIconPath { get; set; }

        public class RankData
        {
            [JsonIgnore]
            public Texture2D Icon { get; set; }

            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string IconPath { get; set; }

            public int Threshold { get; set; }

            [OnDeserialized]
            public void OnDeserialized(StreamingContext ctx)
            {
                Icon = Mod.instance.Helper.GameContent.Load<Texture2D>(IconPath);
            }
        }
        public Dictionary<string, RankData> Ranks { get; set; } = new();

        public Dictionary<string, int> MonsterKillAdjustments { get; set; } = new();
        public Dictionary<string, int> SpecialOrderAdjustments { get; set; } = new();
        public Dictionary<string, int> DialogueResponseAdjustments { get; set; } = new();

        [OnDeserialized]
        public void OnDeserialized(StreamingContext ctx)
        {
            Icon = Mod.instance.Helper.GameContent.Load<Texture2D>(IconPath);
            NeutralRankIcon = Mod.instance.Helper.GameContent.Load<Texture2D>(NeutralRankIconPath);
        }
    }
}
