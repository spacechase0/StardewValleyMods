using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public string Name => parent.smapiPack.Translation.Get( $"fence.{ID}.name" );
        [JsonIgnore]
        public string Description => parent.smapiPack.Translation.Get( $"fence.{ID}.description" );

        public string ObjectTexture { get; set; }
        public string PlacedTilesheet { get; set; }

        public int MaxHealth { get; set; }
        public ItemAbstraction RepairMaterial { get; set; }

        [DefaultValue( ToolType.Axe )]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public ToolType BreakTool { get; set; }

        public string PlacementSound { get; set; }
        public string RepairSound { get; set; }


        public override TexturedRect GetTexture()
        {
            return parent.GetTexture( ObjectTexture, 16, 16 );
        }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems( ( item ) =>
            {
                if ( item is CustomFence cfence )
                {
                    if ( cfence.SourcePack == parent.smapiPack.Manifest.UniqueID && cfence.Id == ID )
                        return null;
                }
                return item;
            } );
        }

        public override Item ToItem()
        {
            return new CustomFence( this );
        }
    }
}
