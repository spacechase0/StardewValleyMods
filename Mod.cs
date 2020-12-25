using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FireArcadeGame
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private World world;

        public override void Entry( IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.GameLoop.UpdateTicked += onUpdate;
            helper.Events.Display.Rendered += onRendered;
        }

        private void onUpdate( object sender, UpdateTickedEventArgs e )
        {
            if ( world == null )
                world = new World();
            world.Update();
        }

        private void onRendered( object sender, RenderedEventArgs e )
        {
            if ( world == null )
                world = new World();
            world.Render();
        }
    }
}
