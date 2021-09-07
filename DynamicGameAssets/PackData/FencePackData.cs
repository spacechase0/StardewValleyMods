using System.ComponentModel;
using DynamicGameAssets.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpaceShared;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class FencePackData : CommonPackData
    {
        public enum ToolType
        {
            Axe,
            Pickaxe,
        }

        [JsonIgnore]
        public string Name => this.pack.smapiPack.Translation.Get($"fence.{this.ID}.name");

        [JsonIgnore]
        public string Description => this.pack.smapiPack.Translation.Get($"fence.{this.ID}.description");

        public string ObjectTexture { get; set; }
        public string PlacedTilesheet { get; set; }

        public int MaxHealth { get; set; }
        public ItemAbstraction RepairMaterial { get; set; }

        [DefaultValue(ToolType.Axe)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ToolType BreakTool { get; set; }

        public string PlacementSound { get; set; }
        public string RepairSound { get; set; }

        public override TexturedRect GetTexture()
        {
            return this.pack.GetTexture(this.ObjectTexture, 16, 16);
        }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems((item) =>
           {
               if (item is CustomFence cfence)
               {
                   if (cfence.SourcePack == this.pack.smapiPack.Manifest.UniqueID && cfence.Id == this.ID)
                       return null;
               }
               return item;
           });
        }

        public override Item ToItem()
        {
            return new CustomFence(this);
        }
    }
}
