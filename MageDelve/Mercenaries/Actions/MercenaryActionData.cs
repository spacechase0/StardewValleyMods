using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MageDelve.Mercenaries.Effects;
using Newtonsoft.Json.Linq;

namespace MageDelve.Mercenaries.Actions
{
    public class MercenaryActionData
    {
        public string Id { get; set; }
        public string ActionType { get; set; }

        public int Priority { get; set; }
        public int Weight { get; set; } = 1;

        public JObject Parameters { get; set; } = new();

        public string AdditionalConditions { get; set; }

        public Dictionary<string, float> Cooldowns { get; set; } = new();
        public float ActionLength { get; set; }

        public static Dictionary<string, Func<Mercenary, MercenaryActionData, bool>> ActionTypes { get; internal set; } = new();
    }

    public class MeleeAttackMercenaryActionParameters
    {
        public int MaxEngagementRadius { get; set; } = 10;
        public int MinIgnoreRadius { get; set; } = 20;

        public string MeleeWeaponId { get; set; }

        public List<string> WeaponEnchantments { get; set; } = new();
        public Dictionary<string, string> WeaponModData { get; set; } = new();
        public bool ShowWeapon = true;

        public List<string> MonsterBlockList { get; set; } = new();
        public List<string> MonsterAllowList { get; set; } // null means everything is allowed
    }
    public class SingleTargetEffectMercenaryActionParameters
    {
        public enum TargetType
        {
            Farmer,
            Party,
            Monster,
        }

        public TargetType Target { get; set; }
        public int MinimumHealthMissing { get; set; } = 0;
        public int WithinTiles { get; set; } = int.MaxValue;

        public List<EffectData> CasterEffects;
        public List<EffectData> TargetEffects;
    }

    public class PartyEffectMercenaryActionParameters
    {
        public int MinimumHealthMissing { get; set; } = 0;
        public int MinimumQualifyingPartyMembers { get; set; } = 1;
        public int WithinTiles { get; set; } = int.MaxValue;

        public List<EffectData> CasterEffects;
        public List<EffectData> TargetEffects;
    }
}
