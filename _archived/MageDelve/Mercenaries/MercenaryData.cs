using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MageDelve.Mercenaries.Actions;
using MageDelve.Mercenaries.Effects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceShared;

namespace MageDelve.Mercenaries
{
    public class MercenaryData
    {
        public string ID { get; set; }

        public string CanRecruit { get; set; } = "TRUE";
        public int RecruitCost { get; set; } = 0;
        public List<EffectData> DismissEffects { get; set; }

        public string CurrentDialogueString { get; set; }

        public List<MercenaryActionData> Actions { get; set; } = new();

        [JsonIgnore]
        internal List<List<MercenaryActionData>> ActionsByPriority { get; set; }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext ctx)
        {
            try
            {
                Dictionary<int, List<MercenaryActionData>> actionsDict = new();
                foreach (var action in Actions)
                {
                    if (!actionsDict.TryGetValue(action.Priority, out var actionsList))
                        actionsDict.Add(action.Priority, actionsList = new());
                    actionsList.Add(action);
                }

                var actionsByPriority = actionsDict.ToList();
                actionsByPriority.Sort( (a,b) => b.Key - a.Key );
                ActionsByPriority = actionsByPriority.Select(kvp => kvp.Value).ToList();
            }
            catch (Exception e)
            {
                Log.Error($"Error post-deserializing mercenary data {ID}: {e}");
            }
        }
    }
}
