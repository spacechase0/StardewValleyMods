using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    // Note: Unlike other things, these need to have a globally-unique ID, not pack-unique.
    public class CraftingRecipePackData : CommonPackData
    {
        [JsonIgnore]
        public string Name => this.pack.smapiPack.Translation.Get($"crafting.{this.ID}.name");

        [JsonIgnore]
        public string Description => this.pack.smapiPack.Translation.Get($"crafting.{this.ID}.description");

        [DefaultValue(false)]
        public bool IsCooking { get; set; } = false;

        [DefaultValue(false)]
        public bool KnownByDefault { get; set; } = false;

        [DefaultValue(null)]
        public string SkillUnlockName { get; set; }

        [DefaultValue(0)]
        public int SkillUnlockLevel { get; set; }

        public class IngredientAbstraction : ItemAbstraction
        {
            [XmlIgnore]
            public CraftingRecipePackData parent;

            [DefaultValue(null)]
            public string NameOverride { get; set; }

            [DefaultValue(null)]
            public string IconOverride { get; set; }

            public override Texture2D Icon => this.IconOverride == null ? base.Icon : this.parent.pack.GetTexture(this.IconOverride, 16, 16).Texture;
            public override Rectangle IconSubrect => this.IconOverride == null ? base.IconSubrect : (this.parent.pack.GetTexture(this.IconOverride, 16, 16).Rect ?? new Rectangle(0, 0, this.Icon.Width, this.Icon.Height));
        }

        [JsonConverter(typeof(ItemAbstractionWeightedListConverter))]
        public List<Weighted<ItemAbstraction>> Result { get; set; }
        public List<IngredientAbstraction> Ingredients { get; set; }

        [JsonIgnore]
        public string CraftingDataKey => this.ID;

        [JsonIgnore]
        public string CraftingDataValue => "0 1/meow/0 1/" + (!this.IsCooking ? (this.Result[0].Value.IconSubrect.Size.Y > 20 ? "true/" : "false/") : string.Empty) + $"{this.SkillUnlockName} {this.SkillUnlockLevel}/{this.Name}";

        public override void PostLoad()
        {
            foreach (var ingred in this.Ingredients)
                ingred.parent = this;
        }

        public override void OnDisabled()
        {
            if (this.RemoveAllTracesWhenDisabled)
            {
                foreach (var farmer in Game1.getAllFarmers())
                {
                    (this.IsCooking ? farmer.cookingRecipes : farmer.craftingRecipes).Remove(this.CraftingDataKey);
                }
            }
        }

        public override Item ToItem()
        {
            return new CustomCraftingRecipe(this);
        }

        public override TexturedRect GetTexture()
        {
            return null;
        }

        public override object Clone()
        {
            var ret = (CraftingRecipePackData)base.Clone();
            ret.Result = new List<Weighted<ItemAbstraction>>();
            foreach (var choice in this.Result)
                ret.Result.Add((Weighted<ItemAbstraction>)choice.Clone());
            ret.Ingredients = new List<IngredientAbstraction>();
            foreach (var ingred_ in this.Ingredients)
            {
                var ingred = (IngredientAbstraction)ingred_.Clone();
                ingred.parent = ret;
                ret.Ingredients.Add(ingred);
            }
            return ret;
        }
    }
}
