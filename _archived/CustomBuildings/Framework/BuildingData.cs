using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;

namespace CustomBuildings.Framework
{
    internal class BuildingData
    {
        [JsonIgnore]
        public Texture2D Texture;

        [JsonIgnore]
        public Func<xTile.Map> MapLoader;

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

        public int Price { get; set; } = 0;
        public List<Ingredient> Ingredients { get; set; } = new();
        public bool Magical { get; set; } = false;
        public int DaysToConstruct { get; set; } = 2;

        public Dictionary<string, string> NameLocalization = new();
        public Dictionary<string, string> DescriptionLocalization = new();

        public string LocalizedName()
        {
            var lang = LocalizedContentManager.CurrentLanguageCode;
            return lang != LocalizedContentManager.LanguageCode.en && this.NameLocalization != null && this.NameLocalization.TryGetValue(lang.ToString(), out string localization)
                ? localization
                : this.Name;
        }

        public string LocalizedDescription()
        {
            var lang = LocalizedContentManager.CurrentLanguageCode;
            return lang != LocalizedContentManager.LanguageCode.en && this.DescriptionLocalization != null && this.DescriptionLocalization.TryGetValue(lang.ToString(), out string localization)
                ? localization
                : this.Description;
        }

        public string BlueprintString()
        {
            string str = "";
            foreach (var ingredient in this.Ingredients)
                str += Mod.ResolveObjectId(ingredient.Object) + " " + ingredient.Count + " ";
            str = str.Substring(0, str.Length - 1);
            str += $"/{this.TileWidth}/{this.TileHeight}/{this.HumanDoorX}/{this.HumanDoorY}/{this.AnimalDoorX}/{this.AnimalDoorY}/{this.Id}/{this.LocalizedName()}/{this.LocalizedDescription()}/";
            str += this.PreviousTier != null ? "Upgrades" : "Buildings";
            str += "/" + this.PreviousTier + $"/{this.MenuWidth}/{this.MenuHeight}/{this.MaxOccupants}/none/Farm/{this.Price}/{this.Magical}/{this.DaysToConstruct}";
            return str;
        }
    }
}
