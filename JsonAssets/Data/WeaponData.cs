using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpaceShared;
using StardewValley;
using StardewValley.Tools;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class WeaponData : DataNeedsIdWithTexture
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Type_
        {
            Dagger = MeleeWeapon.dagger,
            Club = MeleeWeapon.club,
            Sword = MeleeWeapon.defenseSword,
        }

        public string Description { get; set; }
        public Type_ Type { get; set; }

        public int MinimumDamage { get; set; }
        public int MaximumDamage { get; set; }
        public double Knockback { get; set; }
        public int Speed { get; set; }
        public int Accuracy { get; set; }
        public int Defense { get; set; }
        public int MineDropVar { get; set; }
        public int MineDropMinimumLevel { get; set; }
        public int ExtraSwingArea { get; set; }
        public double CritChance { get; set; }
        public double CritMultiplier { get; set; }

        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Pierre";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public bool CanTrash { get; set; } = true;

        public Dictionary<string, string> NameLocalization = new();
        public Dictionary<string, string> DescriptionLocalization = new();

        public string LocalizedName()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            /*if (currLang == LocalizedContentManager.LanguageCode.en)
                return Name;*/
            if (this.NameLocalization == null || !this.NameLocalization.ContainsKey(currLang.ToString()))
                return this.Name;
            return this.NameLocalization[currLang.ToString()];
        }

        public string LocalizedDescription()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            /*if (currLang == LocalizedContentManager.LanguageCode.en)
                return Description;*/
            if (this.DescriptionLocalization == null || !this.DescriptionLocalization.ContainsKey(currLang.ToString()))
                return this.Description;
            return this.DescriptionLocalization[currLang.ToString()];
        }

        public int GetWeaponId() { return this.Id; }

        internal string GetWeaponInformation()
        {
            return $"{this.Name}/{this.LocalizedDescription()}/{this.MinimumDamage}/{this.MaximumDamage}/{this.Knockback}/{this.Speed}/{this.Accuracy}/{this.Defense}/{(int)this.Type}/{this.MineDropVar}/{this.MineDropMinimumLevel}/{this.ExtraSwingArea}/{this.CritChance}/{this.CritMultiplier}/{this.LocalizedName()}";
        }
    }
}
