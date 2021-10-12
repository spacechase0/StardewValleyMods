using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace BetterShopMenu.Framework
{
    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>
    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for consistency and to match translation conventions.")]
    internal static class I18n
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's translation helper.</summary>
        private static ITranslationHelper Translations;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">The mod's translation helper.</param>
        public static void Init(ITranslationHelper translations)
        {
            I18n.Translations = translations;
        }

        /// <summary>Get a translation equivalent to "Category:".</summary>
        public static string Filter_Category()
        {
            return I18n.GetByKey("filter.category");
        }

        /// <summary>Get a translation equivalent to "Sorting:".</summary>
        public static string Filter_Sorting()
        {
            return I18n.GetByKey("filter.sorting");
        }

        /// <summary>Get a translation equivalent to "None".</summary>
        public static string Sort_None()
        {
            return I18n.GetByKey("sort.none");
        }

        /// <summary>Get a translation equivalent to "Name".</summary>
        public static string Sort_Name()
        {
            return I18n.GetByKey("sort.name");
        }

        /// <summary>Get a translation equivalent to "Price".</summary>
        public static string Sort_Price()
        {
            return I18n.GetByKey("sort.price");
        }

        /// <summary>Get a translation equivalent to "Everything".</summary>
        public static string Categories_Everything()
        {
            return I18n.GetByKey("categories.everything");
        }

        /// <summary>Get a translation equivalent to "Other".</summary>
        public static string Categories_Other()
        {
            return I18n.GetByKey("categories.other");
        }

        /// <summary>Get a translation equivalent to "Artisan Goods".</summary>
        public static string Categories_ArtisanGoods()
        {
            return I18n.GetByKey("categories.artisan-goods");
        }

        /// <summary>Get a translation equivalent to "Bait".</summary>
        public static string Categories_Bait()
        {
            return I18n.GetByKey("categories.bait");
        }

        /// <summary>Get a translation equivalent to "Big Craftables".</summary>
        public static string Categories_BigCraftables()
        {
            return I18n.GetByKey("categories.big-craftables");
        }

        /// <summary>Get a translation equivalent to "Boots".</summary>
        public static string Categories_Boots()
        {
            return I18n.GetByKey("categories.boots");
        }

        /// <summary>Get a translation equivalent to "Building Resources".</summary>
        public static string Categories_BuildingResources()
        {
            return I18n.GetByKey("categories.building-resources");
        }

        /// <summary>Get a translation equivalent to "Clothing".</summary>
        public static string Categories_Clothing()
        {
            return I18n.GetByKey("categories.clothing");
        }

        /// <summary>Get a translation equivalent to "Cooking".</summary>
        public static string Categories_Cooking()
        {
            return I18n.GetByKey("categories.cooking");
        }

        /// <summary>Get a translation equivalent to "Crafting".</summary>
        public static string Categories_Crafting()
        {
            return I18n.GetByKey("categories.crafting");
        }

        /// <summary>Get a translation equivalent to "Egg".</summary>
        public static string Categories_Eggs()
        {
            return I18n.GetByKey("categories.eggs");
        }

        /// <summary>Get a translation equivalent to "Equipment".</summary>
        public static string Categories_Equipment()
        {
            return I18n.GetByKey("categories.equipment");
        }

        /// <summary>Get a translation equivalent to "Fertilizer".</summary>
        public static string Categories_Fertilizer()
        {
            return I18n.GetByKey("categories.fertilizer");
        }

        /// <summary>Get a translation equivalent to "Fish".</summary>
        public static string Categories_Fish()
        {
            return I18n.GetByKey("categories.fish");
        }

        /// <summary>Get a translation equivalent to "Flowers".</summary>
        public static string Categories_Flowers()
        {
            return I18n.GetByKey("categories.flowers");
        }

        /// <summary>Get a translation equivalent to "Fruits".</summary>
        public static string Categories_Fruits()
        {
            return I18n.GetByKey("categories.fruits");
        }

        /// <summary>Get a translation equivalent to "Furniture".</summary>
        public static string Categories_Furniture()
        {
            return I18n.GetByKey("categories.furniture");
        }

        /// <summary>Get a translation equivalent to "Gems".</summary>
        public static string Categories_Gems()
        {
            return I18n.GetByKey("categories.gems");
        }

        /// <summary>Get a translation equivalent to "Greens".</summary>
        public static string Categories_Greens()
        {
            return I18n.GetByKey("categories.greens");
        }

        /// <summary>Get a translation equivalent to "Hats".</summary>
        public static string Categories_Hats()
        {
            return I18n.GetByKey("categories.hats");
        }

        /// <summary>Get a translation equivalent to "Ingredients".</summary>
        public static string Categories_Ingredients()
        {
            return I18n.GetByKey("categories.ingredients");
        }

        /// <summary>Get a translation equivalent to "Junk".</summary>
        public static string Categories_Junk()
        {
            return I18n.GetByKey("categories.junk");
        }

        /// <summary>Get a translation equivalent to "Meat".</summary>
        public static string Categories_Meat()
        {
            return I18n.GetByKey("categories.meat");
        }

        /// <summary>Get a translation equivalent to "Metals".</summary>
        public static string Categories_Metals()
        {
            return I18n.GetByKey("categories.metals");
        }

        /// <summary>Get a translation equivalent to "Milk".</summary>
        public static string Categories_Milk()
        {
            return I18n.GetByKey("categories.milk");
        }

        /// <summary>Get a translation equivalent to "Minerals".</summary>
        public static string Categories_Minerals()
        {
            return I18n.GetByKey("categories.minerals");
        }

        /// <summary>Get a translation equivalent to "Monster Loot".</summary>
        public static string Categories_MonsterLoot()
        {
            return I18n.GetByKey("categories.monster-loot");
        }

        /// <summary>Get a translation equivalent to "Recipes".</summary>
        public static string Categories_Recipes()
        {
            return I18n.GetByKey("categories.recipes");
        }

        /// <summary>Get a translation equivalent to "Rings".</summary>
        public static string Categories_Rings()
        {
            return I18n.GetByKey("categories.rings");
        }

        /// <summary>Get a translation equivalent to "Seeds".</summary>
        public static string Categories_Seeds()
        {
            return I18n.GetByKey("categories.seeds");
        }

        /// <summary>Get a translation equivalent to "Sellable @ Pierre's/Marnie's".</summary>
        public static string Categories_SellToPierreOrMarnie()
        {
            return I18n.GetByKey("categories.sell-to-pierre-or-marnie");
        }

        /// <summary>Get a translation equivalent to "Sellable @ Pierres".</summary>
        public static string Categories_SellToPierre()
        {
            return I18n.GetByKey("categories.sell-to-pierre");
        }

        /// <summary>Get a translation equivalent to "Sellable @ Willy's".</summary>
        public static string Categories_SellToWilly()
        {
            return I18n.GetByKey("categories.sell-to-willy");
        }

        /// <summary>Get a translation equivalent to "Syrups".</summary>
        public static string Categories_Syrups()
        {
            return I18n.GetByKey("categories.syrups");
        }

        /// <summary>Get a translation equivalent to "Tackle".</summary>
        public static string Categories_Tackle()
        {
            return I18n.GetByKey("categories.tackle");
        }

        /// <summary>Get a translation equivalent to "Tools".</summary>
        public static string Categories_Tools()
        {
            return I18n.GetByKey("categories.tools");
        }

        /// <summary>Get a translation equivalent to "Vegetables".</summary>
        public static string Categories_Vegetables()
        {
            return I18n.GetByKey("categories.vegetables");
        }

        /// <summary>Get a translation equivalent to "Weapons".</summary>
        public static string Categories_Weapons()
        {
            return I18n.GetByKey("categories.weapons");
        }

        /// <summary>Get a translation equivalent to "Grid Layout".</summary>
        public static string Config_GridLayout_Name()
        {
            return I18n.GetByKey("config.grid-layout.name");
        }

        /// <summary>Get a translation equivalent to "Whether to use the grid layout in shops.".</summary>
        public static string Config_GridLayout_Tooltip()
        {
            return I18n.GetByKey("config.grid-layout.tooltip");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a translation by its key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        private static Translation GetByKey(string key, object tokens = null)
        {
            if (I18n.Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(I18n.Init)} from the mod's entry method before reading translations.");
            return I18n.Translations.Get(key, tokens);
        }
    }
}

