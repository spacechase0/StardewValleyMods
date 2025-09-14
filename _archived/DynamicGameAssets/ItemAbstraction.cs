using System;
using System.Collections.Generic;
using System.ComponentModel;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpaceShared;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets
{
    public class ItemAbstraction : ICloneable
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ItemType
        {
            DGAItem,
            DGARecipe,
            VanillaObject,
            VanillaObjectColored,
            VanillaBigCraftable,
            VanillaWeapon,
            VanillaHat,
            VanillaClothing,
            VanillaBoots,
            VanillaFurniture,
            ContextTag, // recipes only
            // Missing anything?

            // Must be specified as: package.name.ClassName/constructionArgument
            // Must have name override and icon override when used in crafting recipe
            // Must have a constructor taking a single string argument when used as product
            // Must have static IngredientMatches(Item) function if used as ingredient
            Custom, 
        }

        [DefaultValue(ItemType.DGAItem)]
        public ItemType Type { get; set; } = ItemType.DGAItem;

        public string Value { get; set; }

        [DefaultValue(1)]
        public int Quantity { get; set; } = 1;

        public Color ObjectColor { get; set; }

        public bool ShouldSerializeObjectColor() { return this.ObjectColor != default; }

        [JsonIgnore]
        public virtual Texture2D Icon
        {
            get
            {
                int? valAsInt = null;
                if (int.TryParse(this.Value, out int x))
                    valAsInt = x;

                switch (this.Type)
                {
                    case ItemType.DGAItem: return Mod.Find(this.Value)?.GetTexture()?.Texture ?? Game1.staminaRect;
                    case ItemType.DGARecipe:
                        Log.Error("Crafting recipes don't have an icon texture");
                        return null;
                    case ItemType.VanillaObject:
                    case ItemType.VanillaObjectColored:
                        return Game1.objectSpriteSheet;
                    case ItemType.VanillaBigCraftable: return Game1.bigCraftableSpriteSheet;
                    case ItemType.VanillaWeapon: return Tool.weaponsTexture;
                    case ItemType.VanillaHat: return FarmerRenderer.hatsTexture;
                    case ItemType.VanillaClothing:
                        if (valAsInt.HasValue)
                            return new Clothing(valAsInt.Value).clothesType.Value == (int)Clothing.ClothesType.SHIRT ? FarmerRenderer.shirtsTexture : FarmerRenderer.pantsTexture;
                        foreach (var info in Game1.content.Load<Dictionary<int, string>>("Data\\ClothingInformation"))
                        {
                            if (info.Value.Split('/')[0] == this.Value)
                                return new Clothing(info.Key).clothesType.Value == (int)Clothing.ClothesType.SHIRT ? FarmerRenderer.shirtsTexture : FarmerRenderer.pantsTexture;
                        }
                        break;
                    case ItemType.VanillaBoots: return Game1.objectSpriteSheet;
                    case ItemType.VanillaFurniture: return Furniture.furnitureTexture;
                    case ItemType.ContextTag:
                        Log.Error("Context tag ItemAbstraction instances don't have an icon texture");
                        return null;
                    case ItemType.Custom:
                        Log.Error("Custom ItemAbstraction instances don't have an icon texture");
                        return null;
                }

                Log.Error("Failed getting ItemAbstraction icon for " + this.Type + " " + this.Value + "!");
                return null;
            }
        }

        [JsonIgnore]
        public virtual Rectangle IconSubrect
        {
            get
            {
                int? valAsInt = null;
                if (int.TryParse(this.Value, out int x))
                    valAsInt = x;

                switch (this.Type)
                {
                    case ItemType.DGAItem:
                        var found = Mod.Find(this.Value);
                        if (found == null)
                            return new Rectangle(0, 0, 1, 1);
                        return found.GetTexture().Rect ?? new Rectangle(0, 0, found.GetTexture().Texture.Width, found.GetTexture().Texture.Height);
                    case ItemType.DGARecipe:
                        Log.Error("Recipes don't have an icon subrect.");
                        return default;
                    case ItemType.VanillaObject:
                    case ItemType.VanillaObjectColored:
                        if (valAsInt.HasValue)
                        {
                            var dummy = new CraftingRecipe("Torch");
                            return Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, dummy.getSpriteIndexFromRawIndex(valAsInt.Value), 16, 16);
                        }
                        foreach (var info in Game1.objectInformation)
                        {
                            if (info.Value.Split('/')[StardewValley.Object.objectInfoNameIndex] == this.Value)
                                return Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, info.Key, 16, 16);
                        }
                        break;
                    case ItemType.VanillaBigCraftable:
                        if (valAsInt.HasValue)
                            return Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, valAsInt.Value, 16, 32);
                        foreach (var info in Game1.bigCraftablesInformation)
                        {
                            if (info.Value.Split('/')[StardewValley.Object.objectInfoNameIndex] == this.Value)
                                return Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, info.Key, 16, 32);
                        }
                        break;
                    case ItemType.VanillaWeapon:
                        if (valAsInt.HasValue)
                            return Game1.getSourceRectForStandardTileSheet(Tool.weaponsTexture, valAsInt.Value, 16, 16);
                        foreach (var info in Game1.content.Load<Dictionary<int, string>>("Data\\weapons"))
                        {
                            if (info.Value.Split('/')[0] == this.Value)
                                return Game1.getSourceRectForStandardTileSheet(Tool.weaponsTexture, info.Key, 16, 16);
                        }
                        break;
                    case ItemType.VanillaHat:
                        if (valAsInt.HasValue)
                            return new Rectangle((int)valAsInt.Value * 20 % FarmerRenderer.hatsTexture.Width, (int)valAsInt.Value * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20);
                        foreach (var info in Game1.content.Load<Dictionary<int, string>>("Data\\hats"))
                        {
                            if (info.Value.Split('/')[0] == this.Value)
                                return new Rectangle((int)info.Key * 20 % FarmerRenderer.hatsTexture.Width, (int)info.Key * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20);
                        }
                        break;
                    case ItemType.VanillaClothing:
                        // 
                        // 
                        Clothing clothing = null;
                        if (valAsInt.HasValue)
                            clothing = new Clothing(valAsInt.Value);
                        foreach (var info in Game1.content.Load<Dictionary<int, string>>("Data\\ClothingInformation"))
                        {
                            if (info.Value.Split('/')[0] == this.Value)
                                clothing = new Clothing(info.Key);
                        }
                        if (clothing != null)
                        {
                            return clothing.clothesType.Value == (int)Clothing.ClothesType.SHIRT ?
                                new Rectangle(clothing.indexInTileSheetMale.Value * 8 % 128, clothing.indexInTileSheetMale.Value * 8 / 128 * 32, 8, 8) :
                                new Rectangle(192 * (clothing.indexInTileSheetMale.Value % (FarmerRenderer.pantsTexture.Width / 192)), 688 * (clothing.indexInTileSheetMale.Value / (FarmerRenderer.pantsTexture.Width / 192)) + 672, 16, 16);
                        }
                        break;
                    case ItemType.VanillaBoots:
                        if (valAsInt.HasValue)
                            return Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, valAsInt.Value, 16, 16);
                        foreach (var info in Game1.content.Load<Dictionary<int, string>>("Data\\Boots"))
                        {
                            if (info.Value.Split('/')[0] == this.Value)
                                return Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, info.Key, 16, 16);
                        }
                        break;
                    case ItemType.VanillaFurniture:
                        Furniture furniture = null;
                        if (valAsInt.HasValue)
                            furniture = Furniture.GetFurnitureInstance(valAsInt.Value);
                        foreach (var info in Game1.content.Load<Dictionary<int, string>>("Data\\Furniture"))
                        {
                            if (info.Value.Split('/')[0] == this.Value)
                                furniture = Furniture.GetFurnitureInstance(info.Key);
                        }
                        if (furniture != null)
                            return furniture.defaultSourceRect.Value;
                        break;
                    case ItemType.ContextTag:
                        Log.Error("Context tag ItemAbstraction instances have no icon rect!");
                        return default;
                    case ItemType.Custom:
                        Log.Error( "Custom ItemAbstraction instances have no icon rect!" );
                        return default;
                }

                Log.Error("Failed getting ItemAbstraction icon rect for " + this.Type + " " + this.Value + "!");
                return default;
            }
        }

        public bool Matches(Item item)
        {
            int? valAsInt = null;
            if (int.TryParse(this.Value, out int x))
                valAsInt = x;

            switch (this.Type)
            {
                case ItemType.DGAItem:
                    return (item is IDGAItem ditem && ditem.FullId == this.Value);
                case ItemType.DGARecipe:
                    return false;
                case ItemType.VanillaObject:
                    return (item is StardewValley.Object obj && !obj.bigCraftable.Value && (obj.Name == this.Value || (valAsInt.HasValue && (valAsInt.Value == obj.ParentSheetIndex || valAsInt.Value == obj.Category))));
                case ItemType.VanillaObjectColored:
                    return (item is ColoredObject cobj && cobj.color.Value == this.ObjectColor && (cobj.Name == this.Value || (valAsInt.HasValue && (valAsInt.Value == cobj.ParentSheetIndex || valAsInt.Value == cobj.Category))));
                case ItemType.VanillaBigCraftable:
                    return (item is StardewValley.Object bobj && bobj.bigCraftable.Value && (bobj.Name == this.Value || (valAsInt.HasValue && valAsInt.Value == bobj.ParentSheetIndex)));
                case ItemType.VanillaWeapon:
                    return (item is StardewValley.Tools.MeleeWeapon weapon && (weapon.Name == this.Value || (valAsInt.HasValue && valAsInt.Value == weapon.InitialParentTileIndex)));
                case ItemType.VanillaHat:
                    return (item is Hat hat && (hat.Name == this.Value || (valAsInt.HasValue && valAsInt.Value == hat.which.Value)));
                case ItemType.VanillaClothing:
                    return (item is Clothing clothing && (clothing.Name == this.Value || (valAsInt.HasValue && valAsInt.Value == clothing.ParentSheetIndex)));
                case ItemType.VanillaBoots:
                    return (item is Boots boots && (boots.Name == this.Value || (valAsInt.HasValue && valAsInt.Value == boots.indexInTileSheet.Value)));
                case ItemType.VanillaFurniture:
                    return (item is Furniture furniture && (furniture.Name == this.Value || (valAsInt.HasValue && valAsInt.Value == furniture.ParentSheetIndex)));
                case ItemType.ContextTag:
                    return item?.HasContextTag(this.Value) ?? false;
                case ItemType.Custom:
                    string type = Value.Substring( 0, Value.IndexOf( '/' ) );
                    string arg = Value.Substring( Value.IndexOf( '/' ) + 1 );
                    var meth = AccessTools.Method( AccessTools.TypeByName( type ), "IngredientMatches", new[] { typeof( Item ), typeof( string ) } );
                    if ( meth == null )
                    {
                        Log.Error( "Custom ItemAbstraction instance must have an IngredientMatches method! " + Value );
                        return false;
                    }
                    return ( bool ) meth.Invoke( null, new object[] { item, arg } );
            }

            Log.Error("Unknown ItemAbstraction type?");
            return false;
        }

        public Item Create()
        {
            int? valAsInt = null;
            if (int.TryParse(this.Value, out int x))
                valAsInt = x;

            switch (this.Type)
            {
                case ItemType.DGAItem:
                    {
                        var ret = Mod.Find(this.Value)?.ToItem();
                        if (ret == null)
                        {
                            Log.Error($"Failed to create item for {this.Value}! Does it exist and is an item (ie. not a crop or fruit tree or something)?");
                            return new StardewValley.Object(Mod.BaseFakeObjectId, 1);
                        }
                        if (ret is CustomObject obj && this.ObjectColor.A > 0)
                            obj.ObjectColor = this.ObjectColor;
                        if (this.Quantity > 1)
                        {
                            if (ret.maximumStackSize() < 1)
                            {
                                Log.Warn($"Recipe trying to stack {this.Value}, but it is not stackable. Defaulting to producing 1 item.");
                            }
                            else if (this.Quantity > ret.maximumStackSize())
                            {
                                Log.Warn($"Recipe would produce more than the maximum stack size of {this.Value}, defaulting to maximum stack size.");
                                ret.Stack = ret.maximumStackSize();
                            }
                            else
                            {
                                ret.Stack = this.Quantity;
                            }
                        }
                        return ret;
                    }

                case ItemType.DGARecipe:
                    {
                        CommonPackData data = Mod.Find(this.Value);

                        if (data is not CraftingRecipePackData recipeData)
                        {
                            Log.Error($"Failed to create recipe for '{this.Value}': " + (data is null
                                ? "no such data could be found."
                                : $"unexpected data type {data.GetType().Name}, expected {nameof(CraftingRecipePackData)}."
                            ));
                            return new StardewValley.Object(Mod.BaseFakeObjectId, 1);
                        }

                        return new CustomCraftingRecipe(recipeData);
                    }

                case ItemType.VanillaObject:
                    if (valAsInt.HasValue)
                        return new StardewValley.Object(valAsInt.Value, this.Quantity);
                    foreach (var info in Game1.objectInformation)
                    {
                        if (info.Value.Split('/')[StardewValley.Object.objectInfoNameIndex] == this.Value)
                            return new StardewValley.Object(info.Key, this.Quantity);
                    }
                    break;

                case ItemType.VanillaObjectColored:
                    if (valAsInt.HasValue)
                        return new ColoredObject(valAsInt.Value, this.Quantity, this.ObjectColor);
                    foreach (var info in Game1.objectInformation)
                    {
                        if (info.Value.Split('/')[StardewValley.Object.objectInfoNameIndex] == this.Value)
                            return new ColoredObject(info.Key, this.Quantity, this.ObjectColor);
                    }
                    break;

                case ItemType.VanillaBigCraftable:
                    if (valAsInt.HasValue)
                        return new StardewValley.Object(Vector2.Zero, valAsInt.Value) { Stack = this.Quantity };
                    foreach (var info in Game1.bigCraftablesInformation)
                    {
                        if (info.Value.Split('/')[StardewValley.Object.objectInfoNameIndex] == this.Value)
                            return new StardewValley.Object(Vector2.Zero, info.Key) { Stack = this.Quantity };
                    }
                    break;

                case ItemType.VanillaWeapon:
                    if (valAsInt.HasValue)
                        return new StardewValley.Tools.MeleeWeapon(valAsInt.Value);
                    foreach (var info in Game1.content.Load<Dictionary<int, string>>("Data\\weapons"))
                    {
                        if (info.Value.Split('/')[0] == this.Value)
                            return new StardewValley.Tools.MeleeWeapon(info.Key);
                    }
                    break;

                case ItemType.VanillaHat:
                    if (valAsInt.HasValue)
                        return new Hat(valAsInt.Value);
                    foreach (var info in Game1.content.Load<Dictionary<int, string>>("Data\\hats"))
                    {
                        if (info.Value.Split('/')[0] == this.Value)
                            return new Hat(info.Key);
                    }
                    break;

                case ItemType.VanillaClothing:
                    if (valAsInt.HasValue)
                        return new Clothing(valAsInt.Value);
                    foreach (var info in Game1.content.Load<Dictionary<int, string>>("Data\\ClothingInformation"))
                    {
                        if (info.Value.Split('/')[0] == this.Value)
                            return new Clothing(info.Key);
                    }
                    break;

                case ItemType.VanillaBoots:
                    if (valAsInt.HasValue)
                        return new Boots(valAsInt.Value);
                    foreach (var info in Game1.content.Load<Dictionary<int, string>>("Data\\Boots"))
                    {
                        if (info.Value.Split('/')[0] == this.Value)
                            return new Boots(info.Key);
                    }
                    break;

                case ItemType.VanillaFurniture:
                    if (valAsInt.HasValue)
                        return Furniture.GetFurnitureInstance(valAsInt.Value);
                    foreach (var info in Game1.content.Load<Dictionary<int, string>>("Data\\Furniture"))
                    {
                        if (info.Value.Split('/')[0] == this.Value)
                            return Furniture.GetFurnitureInstance(info.Key);
                    }
                    break;

                case ItemType.ContextTag:
                    Log.Error("Context tag ItemAbstraction instances cannot be created!");
                    return new StardewValley.Object(Mod.BaseFakeObjectId, 1);
                case ItemType.Custom:
                    string type = Value.Substring( 0, Value.IndexOf( '/' ) );
                    string arg = Value.Substring( Value.IndexOf( '/' ) + 1 );
                    var ctor = AccessTools.Constructor( AccessTools.TypeByName( type ), new[] { typeof( string ) } );
                    if ( ctor == null )
                    {
                        Log.Error( "Custom ItemAbstraction instance was unable be created! " + Value );
                        return new StardewValley.Object( 1720, 1 );
                    }
                    return ( Item ) ctor.Invoke( new object[] { arg } );
            }

            Log.Error($"Unknown item {this.Type} {this.Value} x {this.Quantity}");
            return new StardewValley.Object(Mod.BaseFakeObjectId, 1);
        }

        public virtual object Clone() => this.MemberwiseClone();
    }

    public class ItemAbstractionWeightedListConverter : JsonConverter<List<Weighted<ItemAbstraction>>>
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override List<Weighted<ItemAbstraction>> ReadJson(JsonReader reader, Type objectType, List<Weighted<ItemAbstraction>> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var ret = new List<Weighted<ItemAbstraction>>();
            if (reader.TokenType == JsonToken.StartObject)
            {
                ret.Add(new Weighted<ItemAbstraction>(1.0, serializer.Deserialize<ItemAbstraction>(reader)));
            }
            else
            {
                ret = serializer.Deserialize<List<Weighted<ItemAbstraction>>>(reader);
            }
            return ret;
        }

        public override void WriteJson(JsonWriter writer, List<Weighted<ItemAbstraction>> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
