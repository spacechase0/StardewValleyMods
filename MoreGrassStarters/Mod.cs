using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using SpaceShared.Migrations;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;

namespace MoreGrassStarters
{
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The lowest grass type value for custom grass.</summary>
        public int MinGrassType = 5;

        /// <summary>The highest grass type value for custom grass.</summary>
        public int MaxGrassType => this.MinGrassType + GrassStarterItem.ExtraGrassTypes;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;

            if (File.Exists(Path.Combine(this.Helper.DirectoryPath, "assets", "grass.png")))
            {
                GrassStarterItem.Tex2 = helper.ModContent.Load<Texture2D>("assets/grass.png");
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
            var spaceCore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            spaceCore.RegisterSerializerType(typeof(CustomGrass));
            spaceCore.RegisterSerializerType(typeof(GrassStarterItem));
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                PyTkMigrator.MigrateItems("MoreGrassStarters.GrassStarterItem,  MoreGrassStarters", data =>
                {
                    int which = data.GetOrDefault("whichGrass", int.Parse, defaultValue: this.MinGrassType);
                    return new GrassStarterItem(which);
                });
                PyTkMigrator.MigrateTerrainFeatures("MoreGrassStarters.CustomGrass,  MoreGrassStarters", (feature, data) =>
                {
                    int type = data.GetOrDefault("Type", int.Parse, defaultValue: this.MinGrassType);
                    int weedCount = data.GetOrDefault("WeedCount", int.Parse);
                    return new CustomGrass(type, weedCount);
                });
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.DayStarted"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (GameLocation location in CommonHelper.GetLocations())
            {
                foreach ((Vector2 tile, TerrainFeature feature) in location.terrainFeatures.FieldDict)
                {
                    if (feature is not Grass grass)
                        continue;

                    bool shouldMigrate =
                        feature is not CustomGrass
                        && grass.grassType.Value >= this.MinGrassType
                        && grass.grassType.Value <= this.MaxGrassType
                        && this.HasNearbyCustomGrass(location, tile, 3);

                    if (shouldMigrate)
                        location.terrainFeatures[tile] = new CustomGrass(grass.grassType.Value, grass.numberOfWeeds.Value);
                }
            }
        }

        /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not ShopMenu menu || menu.portraitPerson == null)
                return;

            if (menu.portraitPerson.Name == "Pierre")
            {
                var forSale = menu.forSale;
                var itemPriceAndStock = menu.itemPriceAndStock;

                for (int i = Grass.caveGrass; i < 5 + GrassStarterItem.ExtraGrassTypes; ++i)
                {
                    var item = new GrassStarterItem(i);
                    forSale.Add(item);
                    itemPriceAndStock.Add(item, new[] { 100, int.MaxValue });
                }
            }
        }

        /// <summary>Get whether any nearby tile position contains a <see cref="CustomGrass"/> instance.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="origin">The origin from which to search nearby tiles.</param>
        /// <param name="radius">The straight distance in tiles to search outward from the origin.</param>
        private bool HasNearbyCustomGrass(GameLocation location, Vector2 origin, int radius)
        {
            int originX = (int)origin.X;
            int originY = (int)origin.Y;

            int minX = originX - radius;
            int maxX = originX + radius;
            int minY = originY - radius;
            int maxY = originY + radius;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (x == originX && y == originY)
                        continue;

                    if (location.terrainFeatures.TryGetValue(new Vector2(x, y), out TerrainFeature feature) && feature is CustomGrass)
                        return true;
                }
            }

            return false;
        }
    }
}
