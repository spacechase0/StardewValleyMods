using Microsoft.Xna.Framework.Graphics;

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else

namespace SpaceShared.UI
{
    internal
#endif
    interface ISingleTexture
    {
        public Texture2D Texture { get; set; }
    }
}
