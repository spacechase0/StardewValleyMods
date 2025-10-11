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

internal class FirstPersonVRCursor : IFirstPersonCursor
{
    private Func<Vector3> positionFunc;
    private Func<Vector3> facingFunc;
    private Func<Item> holdingFunc;
    private Func<bool> menuLeftClick;
    private Func<bool> menuRightClick;
    private Func<Vector2> menuScroll;

    public Vector3 Position => positionFunc();

    public Vector3 Facing => facingFunc();

    public Item Holding => holdingFunc();

    public bool MenuLeftClick => menuLeftClick();

    public bool MenuRightClick => menuRightClick();

    public Vector2 MenuScroll => menuScroll();

    public FirstPersonVRCursor(Func<Vector3> positionFunc, Func<Vector3> facingFunc, Func<Item> holdingFunc, Func<bool> menuLeftClick, Func<bool> menuRightClick, Func<Vector2> menuScroll)
    {
        this.positionFunc = positionFunc;
        this.facingFunc = facingFunc;
        this.holdingFunc = holdingFunc;
        this.menuLeftClick = menuLeftClick;
        this.menuRightClick = menuRightClick;
        this.menuScroll = menuScroll;
    }
}
