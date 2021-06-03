using System;

namespace FireArcadeGame.Objects
{
    public class Character : BaseObject
    {
        public virtual RectangleF BoundingBox { get; } = new RectangleF( 0, 0, 0.35f, 0.35f );

        public virtual bool Floats { get; } = false;
        public int Health { get; set; } = 1;

        public Character( World world )
        :   base( world )
        {
        }

        public virtual void Hurt( int amt )
        {
            Health -= amt;
        }

        public virtual void DoMovement() { }

        public override void Update()
        {
            var oldPos = Position;

            DoMovement();

            // Lazy implementation - would use something better if using a real engine
            Func< float, float, bool > solidCheck = Floats ? World.map.IsAirSolid : World.map.IsSolid;
            if ( solidCheck( Position.X, Position.Z ) )
            {
                if ( !solidCheck( oldPos.X, Position.Z ) )
                    Position.X = oldPos.X;
                else if ( !solidCheck( Position.X, oldPos.Z ) )
                    Position.Z = oldPos.Z;
                else
                    Position = oldPos;
            }
        }
    }
}
