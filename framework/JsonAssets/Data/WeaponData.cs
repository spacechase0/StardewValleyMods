using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JsonAssets.Framework;
using SpaceShared;
using StardewValley;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class WeaponData : DataNeedsIdWithTexture, ITranslatableItem
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string Description
        {
            get => descript;
            set => descript = value ?? " ";
        }
        private string descript = " ";

        public WeaponType Type { get; set; }

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

        /// <inheritdoc />
        public Dictionary<string, string> NameLocalization { get; set; } = new();

        /// <inheritdoc />
        public Dictionary<string, string> DescriptionLocalization { get; set; } = new();

        /// <inheritdoc />
        public string TranslationKey { get; set; }


        /*********
        ** Public methods
        *********/
        internal StardewValley.GameData.Weapons.WeaponData GetWeaponInformation()
        {
            var weapon = new StardewValley.GameData.Weapons.WeaponData()
            {
                Name = this.Name.FixIdJA("W"),
                DisplayName = this.LocalizedName(),
                Description = this.LocalizedDescription(),
                MinDamage = this.MinimumDamage,
                MaxDamage = this.MaximumDamage,
                Knockback = (float)this.Knockback,
                Speed = this.Speed,
                Precision = this.Accuracy,
                Defense = this.Defense,
                Type = (int)this.Type,
                MineBaseLevel = this.MineDropVar,
                MineMinLevel = this.MineDropMinimumLevel,
                AreaOfEffect = this.ExtraSwingArea,
                CritChance = (float)this.CritChance,
                CritMultiplier = (float)this.CritMultiplier,
                Texture = $"JA/Weapon/{this.Name.FixIdJA("W")}",
                SpriteIndex = 0,
            };
            return weapon;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.PurchaseRequirements ??= new List<string>();
            this.AdditionalPurchaseData ??= new List<PurchaseData>();
            this.NameLocalization ??= new();
            this.DescriptionLocalization ??= new();

            this.PurchaseRequirements.FilterNulls();
            this.AdditionalPurchaseData.FilterNulls();
        }
    }
}
