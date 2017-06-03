using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Locations
{
    public abstract class CustomDecoratableLocation : DecoratableLocation
    {
        public new abstract List<Microsoft.Xna.Framework.Rectangle> getWalls();
        public new abstract List<Microsoft.Xna.Framework.Rectangle> getFloors();

        public CustomDecoratableLocation() : base() { }
        public CustomDecoratableLocation(xTile.Map m, string name) : base(m, name) { }

        public override void setWallpaper(int which, int whichRoom = -1, bool persist = false)
        {
            List<Microsoft.Xna.Framework.Rectangle> walls = getWalls();
            if (persist)
            {
                while (this.wallPaper.Count < walls.Count)
                    this.wallPaper.Add(0);
                if (whichRoom == -1)
                {
                    for (int index = 0; index < this.wallPaper.Count; ++index)
                        this.wallPaper[index] = which;
                }
                else if (whichRoom <= this.wallPaper.Count - 1)
                    this.wallPaper[whichRoom] = which;
            }
            int index1 = which % 16 + which / 16 * 48;
            if (whichRoom == -1)
            {
                foreach (Microsoft.Xna.Framework.Rectangle rectangle in walls)
                {
                    for (int x = rectangle.X; x < rectangle.Right; ++x)
                    {
                        this.setMapTileIndex(x, rectangle.Y, index1, "Back", 0);
                        this.setMapTileIndex(x, rectangle.Y + 1, index1 + 16, "Back", 0);
                        if (rectangle.Height >= 3)
                        {
                            if (this.map.GetLayer("Buildings").Tiles[x, rectangle.Y + 2].TileSheet.Equals((object)this.map.TileSheets[2]))
                                this.setMapTileIndex(x, rectangle.Y + 2, index1 + 32, "Buildings", 0);
                            else
                                this.setMapTileIndex(x, rectangle.Y + 2, index1 + 32, "Back", 0);
                        }
                    }
                }
            }
            else
            {
                Microsoft.Xna.Framework.Rectangle rectangle = walls[Math.Min(walls.Count - 1, whichRoom)];
                for (int x = rectangle.X; x < rectangle.Right; ++x)
                {
                    this.setMapTileIndex(x, rectangle.Y, index1, "Back", 0);
                    this.setMapTileIndex(x, rectangle.Y + 1, index1 + 16, "Back", 0);
                    if (rectangle.Height >= 3)
                    {
                        if (this.map.GetLayer("Buildings").Tiles[x, rectangle.Y + 2].TileSheet.Equals((object)this.map.TileSheets[2]))
                            this.setMapTileIndex(x, rectangle.Y + 2, index1 + 32, "Buildings", 0);
                        else
                            this.setMapTileIndex(x, rectangle.Y + 2, index1 + 32, "Back", 0);
                    }
                }
            }
        }

        public override void setFloor(int which, int whichRoom = -1, bool persist = false)
        {
            List<Microsoft.Xna.Framework.Rectangle> floors = getFloors();
            if (persist)
            {
                while (this.floor.Count < floors.Count)
                    this.floor.Add(0);
                if (whichRoom == -1)
                {
                    for (int index = 0; index < this.floor.Count; ++index)
                        this.floor[index] = which;
                }
                else
                {
                    if (whichRoom > this.floor.Count - 1)
                        return;
                    this.floor[whichRoom] = which;
                }
            }
            int index1 = 336 + which % 8 * 2 + which / 8 * 32;
            if (whichRoom == -1)
            {
                foreach (Microsoft.Xna.Framework.Rectangle rectangle in floors)
                {
                    int x = rectangle.X;
                    while (x < rectangle.Right)
                    {
                        int y = rectangle.Y;
                        while (y < rectangle.Bottom)
                        {
                            if (rectangle.Contains(x, y))
                                this.setMapTileIndex(x, y, index1, "Back", 0);
                            if (rectangle.Contains(x + 1, y))
                                this.setMapTileIndex(x + 1, y, index1 + 1, "Back", 0);
                            if (rectangle.Contains(x, y + 1))
                                this.setMapTileIndex(x, y + 1, index1 + 16, "Back", 0);
                            if (rectangle.Contains(x + 1, y + 1))
                                this.setMapTileIndex(x + 1, y + 1, index1 + 17, "Back", 0);
                            y += 2;
                        }
                        x += 2;
                    }
                }
            }
            else
            {
                Microsoft.Xna.Framework.Rectangle rectangle = floors[whichRoom];
                int x = rectangle.X;
                while (x < rectangle.Right)
                {
                    int y = rectangle.Y;
                    while (y < rectangle.Bottom)
                    {
                        if (rectangle.Contains(x, y))
                            this.setMapTileIndex(x, y, index1, "Back", 0);
                        if (rectangle.Contains(x + 1, y))
                            this.setMapTileIndex(x + 1, y, index1 + 1, "Back", 0);
                        if (rectangle.Contains(x, y + 1))
                            this.setMapTileIndex(x, y + 1, index1 + 16, "Back", 0);
                        if (rectangle.Contains(x + 1, y + 1))
                            this.setMapTileIndex(x + 1, y + 1, index1 + 17, "Back", 0);
                        y += 2;
                    }
                    x += 2;
                }
            }
        }
    }
}
