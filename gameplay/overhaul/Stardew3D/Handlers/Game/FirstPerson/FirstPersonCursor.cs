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

    public bool MenuLeftClickJustPressed { get; set; }
    public bool MenuLeftClickHeld { get; set; }
    public bool MenuLeftClickJustReleased { get; set; }
    public bool MenuRightClickJustPressed { get; set; }
    public bool MenuRightClickHeld { get; set; }
    public bool MenuRightClickJustReleased { get; set; }
    public Vector2 MenuScroll { get; set; }

    public Item Holding => Game1.player.CurrentItem;

    public FirstPersonCursor(FirstPersonGameHandler gameHandler)
    {
        GameHandler = gameHandler;
    }
}
