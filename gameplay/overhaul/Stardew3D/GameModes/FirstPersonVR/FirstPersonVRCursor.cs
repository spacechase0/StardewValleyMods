using Microsoft.Xna.Framework;
using StardewValley;

namespace Stardew3D.GameModes.FirstPersonVR;

internal class FirstPersonVRCursor : IGameCursor
{
    private Func<Matrix> pointerFunc;
    private Func<Matrix> gripFunc;
    private Func<Vector3> linearVelocityFunc;
    private Func<Vector3> angularVelocityFunc;
    private Func<bool> menuLeftClick;
    private Func<bool> menuRightClick;
    private Func<Vector2> menuScroll;

    private Func<Item> holdingFunc;
    private Func<bool> useItem;
    private Func<bool> interact;

    private bool menuLeftClickState, menuRightClickState, useItemState, interactState;
    private bool prevMenuLeftClickState, prevMenuRightClickState, prevUseItemState, prevInteractState;

    public Vector3 PointerPosition => pointerFunc().Translation;
    public Vector3 PointerFacing => pointerFunc().Forward;
    public Vector3 PointerUp => pointerFunc().Up;

    public Vector3 GripPosition => gripFunc().Translation;
    public Vector3 GripFacing => gripFunc().Forward;
    public Vector3 GripUp => gripFunc().Up;

    public Vector3 LinearVelocity => linearVelocityFunc();
    public Vector3 AngularVelocity => angularVelocityFunc();

    public bool MenuLeftClickJustPressed => !prevMenuLeftClickState && menuLeftClickState;
    public bool MenuLeftClickHeld => menuLeftClickState;
    public bool MenuLeftClickJustReleased => prevMenuLeftClickState && !menuLeftClickState;
    public bool MenuRightClickJustPressed => !prevMenuRightClickState && menuRightClickState;
    public bool MenuRightClickHeld => menuRightClickState;
    public bool MenuRightClickJustReleased => prevMenuRightClickState && !menuRightClickState;
    public Vector2 MenuScroll => menuScroll();

    public ISalable Holding => holdingFunc();
    public bool UseItemJustPressed => !prevUseItemState && useItemState;
    public bool UseItemHeld => useItemState;
    public bool UseItemJustReleased => prevUseItemState && !useItemState;
    public bool InteractJustPressed => !prevInteractState && interactState;
    public bool InteractHeld => interactState;
    public bool InteractJustReleased => prevInteractState && !interactState;

    public bool FlipMenuSprite { get; init; } = false;

    public FirstPersonVRCursor(
        Func<Matrix> pointerFunc, Func<Matrix> gripFunc,
        Func<Vector3> linearVelocityFunc, Func<Vector3> angularVelocityFunc,
        Func<bool> menuLeftClick, Func<bool> menuRightClick, Func<Vector2> menuScroll,
        Func<Item> holdingFunc, Func<bool> useItem, Func<bool> interact)
    {
        this.pointerFunc = pointerFunc;
        this.gripFunc = gripFunc;
        this.linearVelocityFunc = linearVelocityFunc;
        this.angularVelocityFunc = angularVelocityFunc;
        this.menuLeftClick = menuLeftClick;
        this.menuRightClick = menuRightClick;
        this.menuScroll = menuScroll;
        this.holdingFunc = holdingFunc;
        this.useItem = useItem;
        this.interact = interact;
    }

    public void Update(IGameMode parent)
    {
        prevMenuLeftClickState = menuLeftClickState;
        prevMenuRightClickState = menuRightClickState;
        prevUseItemState = useItemState;
        prevInteractState = interactState;

        menuLeftClickState = menuLeftClick();
        menuRightClickState = menuRightClick();
        useItemState = useItem();
        interactState = interact();
    }
}
