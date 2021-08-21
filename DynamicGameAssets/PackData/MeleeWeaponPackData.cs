using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
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

        public string Name => parent.smapiPack.Translation.Get( $"melee-weapon.{ID}.name" );
        public string Description => parent.smapiPack.Translation.Get( $"melee-weapon.{ID}.description" );

        public string Texture { get; set; }

        public WeaponType Type { get; set; }
        public int MinimumDamage { get; set; }
        public int MaximumDamage { get; set; }
        public double Knockback { get; set; }
        public int Speed { get; set; }
        public int Accuracy { get; set; }
        public int Defense { get; set; }
        public int ExtraSwingArea { get; set; }
        public double CritChance { get; set; }
        public double CritMultiplier { get; set; }

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
