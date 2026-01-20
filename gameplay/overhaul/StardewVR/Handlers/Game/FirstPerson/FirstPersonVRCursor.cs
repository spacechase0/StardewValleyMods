using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewVR.Handlers.Game.FirstPerson;
using StardewVR.Hardware;

namespace Stardew3D.Handlers.Game.FirstPerson;

internal class FirstPersonVRCursor : IGameCursor
{
    private Func<Vector3> pointerPositionFunc;
    private Func<Vector3> pointerFacingFunc;
    private Func<Vector3> pointerUpFunc;
    private Func<Vector3> gripPositionFunc;
    private Func<Vector3> gripFacingFunc;
    private Func<Vector3> gripUpFunc;
    private Func<Vector3> linearVelocityFunc;
    private Func<Vector3> angularVelocityFunc;
    private Func<bool> menuLeftClick;
    private Func<bool> menuRightClick;
    private Func<Vector2> menuScroll;

    private Func<Item> holdingFunc;

    private bool menuLeftClickState, menuRightClickState;
    private bool prevMenuLeftClickState, prevMenuRightClickState;

    public Vector3 PointerPosition => pointerPositionFunc();
    public Vector3 PointerFacing => pointerFacingFunc();
    public Vector3 PointerUp => pointerUpFunc();

    public Vector3 GripPosition => gripPositionFunc();
    public Vector3 GripFacing => gripFacingFunc();
    public Vector3 GripUp => gripUpFunc();

    public Vector3 LinearVelocity => linearVelocityFunc();
    public Vector3 AngularVelocity => angularVelocityFunc();

    public bool MenuLeftClickJustPressed => !prevMenuLeftClickState && menuLeftClickState;
    public bool MenuLeftClickHeld => menuLeftClickState;
    public bool MenuLeftClickJustReleased => prevMenuLeftClickState && !menuLeftClickState;
    public bool MenuRightClickJustPressed => !prevMenuRightClickState && menuRightClickState;
    public bool MenuRightClickHeld => menuRightClickState;
    public bool MenuRightClickJustReleased => prevMenuRightClickState && !menuRightClickState;
    public Vector2 MenuScroll => menuScroll();

    public Item Holding => holdingFunc();

    public bool FlipMenuSprite { get; init; } = false;

    public FirstPersonVRCursor(
        Func<Vector3> pointerPositionFunc, Func<Vector3> pointerFacingFunc, Func<Vector3> pointerUpFunc,
        Func<Vector3> gripPositionFunc, Func<Vector3> gripFacingFunc, Func<Vector3> gripUpFunc,
        Func<Vector3> linearVelocityFunc, Func<Vector3> angularVelocityFunc,
        Func<bool> menuLeftClick, Func<bool> menuRightClick, Func<Vector2> menuScroll, Func<Item> holdingFunc)
    {
        this.pointerPositionFunc = pointerPositionFunc;
        this.pointerFacingFunc = pointerFacingFunc;
        this.pointerUpFunc = pointerUpFunc;
        this.gripPositionFunc = gripPositionFunc;
        this.gripFacingFunc = gripFacingFunc;
        this.gripUpFunc = gripUpFunc;
        this.linearVelocityFunc = linearVelocityFunc;
        this.angularVelocityFunc = angularVelocityFunc;
        this.menuLeftClick = menuLeftClick;
        this.menuRightClick = menuRightClick;
        this.menuScroll = menuScroll;
        this.holdingFunc = holdingFunc;
    }

    public void Update()
    {
        prevMenuLeftClickState = menuLeftClickState;
        prevMenuRightClickState = menuRightClickState;
        menuLeftClickState = menuLeftClick();
        menuRightClickState = menuRightClick();
    }
}
