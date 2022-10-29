using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures.Game.Locations;
using Netcode;
using StardewValley;

namespace MoonMisadventures.Game.Items
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_AnimalGauntlets" )]
    public class AnimalGauntlets : Tool
    {
        public readonly NetRef< FarmAnimal > holding = new();

        public AnimalGauntlets()
        {
            this.Name = this.BaseName = "AnimalGauntlets";
            this.InstantUse = true;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields( holding );
        }

        protected override string loadDisplayName()
        {
            return I18n.Tool_AnimalGauntlets_Name();
        }

        protected override string loadDescription()
        {
            return I18n.Tool_AnimalGauntlets_Description();
        }

        public override string getDescription()
        {
            string str = base.getDescription();
            if ( holding.Value != null )
                str += $"\n{I18n.Tool_AnimalGauntlets_Holding()} {this.holding.Value.displayType}";
            return str;
        }

        public override bool canBeTrashed()
        {
            return true;
        }

        public override void DoFunction( GameLocation location, int x, int y, int power, Farmer who )
        {
            who.CanMove = true;
            who.UsingTool = false;
            who.canReleaseTool = true;

            if ( location is IAnimalLocation aloc )
            {
                if ( location is not LunarLocation )
                {
                    Game1.addHUDMessage( new HUDMessage( I18n.Tool_AnimalGauntlets_MoonRequirement() ) );
                }

                var spot = Mod.instance.Helper.Input.GetCursorPosition();

                if ( holding.Value == null )
                {
                    foreach ( long key in aloc.Animals.Keys )
                    {
                        var animal = aloc.Animals[ key ];
                        if ( animal.GetCursorPetBoundingBox().Contains( spot.AbsolutePixels ) )
                        {
                            aloc.Animals.Remove( key );
                            holding.Value = animal;
                            break;
                        }
                    }
                }
                else
                {
                    if ( location.isTileLocationTotallyClearAndPlaceableIgnoreFloors( spot.Tile ) )
                    {
                        holding.Value.position.Value = spot.AbsolutePixels;
                        aloc.Animals.Add( holding.Value.myID.Value, holding.Value );
                        holding.Value = null;
                    }
                }
            }
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            spriteBatch.Draw( Assets.AnimalGauntlets, location, null, Color.White * transparency, 0, Vector2.Zero, scaleSize * 4, SpriteEffects.None, layerDepth );
        }

        public override Item getOne()
        {
            var ret = new AnimalGauntlets();
            // not sure how to clone the animal... only allow lunar for now
            if ( holding.Value is LunarAnimal lanimal )
            {
                var mp = Mod.instance.Helper.Reflection.GetField< Multiplayer >( typeof( Game1 ), "multiplayer" ).GetValue();
                var other = new LunarAnimal( lanimal.lunarType.Value, Vector2.Zero, mp.getNewID() );
                other.age.Value = lanimal.age.Value;
                other.currentProduce.Value = lanimal.age.Value;
                other.happiness.Value = lanimal.happiness.Value;
                other.fullness.Value = lanimal.fullness.Value;
                other.wasPet.Value = lanimal.wasPet.Value;
                ret.holding.Value = other;
            }
            ret._GetOneFrom( this );
            return ret;
        }
    }
}
