using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Stardew3D.Handlers.Game.FirstPerson;

public class FirstPersonCursor : IFirstPersonCursor
{
    public FirstPersonGameHandler GameHandler { get; }

    public Vector3 Position => GameHandler.Camera.Position;

    public Vector3 Facing => GameHandler.Camera.Forward;

    public Item Holding => Game1.player.CurrentItem;

    public FirstPersonCursor(FirstPersonGameHandler gameHandler)
    {
        GameHandler = gameHandler;
    }
}
