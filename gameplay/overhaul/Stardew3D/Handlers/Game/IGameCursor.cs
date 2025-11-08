using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Stardew3D.Handlers.Game;

public interface IGameCursor
{
    public Vector3 Position { get; }
    public Vector3 Facing { get; }
    public Vector3 Up { get; }

    public bool LeftClickJustPressed { get; }
    public bool LeftClickHeld { get; }
    public bool LeftClickJustReleased { get; }
    public bool RightClickJustPressed { get; }
    public bool RightClickHeld { get; }
    public bool RightClickJustReleased { get; }
    public Vector2 Scroll { get; }

    public Item Holding { get; }
}
