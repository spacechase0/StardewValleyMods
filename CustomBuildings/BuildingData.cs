using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;

namespace CustomBuildings
{
    public class BuildingData
    {
        [JsonIgnore]
        public Texture2D texture;

        [JsonIgnore]
        public Func<xTile.Map> mapLoader;

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int BuildingHeight { get; set; } = 16;
        public int TileWidth { get; set; } = 1;
        public int TileHeight { get; set; } = 1;
        public int MenuWidth { get; set; } = 16;
        public int MenuHeight { get; set; } = 16;

        public int HumanDoorX { get; set; } = 0;
        public int HumanDoorY { get; set; } = 0;

        public bool HousesAnimals { get; set; } = false;
        public int MaxOccupants { get; set; } = -1;
        public int FeedHopperX { get; set; } = -1;
        public int FeedHopperY { get; set; } = -1;
        public int IncubatorX { get; set; } = -1;
        public int IncubatorY { get; set; } = -1;
        public int AnimalDoorX { get; set; } = -1;
        public int AnimalDoorY { get; set; } = -1;
        public int AnimalDoorWidth { get; set; } = -1;
        public int AnimalDoorHeight { get; set; } = -1;
        public bool AutoFeedsAnimals = false;

        public string PreviousTier { get; set; }
        public Dictionary<Rectangle, Rectangle> MoveObjectsWhenUpgradedTo { get; set; }
        public int UpgradeSignX { get; set; }
        public int UpgradeSignY { get; set; }

        public class Ingredient
        {
            public object Object { get; set; }
            public int Count { get; set; }
        }
        public int Price { get; set; } = 0;
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        public bool Magical { get; set; } = false;
        public int DaysToConstruct { get; set; } = 2;

        public Dictionary<string, string> NameLocalization = new Dictionary<string, string>();
        public Dictionary<string, string> DescriptionLocalization = new Dictionary<string, string>();

        public string LocalizedName()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            if (currLang == LocalizedContentManager.LanguageCode.en)
                return Name;
            if (NameLocalization == null || !NameLocalization.ContainsKey(currLang.ToString()))
                return Name;
            return NameLocalization[currLang.ToString()];
        }

        public string LocalizedDescription()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            if (currLang == LocalizedContentManager.LanguageCode.en)
                return Description;
            if (DescriptionLocalization == null || !DescriptionLocalization.ContainsKey(currLang.ToString()))
                return Description;
            return DescriptionLocalization[currLang.ToString()];
        }

        public string BlueprintString()
        {
            var str = "";
            foreach (var ingredient in Ingredients)
                str += Mod.ResolveObjectId(ingredient.Object) + " " + ingredient.Count + " ";
            str = str.Substring(0, str.Length - 1);
            str += $"/{TileWidth}/{TileHeight}/{HumanDoorX}/{HumanDoorY}/{AnimalDoorX}/{AnimalDoorY}/{Id}/{LocalizedName()}/{LocalizedDescription()}/";
            str += PreviousTier != null ? "Upgrades" : "Buildings";
            str += "/" + PreviousTier + $"/{MenuWidth}/{MenuHeight}/{MaxOccupants}/none/Farm/{Price}/{Magical}/{DaysToConstruct}";
            return str;
        }
    }
}
