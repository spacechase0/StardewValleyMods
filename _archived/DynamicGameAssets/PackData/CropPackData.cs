using System;
using System.Collections.Generic;
using System.ComponentModel;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpaceShared;
using StardewValley;
using StardewValley.TerrainFeatures;

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

        [DefaultValue(CropType.Normal)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CropType Type { get; set; }

        [DefaultValue(false)]
        public bool CanGrowNow { get; set; } = false; // must be controlled using dynamic fields

        public class HarvestedDropData : ICloneable
        {
            [DefaultValue(1)]
            public int MininumHarvestedQuantity { get; set; } = 1;

            [DefaultValue(1)]
            public int MaximumHarvestedQuantity { get; set; } = 1;

            [DefaultValue(0)]
            public double ExtraQuantityChance { get; set; }

            [JsonConverter(typeof(ItemAbstractionWeightedListConverter))]
            public List<Weighted<ItemAbstraction>> Item { get; set; }

            public object Clone()
            {
                var ret = (HarvestedDropData)this.MemberwiseClone();
                ret.Item = new List<Weighted<ItemAbstraction>>();
                foreach (var choice in this.Item)
                    ret.Item.Add((Weighted<ItemAbstraction>)choice.Clone());
                return ret;
            }
        }

        [DefaultValue(null)]
        public List<Color> Colors { get; set; }

        public class PhaseData : ICloneable
        {
            public string[] TextureChoices { get; set; }

            [DefaultValue(null)]
            public string[] TextureColorChoices { get; set; }

            public int Length { get; set; }

            [DefaultValue(false)]
            public bool Scythable { get; set; }

            [DefaultValue(false)]
            public bool Trellis { get; set; }

            public List<HarvestedDropData> HarvestedDrops { get; set; } = new();
            public int HarvestedExperience { get; set; } = 0;

            [DefaultValue(-1)]
            public int HarvestedNewPhase { get; set; } = -1;

            public bool ShouldSerializeHarvestedDrops() { return this.HarvestedDrops.Count > 0; }

            public object Clone()
            {
                var ret = (PhaseData)this.MemberwiseClone();
                ret.HarvestedDrops = new List<HarvestedDropData>();
                foreach (var drop in this.HarvestedDrops)
                    ret.HarvestedDrops.Add((HarvestedDropData)drop.Clone());
                return ret;
            }
        }
        public List<PhaseData> Phases { get; set; } = new();

        [DefaultValue(0.01f)]
        public float GiantChance { get; set; } = 0.01f;

        [DefaultValue(null)]
        public string[] GiantTextureChoices { get; set; }

        public List<HarvestedDropData> GiantDrops { get; set; } = new();

        public bool ShouldSerializeGiantDrops() { return this.GiantDrops.Count > 0; }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllTerrainFeatures((tf) =>
            {
                if (tf is HoeDirt hd && hd.crop is CustomCrop ccrop)
                {
                    if (ccrop.SourcePack == this.pack.smapiPack.Manifest.UniqueID && ccrop.Id == this.ID)
                        hd.crop = null;
                }
                else if (tf is CustomGiantCrop cgc)
                {
                    if (cgc.SourcePack == this.pack.smapiPack.Manifest.UniqueID && cgc.Id == this.ID)
                        return null;
                }
                return tf;
            });
        }

        public override Item ToItem()
        {
            return null;
        }

        public override TexturedRect GetTexture()
        {
            return this.pack.GetMultiTexture(this.Phases[this.Phases.Count - 1].TextureChoices, 0, 16, 32);
        }

        public override object Clone()
        {
            var ret = (CropPackData)base.Clone();
            if (ret.Colors != null)
            {
                ret.Colors = new List<Color>();
                foreach (var color in this.Colors)
                    ret.Colors.Add(color);
            }
            ret.Phases = new List<PhaseData>();
            foreach (var drop in this.Phases)
                ret.Phases.Add((PhaseData)drop.Clone());
            ret.GiantDrops = new List<HarvestedDropData>();
            foreach (var drop in this.GiantDrops)
                ret.GiantDrops.Add((HarvestedDropData)drop.Clone());
            return ret;
        }
    }
}
