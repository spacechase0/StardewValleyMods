using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
    public class CropPackData : CommonPackData
    {
        public enum CropType
        {
            Normal,
            Indoors,
            Paddy,
        }
        public CropType Type { get; set; }

        public bool CanGrowNow { get; set; } = true; // must be controlled using dynamic fields

        public class HarvestedDropData : ICloneable
        {
            public int MininumHarvestedQuantity { get; set; } = 1;
            public int MaximumHarvestedQuantity { get; set; } = 1;
            public float ExtraQuantityChance { get; set; }

            public ItemAbstraction Item { get; set; }

            public object Clone()
            {
                var ret = ( HarvestedDropData ) this.MemberwiseClone();
                ret.Item = ( ItemAbstraction ) this.Item.Clone();
                return ret;
            }
        }

        public class PhaseData : ICloneable
        {
            public string[] TextureChoices { get; set; }
            // TODO: Color texture
            public int Length { get; set; }

            public bool Scythable { get; set; }
            public bool Trellis { get; set; }

            public List<HarvestedDropData> HarvestedDrops { get; set; } = new List<HarvestedDropData>();
            public int HarvestedExperience { get; set; } = 0;
            public int HarvestedNewPhase { get; set; } = -1;
            // TODO: Color texture

            public object Clone()
            {
                var ret = ( PhaseData ) this.MemberwiseClone();
                ret.HarvestedDrops = new List<HarvestedDropData>();
                foreach ( var drop in this.HarvestedDrops )
                    ret.HarvestedDrops.Add( ( HarvestedDropData ) drop.Clone() );
                return ret;
            }
        }
        public List<PhaseData> Phases { get; set; } = new List<PhaseData>();

        public float GiantChance { get; set; } = 0.01f;
        public string[] GiantTextureChoices { get; set; }
        public List<HarvestedDropData> GiantDrops { get; set; } = new List<HarvestedDropData>();

        public override void OnDisabled()
        {
            MyUtility.iterateAllTerrainFeatures( ( tf ) =>
            {
                if ( tf is HoeDirt hd && hd.crop is CustomCrop ccrop )
                {
                    if ( ccrop.SourcePack == parent.smapiPack.Manifest.UniqueID && ccrop.Id == ID )
                        hd.crop = null;
                }
                else if ( tf is CustomGiantCrop cgc )
                {
                    if ( cgc.SourcePack == parent.smapiPack.Manifest.UniqueID && cgc.Id == ID )
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
            return parent.GetMultiTexture( Phases[ ^1 ].TextureChoices, 0, 16, 32 );
        }

        public override object Clone()
        {
            var ret = ( CropPackData ) base.Clone();
            ret.Phases = new List<PhaseData>();
            foreach ( var drop in this.Phases )
                ret.Phases.Add( ( PhaseData ) drop.Clone() );
            ret.GiantDrops = new List<HarvestedDropData>();
            foreach ( var drop in this.GiantDrops )
                ret.GiantDrops.Add( ( HarvestedDropData ) drop.Clone() );
            return ret;
        }
    }
}
