using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ExperienceBars
{
    public interface IApi
    {
        void DrawExperienceBar(Texture2D icon, int level, float percentFull, Color color);
        void SetDrawLuck(bool luck);
    }

    public class Api : IApi
    {
        public void DrawExperienceBar(Texture2D icon, int level, float percentFull, Color color)
        {
            Mod.renderSkillBar(10, Mod.expBottom, icon, new Rectangle(0, 0, icon.Width, icon.Height), level, percentFull, color);
            Mod.expBottom += 40;
        }

        public void SetDrawLuck(bool luck)
        {
            Mod.renderLuck = luck;
        }
    }
}
