using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheftOfTheWinterSTar
{
    public class SaveData
    {
        public ArenaStage ArenaStage { get; set; } = ArenaStage.NotTriggered;
        public bool DidProjectilePuzzle { get; set; } = false;
        public bool BeatBoss { get; set; } = false;
    }
}
