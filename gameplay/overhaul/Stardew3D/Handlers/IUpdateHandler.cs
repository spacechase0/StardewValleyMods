using Microsoft.Xna.Framework;

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
