using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore
{
    public interface IUpdateEvenWithoutFarmer
    {
        void UpdateEvenWithoutFarmer( GameLocation loc, GameTime time );
    }
}
