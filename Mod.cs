using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using StardewValley.BellsAndWhistles;
using System.IO;

namespace CustomCritters
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public override void Entry(IModHelper helper)
        {
            instance = this;

            PlayerEvents.Warped += onLocationChanged;
            /*
            var ce = new Critters.CritterEntry();
            ce.Id = "eemie.bee";
            var a = new Critters.CritterEntry.Animation_();
            a.Frames.Add(new Critters.CritterEntry.Animation_.AnimationFrame_());
            ce.Animations.Add("test",a);
            ce.SpawnConditions.Add(new Critters.CritterEntry.SpawnCondition_());
            var sl = new Critters.CritterEntry.SpawnLocation_();
            sl.Conditions.Add(new Critters.CritterEntry.SpawnLocation_.ConditionEntry_());
            ce.SpawnLocations.Add(sl);
            helper.WriteJsonFile("test.json", ce);
            */

            Log.info("Creating critter types...");
            foreach ( var file in Directory.EnumerateDirectories( Path.Combine( helper.DirectoryPath, "Critters" ) ) )
            {
                var ce = helper.ReadJsonFile<CritterEntry>(Path.Combine(file, "critter.json"));
                if ( ce == null )
                {
                    Log.warn("\tFailed to load critter data for " + file);
                    continue;
                }
                else if ( !File.Exists( Path.Combine(file, "critter.png" ) ) )
                {
                    Log.warn("\tCritter " + file + " has no image, skipping");
                    continue;
                }
                Log.info("\tCritter type: " + ce.Id);
                CritterEntry.Register(ce);
            }
        }

        private void onLocationChanged( object sender, EventArgsPlayerWarped args )
        {
            if (Game1.CurrentEvent != null)
                return;

            foreach ( var entry in CritterEntry.critters )
            {
                for (int i = 0; i < entry.Value.SpawnAttempts; ++i)
                {
                    if (entry.Value.check(args.NewLocation))
                    {
                        var spot = entry.Value.pickSpot(args.NewLocation);
                        if (spot == null)
                            continue;

                        args.NewLocation.addCritter(entry.Value.makeCritter(spot.Value));
                    }
                }
            }
        }
    }
}
