using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    // Note: Unlike other things, these need to have a globally-unique ID, not pack-unique.
    public class CraftingPackData : CommonPackData
    {
        public string Name => parent.smapiPack.Translation.Get($"crafting.{ID}.name");
        public string Description => parent.smapiPack.Translation.Get($"crafting.{ID}.description");

        public bool IsCooking { get; set; } = false;

        public class IngredientAbstraction : ItemAbstraction
        {
            [XmlIgnore]
            public CraftingPackData parent;

            public string NameOverride { get; set; }
            public string IconOverride { get; set; }

            public override Texture2D Icon => IconOverride == null ? base.Icon : parent.parent.GetTexture(IconOverride, 16, 16).Texture;
            public override Rectangle IconSubrect => IconOverride == null ? base.IconSubrect : (parent.parent.GetTexture(IconOverride, 16, 16).Rect ?? new Rectangle(0, 0, Icon.Width, Icon.Height));
        }

        public ItemAbstraction Result { get; set; }
        public List<IngredientAbstraction> Ingredients { get; set; }

        public string CraftingDataKey => this.ID;
        public string CraftingDataValue => "0 1//0 1/false//" + Name;

        public override void PostLoad()
        {
            foreach (var ingred in Ingredients)
                ingred.parent = this;
        }

        public override void OnDisabled()
        {
            if (RemoveAllTracesWhenDisabled)
            {
                foreach (var farmer in Game1.getAllFarmers())
                {
                    if ( IsCooking )
                    {
                        Log.Warn("TODO - cooking recipe ondisabled w/ removealltraces");
                    }
                    else
                    {
                        farmer.craftingRecipes.Remove( CraftingDataKey );
                    }
                }
            }
        }

        public override Item ToItem()
        {
            return new CustomCraftingRecipe( this );
        }

        public override TexturedRect GetTexture()
        {
            return null;
        }

        public override object Clone()
        {
            var ret = ( CraftingPackData ) base.Clone();
            ret.Result = ( ItemAbstraction ) Result.Clone();
            ret.Ingredients = new List<IngredientAbstraction>();
            foreach (var ingred in Ingredients)
                ret.Ingredients.Add((IngredientAbstraction) ingred.Clone());
            return ret;
        }
    }
}
