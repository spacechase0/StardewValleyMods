using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGameAssets.PackData
{
    public class BigCraftablePackData : CommonPackData
    {
        public string Texture { get; set; }

        [JsonIgnore]
        public string Name => parent.smapiPack.Translation.Get( $"big-craftable.{ID}.name" );
        [JsonIgnore]
        public string Description => parent.smapiPack.Translation.Get( $"big-craftable.{ID}.description" );

        [DefaultValue(null)]
        public int? SellPrice { get; set; }
        [DefaultValue(false)]
        public bool ForcePriceOnAllInstances { get; set; }
        [DefaultValue(false)]
        public bool ProvidesLight { get; set; }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems( ( item ) =>
            {
                if ( item is CustomBigCraftable cbc )
                {
                    if ( cbc.SourcePack == parent.smapiPack.Manifest.UniqueID && cbc.Id == ID )
                        return null;
                }
                return item;
            } );
        }

        public override Item ToItem()
        {
            return new CustomBigCraftable( this, Vector2.Zero );
        }

        public override TexturedRect GetTexture()
        {
            return parent.GetTexture(Texture, 16, 16);
        }

        public override object Clone()
        {
            return ( BigCraftablePackData ) base.Clone();
        }
    }
}
