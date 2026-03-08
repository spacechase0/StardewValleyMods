using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
    public bool UseItemJustPressed => !Game1.isOneOfTheseKeysDown(prevKeyboardState, Game1.options.useToolButton) && Game1.isOneOfTheseKeysDown(currKeyboardState, Game1.options.useToolButton);
    public bool UseItemHeld => Game1.isOneOfTheseKeysDown(currKeyboardState, Game1.options.useToolButton);
    public bool UseItemJustReleased => Game1.isOneOfTheseKeysDown(prevKeyboardState, Game1.options.useToolButton) && !Game1.isOneOfTheseKeysDown(currKeyboardState, Game1.options.useToolButton);
    public bool InteractJustPressed => !Game1.isOneOfTheseKeysDown(prevKeyboardState, Game1.options.actionButton) && Game1.isOneOfTheseKeysDown(currKeyboardState, Game1.options.actionButton);
    public bool InteractHeld => Game1.isOneOfTheseKeysDown(currKeyboardState, Game1.options.actionButton);
    public bool InteractJustReleased => Game1.isOneOfTheseKeysDown(prevKeyboardState, Game1.options.actionButton) && !Game1.isOneOfTheseKeysDown(currKeyboardState, Game1.options.actionButton);

    public FirstPersonCursor(FirstPersonGameHandler gameHandler)
    {
        GameHandler = gameHandler;
    }

    private KeyboardState currKeyboardState;
    private KeyboardState prevKeyboardState;
    public void Update(IGameHandler parent)
    {
        prevKeyboardState = currKeyboardState;
        currKeyboardState = Game1.GetKeyboardState();
    }
}
