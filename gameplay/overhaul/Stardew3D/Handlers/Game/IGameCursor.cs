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

    // TODO: Abstract this into an input sets sort of thing
    public bool MenuLeftClickJustPressed { get; }
    public bool MenuLeftClickHeld { get; }
    public bool MenuLeftClickJustReleased { get; }
    public bool MenuRightClickJustPressed { get; }
    public bool MenuRightClickHeld { get; }
    public bool MenuRightClickJustReleased { get; }
    public Vector2 MenuScroll { get; }

    public Item Holding { get; }
}
