using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Mods;

namespace Stardew3D;
public interface IMenuHandler
{
    public void UpdateMenu(GameTime time, Action<GameTime> forceMenuUpdateIfNotAlreadyRun );
    public void RenderMenu(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Action<RenderSteps, SpriteBatch, GameTime, RenderTarget2D> forceMenuRenderIfNotAlreadyRun);
}
