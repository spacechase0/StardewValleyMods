using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using Newtonsoft.Json;
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
        public string Name => parent.smapiPack.Translation.Get( $"melee-weapon.{ID}.name" );
        [JsonIgnore]
        public string Description => parent.smapiPack.Translation.Get( $"melee-weapon.{ID}.description" );

        public string Texture { get; set; }

        public WeaponType Type { get; set; }
        public int MinimumDamage { get; set; }
        public int MaximumDamage { get; set; }
        [DefaultValue( 0.0 )]
        public double Knockback { get; set; }
        [DefaultValue( 0 )]
        public int Speed { get; set; }
        [DefaultValue( 0 )]
        public int Accuracy { get; set; }
        [DefaultValue( 0 )]
        public int Defense { get; set; }
        [DefaultValue( 0 )]
        public int ExtraSwingArea { get; set; }
        [DefaultValue( 0.0 )]
        public double CritChance { get; set; }
        [DefaultValue( 0.0 )]
        public double CritMultiplier { get; set; }

        [DefaultValue( true )]
        public bool CanTrash { get; set; } = true;


        public override TexturedRect GetTexture()
        {
            return parent.GetTexture( Texture, 16, 16 );
        }

        public override void OnDisabled()
        {
            MyUtility.iterateAllItems( ( item ) =>
            {
                if ( item is CustomMeleeWeapon cweapon )
                {
                    if ( cweapon.SourcePack == parent.smapiPack.Manifest.UniqueID && cweapon.Id == ID )
                        return null;
                }
                return item;
            } );
        }

        public override Item ToItem()
        {
            return new CustomMeleeWeapon( this );
        }
    }
}
