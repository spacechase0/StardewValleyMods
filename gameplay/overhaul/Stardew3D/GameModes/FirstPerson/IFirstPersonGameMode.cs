using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Stardew3D.GameModes;

namespace Stardew3D.GameModes.FirstPerson;

public interface IFirstPersonGameMode : IGameMode
{
    public Vector3 MovementFacing { get; }
    public Vector2 MovementAmount { get; }
    public Vector2 MovementAmountForced { get; }
}
