using System.ComponentModel;
using DynamicGameAssets.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpaceShared;
using StardewValley;
using StardewValley.Tools;

namespace DynamicGameAssets.PackData
{
    public class MeleeWeaponPackData : CommonPackData
    {
        public enum WeaponType
        {
            Dagger = MeleeWeapon.dagger,
            Club = MeleeWeapon.club,
            Sword = MeleeWeapon.defenseSword,
        }

        [JsonIgnore]
        public string Name => this.pack.smapiPack.Translation.Get($"melee-weapon.{this.ID}.name");

        [JsonIgnore]
        public string Description => this.pack.smapiPack.Translation.Get($"melee-weapon.{this.ID}.description");

        public string Texture { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WeaponType Type { get; set; }

        public int MinimumDamage { get; set; }
        public int MaximumDamage { get; set; }

        [DefaultValue(0.0)]
        public double Knockback { get; set; }

        [DefaultValue(0)]
        public int Speed { get; set; }

        [DefaultValue(0)]
        public int Accuracy { get; set; }

        [DefaultValue(0)]
        public int Defense { get; set; }

        [DefaultValue(0)]
        public int ExtraSwingArea { get; set; }

        [DefaultValue(0.0)]
        public double CritChance { get; set; }

        [DefaultValue(0.0)]
        public double CritMultiplier { get; set; }

        [DefaultValue(true)]
        public bool CanTrash { get; set; } = true;


        public override TexturedRect GetTexture()
        {
            return this.pack.GetTexture(this.Texture, 16, 16);
        }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems((item) =>
            {
                if (item is CustomMeleeWeapon cweapon)
                {
                    if (cweapon.SourcePack == this.pack.smapiPack.Manifest.UniqueID && cweapon.Id == this.ID)
                        return null;
                }
                return item;
            });
        }

        public override Item ToItem()
        {
            return new CustomMeleeWeapon(this);
        }
    }
}
