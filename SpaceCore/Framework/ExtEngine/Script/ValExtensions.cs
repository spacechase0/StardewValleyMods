using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Miniscript;

namespace SpaceCore.Framework.ExtEngine.Script
{
    public static class ValExtensions
    {
        public static Vector2 ToVector2(this ValMap map)
        {
            return new(map.map[new ValString("x")].FloatValue(), map.map[new ValString("y")].FloatValue());
        }

        public static Rectangle ToRectangle(this ValMap map)
        {
            return new(map.map[new ValString("x")].IntValue(), map.map[new ValString("y")].IntValue(),
                       map.map[new ValString("width")].IntValue(), map.map[new ValString("height")].IntValue());
        }
    }
}
