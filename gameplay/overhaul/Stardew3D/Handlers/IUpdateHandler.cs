using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Stardew3D.Handlers;
public interface IUpdateHandler
{
    struct UpdateContext
    {
        public GameTime Time;
        public Action<UpdateContext> ForceUpdateIfNotAlreadyRun;
    }

    public void Update(UpdateContext ctx);
}
