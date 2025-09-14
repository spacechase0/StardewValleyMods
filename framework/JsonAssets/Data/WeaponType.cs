using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StardewValley.Tools;

namespace JsonAssets.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WeaponType
    {
        Dagger = MeleeWeapon.dagger,
        Club = MeleeWeapon.club,
        Sword = MeleeWeapon.defenseSword
    }
}
