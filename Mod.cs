using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Spenny
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            GameEvents.EighthUpdateTick += test;
        }

        private void test(object sender, EventArgs args)
        {
            var penny = Game1.getCharacterFromName("Penny");
            if (penny == null)
                return;

            penny.faceDirection((penny.FacingDirection + 1) % 4);
            if ( penny.yJumpOffset == 0 )
                penny.jump();
        }
    }
}
