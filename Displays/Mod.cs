using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Displays
{
    public class Mod : StardewModdingAPI.Mod, IAssetLoader
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;

            helper.ConsoleCommands.Add("player_adddisplay", "mannequin", this.DoCommand);
        }

        public void OnGameLaunched(object sender, GameLaunchedEventArgs args)
        {
            var sc = this.Helper.ModRegistry.GetApi<SpaceCoreAPI>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(Mannequin));
        }

        public void DoCommand(string cmd, string[] args)
        {
            if (!Context.IsPlayerFree)
                return;

            if (args.Length == 0)
            {
                Log.error("Invalid command arguments. Format: player_adddisplay <item> [type] [amount]\nSuch as:\n\tplayer_adddisplay mannequin [plain|male|female] [amt]");
                return;
            }

            Item item = null;
            if (args[0] == "mannequin")
            {
                var mannType = Mannequin.MannequinType.Plain;
                var mannGender = Mannequin.MannequinGender.Male;
                if (args.Length >= 2)
                {
                    switch (args[1].ToLower())
                    {
                        case "male":
                            mannGender = Mannequin.MannequinGender.Male;
                            break;
                        case "female":
                            mannGender = Mannequin.MannequinGender.Female;
                            break;
                        default:
                            Log.error("Unknown mannequin type. Choices are: male, female");
                            return;
                    }
                }
                item = new Mannequin(mannType, mannGender, Vector2.Zero);
            }

            if (item == null)
            {
                Log.error("Invalid display item");
                return;
            }

            if (args.Length >= 3)
            {
                item.Stack = int.Parse(args[2]);
            }

            Game1.player.addItemByMenuIfNecessary(item);
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu shop)
            {
                if (shop.portraitPerson?.Name == "Robin")
                {
                    var mm = new Mannequin(Mannequin.MannequinType.Plain, Mannequin.MannequinGender.Male, Vector2.Zero);
                    var mf = new Mannequin(Mannequin.MannequinType.Plain, Mannequin.MannequinGender.Female, Vector2.Zero);
                    shop.forSale.Add(mm);
                    shop.forSale.Add(mf);
                    shop.itemPriceAndStock.Add(mm, new[] { 100, int.MaxValue });
                    shop.itemPriceAndStock.Add(mf, new[] { 100, int.MaxValue });
                }
            }
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Characters\\Farmer\\farmer_transparent");
        }

        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)this.Helper.Content.Load<Texture2D>("assets/farmer_transparent.png");
        }
    }
}
