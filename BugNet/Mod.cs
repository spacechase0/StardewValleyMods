using System;
using System.Collections.Generic;
using System.Linq;
using BugNet.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace BugNet
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        internal static IJsonAssetsApi Ja;
        private static readonly Dictionary<string, CritterData> CrittersData = new();

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            BugNetTool.Texture = helper.Content.Load<Texture2D>("assets/bugnet.png");

            var tilesheet = helper.Content.Load<Texture2D>("assets/critters.png");

            void Register(string name, int index, Func<int, int, Critter> releaseFunc)
            {
                this.RegisterCritter(name, tilesheet, new Rectangle(index % 4 * 16, index / 4 * 16, 16, 16), $"critter.{name}", releaseFunc);
            }
            Register("SummerButterflyBlue", 0, (x, y) => CritterBuilder.MakeButterfly(x, y, 128));
            Register("SummerButterflyGreen", 1, (x, y) => CritterBuilder.MakeButterfly(x, y, 148));
            Register("SummerButterflyRed", 2, (x, y) => CritterBuilder.MakeButterfly(x, y, 132));
            Register("SummerButterflyPink", 3, (x, y) => CritterBuilder.MakeButterfly(x, y, 152));
            Register("SummerButterflyYellow", 4, (x, y) => CritterBuilder.MakeButterfly(x, y, 136));
            Register("SummerButterflyOrange", 5, (x, y) => CritterBuilder.MakeButterfly(x, y, 156));
            Register("SpringButterflyPalePink", 6, (x, y) => CritterBuilder.MakeButterfly(x, y, 160));
            Register("SpringButterflyMagenta", 7, (x, y) => CritterBuilder.MakeButterfly(x, y, 180));
            Register("SpringButterflyWhite", 8, (x, y) => CritterBuilder.MakeButterfly(x, y, 163));
            Register("SpringButterflyYellow", 9, (x, y) => CritterBuilder.MakeButterfly(x, y, 183));
            Register("SpringButterflyPurple", 10, (x, y) => CritterBuilder.MakeButterfly(x, y, 166));
            Register("SpringButterflyPink", 11, (x, y) => CritterBuilder.MakeButterfly(x, y, 186));
            Register("BrownBird", 12, (x, y) => CritterBuilder.MakeBird(x, y, Birdie.brownBird));
            Register("BlueBird", 13, (x, y) => CritterBuilder.MakeBird(x, y, Birdie.blueBird));
            Register("GreenFrog", 14, (x, y) => CritterBuilder.MakeFrog(x, y, false));
            Register("OliveFrog", 15, (x, y) => CritterBuilder.MakeFrog(x, y, false));
            Register("Firefly", 16, CritterBuilder.MakeFirefly);
            Register("Squirrel", 17, CritterBuilder.MakeSquirrel);
            Register("GrayRabbit", 18, (x, y) => CritterBuilder.MakeRabbit(x, y, false));
            Register("WhiteRabbit", 19, (x, y) => CritterBuilder.MakeRabbit(x, y, true));
            Register("WoodPecker", 20, CritterBuilder.MakeWoodpecker);
            Register("Seagull", 21, CritterBuilder.MakeSeagull);
            Register("Owl", 22, CritterBuilder.MakeOwl);
            Register("Crow", 23, CritterBuilder.MakeCrow);
            Register("Cloud", 24, CritterBuilder.MakeCloud);
            Register("BlueParrot", 25, (x, y) => CritterBuilder.MakeParrot(x, y, false));
            Register("GreenParrot", 26, (x, y) => CritterBuilder.MakeParrot(x, y, true));
            Register("Monkey", 27, CritterBuilder.MakeMonkey);
            Register("OrangeIslandButterfly", 28, (x, y) => CritterBuilder.MakeButterfly(x, y, 364, true));
            Register("PinkIslandButterfly", 29, (x, y) => CritterBuilder.MakeButterfly(x, y, 368, true));
            Register("PurpleBird", 30, (x, y) => CritterBuilder.MakeBird(x, y, 115/*Birdie.greenBird*/));
            Register("RedBird", 31, (x, y) => CritterBuilder.MakeBird(x, y, 120/*Birdie.redBird*/));
            Register("SunsetTropicalButterfly", 32, (x, y) => CritterBuilder.MakeButterfly(x, y, 372, true));
            Register("TropicalButterfly", 33, (x, y) => CritterBuilder.MakeButterfly(x, y, 376, true));
            //register("Marsupial", 34, (x, y) => Critters.MakeMarsupial(x, y));
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Mod.Ja = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            var spaceCore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            spaceCore.RegisterSerializerType(typeof(BugNetTool));
        }

        /// <summary>Add a new critter which can be caught.</summary>
        /// <param name="critterId">The unique critter ID.</param>
        /// <param name="texture">The texture to show in the critter cage.</param>
        /// <param name="textureArea">The pixel area within the <paramref name="texture"/> to show in the critter cage.</param>
        /// <param name="translationKey">The translation key for the critter name.</param>
        /// <param name="makeCritter">Create a critter instance at the given X and Y tile position.</param>
        private void RegisterCritter(string critterId, Texture2D texture, Rectangle textureArea, string translationKey, Func<int, int, Critter> makeCritter)
        {
            // get translations
            this.GetTranslationsInAllLocales(
                translationKey,
                out string defaultCritterName,
                out Dictionary<string, string> critterNameTranslations
            );
            this.GetTranslationsInAllLocales(
                "critter.cage",
                out string defaultCageName,
                out Dictionary<string, string> cageNameTranslations,
                format: (locale, translation) => translation.Tokens(new { critterName = locale == "default" ? defaultCritterName : critterNameTranslations[locale] }).ToString()
            );

            // save critter data
            Mod.CrittersData.Add(critterId, new CritterData(
                defaultName: defaultCritterName,
                translatedName: () => this.Helper.Translation.Get(translationKey),
                texture: new TextureTarget(texture, textureArea),
                makeCritter: makeCritter
            ));

            // register cage with Json Assets
            var texData = new Color[16 * 16];
            texture.GetData(0, textureArea, texData, 0, texData.Length);
            var jaTex = new Texture2D(Game1.graphics.GraphicsDevice, 16, 16);
            jaTex.SetData(texData);

            JsonAssets.Mod.instance.RegisterObject(this.ModManifest, new JsonAssets.Data.ObjectData
            {
                Name = defaultCageName,
                NameLocalization = cageNameTranslations,
                Description = "It's a critter! In a cage!",
                Texture = jaTex,
                Category = JsonAssets.Data.ObjectCategory.MonsterLoot,
                CategoryTextOverride = "Critter",
                Price = critterId.Contains("Butterfly") ? 50 : 100,
                ContextTags = new List<string>(new[] { "critter" }),
                HideFromShippingCollection = true
            });
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not ShopMenu { portraitPerson: { Name: "Pierre" } } menu)
                return;

            Log.Debug("Adding bug net to Pierre's shop.");

            var forSale = menu.forSale;
            var itemPriceAndStock = menu.itemPriceAndStock;

            var tool = new BugNetTool();
            forSale.Add(tool);
            itemPriceAndStock.Add(tool, new[] { 500, 1 });
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton() && Game1.player.ActiveObject != null &&
                 Game1.player.ActiveObject.Name.StartsWith("Critter Cage: "))
            {
                // Get the critter ID
                CritterData activeCritter = null;
                foreach (var critterData in Mod.CrittersData)
                {
                    int check = Mod.Ja.GetObjectId($"Critter Cage: {critterData.Value.DefaultName}");
                    if (check == Game1.player.ActiveObject.ParentSheetIndex)
                    {
                        activeCritter = critterData.Value;
                        break;
                    }
                }

                // Spawn the critter
                int x = (int)e.Cursor.GrabTile.X + 1, y = (int)e.Cursor.GrabTile.Y + 1;
                var critter = activeCritter.MakeCritter(x, y);
                Game1.player.currentLocation.addCritter(critter);

                Game1.player.reduceActiveItemByOne();

                this.Helper.Input.Suppress(e.Button);
            }
        }

        internal static string GetCritterDefaultName(string critter)
        {
            return Mod.CrittersData.TryGetValue(critter, out CritterData critterData)
                ? critterData.DefaultName
                : "???";
        }

        internal static string GetCritterIdFrom(Critter critter)
        {
            int baseFrame = critter switch
            {
                Cloud => -2,
                Frog frog => Mod.Instance.Helper.Reflection.GetField<bool>(frog, "waterLeaper").GetValue() ? -3 : -4,
                OverheadParrot parrot => -10 - parrot.sourceRect.Y,
                CalderaMonkey => -100,
                _ => critter.baseFrame
            };

            return baseFrame switch
            {
                -10 or -34 => "GreenParrot",
                -58 or -82 => "BlueParrot",
                -100 => "Monkey",
                -3 => "GreenFrog",
                -4 => "OliveFrog",
                -2 => "Cloud",
                -1 => "Firefly",
                0 => "Seagull",
                14 => "Crow",
                25 => "BrownBird",
                45 => "BlueBird",
                54 => "GrayRabbit",
                74 => "WhiteRabbit",
                60 => "Squirrel",
                83 => "Owl",
                115 => "PurpleBird",
                125 => "RedBird",
                160 => "SpringButterflyPalePink",
                163 => "SpringButterflyWhite",
                166 => "SpringButterflyPurple",
                180 => "SpringButterflyMagenta",
                183 => "SpringButterflyYellow",
                186 => "SpringButterflyPink",
                128 => "SummerButterflyBlue",
                132 => "SummerButterflyRed",
                136 => "SummerButterflyYellow",
                148 => "SummerButterflyGreen",
                152 => "SummerButterflyPink",
                156 => "SummerButterflyOrange",
                320 => "WoodPecker",
                364 => "OrangeIslandButterfly",
                368 => "PinkIslandButterfly",
                372 => "SunsetTropicalButterfly",
                376 => "TropicalButterfly",
                _ => "???"
            };
        }

        /// <summary>Get the translations in all available locales for a given translation key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="defaultText">The default text.</param>
        /// <param name="translations">The translation text in each locale.</param>
        /// <param name="format">Format a translation.</param>
        private void GetTranslationsInAllLocales(string key, out string defaultText, out Dictionary<string, string> translations, Func<string, Translation, string> format = null)
        {
            translations = this.Helper.Translation
                .GetInAllLocales(key, withFallback: true)
                .ToDictionary(
                    localeSet => localeSet.Key,
                    localeSet => format?.Invoke(localeSet.Key, localeSet.Value) ?? localeSet.Value.ToString()
                );

            if (!translations.TryGetValue("default", out defaultText))
                defaultText = null;
            translations.Remove("default");
        }
    }
}
