using Microsoft.Xna.Framework;
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
        public new abstract List<Rectangle> getWalls();
        public new abstract List<Rectangle> getFloors();

        public CustomDecoratableLocation() : base()
        {
            List<Rectangle> list = getWalls();
            while (this.wallPaper.Count < list.Count)
            {
                this.wallPaper.Add(0);
            }
            list = getFloors();
            while (this.floor.Count < list.Count)
            {
                this.floor.Add(0);
            }
        }

        public CustomDecoratableLocation(string mapPath, string name) : base(mapPath, name)
        {
            List<Rectangle> list = getWalls();
            while (this.wallPaper.Count < list.Count)
            {
                this.wallPaper.Add(0);
            }
            list = getFloors();
            while (this.floor.Count < list.Count)
            {
                this.floor.Add(0);
            }
        }
        protected override void doSetVisibleWallpaper(int whichRoom, int which)
        {
            List<Microsoft.Xna.Framework.Rectangle> rooms = getWalls();
            int tileSheetIndex = which % 16 + which / 16 * 48;
            if (whichRoom == -1)
            {
                using (List<Microsoft.Xna.Framework.Rectangle>.Enumerator enumerator = rooms.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Microsoft.Xna.Framework.Rectangle r = enumerator.Current;
                        for (int x = r.X; x < r.Right; x++)
                        {
                            base.setMapTileIndex(x, r.Y, tileSheetIndex, "Back", 0);
                            base.setMapTileIndex(x, r.Y + 1, tileSheetIndex + 16, "Back", 0);
                            if (r.Height >= 3)
                            {
                                if (this.map.GetLayer("Buildings").Tiles[x, r.Y + 2].TileSheet.Equals(this.map.TileSheets[2]))
                                {
                                    base.setMapTileIndex(x, r.Y + 2, tileSheetIndex + 32, "Buildings", 0);
                                }
                                else
                                {
                                    base.setMapTileIndex(x, r.Y + 2, tileSheetIndex + 32, "Back", 0);
                                }
                            }
                        }
                    }
                    return;
                }
            }
            if (rooms.Count > whichRoom)
            {
                Microsoft.Xna.Framework.Rectangle r2 = rooms[whichRoom];
                for (int x2 = r2.X; x2 < r2.Right; x2++)
                {
                    base.setMapTileIndex(x2, r2.Y, tileSheetIndex, "Back", 0);
                    base.setMapTileIndex(x2, r2.Y + 1, tileSheetIndex + 16, "Back", 0);
                    if (r2.Height >= 3)
                    {
                        if (this.map.GetLayer("Buildings").Tiles[x2, r2.Y + 2].TileSheet.Equals(this.map.TileSheets[2]))
                        {
                            base.setMapTileIndex(x2, r2.Y + 2, tileSheetIndex + 32, "Buildings", 0);
                        }
                        else
                        {
                            base.setMapTileIndex(x2, r2.Y + 2, tileSheetIndex + 32, "Back", 0);
                        }
                    }
                }
            }
        }

        protected override void doSetVisibleFloor(int whichRoom, int which)
        {
            List<Microsoft.Xna.Framework.Rectangle> rooms = getFloors();
            int tileSheetIndex = 336 + which % 8 * 2 + which / 8 * 32;
            if (whichRoom == -1)
            {
                using (List<Microsoft.Xna.Framework.Rectangle>.Enumerator enumerator = rooms.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Microsoft.Xna.Framework.Rectangle r = enumerator.Current;
                        for (int x = r.X; x < r.Right; x += 2)
                        {
                            for (int y = r.Y; y < r.Bottom; y += 2)
                            {
                                if (r.Contains(x, y))
                                {
                                    base.setMapTileIndex(x, y, tileSheetIndex, "Back", 0);
                                }
                                if (r.Contains(x + 1, y))
                                {
                                    base.setMapTileIndex(x + 1, y, tileSheetIndex + 1, "Back", 0);
                                }
                                if (r.Contains(x, y + 1))
                                {
                                    base.setMapTileIndex(x, y + 1, tileSheetIndex + 16, "Back", 0);
                                }
                                if (r.Contains(x + 1, y + 1))
                                {
                                    base.setMapTileIndex(x + 1, y + 1, tileSheetIndex + 17, "Back", 0);
                                }
                            }
                        }
                    }
                    return;
                }
            }
            if (rooms.Count > whichRoom)
            {
                Microsoft.Xna.Framework.Rectangle r2 = rooms[whichRoom];
                for (int x2 = r2.X; x2 < r2.Right; x2 += 2)
                {
                    for (int y2 = r2.Y; y2 < r2.Bottom; y2 += 2)
                    {
                        if (r2.Contains(x2, y2))
                        {
                            base.setMapTileIndex(x2, y2, tileSheetIndex, "Back", 0);
                        }
                        if (r2.Contains(x2 + 1, y2))
                        {
                            base.setMapTileIndex(x2 + 1, y2, tileSheetIndex + 1, "Back", 0);
                        }
                        if (r2.Contains(x2, y2 + 1))
                        {
                            base.setMapTileIndex(x2, y2 + 1, tileSheetIndex + 16, "Back", 0);
                        }
                        if (r2.Contains(x2 + 1, y2 + 1))
                        {
                            base.setMapTileIndex(x2 + 1, y2 + 1, tileSheetIndex + 17, "Back", 0);
                        }
                    }
                }
            }
        }
    }
}
