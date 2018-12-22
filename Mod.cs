using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.IO;

namespace CustomCritters
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public override void Entry(IModHelper helper)
        {
            instance = this;

            helper.Events.Player.Warped += onWarped;

            Log.info("Creating critter types...");
            foreach ( var folderPath in Directory.EnumerateDirectories( Path.Combine( helper.DirectoryPath, "Critters" ) ) )
            {
                var pack = helper.ContentPacks.CreateFake(folderPath);
                var ce = pack.ReadJsonFile<CritterEntry>("critter.json");
                if ( ce == null )
                {
                    Log.warn($"\tFailed to load critter data for {folderPath}: no critter.json found.");
                    continue;
                }
                if ( !File.Exists( Path.Combine(folderPath, "critter.png" ) ) )
                {
                    Log.warn($"\tCritter {folderPath} has no image, skipping");
                    continue;
                }
                Log.info($"\tCritter type: {ce.Id}");
                CritterEntry.Register(ce);
            }
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onWarped( object sender, WarpedEventArgs e )
        {
            if (!e.IsLocalPlayer || Game1.CurrentEvent != null)
                return;

            foreach ( var entry in CritterEntry.critters )
            {
                for (int i = 0; i < entry.Value.SpawnAttempts; ++i)
                {
                    if (entry.Value.check(e.NewLocation))
                    {
                        var spot = entry.Value.pickSpot(e.NewLocation);
                        if (spot == null)
                            continue;

                        e.NewLocation.addCritter(entry.Value.makeCritter(spot.Value));
                    }
                }
            }
        }
    }
}
