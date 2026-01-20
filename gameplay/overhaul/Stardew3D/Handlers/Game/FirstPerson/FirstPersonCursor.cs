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

    public Vector3 PointerPosition => GameHandler.Camera.Position;
    public Vector3 PointerFacing => GameHandler.Camera.Forward;
    public Vector3 PointerUp => GameHandler.Camera.Up;

    public Vector3 GripPosition => GameHandler.Camera.Position;
    public Vector3 GripFacing => GameHandler.Camera.Forward;
    public Vector3 GripUp => GameHandler.Camera.Up;

    public Vector3 LinearVelocity => Vector3.Zero;
    public Vector3 AngularVelocity => Vector3.Zero; // TODO: This one could probably be implemented in flatscreen too, just in case

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
