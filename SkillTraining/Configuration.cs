using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace SkillTraining
{
    internal class Configuration
    {
        public int PricePerExperiencePoint { get; set; } = 10;
        public int MaxTrainableLevel { get; set; } = 7;
    }
}
