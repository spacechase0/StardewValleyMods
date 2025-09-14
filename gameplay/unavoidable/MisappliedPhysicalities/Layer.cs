using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisappliedPhysicalities
{
    public enum Layer
    {
        Underground,
        GroundLevel,
        Elevated,
    }

    public static class LayerExtensions
    {
        public static Layer? GetAbove( this Layer layer )
        {
            switch ( layer )
            {
                case Layer.Underground: return Layer.GroundLevel;
                case Layer.GroundLevel: return Layer.Elevated;
                default: return null;
            }
        }

        public static Layer? GetBelow( this Layer layer )
        {
            switch ( layer )
            {
                case Layer.GroundLevel: return Layer.Underground;
                case Layer.Elevated: return Layer.GroundLevel;
                default: return null;
            }
        }
    }
}
