using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI.Events;
using StardewValley;

namespace MoonMisadventures.Game.Items
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_AnimalGauntlets" )]
    public class AnimalGauntlets : Tool
    {
        public readonly NetRef< FarmAnimal > holding = new();

        public AnimalGauntlets()
        {
            this.Name = "AnimalGauntlets";
            this.InstantUse = true;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields( holding );
        }

        protected override string loadDisplayName()
        {
            return Mod.instance.Helper.Translation.Get( "item.animal-gauntlets.name" );
        }

        protected override string loadDescription()
        {
            return Mod.instance.Helper.Translation.Get( "item.animal-gauntlets.description" );
        }

        public override void DoFunction( GameLocation location, int x, int y, int power, Farmer who )
        {
            who.CanMove = true;
            who.UsingTool = false;
            who.canReleaseTool = true;

        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            spriteBatch.Draw( Assets.AnimalGauntlets, location, null, Color.White * transparency, 0, Vector2.Zero, scaleSize * 4, SpriteEffects.None, layerDepth );
        }

        public override Item getOne()
        {
            var ret = new AnimalGauntlets();
            // not sure how to clone the animal...
            ret._GetOneFrom( this );
            return ret;
        }
    }

    // Taken from an old WIP mod of mine
    internal class Holding
    {
        public Character character;
        public long animalKey;

        public void draw( SpriteBatch b, Vector2 pos )
        {
            character.Position = pos;
            character.draw( b );
            character.drawAboveAlwaysFrontLayer( b );
        }
    }

    internal class ThrownHeld
    {
        private Holding held;
        private GameLocation loc;
        private Vector2 start;
        private Vector2 pos;
        private Vector2 target;
        private int height;
        private float velZ;
        private int frame = 0;

        public ThrownHeld( Holding holding, GameLocation theLoc, Vector2 startPos, Vector2 targetPos )
        {
            held = holding;
            loc = theLoc;
            start = startPos;
            pos = startPos;
            target = targetPos;

            velZ = 16;

            Mod.instance.Helper.Events.GameLoop.UpdateTicked += update;
            Mod.instance.Helper.Events.Display.RenderedWorld += draw;
        }

        public void update( object sender, UpdateTickedEventArgs args )
        {
            height -= ( int ) velZ;
            velZ -= 0.5f;

            double angle1 = Math.Atan2(target.Y - start.Y, target.X - start.X);
            pos += ( target - start ) / 30;
            double angle2 = Math.Atan2(target.Y - start.Y, target.X - start.X);

            if ( Vector2.Distance( pos, target ) < 16 || Math.Abs( angle1 - angle2 ) > 1 )
            {
                held.character.yJumpOffset = 0;
                held.character.Position = target;
                if ( held.character is FarmAnimal animal && loc is IAnimalLocation animalLoc )
                {
                    animalLoc.Animals.Add( held.animalKey, animal );
                }
                else
                {
                    loc.characters.Add( held.character as NPC );
                }

                done();
            }
        }

        public void draw( object sender, RenderedWorldEventArgs args )
        {
            if ( Game1.currentLocation != loc )
                return;

            //held.character.yJumpOffset = height;
            held.draw( args.SpriteBatch, pos );
        }

        private void done()
        {
            Mod.instance.Helper.Events.GameLoop.UpdateTicked -= update;
            Mod.instance.Helper.Events.Display.RenderedWorld -= draw;
        }
    }
}
