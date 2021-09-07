using System.Xml.Serialization;

namespace DynamicGameAssets.Game
{
    public interface IDGAItem
    {
        [XmlIgnore]
        string SourcePack { get; }
        [XmlIgnore]
        string Id { get; }
        [XmlIgnore]
        string FullId { get; }
    }
}
