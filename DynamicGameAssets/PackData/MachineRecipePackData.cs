using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ContentPatcher;
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
        //internal IManagedConditions liveConditionsObj;

        public string MachineId { get; set; }

        [JsonConverter( typeof( ItemAbstractionWeightedListConverter ) )]
        public List<Weighted<ItemAbstraction>> Result { get; set; }
        public List<ItemAbstraction> Ingredients { get; set; }
        public int MinutesToProcess { get; set; }

        /*private Dictionary<string, string> _liveConditions = new Dictionary<string, string>();
        public Dictionary<string, string> LiveConditions // TODO: Better name
        {
            get { return _liveConditions; }
            set
            {
                _liveConditions = value;
                if ( parent != null )
                    liveConditionsObj = Mod.instance.cp.ParseConditions( parent.smapiPack.Manifest, LiveConditions, parent.conditionVersion );
            }
        }*/

        [DefaultValue( "furnace" )]
        public string StartWorkingSound { get; set; } = "furnace";
        [DefaultValue( null )]
        public bool? WorkingLightOverride { get; set; }
        [DefaultValue( null )]
        public string MachineWorkingTextureOverride { get; set; }
        [DefaultValue( null )]
        public string MachineFinishedTextureOverride { get; set; }

        public override void PostLoad()
        {
            //LiveConditions = LiveConditions;
        }

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
