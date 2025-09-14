using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using xTile;

namespace #REPLACE_packname
{
    public class Runner : global::ContentCode.BaseRunner
    {
        public override void Run( object[] args )
        {
            var arg = args[ 0 ] as SpaceCore.Events.EventArgsAction;
            ActualRun( arg.ActionString );
        }

        public void ActualRun(string actionString)
        {
            #REPLACE_code
        }
    }
}
