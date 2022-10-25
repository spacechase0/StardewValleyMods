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
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        internal static IJsonAssetsApi Ja;
        private static readonly Dictionary<string, CritterData> CrittersData = new();

        /// <summary>The placeholder texture for custom critter cages.</summary>
        private TextureTarget PlaceholderSprite;


        /*********
        ** Accessors
        *********/
        public static Mod Instance;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            BugNetTool.Texture = helper.ModContent.Load<Texture2D>("assets/bugnet.png");

            var tilesheet = helper.ModContent.Load<Texture2D>("assets/critters.png");

            Rectangle GetTilesheetArea(int index)
            {
                return new Rectangle(index % 4 * 16, index / 4 * 16, 16, 16);
            }
            void Register(string name, int index, CritterBuilder critterBuilder)
            {
                this.RegisterCritter(
                    critterId: name,
                    texture: tilesheet,
                    textureArea: GetTilesheetArea(index),
                    translationKey: $"critter.{name}",
                    isThisCritter: critterBuilder.IsThisCritter,
                    makeCritter: critterBuilder.MakeCritter
                );
            }

            this.PlaceholderSprite = new TextureTarget(tilesheet, GetTilesheetArea(24)); // empty jar sprite

            Register("SummerButterflyBlue", 0, CritterBuilder.ForButterfly(128));
            Register("SummerButterflyGreen", 1, CritterBuilder.ForButterfly(148));
            Register("SummerButterflyRed", 2, CritterBuilder.ForButterfly(132));
            Register("SummerButterflyPink", 3, CritterBuilder.ForButterfly(152));
            Register("SummerButterflyYellow", 4, CritterBuilder.ForButterfly(136));
            Register("SummerButterflyOrange", 5, CritterBuilder.ForButterfly(156));
            Register("SpringButterflyPalePink", 6, CritterBuilder.ForButterfly(160));
            Register("SpringButterflyMagenta", 7, CritterBuilder.ForButterfly(180));
            Register("SpringButterflyWhite", 8, CritterBuilder.ForButterfly(163));
            Register("SpringButterflyYellow", 9, CritterBuilder.ForButterfly(183));
            Register("SpringButterflyPurple", 10, CritterBuilder.ForButterfly(166));
            Register("SpringButterflyPink", 11, CritterBuilder.ForButterfly(186));
            Register("BrownBird", 12, CritterBuilder.ForBird(Birdie.brownBird));
            Register("BlueBird", 13, CritterBuilder.ForBird(Birdie.blueBird));
            Register("GreenFrog", 14, CritterBuilder.ForFrog(olive: false));
            Register("OliveFrog", 15, CritterBuilder.ForFrog(olive: false));
            Register("Firefly", 16, CritterBuilder.ForFirefly());
            Register("Squirrel", 17, CritterBuilder.ForSquirrel());
            Register("GrayRabbit", 18, CritterBuilder.ForRabbit(white: false));
            Register("WhiteRabbit", 19, CritterBuilder.ForRabbit(white: true));
            Register("WoodPecker", 20, CritterBuilder.ForWoodpecker());
            Register("Seagull", 21, CritterBuilder.ForSeagull());
            Register("Owl", 22, CritterBuilder.ForOwl());
            Register("Crow", 23, CritterBuilder.ForCrow());
            Register("Cloud", 24, CritterBuilder.ForCloud());
            Register("BlueParrot", 25, CritterBuilder.ForParrot(green: false));
            Register("GreenParrot", 26, CritterBuilder.ForParrot(green: true));
            Register("Monkey", 27, CritterBuilder.ForMonkey());
            Register("OrangeIslandButterfly", 28, CritterBuilder.ForButterfly(364, island: true));
            Register("PinkIslandButterfly", 29, CritterBuilder.ForButterfly(368, island: true));
            Register("PurpleBird", 30, CritterBuilder.ForBird(Birdie.greenBird));
            Register("RedBird", 31, CritterBuilder.ForBird(Birdie.redBird));
            Register("SunsetTropicalButterfly", 32, CritterBuilder.ForButterfly(372, island: true));
            Register("TropicalButterfly", 33, CritterBuilder.ForButterfly(376, island: true));
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return new BugNetApi(this.RegisterCritter, this.PlaceholderSprite, this.Monitor);
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
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
        /// <param name="isThisCritter">Get whether a given critter instance matches this critter.</param>
        /// <param name="makeCritter">Create a critter instance at the given X and Y tile position.</param>
        private void RegisterCritter(string critterId, Texture2D texture, Rectangle textureArea, string translationKey, Func<int, int, Critter> makeCritter, Func<Critter, bool> isThisCritter)
        {
            // get name translations
            this.GetTranslationsInAllLocales(
                translationKey,
                out string defaultCritterName,
                out Dictionary<string, string> critterNameTranslations
            );

            // register critter
            this.RegisterCritter(critterId, texture, textureArea, defaultCritterName, critterNameTranslations, makeCritter, isThisCritter);
        }

        /// <summary>Add a new critter which can be caught.</summary>
        /// <param name="critterId">The unique critter ID.</param>
        /// <param name="texture">The texture to show in the critter cage.</param>
        /// <param name="textureArea">The pixel area within the <paramref name="texture"/> to show in the critter cage.</param>
        /// <param name="defaultCritterName">The default English critter name.</param>
        /// <param name="translatedCritterNames">The translated critter names in each available locale.</param>
        /// <param name="makeCritter">Create a critter instance at the given X and Y tile position.</param>
        /// <param name="isThisCritter">Get whether a given critter instance matches this critter.</param>
        private void RegisterCritter(string critterId, Texture2D texture, Rectangle textureArea, string defaultCritterName, Dictionary<string, string> translatedCritterNames, Func<int, int, Critter> makeCritter, Func<Critter, bool> isThisCritter)
        {
            // get translations
            string TranslateCritterName(string locale)
            {
                return translatedCritterNames.GetOrDefault(locale) ?? defaultCritterName;
            }
            this.GetTranslationsInAllLocales("cage.name", out string defaultCageName, out var cageNameTranslations, format: (locale, translation) => translation.Tokens(new { critterName = TranslateCritterName(locale) }).ToString());
            this.GetTranslationsInAllLocales("cage.description", out string defaultCageDescription, out var cageDescriptionTranslations);

            // save critter data
            Mod.CrittersData.Add(critterId, new CritterData(
                defaultName: defaultCritterName,
                translatedName: () => TranslateCritterName(this.Helper.GameContent.CurrentLocale),
                texture: new TextureTarget(texture, textureArea),
                isThisCritter: isThisCritter,
                makeCritter: makeCritter
            ));

            // register cage with Json Assets
            JsonAssets.Mod.instance.RegisterObject(this.ModManifest, new JsonAssets.Data.ObjectData
            {
                Name = defaultCageName,
                NameLocalization = cageNameTranslations,
                Description = defaultCageDescription,
                DescriptionLocalization = cageDescriptionTranslations,
                Texture = this.CloneTextureArea(texture, textureArea),
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
            if (e.Button.IsActionButton() && Game1.player.ActiveObject?.Name.StartsWith("Critter Cage: ") is true)
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

        /// <summary>Get the data for a given critter, if it's supported by BugNet.</summary>
        /// <param name="critter">The critter to match.</param>
        /// <param name="data">The critter data.</param>
        /// <returns>Returns whether the critter data was found.</returns>
        internal static bool TryGetCritter(Critter critter, out CritterData data)
        {
            data = Mod.CrittersData.Values.FirstOrDefault(p => p.IsThisCritter(critter));
            return data != null;
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

            defaultText = translations.GetOrDefault("default");
            translations.Remove("default");
        }

        /// <summary>Copy an area in a texture into a new texture.</summary>
        /// <param name="texture">The texture to copy.</param>
        /// <param name="textureArea">The pixel area within the <paramref name="texture"/> to copy.</param>
        private Texture2D CloneTextureArea(Texture2D texture, Rectangle textureArea)
        {
            // 256 is kinda borderline for array rental.
            Color[] data = GC.AllocateUninitializedArray<Color>(textureArea.Width * textureArea.Height);
            texture.GetData(0, textureArea, data, 0, data.Length);
            Texture2D newTexture = new Texture2D(Game1.graphics.GraphicsDevice, textureArea.Width, textureArea.Height);
            newTexture.SetData(data);

            return newTexture;
        }
    }
}
