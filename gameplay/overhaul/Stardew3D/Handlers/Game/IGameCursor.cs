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
    public Matrix Pointer
    {
        get
        {
            Matrix transform = Matrix.Identity;
            transform.Translation = PointerPosition;
            transform.Forward = PointerFacing;
            transform.Up = PointerUp;
            transform.Right = Vector3.Cross(PointerFacing, PointerUp);
            return transform;
        }
    }
    public Vector3 PointerPosition { get; }
    public Vector3 PointerFacing { get; }
    public Vector3 PointerUp { get; }

    public Matrix Grip
    {
        get
        {
            Matrix transform = Matrix.Identity;
            transform.Translation = GripPosition;
            transform.Forward = GripFacing;
            transform.Up = GripUp;
            transform.Right = Vector3.Cross(GripFacing, GripUp);
            return transform;
        }
    }
    public Vector3 GripPosition { get; }
    public Vector3 GripFacing { get; }
    public Vector3 GripUp { get; }

    public Vector3 LinearVelocity { get; }
    public Vector3 AngularVelocity { get; }

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
