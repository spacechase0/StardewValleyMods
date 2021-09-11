using System.ComponentModel;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class BigCraftablePackData : CommonPackData
    {
        public string Texture { get; set; }

        [JsonIgnore]
        public string Name => this.pack.smapiPack.Translation.Get($"big-craftable.{this.ID}.name");

        [JsonIgnore]
        public string Description => this.pack.smapiPack.Translation.Get($"big-craftable.{this.ID}.description");

        [DefaultValue(null)]
        public int? SellPrice { get; set; }

        [DefaultValue(false)]
        public bool ForcePriceOnAllInstances { get; set; }

        [DefaultValue(false)]
        public bool ProvidesLight { get; set; }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems((item) =>
           {
               if (item is CustomBigCraftable cbc)
               {
                   if (cbc.SourcePack == this.pack.smapiPack.Manifest.UniqueID && cbc.Id == this.ID)
                       return null;
               }
               return item;
           });
        }

        public override Item ToItem()
        {
            return new CustomBigCraftable(this, Vector2.Zero);
        }

        public override TexturedRect GetTexture()
        {
            return this.pack.GetTexture(this.Texture, 16, 32);
        }

        public override object Clone()
        {
            return (BigCraftablePackData)base.Clone();
        }
    }
}
