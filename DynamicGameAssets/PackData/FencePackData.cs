using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
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

        public string Name => parent.smapiPack.Translation.Get( $"fence.{ID}.name" );
        public string Description => parent.smapiPack.Translation.Get( $"fence.{ID}.description" );

        public string ObjectTexture { get; set; }
        public string PlacedTilesheet { get; set; }

        public int MaxHealth { get; set; }
        public ItemAbstraction RepairMaterial { get; set; }

        public ToolType BreakTool { get; set; }

        public string PlacementSound { get; set; }
        public string RepairSound { get; set; }


        public override TexturedRect GetTexture()
        {
            return parent.GetTexture( ObjectTexture, 16, 16 );
        }

        public override void OnDisabled()
        {
            MyUtility.iterateAllItems( ( item ) =>
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
