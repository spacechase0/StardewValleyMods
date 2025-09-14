using Displays.Framework;
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
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
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

            helper.ConsoleCommands.Add("player_adddisplay", "mannequin", this.HandleCommand);

            helper.Events.Content.AssetRequested += this.OnAssetRequested;
        }

        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var sc = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(Mannequin));
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Characters\\Farmer\\farmer_transparent"))
                e.LoadFromModFile<Texture2D>("assets/farmer_transparent.png", AssetLoadPriority.Exclusive);
        }

        private void HandleCommand(string cmd, string[] args)
        {
            if (!Context.IsPlayerFree)
                return;

            if (args.Length == 0)
            {
                Log.Error("Invalid command arguments. Format: player_adddisplay <item> [type] [amount]\nSuch as:\n\tplayer_adddisplay mannequin [plain|male|female] [amt]");
                return;
            }

            Item item = null;
            if (args[0] == "mannequin")
            {
                var mannType = MannequinType.Plain;
                var mannGender = MannequinGender.Male;
                if (args.Length >= 2)
                {
                    switch (args[1].ToLower())
                    {
                        case "male":
                            mannGender = MannequinGender.Male;
                            break;
                        case "female":
                            mannGender = MannequinGender.Female;
                            break;
                        default:
                            Log.Error("Unknown mannequin type. Choices are: male, female");
                            return;
                    }
                }
                item = new Mannequin(mannType, mannGender, Vector2.Zero);
            }

            if (item == null)
            {
                Log.Error("Invalid display item");
                return;
            }

            if (args.Length >= 3)
            {
                item.Stack = int.Parse(args[2]);
            }

            Game1.player.addItemByMenuIfNecessary(item);
        }

        /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu shop)
            {
                if (shop.portraitPerson?.Name == "Robin")
                {
                    var mm = new Mannequin(MannequinType.Plain, MannequinGender.Male, Vector2.Zero);
                    var mf = new Mannequin(MannequinType.Plain, MannequinGender.Female, Vector2.Zero);
                    shop.forSale.Add(mm);
                    shop.forSale.Add(mf);
                    shop.itemPriceAndStock.Add(mm, new ItemStockInformation() { price = 100, stock = int.MaxValue });
                    shop.itemPriceAndStock.Add(mf, new ItemStockInformation() { price = 100, stock = int.MaxValue });
                }
            }
        }
    }
}
