using System.Collections.Generic;

namespace CustomCritters.Framework.CritterData
{
    internal class BehaviorModel
    {
        public string Type { get; set; }
        public float Speed { get; set; }
        public List<BehaviorPatrolPoint> PatrolPoints { get; set; } = new();
        public int PatrolPointDelay { get; set; }
        public int PatrolPointDelayAddRandom { get; set; }
    }
}
