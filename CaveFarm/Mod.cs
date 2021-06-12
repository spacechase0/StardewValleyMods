using System;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace CaveFarm
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.ConsoleCommands.Add("walls", "TODO", this.WallsCommand);
        }

        private void WallsCommand(string cmd, string[] args)
        {
            for (int ix = 0; ix < Game1.currentLocation.Map.Layers[0].LayerSize.Width; ++ix)
            {
                for (int iy = 0; iy < Game1.currentLocation.Map.Layers[0].LayerSize.Height; ++iy)
                {
                    if (Math.Abs(Game1.player.getTileX() - ix) < 3 && Math.Abs(Game1.player.getTileY() - iy) < 3)
                        continue;
                    var key = new Vector2(ix, iy);
                    Game1.currentLocation.terrainFeatures[key] = new CaveWall();
                }
            }
        }
    }
}
