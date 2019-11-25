using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace BugNet
{
    public class Mod : StardewModdingAPI.Mod
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

        public static Mod instance;
        private static Dictionary<string, CritterData> CrittersData = new Dictionary<string, CritterData>();
        
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            
            helper.Events.Display.MenuChanged += onMenuChanged;

            helper.ConsoleCommands.Add("player_addcritter", "Adds a critter", addCritterCommand);

            BugNetTool.Texture = helper.Content.Load<Texture2D>("assets/bugnet.png");

            var tilesheet = helper.Content.Load<Texture2D>("assets/critters.png");
            Action<string, int, Func<int, int, Critter>> register = (name, index, releaseFunc) => RegisterCritter(name, tilesheet, new Rectangle(index % 4 * 16, index / 4 * 16, 16, 16), () => helper.Translation.Get("critter." + name), releaseFunc);
            register("SummerButterflyBlue", 0, (x, y) => Critters.MakeButterfly(x, y, 128));
            register("SummerButterflyGreen", 1, (x, y) => Critters.MakeButterfly(x, y, 148));
            register("SummerButterflyRed", 2, (x, y) => Critters.MakeButterfly(x, y, 132));
            register("SummerButterflyPink", 3, (x, y) => Critters.MakeButterfly(x, y, 152));
            register("SummerButterflyYellow", 4, (x, y) => Critters.MakeButterfly(x, y, 136));
            register("SummerButterflyOrange", 5, (x, y) => Critters.MakeButterfly(x, y, 156));
            register("SpringButterflyPalePink", 6, (x, y) => Critters.MakeButterfly(x, y, 160));
            register("SpringButterflyMagenta", 7, (x, y) => Critters.MakeButterfly(x, y, 180));
            register("SpringButterflyWhite", 8, (x, y) => Critters.MakeButterfly(x, y, 163));
            register("SpringButterflyYellow", 9, (x, y) => Critters.MakeButterfly(x, y, 183));
            register("SpringButterflyPurple", 10, (x, y) => Critters.MakeButterfly(x, y, 166));
            register("SpringButterflyPink", 11, (x, y) => Critters.MakeButterfly(x, y, 186));
            register("BrownBird", 12, (x, y) => Critters.MakeBird(x, y, false));
            register("BlueBird", 13, (x, y) => Critters.MakeBird(x, y, true));
            register("GreenFrog", 14, (x, y) => Critters.MakeFrog(x, y, false));
            register("OliveFrog", 15, (x, y) => Critters.MakeFrog(x, y, false));
            register("Firefly", 16, (x, y) => Critters.MakeFirefly(x, y));
            register("Squirrel", 17, (x, y) => Critters.MakeSquirrel(x, y));
            register("GrayRabbit", 18, (x, y) => Critters.MakeRabbit(x, y, false));
            register("WhiteRabbit", 19, (x, y) => Critters.MakeRabbit(x, y, true));
            register("WoodPecker", 20, (x, y) => Critters.MakeWoodpecker(x, y));
            register("Seagull", 21, (x, y) => Critters.MakeSeagull(x, y));
            register("Owl", 22, (x, y) => Critters.MakeOwl(x, y));
            register("Crow", 23, (x, y) => Critters.MakeCrow(x, y));
            register("Cloud", 24, (x, y) => Critters.MakeCloud(x, y));
        }

        private void RegisterCritter(string critterId, Texture2D tex, Rectangle texRect, Func<string> getLocalizedName, Func<int, int, Critter> makeFunc)
        {
            CrittersData.Add(critterId, new CritterData()
            {
                Texture = new TextureTarget() { Texture = tex, SourceRect = texRect },
                Name = getLocalizedName,
                MakeFunction = makeFunc,
            });
        }

        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!(e.NewMenu is ShopMenu menu) || menu.portraitPerson?.Name != "Pierre")
                return;

            Log.debug($"Adding bug net to Pierre's shop.");

            var forSale = menu.forSale;
            var itemPriceAndStock = menu.itemPriceAndStock;

            var tool = new BugNetTool();
            forSale.Add(tool);
            itemPriceAndStock.Add(tool, new int[] { 500, 1 });
        }

        private void addCritterCommand(string cmd, string[] args)
        {
            if ( args.Length != 1 )
            {
                Log.info("Usage: player_addcritter <critter>");

                var critters = CrittersData.Keys.ToList();
                string choices = critters[0];
                for (int i = 1; i < critters.Count; ++i)
                {
                    choices += ", " + critters[i];
                }
                Log.info("Choices: " + choices);
                return;
            }

            if (!CrittersData.ContainsKey(args[0]))
            {
                Log.info("Invalid critter.");
                return;
            }

            Game1.player.addItemToInventory(new CritterItem(args[0]));
        }

        internal static Texture2D GetCritterTexture(string critter)
        {
            return CrittersData.ContainsKey(critter) ? CrittersData[critter].Texture.Texture : Game1.staminaRect;
        }

        internal static Rectangle GetCritterRect(string critter)
        {
            return CrittersData.ContainsKey(critter) ? CrittersData[critter].Texture.SourceRect : new Rectangle(0, 0, 1, 1);
        }

        internal static string GetCritterName(string critter)
        {
            return CrittersData.ContainsKey(critter) ? CrittersData[critter].Name() : "???";
        }

        internal static Func<int, int, Critter> GetCritterMaker(string critter)
        {
            return CrittersData.ContainsKey(critter) ? CrittersData[critter].MakeFunction : ((x, y) => null);
        }

        internal static string GetCritterIdFrom(Critter critter)
        {
            int bframe = critter.baseFrame;
            if (critter is Cloud)
                bframe = -2;
            if (critter is Frog frog)
                bframe = Mod.instance.Helper.Reflection.GetField<bool>(frog, "waterLeaper").GetValue() ? -3 : -4;

            switch (bframe)
            {
                case  -3: return "GreenFrog";
                case  -4: return "OliveFrog";
                case  -2: return "Cloud";
                case  -1: return "Firefly";
                case   0: return "Seagull";
                case  14: return "Crow";
                case  25: return "BrownBird";
                case  45: return "BlueBird";
                case  54: return "GrayRabbit";
                case  74: return "WhiteRabbit";
                case  60: return "Squirrel";
                case  83: return "Owl";
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
            }

            return "???";
        }
    }
}
