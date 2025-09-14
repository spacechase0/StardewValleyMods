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
        public static Texture2D Drill;
        public static Texture2D WireCutter;

        public static Texture2D ConveyorBelt;
        public static Texture2D Unhopper;
        public static Texture2D LogicConnector;
        public static Texture2D LeverBlock;

        internal static void Load( IModContentHelper content )
        {
            Assets.Drill = content.Load<Texture2D>( "assets/drill.png" );
            Assets.WireCutter = content.Load<Texture2D>( "assets/wire-cutter.png" );

            Assets.ConveyorBelt = content.Load<Texture2D>( "assets/conveyor.png" );
            Assets.Unhopper = content.Load<Texture2D>( "assets/unhopper.png" );
            Assets.LogicConnector = content.Load<Texture2D>( "assets/logic-connector.png" );
            Assets.LeverBlock = content.Load<Texture2D>( "assets/lever-block.png" );
        }
    }
}
