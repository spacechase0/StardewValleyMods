using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew3D.Data;

public class WallDefinitionData
{
    public class WallSegmentData
    {
        public enum SegmentContinuationMode
        {
            StretchIfNeeded,
            Stretch,
            Tile,
        }

        public string Tilesheet { get; set; }
        public Rectangle TextureRegion { get; set; }

        public SegmentContinuationMode ContinuationMode { get; set; } = SegmentContinuationMode.StretchIfNeeded;
    }

    public List<WallSegmentData> VerticalSegments { get; set; } = new();
}
