using System;
using System.Collections.Generic;
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
        public class TextureTarget
        {
            public Texture2D Texture { get; set; }
            public Rectangle SourceRect { get; set; }
        }

        public class CritterData
        {
            public TextureTarget Texture { get; set; }
            public Func<string> Name { get; set; }
            public Func<int, int, Critter> MakeFunction { get; set; }
        }

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
                this.RegisterCritter(name, tilesheet, new Rectangle(index % 4 * 16, index / 4 * 16, 16, 16), () => helper.Translation.Get("critter." + name), releaseFunc);
            }
            Register("SummerButterflyBlue", 0, (x, y) => Critters.MakeButterfly(x, y, 128));
            Register("SummerButterflyGreen", 1, (x, y) => Critters.MakeButterfly(x, y, 148));
            Register("SummerButterflyRed", 2, (x, y) => Critters.MakeButterfly(x, y, 132));
            Register("SummerButterflyPink", 3, (x, y) => Critters.MakeButterfly(x, y, 152));
            Register("SummerButterflyYellow", 4, (x, y) => Critters.MakeButterfly(x, y, 136));
            Register("SummerButterflyOrange", 5, (x, y) => Critters.MakeButterfly(x, y, 156));
            Register("SpringButterflyPalePink", 6, (x, y) => Critters.MakeButterfly(x, y, 160));
            Register("SpringButterflyMagenta", 7, (x, y) => Critters.MakeButterfly(x, y, 180));
            Register("SpringButterflyWhite", 8, (x, y) => Critters.MakeButterfly(x, y, 163));
            Register("SpringButterflyYellow", 9, (x, y) => Critters.MakeButterfly(x, y, 183));
            Register("SpringButterflyPurple", 10, (x, y) => Critters.MakeButterfly(x, y, 166));
            Register("SpringButterflyPink", 11, (x, y) => Critters.MakeButterfly(x, y, 186));
            Register("BrownBird", 12, (x, y) => Critters.MakeBird(x, y, Birdie.brownBird));
            Register("BlueBird", 13, (x, y) => Critters.MakeBird(x, y, Birdie.blueBird));
            Register("GreenFrog", 14, (x, y) => Critters.MakeFrog(x, y, false));
            Register("OliveFrog", 15, (x, y) => Critters.MakeFrog(x, y, false));
            Register("Firefly", 16, Critters.MakeFirefly);
            Register("Squirrel", 17, Critters.MakeSquirrel);
            Register("GrayRabbit", 18, (x, y) => Critters.MakeRabbit(x, y, false));
            Register("WhiteRabbit", 19, (x, y) => Critters.MakeRabbit(x, y, true));
            Register("WoodPecker", 20, Critters.MakeWoodpecker);
            Register("Seagull", 21, Critters.MakeSeagull);
            Register("Owl", 22, Critters.MakeOwl);
            Register("Crow", 23, Critters.MakeCrow);
            Register("Cloud", 24, Critters.MakeCloud);
            Register("BlueParrot", 25, (x, y) => Critters.MakeParrot(x, y, false));
            Register("GreenParrot", 26, (x, y) => Critters.MakeParrot(x, y, true));
            Register("Monkey", 27, Critters.MakeMonkey);
            Register("OrangeIslandButterfly", 28, (x, y) => Critters.MakeButterfly(x, y, 364, true));
            Register("PinkIslandButterfly", 29, (x, y) => Critters.MakeButterfly(x, y, 368, true));
            Register("PurpleBird", 30, (x, y) => Critters.MakeBird(x, y, 115/*Birdie.greenBird*/));
            Register("RedBird", 31, (x, y) => Critters.MakeBird(x, y, 120/*Birdie.redBird*/));
            Register("SunsetTropicalButterfly", 32, (x, y) => Critters.MakeButterfly(x, y, 372, true));
            Register("TropicalButterfly", 33, (x, y) => Critters.MakeButterfly(x, y, 376, true));
            //register("Marsupial", 34, (x, y) => Critters.MakeMarsupial(x, y));
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Mod.Ja = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            var spaceCore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            spaceCore.RegisterSerializerType(typeof(BugNetTool));
        }

        private void RegisterCritter(string critterId, Texture2D tex, Rectangle texRect, Func<string> getLocalizedName, Func<int, int, Critter> makeFunc)
        {
            Mod.CrittersData.Add(critterId, new CritterData
            {
                Texture = new TextureTarget { Texture = tex, SourceRect = texRect },
                Name = getLocalizedName,
                MakeFunction = makeFunc
            });

            var texData = new Color[16 * 16];
            tex.GetData(0, texRect, texData, 0, texData.Length);
            var jaTex = new Texture2D(Game1.graphics.GraphicsDevice, 16, 16);
            jaTex.SetData(texData);

            JsonAssets.Mod.instance.RegisterObject(this.ModManifest, new JsonAssets.Data.ObjectData
            {
                Name = $"Critter Cage: {getLocalizedName()}",
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
            if (!(e.NewMenu is ShopMenu menu) || menu.portraitPerson?.Name != "Pierre")
                return;

            Log.Debug($"Adding bug net to Pierre's shop.");

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
                    int check = Mod.Ja.GetObjectId("Critter Cage: " + critterData.Value.Name());
                    if (check == Game1.player.ActiveObject.ParentSheetIndex)
                    {
                        activeCritter = critterData.Value;
                        break;
                    }
                }

                // Spawn the critter
                int x = (int)e.Cursor.GrabTile.X + 1, y = (int)e.Cursor.GrabTile.Y + 1;
                var critter = activeCritter.MakeFunction(x, y);
                Game1.player.currentLocation.addCritter(critter);

                Game1.player.reduceActiveItemByOne();

                this.Helper.Input.Suppress(e.Button);
            }
        }

        internal static string GetCritterName(string critter)
        {
            return Mod.CrittersData.TryGetValue(critter, out CritterData critterData)
                ? critterData.Name()
                : "???";
        }

        internal static string GetCritterIdFrom(Critter critter)
        {
            int bframe = critter switch
            {
                Cloud => -2,
                Frog frog => Mod.Instance.Helper.Reflection.GetField<bool>(frog, "waterLeaper").GetValue() ? -3 : -4,
                OverheadParrot parrot => -10 - parrot.sourceRect.Y,
                CalderaMonkey => -100,
                _ => critter.baseFrame
            };

            switch (bframe)
            {
                case -10:
                case -34:
                    return "GreenParrot";
                case -58:
                case -82:
                    return "BlueParrot";
                case -100: return "Monkey";
                case -3: return "GreenFrog";
                case -4: return "OliveFrog";
                case -2: return "Cloud";
                case -1: return "Firefly";
                case 0: return "Seagull";
                case 14: return "Crow";
                case 25: return "BrownBird";
                case 45: return "BlueBird";
                case 54: return "GrayRabbit";
                case 74: return "WhiteRabbit";
                case 60: return "Squirrel";
                case 83: return "Owl";
                case 115: return "PurpleBird";
                case 125: return "RedBird";
                case 160: return "SpringButterflyPalePink";
                case 163: return "SpringButterflyWhite";
                case 166: return "SpringButterflyPurple";
                case 180: return "SpringButterflyMagenta";
                case 183: return "SpringButterflyYellow";
                case 186: return "SpringButterflyPink";
                case 128: return "SummerButterflyBlue";
                case 132: return "SummerButterflyRed";
                case 136: return "SummerButterflyYellow";
                case 148: return "SummerButterflyGreen";
                case 152: return "SummerButterflyPink";
                case 156: return "SummerButterflyOrange";
                case 320: return "WoodPecker";
                case 364: return "OrangeIslandButterfly";
                case 368: return "PinkIslandButterfly";
                case 372: return "SunsetTropicalButterfly";
                case 376: return "TropicalButterfly";
            }

            return "???";
        }
    }
}
