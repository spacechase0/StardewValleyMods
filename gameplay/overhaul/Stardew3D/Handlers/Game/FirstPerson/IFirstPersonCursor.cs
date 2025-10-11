using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Stardew3D.Handlers.Game.FirstPerson;

public interface IFirstPersonCursor
{
    public Vector3 Position { get; }
    public Vector3 Facing { get; }

    public Item Holding { get; }
}
