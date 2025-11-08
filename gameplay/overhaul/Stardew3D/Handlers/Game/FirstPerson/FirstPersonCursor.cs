using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Stardew3D.Handlers.Game.FirstPerson;

public class FirstPersonCursor : IGameCursor
{
    public FirstPersonGameHandler GameHandler { get; }

    public Vector3 Position => GameHandler.Camera.Position;
    public Vector3 Facing => GameHandler.Camera.Forward;
    public Vector3 Up => GameHandler.Camera.Up;

    public bool LeftClickJustPressed { get; set; }
    public bool LeftClickHeld { get; set; }
    public bool LeftClickJustReleased { get; set; }
    public bool RightClickJustPressed { get; set; }
    public bool RightClickHeld { get; set; }
    public bool RightClickJustReleased { get; set; }
    public Vector2 Scroll { get; set; }

    public Item Holding => Game1.player.CurrentItem;

    public FirstPersonCursor(FirstPersonGameHandler gameHandler)
    {
        GameHandler = gameHandler;
    }
}
