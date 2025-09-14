using System;
using System.Collections.Generic;
using System.IO;
using CustomCritters.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CustomCritters
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Accessors
        *********/
        public static Mod Instance;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Player.Warped += this.OnWarped;

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


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // register critters with BugNet
            var bugNet = this.Helper.ModRegistry.GetApi<IBugNetApi>("spacechase0.BugNet");
            if (bugNet is not null)
            {
                foreach (CritterEntry critter in CritterEntry.Critters.Values)
                {
                    Texture2D texture = CustomCritter.LoadCritterTexture(critter.Id);

                    bugNet.RegisterCritter(
                        manifest: this.ModManifest,
                        critterId: $"{this.ModManifest.UniqueID}/{critter.Id}",
                        texture: texture,
                        textureArea: new Rectangle(0, 0, texture.Width, texture.Height),
                        defaultCritterName: critter.Id, // TODO: add name fields to critter.json
                        translatedCritterNames: new Dictionary<string, string>(),
                        makeCritter: (x, y) => critter.MakeCritter(new Vector2(x, y)),
                        isThisCritter: instance => (instance as CustomCritter)?.Data.Id == critter.Id
                    );
                }
            }
        }

        /// <inheritdoc cref="IPlayerEvents.Warped"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer || Game1.CurrentEvent != null)
                return;

            foreach (var entry in CritterEntry.Critters)
            {
                for (int i = 0; i < entry.Value.SpawnAttempts; ++i)
                {
                    if (entry.Value.Check(e.NewLocation))
                    {
                        var spot = entry.Value.PickSpot(e.NewLocation);
                        if (spot == null)
                            continue;

                        e.NewLocation.addCritter(entry.Value.MakeCritter(spot.Value));
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
