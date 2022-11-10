using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Miniscript;
using SpaceCore.Framework.ExtEngine.Models;
using SpaceShared;
using StardewModdingAPI.Events;
using StardewValley;

namespace SpaceCore.Framework.ExtEngine
{
    internal class ExtensionEngine
    {
        public static void Init()
        {
            SpaceCore.Instance.Helper.Events.Content.AssetRequested += OnAssetRequested;

            SpaceCore.Instance.Helper.ConsoleCommands.Add("ext_ui", "...", OnExtUi);
        }

        private static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/UI"))
                e.LoadFrom(() => new Dictionary<string, UiContentModel>(), AssetLoadPriority.Exclusive);
        }

        private static void OnExtUi(string cmd, string[] args)
        {
            if (args.Length != 1) Log.Info("Bad arguments");
            var data = Game1.content.Load<Dictionary<string, UiContentModel>>("spacechase0.SpaceCore/UI");
            if (!data.ContainsKey(args[0]))
            {
                Log.Info("Bad ID");
                return;
            }
            Game1.activeClickableMenu = new ExtensionMenu(data[args[0]]);
        }

        public static Interpreter SetupInterpreter()
        {
            Interpreter ret = new();
            ret.standardOutput = (s) => Log.Debug($"Script output: {s}");
            //ret.implicitOutput = (s) => Log.Trace($"Script output: {s}");
            ret.errorOutput = (s) => Log.Error($"Script error: {s}");

            return ret;
        }
    }
}
