using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Content;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;

namespace SkyFarm;

public class Mod : StardewModdingAPI.Mod
{
    public static Mod instance;

    private IContentPatcherApi cp;

    private PatchContentEngine Content { get; set; }

    public Texture2D AirshipTex { get; set; }

    public override void Entry(IModHelper helper)
    {
        instance = this;
        Log.Monitor = Monitor;

        AirshipTex = Helper.ModContent.Load<Texture2D>("assets/airship.png");

        Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
        Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;

        Helper.ConsoleCommands.Add("skyfarm_reload", "...", (cmd, args) => Content.Reload());
        Helper.ConsoleCommands.Add("skyfarm_airship", "...", (cmd, args) =>
        {
            Game1.player.currentLocation.characters.Add(new Airship() { Position = Game1.player.Position });
        });
    }

    private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
    {
        cp = Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");

        var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
        sc.RegisterSerializerType(typeof(Airship));
    }

    private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
    {
        if (Content == null)
        {
            if (cp.IsConditionsApiReady)
            {
                Content = new PatchContentEngine(ModManifest, Helper, "assets/content.txt");
#if !NDEBUG
                Content.OnReloadMonitorInstead("assets");
#endif
                Content.Reload();
            }
            else return;
        }
    }
}
