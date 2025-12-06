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
    private Func<Vector3> positionFunc;
    private Func<Vector3> facingFunc;
    private Func<Vector3> upFunc;
    private Func<bool> menuLeftClick;
    private Func<bool> menuRightClick;
    private Func<Vector2> menuScroll;

    private Func<Item> holdingFunc;

    private bool menuLeftClickState, menuRightClickState;
    private bool prevMenuLeftClickState, prevMenuRightClickState;

    public Vector3 Position => positionFunc();
    public Vector3 Facing => facingFunc();
    public Vector3 Up => upFunc();

    public bool MenuLeftClickJustPressed => !prevMenuLeftClickState && menuLeftClickState;
    public bool MenuLeftClickHeld => menuLeftClickState;
    public bool MenuLeftClickJustReleased => prevMenuLeftClickState && !menuLeftClickState;
    public bool MenuRightClickJustPressed => !prevMenuRightClickState && menuRightClickState;
    public bool MenuRightClickHeld => menuRightClickState;
    public bool MenuRightClickJustReleased => prevMenuRightClickState && !menuRightClickState;
    public Vector2 MenuScroll => menuScroll();

    public Item Holding => holdingFunc();

    public bool FlipMenuSprite { get; init; } = false;

    public FirstPersonVRCursor(Func<Vector3> positionFunc, Func<Vector3> facingFunc, Func<Vector3> upFunc, Func<bool> menuLeftClick, Func<bool> menuRightClick, Func<Vector2> menuScroll, Func<Item> holdingFunc)
    {
        this.positionFunc = positionFunc;
        this.facingFunc = facingFunc;
        this.upFunc = upFunc;
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
