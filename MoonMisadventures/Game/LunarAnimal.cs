using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;

namespace MoonMisadventures.Game
{
    public enum LunarAnimalType
    {
        Cow,
        Chicken,
    }

    public class LunarAnimal : FarmAnimal
    {
        public readonly NetEnum<LunarAnimalType> lunarType = new();

        public static string GetVanillaTypeFromLunarType( LunarAnimalType type )
        {
            switch ( type )
            {
                case LunarAnimalType.Cow:
                    return "White Cow";
                case LunarAnimalType.Chicken:
                    return "White Chicken";
            }

            throw new ArgumentException( "Invalid lunar animal type" );
        }

        public LunarAnimal() { }
        public LunarAnimal( LunarAnimalType type, Vector2 pos, long id )
        :   base( GetVanillaTypeFromLunarType( type ), id, 0 )
        {
            lunarType.Value = type;
            position.Value = pos;

            switch ( type )
            {
                case LunarAnimalType.Cow:
                    displayName = "Lunar Cow";
                    break;
                case LunarAnimalType.Chicken:
                    displayName = "Lunar Chicken";
                    break;
            }
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields( lunarType );
        }

        public override void reloadData()
        {
            base.reloadData();
            Sprite = new AnimatedSprite( Mod.instance.Helper.Content.GetActualAssetKey( "assets/cow.png" ), 0, 32, 32 );
            Sprite.Texture.Name = "SC0_MM/Cow";
            fullnessDrain.Value *= 2;
            happinessDrain.Value *= 2;
        }
    }
}
