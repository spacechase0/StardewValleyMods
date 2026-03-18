using Microsoft.Xna.Framework;

namespace Stardew3D.GameModes.FirstPerson;

public interface IFirstPersonGameMode : IGameMode
{
    public Vector3 MovementFacing { get; }
    public Vector2 MovementAmount { get; }
    public Vector2 MovementAmountForced { get; }
}
