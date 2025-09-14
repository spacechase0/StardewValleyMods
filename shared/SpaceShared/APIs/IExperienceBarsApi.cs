using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceShared.APIs
{
    public interface IExperienceBarsApi
    {
        void DrawExperienceBar(Texture2D icon, int level, float percentFull, Color color);
        void SetDrawLuck(bool luck);
    }
}
