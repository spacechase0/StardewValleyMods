using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures;
using MoonMisadventures.Game.Locations;
using Netcode;
using StardewValley;

namespace StardewValley.Tools
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
            NetFields.AddField( holding, nameof( holding ) );
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

            var spot = Mod.instance.Helper.Input.GetCursorPosition();

            if ( holding.Value == null )
            {
                foreach ( long key in location.Animals.Keys )
                {
                    var animal = location.Animals[ key ];
                    if ( animal.GetCursorPetBoundingBox().Contains( spot.AbsolutePixels ) )
                    {
                        location.Animals.Remove( key );
                        holding.Value = animal;
                        break;
                    }
                }
            }
            else
            {
                if ( location.isTilePlaceable( spot.Tile ) )
                {
                    holding.Value.position.Value = spot.AbsolutePixels;
                    location.Animals.Add( holding.Value.myID.Value, holding.Value );
                    holding.Value = null;
                }
            }
        }

        /*
        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            spriteBatch.Draw( Assets.AnimalGauntlets, location, null, Color.White * transparency, 0, Vector2.Zero, scaleSize * 4, SpriteEffects.None, layerDepth );
        }
        */

        protected override Item GetOneNew()
        {
            return new AnimalGauntlets();
        }

        protected override void GetOneCopyFrom(Item source)
        {
            base.GetOneCopyFrom(source);

            var gauntlets = source as AnimalGauntlets;

            if (gauntlets.holding.Value != null)
            {
                var animal = new FarmAnimal();

                var a = gauntlets.holding.Value.NetFields.GetFields().ToList();
                var b = animal.NetFields.GetFields().ToList();
                for (int i = 0; i < a.Count; ++i)
                {
                    using MemoryStream ms = new();
                    using BinaryWriter bw = new(ms);
                    a[i].WriteFull(bw);

                    bw.Flush();
                    ms.Position = 0;
                    using BinaryReader br = new(ms);
                    b[i].ReadFull(br, new());
                }

                holding.Value = animal;
            }
        }
    }
}
