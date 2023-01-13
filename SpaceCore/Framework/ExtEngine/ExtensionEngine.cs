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
using SpaceCore.Framework.ExtEngine.Script;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace SpaceCore.Framework.ExtEngine
{
    internal static partial class ExtensionEngine
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
            if (args.Length != 1)
            {
                Log.Info("Bad arguments");
                return;
            }

            var data = Game1.content.Load<Dictionary<string, UiContentModel>>("spacechase0.SpaceCore/UI");
            if (!data.ContainsKey(args[0]))
            {
                Log.Info("Bad ID");
                return;
            }
            Game1.activeClickableMenu = new ExtensionMenu(data[args[0]]);
        }

        internal static Func<Item, Value> makeItemMap;

        public static Interpreter SetupInterpreter()
        {
            Interpreter ret = new();
            ret.standardOutput = (s) => Log.Debug($"Script output: {s}");
            //ret.implicitOutput = (s) => Log.Trace($"Script output: {s}");
            ret.errorOutput = (s) => Log.Error($"Script error: {s}");

            var i = Intrinsic.Create("substituteTokens");
            i.AddParam("text");
            i.code = (ctx, prevResult) =>
            {
                string text = ctx.GetVar("text").ToString();
                var menu = ctx.interpreter.hostData as ExtensionMenu;

                return new Intrinsic.Result(new ValString(SubstituteTokens(menu.origModel.ScriptFile.Substring(0, menu.origModel.ScriptFile.IndexOf('/')), text)));
            };

            i = Intrinsic.Create("hasMail");
            i.AddParam("mail");
            i.AddParam("player", new ValString("current"));
            i.code = (ctx, prevResult) =>
            {
                string mail = ctx.GetVar("mail").ToString();
                var playerVar = ctx.GetVar("player");
                var menu = ctx.interpreter.hostData as ExtensionMenu;

                Farmer player = null;
                if (playerVar is ValString && playerVar.ToString() == "current")
                    player = Game1.player;
                else if (playerVar is ValString && playerVar.ToString() == "master")
                    player = Game1.MasterPlayer;
                else if (playerVar is ValNumber vnum)
                    player = Game1.getFarmerMaybeOffline((long)vnum.value);

                if (player == null)
                {
                    Log.Warn($"Bad player ID ({playerVar}) passed to hasMail by {menu.origModel.ScriptFile}");
                    return new Intrinsic.Result(new ValNumber(0));
                }

                return new Intrinsic.Result(new ValNumber(player.hasOrWillReceiveMail( mail ) ? 1 : 0));
            };

            i = Intrinsic.Create("openMenu");
            i.AddParam("id");
            i.AddParam("asChildMenu", new ValNumber(0));
            i.code = (ctx, prevResult) =>
            {
                string id = ctx.GetVar("id").ToString();
                bool asChild = ctx.GetVar("asChildMenu").BoolValue();
                var menu = ctx.interpreter.hostData as ExtensionMenu;

                var data = Game1.content.Load<Dictionary<string, UiContentModel>>("spacechase0.SpaceCore/UI");
                if (!data.ContainsKey(id))
                {
                    Log.Warn($"In {menu.origModel.ScriptFile}, tried to open menu {id} which does not exist");
                    return Intrinsic.Result.Null;
                }
                var newMenu = new ExtensionMenu(data[id]);
                if (asChild && Game1.activeClickableMenu != null)
                    Game1.activeClickableMenu.SetChildMenu(newMenu);
                else
                    Game1.activeClickableMenu = newMenu;
                return Intrinsic.Result.Null;
            };

            i = Intrinsic.Create("openLetterMenu");
            i.AddParam("text");
            i.AddParam("asChildMenu", new ValNumber(0));
            i.code = (ctx, prevResult) =>
            {
                string text = ctx.GetVar("text").ToString();
                bool asChild = ctx.GetVar("asChildMenu").BoolValue();

                var newMenu = new LetterViewerMenu(text);
                if (asChild && Game1.activeClickableMenu != null)
                    Game1.activeClickableMenu.SetChildMenu(newMenu);
                else
                    Game1.activeClickableMenu = newMenu;
                return Intrinsic.Result.Null;
            };

            makeItemMap = (Item item) =>
            {
                ValMap ret = new();
                ret.map.Add(new ValString("__item"), new ValItem(item));
                ret.map.Add(new ValString("itemId"), new ValString(item.ItemId));
                ret.map.Add(new ValString("qualifiedItemId"), new ValString(item.QualifiedItemId));
                ret.map.Add(new ValString("typeDefinitionId"), new ValString(item.TypeDefinitionId));
                ret.map.Add(new ValString("stack"), new ValNumber(item.Stack));

                // TODO: Script data? Or just modData?

                // TODO: Stuff based on what it is (quality, etc.)...
                // Use reflection to do automatically?

                ret.assignOverride = (key, val) =>
                {
                    switch (key.ToString())
                    {
                        case "itemId":
                            item.ItemId = val.ToString();
                            ret["qualifiedItemId"] = new ValString(item.QualifiedItemId);
                            return true;
                        case "qualifiedItemId": return false;
                        case "typeDefinitionId": return false;
                        case "stack":
                            item.Stack = val.IntValue();
                            return true;
                    }

                    return true;
                };

                return ret;
            };

            SetupInterpreter_ExtensionMenu( ret );

            return ret;
        }
    }
}
