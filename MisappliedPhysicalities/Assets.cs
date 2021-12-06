using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace MisappliedPhysicalities
{
    internal static class Assets
    {
        public static Texture2D RadioactiveTools;
        public static Texture2D MythiciteTools;
        public static Texture2D DrillTool;

        public static Texture2D ConveyorBelt;
        public static Texture2D Unhopper;

        internal static void Load( IContentHelper content )
        {
            RadioactiveTools = content.Load<Texture2D>( "assets/tools-radioactive.png" );
            Assets.MythiciteTools = content.Load<Texture2D>( "assets/tools-mythicite.png" );
            Assets.DrillTool = content.Load<Texture2D>( "assets/drill.png" );

            Assets.ConveyorBelt = content.Load<Texture2D>( "assets/conveyor.png" );
            Assets.Unhopper = content.Load<Texture2D>( "assets/unhopper.png" );
        }
    }
}
