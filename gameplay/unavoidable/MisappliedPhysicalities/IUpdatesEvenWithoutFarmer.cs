using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;

namespace MisappliedPhysicalities
{
    public interface IUpdatesEvenWithoutFarmer
    {
        public void UpdateEvenWithoutFarmer( GameLocation loc, GameTime time );
    }
}
