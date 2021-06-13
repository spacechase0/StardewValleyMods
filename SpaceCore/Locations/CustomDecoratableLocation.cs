using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewValley.Locations;

namespace SpaceCore.Locations
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public abstract class CustomDecoratableLocation : DecoratableLocation
    {
        public new abstract List<Rectangle> getWalls();
        public new abstract List<Rectangle> getFloors();

        protected CustomDecoratableLocation()
        {
            List<Rectangle> list = this.getWalls();
            while (this.wallPaper.Count < list.Count)
            {
                this.wallPaper.Add(0);
            }
            list = this.getFloors();
            while (this.floor.Count < list.Count)
            {
                this.floor.Add(0);
            }
        }

        protected CustomDecoratableLocation(string mapPath, string name)
            : base(mapPath, name)
        {
            List<Rectangle> list = this.getWalls();
            while (this.wallPaper.Count < list.Count)
            {
                this.wallPaper.Add(0);
            }
            list = this.getFloors();
            while (this.floor.Count < list.Count)
            {
                this.floor.Add(0);
            }
        }

        protected override void doSetVisibleWallpaper(int whichRoom, int which)
        {
            List<Rectangle> rooms = this.getWalls();
            int tileSheetIndex = which % 16 + which / 16 * 48;
            if (whichRoom == -1)
            {
                using List<Rectangle>.Enumerator enumerator = rooms.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Rectangle r = enumerator.Current;
                    for (int x = r.X; x < r.Right; x++)
                    {
                        this.setMapTileIndex(x, r.Y, tileSheetIndex, "Back");
                        this.setMapTileIndex(x, r.Y + 1, tileSheetIndex + 16, "Back");
                        if (r.Height >= 3)
                        {
                            string layer = this.map.GetLayer("Buildings").Tiles[x, r.Y + 2].TileSheet.Equals(this.map.TileSheets[2])
                                ? "Buildings"
                                : "Back";
                            this.setMapTileIndex(x, r.Y + 2, tileSheetIndex + 32, layer);
                        }
                    }
                }
                return;
            }
            if (rooms.Count > whichRoom)
            {
                Rectangle r2 = rooms[whichRoom];
                for (int x2 = r2.X; x2 < r2.Right; x2++)
                {
                    this.setMapTileIndex(x2, r2.Y, tileSheetIndex, "Back");
                    this.setMapTileIndex(x2, r2.Y + 1, tileSheetIndex + 16, "Back");
                    if (r2.Height >= 3)
                    {
                        string layer = this.map.GetLayer("Buildings").Tiles[x2, r2.Y + 2].TileSheet.Equals(this.map.TileSheets[2])
                            ? "Buildings"
                            : "Back";
                        this.setMapTileIndex(x2, r2.Y + 2, tileSheetIndex + 32, layer);
                    }
                }
            }
        }

        protected override void doSetVisibleFloor(int whichRoom, int which)
        {
            List<Rectangle> rooms = this.getFloors();
            int tileSheetIndex = 336 + which % 8 * 2 + which / 8 * 32;
            if (whichRoom == -1)
            {
                using List<Rectangle>.Enumerator enumerator = rooms.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Rectangle r = enumerator.Current;
                    for (int x = r.X; x < r.Right; x += 2)
                    {
                        for (int y = r.Y; y < r.Bottom; y += 2)
                        {
                            if (r.Contains(x, y))
                            {
                                this.setMapTileIndex(x, y, tileSheetIndex, "Back");
                            }
                            if (r.Contains(x + 1, y))
                            {
                                this.setMapTileIndex(x + 1, y, tileSheetIndex + 1, "Back");
                            }
                            if (r.Contains(x, y + 1))
                            {
                                this.setMapTileIndex(x, y + 1, tileSheetIndex + 16, "Back");
                            }
                            if (r.Contains(x + 1, y + 1))
                            {
                                this.setMapTileIndex(x + 1, y + 1, tileSheetIndex + 17, "Back");
                            }
                        }
                    }
                }
                return;
            }
            if (rooms.Count > whichRoom)
            {
                Rectangle r2 = rooms[whichRoom];
                for (int x2 = r2.X; x2 < r2.Right; x2 += 2)
                {
                    for (int y2 = r2.Y; y2 < r2.Bottom; y2 += 2)
                    {
                        if (r2.Contains(x2, y2))
                        {
                            this.setMapTileIndex(x2, y2, tileSheetIndex, "Back");
                        }
                        if (r2.Contains(x2 + 1, y2))
                        {
                            this.setMapTileIndex(x2 + 1, y2, tileSheetIndex + 1, "Back");
                        }
                        if (r2.Contains(x2, y2 + 1))
                        {
                            this.setMapTileIndex(x2, y2 + 1, tileSheetIndex + 16, "Back");
                        }
                        if (r2.Contains(x2 + 1, y2 + 1))
                        {
                            this.setMapTileIndex(x2 + 1, y2 + 1, tileSheetIndex + 17, "Back");
                        }
                    }
                }
            }
        }
    }
}
