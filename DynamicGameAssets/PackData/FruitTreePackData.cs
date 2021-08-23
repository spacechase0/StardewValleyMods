using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGameAssets.PackData
{
    public class FruitTreePackData : CommonPackData
    {
        public string Texture { get; set; }

        public bool CanGrowNow { get; set; } = false; // must be controlled using dynamic fields

        [JsonConverter( typeof( ItemAbstractionWeightedListConverter ) )]
        public List<Weighted<ItemAbstraction>> Product { get; set; }

        public override void OnDisabled()
        {
            MyUtility.iterateAllTerrainFeatures( ( tf ) =>
            {
                if ( tf is CustomFruitTree cftree )
                {
                    if ( cftree.SourcePack == parent.smapiPack.Manifest.UniqueID && cftree.Id == ID )
                        return null;
                }
                return tf;
            } );
        }

        public override Item ToItem()
        {
            return null;
        }

        public override TexturedRect GetTexture()
        {
            return parent.GetTexture( Texture, 432, 80 );
        }

        public override object Clone()
        {
            var ret = ( FruitTreePackData ) base.Clone();
            ret.Product = new List<Weighted<ItemAbstraction>>();
            foreach ( var product in this.Product )
                ret.Product.Add( ( Weighted<ItemAbstraction> ) product.Clone() );
            return ret;
        }
    }
}
