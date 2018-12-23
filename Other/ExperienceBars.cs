using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Magic.Other
{
    public interface ExperienceBarsApi
    {
        void DrawExperienceBar(Texture2D icon, int level, float percentFull, Color color);
        void SetDrawLuck(bool luck);
    }
}
