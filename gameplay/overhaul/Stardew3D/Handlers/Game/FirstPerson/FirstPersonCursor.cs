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
    public bool UseItemJustPressed => !prevUseItemState && useItemState;
    public bool UseItemHeld => useItemState;
    public bool UseItemJustReleased => prevUseItemState && !useItemState;
    public bool InteractJustPressed => !prevInteractState && interactState;
    public bool InteractHeld => interactState;
    public bool InteractJustReleased => prevInteractState && !interactState;

    public FirstPersonCursor(FirstPersonGameHandler gameHandler)
    {
        GameHandler = gameHandler;
    }

    private bool useItemState, prevUseItemState;
    private bool interactState, prevInteractState;
    public void Update(IGameHandler parent)
    {
        prevUseItemState = useItemState;
        prevInteractState = interactState;

        useItemState = Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.useToolButton) || Game1.input.GetMouseState().LeftButton == ButtonState.Pressed;
        interactState = Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.actionButton) || Game1.input.GetMouseState().RightButton == ButtonState.Pressed;
    }
}
