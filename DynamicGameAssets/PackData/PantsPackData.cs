using System.ComponentModel;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class PantsPackData : CommonPackData
    {
        [JsonIgnore]
        public string Name => this.pack.smapiPack.Translation.Get($"pants.{this.ID}.name");

        [JsonIgnore]
        public string Description => this.pack.smapiPack.Translation.Get($"pants.{this.ID}.description");

        public string Texture { get; set; }

        public Color DefaultColor { get; set; } = Color.White;

        [DefaultValue(false)]
        public bool Dyeable { get; set; } = false;

        public bool ShouldSerializeDefaultColor() { return this.DefaultColor != Color.White; }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems((item) =>
            {
                if (item is CustomPants cpants)
                {
                    if (cpants.SourcePack == this.pack.smapiPack.Manifest.UniqueID && cpants.Id == this.ID)
                        return null;
                }
                return item;
            });
        }

        public override Item ToItem()
        {
            return new CustomPants(this);
        }

        public override TexturedRect GetTexture()
        {
            var ret = this.pack.GetTexture(this.Texture, 192, 688);
            ret.Rect ??= new Rectangle(0, 0, ret.Texture.Width, ret.Texture.Height);
            ret.Rect = new Rectangle(ret.Rect.Value.X, ret.Rect.Value.Y + 672, 16, 16);
            return ret;
        }
    }
}
