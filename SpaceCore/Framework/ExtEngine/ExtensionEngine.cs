using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Miniscript;
using SpaceCore.Framework.ExtEngine.Models;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SpaceCore.Framework.ExtEngine
{
    internal static class ExtensionEngine
    {
        // content patcher stuff
        private static IMod contentPatcher;
        private static Func<object> screenManager;
        private static PropertyInfo screenManVal;
        private static PropertyInfo tokenManProp;
        private static Func<object> tokenManager;
        private static object logPathBuilder;
        private static Dictionary<string, object> contexts = new();
        private static MethodInfo trackLocalFunc;
        private static ConstructorInfo tokenStringConstructor;
        private static PropertyInfo tokenStringIsReadyProp;
        private static PropertyInfo tokenStringValueProp;

        public static void Init()
        {
            SpaceCore.Instance.Helper.Events.Content.AssetRequested += OnAssetRequested;

            SpaceCore.Instance.Helper.ConsoleCommands.Add("ext_ui", "...", OnExtUi);

            SpaceCore.Instance.Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private static int countdown = 3;
        private static void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (--countdown <= 0)
            {
                SpaceCore.Instance.Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;

                var modInfo = SpaceCore.Instance.Helper.ModRegistry.Get("Pathoschild.ContentPatcher");
                contentPatcher = modInfo.GetType().GetProperty("Mod", BindingFlags.Public | BindingFlags.Instance).GetValue(modInfo) as IMod;
                object smPerScreen = contentPatcher.GetType().GetField("ScreenManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(contentPatcher);
                screenManVal = smPerScreen.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                screenManager = () => screenManVal.GetGetMethod().Invoke(smPerScreen, new object[0]);
                tokenManProp = screenManager().GetType().GetProperty("TokenManager");
                tokenManager = () => tokenManProp.GetGetMethod().Invoke(screenManager(), new object[0]);
                logPathBuilder = AccessTools.TypeByName("ContentPatcher.Framework.LogPathBuilder").GetConstructor(new Type[] { typeof(string[]) }).Invoke(new object[] { new string[] { "SpaceCore shenanigans" } });
                trackLocalFunc = tokenManager().GetType().GetMethod("TrackLocalTokens");
                var tokenStringType = AccessTools.TypeByName("ContentPatcher.Framework.Conditions.TokenString");
                tokenStringConstructor = tokenStringType.GetConstructor(new Type[] { typeof(string), AccessTools.TypeByName("ContentPatcher.Framework.Tokens.IContext"), AccessTools.TypeByName("ContentPatcher.Framework.LogPathBuilder") });
                tokenStringIsReadyProp = tokenStringType.GetProperty("IsReady");
                tokenStringValueProp = tokenStringType.GetProperty("Value");
            }
        }

        public static bool CheckWhen(string contentPack, string when)
        {
            // TODO: Properly use CP's ManagedCondition stuff for this
            string whenSubstituted = SubstituteTokens(contentPack, when);
            using DataTable dt = new();
            object result = dt.Compute(whenSubstituted, string.Empty);
            if (result is not bool)
            {
                Log.Warn($"In {contentPack}, {when} should return true or false!" );
                return false;
            }
            return (bool) result;
        }

        public static string SubstituteTokens(string contentPack, string text)
        {
            if (contentPatcher == null)
            {
                Log.Warn("Content Patcher not found!");
                return text;
            }
            if (!contexts.ContainsKey(contentPack))
            {
                var modInfo = SpaceCore.Instance.Helper.ModRegistry.Get(contentPack);
                var cp = modInfo.GetType().GetProperty( "ContentPack", BindingFlags.Public | BindingFlags.Instance ).GetValue( modInfo ) as IContentPack;
                object newContext = trackLocalFunc.Invoke(tokenManager(), new object[] { cp });
                contexts.Add(contentPack, newContext);
                return SubstituteTokens(contentPack, text);
            }

            object context = contexts[contentPack];
            object tokenStr = tokenStringConstructor.Invoke(new object[] { text, context, logPathBuilder });
            if ((bool)tokenStringIsReadyProp.GetGetMethod().Invoke(tokenStr, null) == false)
            {
                throw new Exception("Tokens not ready!");
            }
            return (string) tokenStringValueProp.GetGetMethod().Invoke(tokenStr, null);
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

            var i = Intrinsic.Create("openMenu");
            i.AddParam("id");
            i.code = (ctx, prevResult) =>
            {
                string id = ctx.GetVar("id").ToString();
                var menu = ctx.interpreter.hostData as ExtensionMenu;

                var data = Game1.content.Load<Dictionary<string, UiContentModel>>("spacechase0.SpaceCore/UI");
                if (!data.ContainsKey(id))
                {
                    Log.Warn($"In {menu.origModel.ScriptFile}, tried to open menu {id} which does not exist");
                    return Intrinsic.Result.Null;
                }
                var newMenu = new ExtensionMenu(data[id]);
                Game1.activeClickableMenu = newMenu;
                return Intrinsic.Result.Null;
            };

            return ret;
        }
    }
}
