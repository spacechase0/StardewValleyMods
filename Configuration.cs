using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherHungerMod
{
    public class Configuration
    {
        public int FullnessUiX = 10;
        public int FullnessUiY = 350;

        public int MaxFullness = 100;
        public double EdibilityMultiplier = 1;
        public double DrainPer10Min = 0.8;

        public int PositiveBuffThreshold = 80;
        public int NegativeBuffThreshold = 25;

        public int StarvationDamagePer10Min = 10;

        public int RelationshipHitForNotFeedingSpouse = 50;
    }
}
