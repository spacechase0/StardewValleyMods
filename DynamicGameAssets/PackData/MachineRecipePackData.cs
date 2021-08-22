using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class MachineRecipePackData : BasePackData
    {
        public string MachineId { get; set; }

        [JsonConverter( typeof( ItemAbstractionWeightedListConverter ) )]
        public List<Weighted<ItemAbstraction>> Result { get; set; }
        public List<ItemAbstraction> Ingredients { get; set; }
        public int MinutesToProcess { get; set; }

        public string StartWorkingSound { get; set; } = "furnace";
        public bool? WorkingLightOverride { get; set; }
        public string MachineWorkingTextureOverride { get; set; }
        public string MachineFinishedTextureOverride { get; set; }

        public override object Clone()
        {
            var ret = ( MachineRecipePackData ) base.Clone();
            ret.Result = new List<Weighted<ItemAbstraction>>();
            foreach ( var choice in Result )
                ret.Result.Add( (Weighted<ItemAbstraction>) choice.Clone() );
            ret.Ingredients = new List<ItemAbstraction>();
            foreach (var ingred in Ingredients)
                ret.Ingredients.Add(( ItemAbstraction ) ingred.Clone());
            return ret;
        }
    }
}
