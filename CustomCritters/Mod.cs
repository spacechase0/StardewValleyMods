using System;
using System.Collections.Generic;
using System.IO;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CustomCritters
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.Player.Warped += this.onWarped;

            // load content packs
            Log.Info("Loading critter content packs...");
            foreach (IContentPack contentPack in this.GetContentPacks())
            {
                CritterEntry data = contentPack.ReadJsonFile<CritterEntry>("critter.json");
                if (data == null)
                {
                    Log.Warn($"   {contentPack.Manifest.Name}: ignored (no critter.json file).");
                    continue;
                }
                if (!File.Exists(Path.Combine(contentPack.DirectoryPath, "critter.png")))
                {
                    Log.Warn($"   {contentPack.Manifest.Name}: ignored (no critter.png file).");
                    continue;
                }
                Log.Info(contentPack.Manifest.Name == data.Id ? contentPack.Manifest.Name : $"   {contentPack.Manifest.Name} (id: {data.Id})");
                CritterEntry.Register(data);
            }
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer || Game1.CurrentEvent != null)
                return;

            foreach (var entry in CritterEntry.critters)
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

        /// <summary>Load available content packs.</summary>
        private IEnumerable<IContentPack> GetContentPacks()
        {
            // SMAPI content packs
            foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
                yield return contentPack;

            // legacy content packs
            string legacyRoot = Path.Combine(this.Helper.DirectoryPath, "Critters");
            Directory.CreateDirectory(legacyRoot);
            foreach (string folderPath in Directory.EnumerateDirectories(legacyRoot))
            {
                yield return this.Helper.ContentPacks.CreateTemporary(
                    directoryPath: folderPath,
                    id: Guid.NewGuid().ToString("N"),
                    name: new DirectoryInfo(folderPath).Name,
                    description: null,
                    author: null,
                    version: new SemanticVersion(1, 0, 0)
                );
            }
        }
    }
}
