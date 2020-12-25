using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireArcadeGame.Objects
{
    public class Player : BaseObject
    {
        public float speed = 0.065f;
        public float turnSpeed = 0.045f;
        public float look = 0;

        public Player( World world )
        :   base( world )
        {
        }

        public override void Update()
        {
            var oldPos = Position;

            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.A ) )
            {
                look -= turnSpeed;
            }
            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.D ) )
            {
                look += turnSpeed;
            }
            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.Q ) )
            {
                Position.X += ( float ) Math.Cos( look - Math.PI / 2 ) * speed;
                Position.Z += ( float ) Math.Sin( look - Math.PI / 2 ) * speed;
            }
            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.E ) )
            {
                Position.X += ( float ) Math.Cos( look + Math.PI / 2 ) * speed;
                Position.Z += ( float ) Math.Sin( look + Math.PI / 2 ) * speed;
            }
            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.W ) )
            {
                Position.X += ( float ) Math.Cos( look ) * speed;
                Position.Z += ( float ) Math.Sin( look ) * speed;
            }
            if ( Game1.input.GetKeyboardState().IsKeyDown( Keys.S ) )
            {
                Position.X -= ( float ) Math.Cos( look ) * speed;
                Position.Z -= ( float ) Math.Sin( look ) * speed;
            }

            // Lazy implementation - would use something better if using a real engine
            if ( World.map.IsSolid( Position.X, Position.Z ) )
            {
                if ( !World.map.IsSolid( oldPos.X, Position.Z ) )
                    Position.X = oldPos.X;
                else if ( !World.map.IsSolid( Position.X, oldPos.Z ) )
                    Position.Z = oldPos.Z;
                else
                    Position = oldPos;
            }

            World.cam.pos = Position;
            World.cam.target = Position + new Vector3( ( float ) Math.Cos( look ), 0, ( float ) Math.Sin( look ) );

            effect.DirectionalLight0.Direction = World.cam.target - Position;
        }
    }
}
