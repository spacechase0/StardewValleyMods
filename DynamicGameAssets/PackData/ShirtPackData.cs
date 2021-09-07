using System.ComponentModel;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class ShirtPackData : CommonPackData
    {
        [JsonIgnore]
        public string Name => this.pack.smapiPack.Translation.Get($"shirt.{this.ID}.name");

        [JsonIgnore]
        public string Description => this.pack.smapiPack.Translation.Get($"shirt.{this.ID}.description");

        public string TextureMale { get; set; }

        [DefaultValue(null)]
        public string TextureMaleColor { get; set; }

        [DefaultValue(null)]
        public string TextureFemale { get; set; }

        [DefaultValue(null)]
        public string TextureFemaleColor { get; set; }

        public Color DefaultColor { get; set; } = Color.White;

        [DefaultValue(false)]
        public bool Dyeable { get; set; } = false;

        public bool ShouldSerializeDefaultColor() { return this.DefaultColor != Color.White; }

        [DefaultValue(false)]
        public bool Sleeveless { get; set; } = false;

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems((item) =>
            {
                if (item is CustomShirt cshirt)
                {
                    if (cshirt.SourcePack == this.pack.smapiPack.Manifest.UniqueID && cshirt.Id == this.ID)
                        return null;
                }
                return item;
            });
        }

        public override Item ToItem()
        {
            return new CustomShirt(this);
        }

        public override TexturedRect GetTexture()
        {
            return this.pack.GetTexture(this.TextureMale, 8, 32);
        }
    }
}
