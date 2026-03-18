using System.Drawing;
using SpaceShared.Attributes;

namespace Stardew3D.DataModels;

[CustomDictionaryAsset("WallDefinitions")]
public partial class WallDefinitionData
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
