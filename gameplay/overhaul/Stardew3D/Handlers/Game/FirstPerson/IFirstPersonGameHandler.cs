using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Stardew3D.Handlers.Game.FirstPerson;

public interface IFirstPersonGameHandler
{
    public Vector3 MovementFacing { get; }
    public Vector2 MovementAmount { get; }
    public Vector2 MovementAmountForced { get; }

    public IReadOnlyList<IFirstPersonCursor> Cursors { get; }
}
