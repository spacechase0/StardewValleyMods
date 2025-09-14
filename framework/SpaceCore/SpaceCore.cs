using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceCore.Framework;
using SpaceCore.VanillaAssetExpansion;
using SpaceCore.Interface;
using SpaceCore.Patches;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Triggers;
using SpaceCore.UI;
using StardewValley.BellsAndWhistles;
using StardewValley.Delegates;
using Location = xTile.Dimensions.Location;
using StardewValley.TerrainFeatures;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;
using StardewValley.Pathfinding;
using static StardewValley.Menus.CharacterCustomization;
using System.Collections;
using StardewValley.TokenizableStrings;
using SpaceCore.Dungeons;
using System.Collections.ObjectModel;
using SpaceCore.Guidebooks;

namespace SpaceCore
{
    // Remove this once vanilla fixes it
    // Reenable once I fix the marriage schedule issue
#if false
    [HarmonyPatch(typeof(NPC), "parseMasterScheduleImpl")]
    public static class FixZeroSchedulePatch
    {
        public static void Prefix(NPC __instance)
        {
            __instance.reloadDefaultLocation();
            Game1.warpCharacter(__instance, __instance.DefaultMap, (__instance.DefaultPosition / Game1.tileSize).ToPoint());
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            List<CodeInstruction> ret = new();

            foreach (var insn in instructions)
            {
                if (insn.opcode == OpCodes.Call && insn.operand is MethodInfo { Name: "warpCharacter" } m)
                {
                    insn.operand = AccessTools.Method(typeof(FixZeroSchedulePatch), nameof(WarpAndSetDefaultMap));
                }

                ret.Add(insn);
            }

            return ret;
        }

        public static void WarpAndSetDefaultMap(NPC npc, string map, Point pos)
        {
            npc.DefaultMap = map;
            npc.DefaultPosition = pos.ToVector2() * Game1.tileSize;
            Game1.warpCharacter(npc, map, pos);
        }
    }
#endif

    public class FarmerExtData
    {
        public static ConditionalWeakTable<Farmer, FarmerExtData> data = new();

        public float HealthRegen { get; set; } = 0;
        public float StaminaRegen { get; set; } = 0;

        public float healthBuffer { get; set; } = 0;
        public float staminaBuffer { get; set; } = 0;

        public readonly NetStringDictionary<Item, NetRef<Item>> ExtraEquippables = new();

        public static void set_equippables(Farmer farmer, NetStringDictionary<Item, NetRef<Item>> newData)
        {
            // not needed for net types
            // (this method is for serialization)
        }

        public static NetStringDictionary<Item, NetRef<Item>> get_equippables(Farmer farmer)
        {
            return farmer.GetExtData().ExtraEquippables;
        }
    }

    [HarmonyPatch(typeof(Farmer), "initNetFields")]
    public static class FarmerAddNetFieldsPatch
    {
        public static void Postfix(Farmer __instance)
        {
            var ext = __instance.GetExtData();
            __instance.NetFields.AddField(ext.ExtraEquippables);
        }
    }

    internal static class Extensions
    {
        public static FarmerExtData GetExtData(this Farmer farmer)
        {
            return FarmerExtData.data.GetOrCreateValue(farmer);
        }
    }

    /// <summary>The mod entry class.</summary>
    internal class SpaceCore : Mod
    {
        /*********
        ** Fields
        *********/
        internal Harmony Harmony;

        /// <summary>Handles migrating legacy data for a save file.</summary>
        private LegacyDataMigrator LegacyDataMigrator;

        /// <summary>Whether the current update tick is the first one raised by SMAPI.</summary>
        private bool IsFirstTick;

        internal class EquipmentSlotData
        {
            public IManifest CorrespondingMod { get; set; }
            public Func<Item, bool> SlotValidator { get; set; }
            public Func<string> DisplayName { get; set; }
            public Texture2D BackgroundTex { get; set; }
            public Rectangle? BackgroundRect { get; set; }
        }

        /*********
        ** Accessors
        *********/
        public Configuration Config { get; set; }
        internal static SpaceCore Instance;
        internal static IReflectionHelper Reflection;
        internal static readonly List<Type> ModTypes = new();
        internal static readonly Dictionary<Type, Dictionary<string, CustomPropertyInfo>> CustomProperties = new();
        internal static readonly Dictionary<string, EquipmentSlotData> EquipmentSlots = new();
        internal static IApi api;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.LegacyDataMigrator = new LegacyDataMigrator(helper.Data, this.Monitor);

            I18n.Init(helper.Translation);
            SpaceCore.Instance = this;
            SpaceCore.Reflection = helper.Reflection;
            Log.Monitor = this.Monitor;
            this.Config = helper.ReadConfig<Configuration>();

            GatherLocals();

            WarpPathfindingCache.IgnoreLocationNames.Add("VolcanoEntrance");

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Content.AssetRequested += this.Content_AssetRequested;

            helper.ConsoleCommands.Add("guidebook", "Open a specific guidebook", (cmd, args) =>
            {
                if (args.Length < 1)
                {
                    Log.Error("Need to specify a guidebook ID");
                    return;
                }

                var data = Game1.content.Load<Dictionary<string, GuidebookData>>("spacechase0.SpaceCore/Guidebooks");
                if (!data.TryGetValue(args[0], out var specificData))
                {
                    Log.Error("Invalid guidebook ID");
                    return;
                }

                Game1.activeClickableMenu = new GuidebookMenu(specificData);
            });
            helper.ConsoleCommands.Add("equipment", "Open the SpaceCore equipment menu", (cmd, args) =>
            {
                Game1.activeClickableMenu = new EquipmentMenu();
            });

            var manualContext = new TriggerActionContext("Manual", Array.Empty<object>(), null);
            GameLocation.RegisterTileAction("spacechase0.SpaceCore_OpenGlobalInventory", (loc, args, farmer, pos) =>
            {
                var chest = new Chest();
                chest.GlobalInventoryId = args[1];
                chest.GetMutex().RequestLock(() =>
                {
                    chest.ShowMenu();
                    Game1.activeClickableMenu.exitFunction = (IClickableMenu.onExit)Delegate.Combine(Game1.activeClickableMenu.exitFunction, () => chest.GetMutex().ReleaseLock());
                });
                return true;
            });
            GameLocation.RegisterTileAction("spacechase0.SpaceCore_TriggerAction", (loc, args, farmer, pos) =>
            {
                if (!VanillaAssetExpansion.VanillaAssetExpansion.manualTriggerActionsById.TryGetValue(args[1], out var trigger))
                {
                    Log.Error($"Failed to find trigger action \"{args[0]}\" with \"Manual\" trigger type");
                    return true;
                }
                if (GameStateQuery.CheckConditions(trigger.Data.Condition) && (!trigger.Data.HostOnly || Game1.IsMasterGame))
                {
                    foreach (var action in trigger.Actions)
                    {
                        if ( !TriggerActionManager.TryRunAction(action, manualContext, out string error, out Exception e) )
                        {
                            Log.Error($"Trigger action {trigger.Data.Id} failed: {error} {e}");
                        }
                    }

                    if (trigger.Data.MarkActionApplied)
                        farmer.triggerActionsRun.Add(trigger.Data.Id);
                }
                return true;
            });
            GameLocation.RegisterTouchAction("spacechase0.SpaceCore_TriggerAction", (loc, args, farmer, pos) =>
            {
                if (!VanillaAssetExpansion.VanillaAssetExpansion.manualTriggerActionsById.TryGetValue(args[1], out var trigger))
                {
                    Log.Error($"Failed to find trigger action \"{args[0]}\" with \"Manual\" trigger type");
                    return;
                }
                if (GameStateQuery.CheckConditions(trigger.Data.Condition) && (!trigger.Data.HostOnly || Game1.IsMasterGame))
                {
                    foreach (var action in trigger.Actions)
                    {
                        if (!TriggerActionManager.TryRunAction(action, manualContext, out string error, out Exception e))
                        {
                            Log.Error($"Trigger action {trigger.Data.Id} failed: {error} {e}");
                        }
                    }

                    if (trigger.Data.MarkActionApplied)
                        farmer.triggerActionsRun.Add(trigger.Data.Id);
                }
            });

            TriggerActionManager.RegisterAction("spacechase0.SpaceCore_PlaySound", (string[] args, TriggerActionContext ctx, out string error) =>
            {
                if ( args.Length < 2 )
                {
                    error = "Not enough arguments";
                    return false;
                }
                error = null;
                if (args.Length >= 3 && bool.TryParse(args[2], out bool local) && local)
                    Game1.player.playNearbySoundAll(args[1]);
                else
                    Game1.playSound(args[1]);
                return true;
            });

            TriggerActionManager.RegisterAction("spacechase0.SpaceCore_ShowHudMessage", (string[] args, TriggerActionContext ctx, out string error) =>
            {
                if ( args.Length < 2 )
                {
                    error = "Not enough arguments";
                    return false;
                }
                Item item = null;
                if (ArgUtility.TryGetOptional(args, 2, out string qualItemId, out error) && qualItemId != null)
                {
                    item = ItemRegistry.Create(qualItemId);
                }

                error = null;
                Game1.addHUDMessage(new HUDMessage(TokenParser.ParseText(args[1])) { noIcon = item == null, messageSubject = item});
                return true;
            });

            TriggerActionManager.RegisterAction("spacechase0.SpaceCore_PlayEvent", (string[] args, TriggerActionContext ctx, out string error) =>
            {
                if (args.Length < 2)
                {
                    error = "Not enough arguments";
                    return false;
                }
                bool checkseen = false;
                if (args.Length >= 3)
                {
                    checkseen = args[2] == "true";
                }

                error = null;
                Game1.PlayEvent(args[1], false, checkseen);
                return true;
            });

            TriggerActionManager.RegisterAction("spacechase0.SpaceCore_DamageCurrentFarmer", (string[] args, TriggerActionContext ctx, out string error) =>
            {
                if (args.Length < 2)
                {
                    error = "Not enough arguments";
                    return false;
                }
                if (!ArgUtility.TryGetInt(args, 1, out int dmg, out error))
                {
                    return false;
                }
                Game1.player.takeDamage(dmg, false, null);
                return true;
            });

            TriggerActionManager.RegisterAction("spacechase0.SpaceCore_ApplyBuff", (string[] args, TriggerActionContext ctx, out string error) =>
            {
                if (args.Length < 3)
                {
                    error = "Not enough arguments";
                    return false;
                }

                try
                {
                    Buff b = new Buff(args[1], source: args[2], displaySource: args[2]);
                    Game1.player.applyBuff(b);
                }
                catch (Exception e)
                {
                    error = "Failed to create and apply buff: " + e;
                    return false;
                }

                error = null;
                return true;
            });

            TriggerActionManager.RegisterAction("spacechase0.SpaceCore_OpenGuidebook", (string[] args, TriggerActionContext ctx, out string error) =>
            {
                if (args.Length < 2)
                {
                    error = "Not enough arguments";
                    return false;
                }

                var data = Game1.content.Load<Dictionary<string, GuidebookData>>("spacechase0.SpaceCore/Guidebooks");
                if (!data.TryGetValue(args[1], out var specificData))
                {
                    error = "Invalid guidebook ID";
                    return false;
                }

                GuidebookMenu menu = new(specificData);
                if (args.Length >= 3)
                {
                    string chapter = args[2], pageId = null;
                    if (args.Length >= 4)
                        pageId = args[3];
                    menu.GotoChapter(chapter, pageId);
                }
                Game1.activeClickableMenu = menu;

                error = null;
                return true;
            });

            GameStateQuery.Register("PLAYER_SEEN_CONVERSATION_TOPIC", (string[] query, GameStateQueryContext ctx) =>
            {
                return GameStateQuery.Helpers.WithPlayer(ctx.Player, query[1], (f) => f.previousActiveDialogueEvents.ContainsKey(query[2]));
            });

            GameStateQuery.Register("NEARBY_CROPS", (string[] query, GameStateQueryContext ctx) =>
            {
                if (!ArgUtility.TryGetInt(query, 1, out int radius, out string error) || !ArgUtility.TryGet(query, 2, out string cropSeedId, out error))
                {
                    Log.Warn($"Error for NEARBY_CROPS: {error}");
                    return false;
                }
                if (ctx.CustomFields == null || !ctx.CustomFields.TryGetValue("Tile", out object tileObj) || tileObj is not Vector2 tile)
                {
                    Log.Warn("No tile for NEARBY_CROPS GSQ");
                    return false;
                }

                for (int ix = -radius; ix <= radius; ++ix)
                {
                    for (int iy = -radius; iy <= radius; ++iy)
                    {
                        if (ix == 0 && iy == 0)
                            continue;

                        if (!ctx.Location.terrainFeatures.TryGetValue(tile + new Vector2(ix, iy), out var tf) || tf is not HoeDirt hd || hd.crop == null)
                            continue;
                        
                        if (hd.crop.netSeedIndex.Value == cropSeedId && hd.crop.currentPhase.Value == hd.crop.phaseDays.Count - 1)
                            return true;
                    }
                }

                return false;
            });

            Event.RegisterCommand("damageFarmer", DamageFarmerEventCommand);
            Event.RegisterCommand("giveHat", GiveHatEventCommand);
            Event.RegisterCommand("setDating", SetDatingEventCommand);
            Event.RegisterCommand("setEngaged", SetEngagedEventCommand);
            Event.RegisterCommand("totemWarpEffect", TotemWarpEventCommand);
            Event.RegisterCommand("setActorScale", SetActorScale);
            Event.RegisterCommand("cycleActorColors", CycleActorColors);
            Event.RegisterCommand("flash", FlashEventCommand);
            Event.RegisterCommand("setRaining", SetRainingEventCommand);
            Event.RegisterCommand("screenShake", ScreenShakeEventCommand);
            Event.RegisterCommand("setZoom", SetZoomEventCommand);
            Event.RegisterCommand("smoothZoom", SmoothZoomEventCommand);
            Event.RegisterCommand("switchEventFull", SwitchEventFullCommand);
            Event.RegisterCommand("reSetUpCharacters", ReSetUpCharactersCommand);

            Commands.Register();
            VanillaAssetExpansion.VanillaAssetExpansion.Init();
            SpawnableImpl.Init();
            DungeonImpl.Init();

            new NpcQuestions().Entry(ModManifest, Helper);

            var serializerManager = new SerializerManager(helper.ModRegistry);

            this.Harmony = HarmonyPatcher.Apply(this,
                new CraftingRecipePatcher(),
                new FarmerPatcher(),
                new ForgeMenuPatcher(),
                new Game1Patcher(),
                new GameLocationPatcher(),
                new GameServerPatcher(),
                new LoadGameMenuPatcher(serializerManager),
                new MultiplayerPatcher(),
                new NpcPatcher(),
                new ReadBookPatcher(),
                new SaveGamePatcher(serializerManager),
                new SerializationPatcher(),
                new UtilityPatcher(),
                new HoeDirtPatcher(),
                new SkillBuffPatcher(),
                new SpriteBatchPatcher(),
                new ToolDataDefinitionPatcher()
            );
        }

        private void ReSetUpCharactersCommand(Event @event, string[] args, EventContext context)
        {
            @event.actors.Clear();
            Helper.Reflection.GetMethod(@event, "setUpCharacters").Invoke(string.Join(' ', args.Skip(1)), context.Location);
            @event.CurrentCommand++;
        }

        private void SwitchEventFullCommand(Event @event, string[] args, EventContext context)
        {
            if (!ArgUtility.TryGet(args, 1, out string newKey, out string error))
            {
                context.LogErrorAndSkip(error);
                return;
            }
            string[] commands;
            if (@event.isFestival)
            {
                if (!@event.TryGetFestivalDataForYear(newKey, out string raw2))
                {
                    DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(90, 2);
                    defaultInterpolatedStringHandler.AppendLiteral("can't load new event from festival field '");
                    defaultInterpolatedStringHandler.AppendFormatted(newKey);
                    defaultInterpolatedStringHandler.AppendLiteral("' because there's no such key in the '");
                    defaultInterpolatedStringHandler.AppendFormatted(@event.id);
                    defaultInterpolatedStringHandler.AppendLiteral("' festival");
                    context.LogErrorAndSkip(defaultInterpolatedStringHandler.ToStringAndClear());
                    return;
                }
                commands = Event.ParseCommands(raw2, context.Event.farmer);
            }
            else
            {
                string assetName = "Data\\Events\\" + Game1.currentLocation.Name;
                if (!Game1.content.DoesAssetExist<Dictionary<string, string>>(assetName))
                {
                    context.LogErrorAndSkip("can't load new event from asset '" + assetName + "' because it doesn't exist");
                    return;
                }
                if (!Game1.content.Load<Dictionary<string, string>>(assetName).TryGetValue(newKey, out string raw))
                {
                    DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(81, 2);
                    defaultInterpolatedStringHandler.AppendLiteral("can't load new event from asset '");
                    defaultInterpolatedStringHandler.AppendFormatted(assetName);
                    defaultInterpolatedStringHandler.AppendLiteral("' because it doesn't contain the required '");
                    defaultInterpolatedStringHandler.AppendFormatted(newKey);
                    defaultInterpolatedStringHandler.AppendLiteral("' key");
                    context.LogErrorAndSkip(defaultInterpolatedStringHandler.ToStringAndClear());
                    return;
                }
                commands = Event.ParseCommands(raw, context.Event.farmer);
            }
            List<string> baseCmd =
            [
                $"playMusic {commands[0]}",
                $"viewport {commands[1]}",
                $"reSetUpCharacters {commands[2]}"
            ];

            @event.ReplaceAllCommands(baseCmd.Concat(commands.Skip(3)).ToArray());
            @event.eventSwitched = true;
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/ExtraEquipmentIcon"))
                e.LoadFromModFile<Texture2D>("assets/extras.png", AssetLoadPriority.Low);
            else if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/Guidebooks"))
                e.LoadFrom(() => new Dictionary<string, GuidebookData>(), AssetLoadPriority.Exclusive);
        }

        private static HashSet<string> ambiguousMethods = new();
        private static Dictionary<string, Dictionary<string, List<int>>> locals = new();
        public static List<int> GetLocalIndexForMethod(MethodBase meth, string local)
        {
            string fullDesc = meth.FullDescription();
            if (ambiguousMethods.Contains(fullDesc))
            {
                Log.Warn($"{Assembly.GetCallingAssembly().FullName} just tried to get the locals for method '{fullDesc}' but it's an ambiguous match - we can't do that method due to the same parameter count and names, sorry!");
                return null;
            }
            if (locals.TryGetValue(fullDesc, out var methLocals) && methLocals.TryGetValue(local, out var ret))
            {
                return ret;
            }
            return null;
        }

        private void GatherLocals()
        {
            using var fs = new FileStream(typeof(Game1).Assembly.Location, FileMode.Open, FileAccess.Read);
            using var pe = new PEReader(fs);
            var stuff = pe.ReadDebugDirectory().ToList();
            var s2 = stuff.Where(x => x.Type == DebugDirectoryEntryType.EmbeddedPortablePdb).ToList();
            var prov = pe.ReadEmbeddedPortablePdbDebugDirectoryData(s2.Single());
            var mr = prov.GetMetadataReader();
            var mr2 = pe.GetMetadataReader();

            Dictionary<string, Type> types = new();

            foreach (var mdih in mr.MethodDebugInformation)
            {
                try
                {
                    var mi = mr2.GetMethodDefinition(mdih.ToDefinitionHandle());
                    var t = mr2.GetTypeDefinition(mi.GetDeclaringType());

                    string s = $"{mr2.GetString(t.Namespace)}.{mr2.GetString(t.Name)}";
                    if (!s.StartsWith("StardewValley"))
                        continue;
                    s += ", Stardew Valley";

                    Type type = null;
                    if (types.ContainsKey(s))
                        type = types[s];
                    else
                        types.Add(s, type = Type.GetType(s));

                    // This is pretty inefficient...
                    string methName = mr2.GetString(mi.Name);
                    if (methName == ".cctor") // static constructor, ignoring these since they don't make sense to patch to begin with
                        continue;
                    var meths = AccessTools.GetDeclaredMethods(type).Where(methInfo => methInfo.Name == methName && methInfo.GetParameters().Length == mi.GetParameters().Count).ToList();
                    MethodBase result = null;
                    IEnumerable<string> targetMethParams = mi.GetParameters().Select(ph => mr2.GetString(mr2.GetParameter(ph).Name));
                    foreach (var meth in meths)
                    {
                        if (meth.GetParameters().Select(pi => pi.Name).SequenceEqual(targetMethParams))
                        {
                            result = meth;
                            break;
                        }
                    }
                    if (result == null)
                    {
                        if (methName == ".ctor")
                        {
                            var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            foreach (var ctor in ctors)
                            {
                                if (ctor.GetParameters().Select(pi => pi.Name).SequenceEqual(targetMethParams))
                                {
                                    result = ctor;
                                    break;
                                }
                            }
                        }
                        if (result == null)
                        {
                            //Log.Warn($"Failed to find matching method for {s}.{methName} with params {string.Join(',', targetMethParams)}");
                            continue;
                        }
                    }

                    Dictionary<string, List<int>> methLocals = new();
                    foreach (var lsh in mr.GetLocalScopes(mdih))
                    {
                        var ls = mr.GetLocalScope(lsh);
                        foreach (var lvh in ls.GetLocalVariables())
                        {
                            var lv = mr.GetLocalVariable(lvh);
                            string s_ = mr.GetString(lv.Name);
                            if (methLocals.ContainsKey(s_))
                            {
                                methLocals[s_].Add(lv.Index);
                            }
                            else methLocals.Add(s_, [lv.Index]);
                        }
                    }

                    string fullDesc = result.FullDescription();
                    if (!ambiguousMethods.Contains(fullDesc))
                    {
                        if (locals.ContainsKey(fullDesc))
                        {
                            ambiguousMethods.Add(fullDesc);
                            locals.Remove(fullDesc);
                        }
                        else locals.Add(fullDesc, methLocals);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("got exception " + mdih + " " + e);
                    continue;
                }
            }
        }

        internal NPC lastInteraction = null;
        internal Dictionary<string, Action> lastChoices = null;
        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (e.Button.IsActionButton() && (Config.SocialInteractions_AlwaysTrigger || Config.SocialInteractions_TriggerModifier.IsDown()))
            {
                Game1.player.forceCanMove();
                Game1.dialogueUp = false;

                Rectangle tileRect = new Rectangle((int)e.Cursor.GrabTile.X * 64, (int)e.Cursor.GrabTile.Y * 64, 64, 64);
                NPC npc = null;
                foreach (var character in Game1.currentLocation.characters)
                {
                    if (!character.IsMonster && character.GetBoundingBox().Intersects(tileRect))
                    {
                        npc = character;
                        break;
                    }
                }
                if (npc == null)
                    npc = Game1.currentLocation.isCharacterAtTile(e.Cursor.Tile + new Vector2(0f, 1f));
                if (npc == null)
                    npc = Game1.currentLocation.isCharacterAtTile(e.Cursor.GrabTile + new Vector2(0f, 1f));

                if (npc == null || //!Utility.withinRadiusOfPlayer( npc.getStandingX(), npc.getStandingY(), 1, Game1.player ) ||
                    !Game1.NPCGiftTastes.ContainsKey(npc.Name) || !Game1.player.friendshipData.ContainsKey(npc.Name))
                    return;
                Helper.Input.Suppress(e.Button);

                lastChoices = new();
                lastChoices.Add(I18n.Interaction_Chat(), () =>
                {
                    bool stowed = Game1.player.netItemStowed.Value;
                    Game1.player.netItemStowed.Value = true;
                    Game1.player.UpdateItemStow();
                    npc.checkAction(Game1.player, Game1.player.currentLocation);
                    Game1.player.netItemStowed.Value = stowed;
                    Game1.player.UpdateItemStow();
                });
                if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.canBeGivenAsGift())
                {
                    lastChoices.Add(I18n.Interaction_GiftHeld(), () => npc.tryToReceiveActiveObject(Game1.player));
                }
                (GetApi() as Api).InvokeASI(npc, (s, a) => lastChoices.Add(s, a));

                List<Response> responses = new();
                foreach (var entry in lastChoices)
                {
                    responses.Add(new(entry.Key, entry.Key));
                }
                Response cancel = new("Cancel", I18n.Interaction_Cancel());
                cancel.SetHotKey(Microsoft.Xna.Framework.Input.Keys.Escape);
                responses.Add(cancel);

                Game1.currentLocation.afterQuestion = (farmer, answer) =>
                {
                    //Log.Debug("hi");
                    Game1.activeClickableMenu = null;
                    Game1.player.CanMove = true;
                    Game1.dialogueUp = false;
                    if (lastChoices.ContainsKey(answer))
                        lastChoices[answer]();
                    else
                        ;// Log.Debug("wat");
                };
                Game1.currentLocation.createQuestionDialogue(I18n.InteractionWith(npc.displayName), responses.ToArray(), "advanced-social-interaction");
                
            }
        }

        private bool StringEqualsGSQ(string[] query, GameStateQueryContext ctx)
        {
            if (!ArgUtility.TryGet(query, 0, out string str1, out string error, allowBlank: false) ||
                 !ArgUtility.TryGet(query, 1, out string str2, out error, allowBlank: false) ||
                 !ArgUtility.TryGetOptionalBool(query, 2, out bool caseSensitive, out error, defaultValue: true))
            {

                return false;
            }

            return string.Equals( str1, str2, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase );
        }

        private Api apiReturned;
        /// <inheritdoc />
        public override object GetApi()
        {
            return apiReturned ??= new Api();
        }

        private static void DamageFarmerEventCommand(Event evt, string[] args, EventContext ctx)
        {
            Game1.eventUp = false;
            try
            {
                evt.farmer.takeDamage(int.Parse(args[1]), false, null);
            }
            finally
            {
                Game1.eventUp = true;
                evt.CurrentCommand++;
            }
        }

        private static void GiveHatEventCommand(Event evt, string[] args, EventContext ctx)
        {
            try
            {
                Game1.player.addItemByMenuIfNecessary(new Hat(args[1]));
            }
            finally
            {
                evt.CurrentCommand++;
            }
        }

        private static void SetDatingEventCommand(Event evt, string[] args, EventContext ctx)
        {
            try
            {
                if (!Game1.player.friendshipData.TryGetValue(args[1], out Friendship f))
                {
                    Log.Warn("Could not find NPC " + args[1] + " to mark as dating");
                }
                else
                {
                    if (args.Length >= 3 && args[2].ToLower().Equals("false"))
                    {
                        if (f.Status == FriendshipStatus.Dating)
                            f.Status = FriendshipStatus.Friendly;
                    }
                    else
                    {
                        f.Status = FriendshipStatus.Dating;
                        Game1.Multiplayer.globalChatInfoMessage("Dating", Game1.player.Name, DataLoader.Characters( Game1.content )[ args[ 1 ] ].DisplayName);
                    }
                    }
            }
            finally
            {
                evt.CurrentCommand++;
            }
        }
        private static void SetEngagedEventCommand(Event evt, string[] args, EventContext ctx)
        {
            try
            {
                if ( args.Length < 4 )
                {
                    Log.Warn("Not enough arguments for setEngaged event command");
                }
                else if (!Game1.player.friendshipData.TryGetValue(args[1], out Friendship f))
                {
                    Log.Warn("Could not find NPC " + args[1] + " to mark as engagement");
                }
                else if (!bool.TryParse(args[2], out bool asRoomate))
                {
                    Log.Warn("Could not parse asRoommate as boolean");
                }
                else if (!int.TryParse(args[3], out int weddingOffset) || weddingOffset < 1)
                {
                    Log.Warn("Could not parse weddingOffset as positive integer");
                }
                else
                {
                    WorldDate weddingDate = new WorldDate(Game1.Date);
                    weddingDate.TotalDays += weddingOffset;
                    while (!Game1.canHaveWeddingOnDay(weddingDate.DayOfMonth, weddingDate.Season))
                        ++weddingDate.TotalDays;

                    Game1.player.spouse = args[1];
                    f.Status = FriendshipStatus.Engaged;
                    f.RoommateMarriage = asRoomate;
                    f.WeddingDate = weddingDate;

                    Game1.Multiplayer.globalChatInfoMessage("Engaged", Game1.player.Name, Game1.getCharacterFromName(args[1]).GetTokenizedDisplayName());
                }
            }
            finally
            {
                evt.CurrentCommand++;
            }
        }
        private static void TotemWarpEventCommand(Event evt, string[] args, EventContext ctx)
        {
            var loc = ctx.Location;


            try
            {
                int tx = int.Parse(args[1]);
                int ty = int.Parse(args[2]);
                string[] colparts = args[3].Split(',');
                Color col = new Color(int.Parse(colparts[0]), int.Parse(colparts[1]), int.Parse(colparts[2]));
                string totem = args[4];
                string[] rectparts = args[5].Split(',');
                Rectangle rect = new(int.Parse(rectparts[0]), int.Parse(rectparts[1]), int.Parse(rectparts[2]), int.Parse(rectparts[3]));

                var mp = SpaceCore.Instance.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

                loc.playSound("warrior");
                loc.TemporarySprites.Add(new TemporaryAnimatedSprite(totem, rect, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false)
                {
                    motion = new Vector2(0f, -1f),
                    scaleChange = 0.01f,
                    alpha = 1f,
                    alphaFade = 0.0075f,
                    shakeIntensity = 1f,
                    initialPosition = new Vector2(tx, ty) * Game1.tileSize + new Vector2(0f, -96f),
                    xPeriodic = true,
                    xPeriodicLoopTime = 1000f,
                    xPeriodicRange = 4f,
                    layerDepth = 1f
                });
                loc.TemporarySprites.Add(new TemporaryAnimatedSprite(totem, rect, 9999f, 1, 999, Game1.player.Position + new Vector2(-64f, -96f), flicker: false, flipped: false)
                {
                    motion = new Vector2(0f, -0.5f),
                    scaleChange = 0.005f,
                    scale = 0.5f,
                    alpha = 1f,
                    alphaFade = 0.0075f,
                    shakeIntensity = 1f,
                    delayBeforeAnimationStart = 10,
                    initialPosition = new Vector2(tx, ty) * Game1.tileSize + new Vector2(-64f, -96f),
                    xPeriodic = true,
                    xPeriodicLoopTime = 1000f,
                    xPeriodicRange = 4f,
                    layerDepth = 0.9999f
                });
                loc.TemporarySprites.Add(new TemporaryAnimatedSprite(totem, rect, 9999f, 1, 999, Game1.player.Position + new Vector2(64f, -96f), flicker: false, flipped: false)
                {
                    motion = new Vector2(0f, -0.5f),
                    scaleChange = 0.005f,
                    scale = 0.5f,
                    alpha = 1f,
                    alphaFade = 0.0075f,
                    delayBeforeAnimationStart = 20,
                    shakeIntensity = 1f,
                    initialPosition = new Vector2(tx,ty)*Game1.tileSize + new Vector2(64f, -96f),
                    xPeriodic = true,
                    xPeriodicLoopTime = 1000f,
                    xPeriodicRange = 4f,
                    layerDepth = 0.9988f
                });
                Game1.screenGlowOnce(col, hold: false);
                Utility.addSprinklesToLocation(loc, tx, ty, 16, 16, 1300, 20, Color.White, null, motionTowardCenter: true);
                DelayedAction.functionAfterDelay(() =>
                {
                    for (int i = 0; i < 12; i++)
                    {
                        loc.TemporarySprites.Add(new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)tx * Game1.tileSize - 256, (int)tx * Game1.tileSize + 192), Game1.random.Next((int)ty * Game1.tileSize - 256, (int)ty * Game1.tileSize + 192)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
                    }
                    loc.playSound("wand");
                    //Game1.displayFarmer = false;
                    //Game1.player.temporarilyInvincible = true;
                    //Game1.player.temporaryInvincibilityTimer = -2000;
                    //Game1.player.freezePause = 1000;
                    Game1.flashAlpha = 1f;
                    //DelayedAction.fadeAfterDelay(totemWarpForReal, 1000);
                    int j = 0;
                    for (int x = tx + 8; x >= tx - 8; x--)
                    {
                        loc.TemporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(x, ty) * 64f, Color.White, 8, flipped: false, 50f)
                        {
                            layerDepth = 1f,
                            delayBeforeAnimationStart = j * 25,
                            motion = new Vector2(-0.25f, 0f)
                        });
                        j++;
                    }
                }, 2000);
            }
            finally
            {
                evt.CurrentCommand++;
            }
        }
        public class AnimatedSpriteExtras
        {
            public Vector2 scale = Vector2.One;
            public int currGradInd;
            public Color[] grad;
        }

        public static ConditionalWeakTable<AnimatedSprite, AnimatedSpriteExtras> spriteExtras = new();
        private static void SetActorScale(Event evt, string[] args, EventContext ctx)
        {
            try
            {
                string actorName = args[1];
                float x = float.Parse(args[2]);
                float y = float.Parse(args[3]);
                NPC actor = evt.getActorByName(actorName);
                spriteExtras.GetOrCreateValue(actor.Sprite).scale = new(x, y);
            }
            finally
            {
                evt.CurrentCommand++;
            }
        }
        private static void CycleActorColors(Event evt, string[] args, EventContext ctx)
        {
            try
            {
                string actorName = args[1];
                NPC actor = evt.getActorByName(actorName);
                var extras = spriteExtras.GetOrCreateValue(actor.Sprite);
                if (args[2] == "null")
                {
                    extras.grad = null;
                    return;
                }
                extras.currGradInd = 0;

                float interval = float.Parse(args[2]);
                List<Color> cols = new List<Color>();
                for (int i = 3; i < args.Length; ++i)
                {
                    string[] colparts = args[i].Split(',');
                    cols.Add(new Color(int.Parse(colparts[0]), int.Parse(colparts[1]), int.Parse(colparts[2])));
                }

                if (cols.Count == 1)
                {
                    spriteExtras.GetOrCreateValue(actor.Sprite).grad = cols.ToArray();
                    return;
                }
                cols.Add(cols[0]);

                int perSection = (int)(60 * interval / (cols.Count - 1));
                Color[] grad = new Color[(perSection * (cols.Count - 1)) + 1];
                for (int i = 0; i < cols.Count - 1; ++i)
                {
                    Color[] gradpart = Util.GetColorGradient(cols[i], cols[i + 1], perSection).ToArray();
                    Array.Copy(gradpart, 0, grad, perSection * i, gradpart.Length);
                }
                grad[grad.Length - 1] = cols[0];

                extras.grad = grad;
            }
            finally
            {
                evt.CurrentCommand++;
            }
        }
        private static void FlashEventCommand(Event @event, string[] args, EventContext ctx)
        {
            try
            {
                float duration = float.Parse(args[1]);
                Game1.flashAlpha = 1 + duration * 6;
            }
            finally
            {
                @event.CurrentCommand++;
            }
        }

        private void SetRainingEventCommand(Event @event, string[] args, EventContext context)
        {
            bool val = Convert.ToBoolean(args[2]);
            Game1.netWorldState.Value.GetWeatherForLocation(args[1]).IsRaining = val;
            if (args[1] == "Default")
                Game1.isRaining = val;

            @event.CurrentCommand++;
        }

        internal float screenShakeIntensity = 0;
        internal float pendingScreenShake = 0;
        internal Vector2 preShakeViewportPos = default(Vector2);
        internal Vector2 shakeViewportPos = default(Vector2);
        internal Vector2 shakeAmount = Vector2.Zero;
        private void ScreenShakeEventCommand(Event @event, string[] args, EventContext context)
        {
            float intensity = Convert.ToSingle(args[1]);
            float duration = Convert.ToSingle(args[2]);
            this.screenShakeIntensity = intensity;
            this.pendingScreenShake = duration;
            this.preShakeViewportPos = this.shakeViewportPos = Game1.currentViewportTarget;

            @event.CurrentCommand++;
        }

        internal float currZoom = 1;
        internal float targetZoom = 1;
        internal float zoomTimeRemaining = -1;

        private void SetZoomEventCommand(Event @event, string[] args, EventContext context)
        {
            targetZoom = Convert.ToSingle(args[1]);
            zoomTimeRemaining = -1;

            @event.CurrentCommand++;
        }

        private void SmoothZoomEventCommand(Event @event, string[] args, EventContext context)
        {
            targetZoom = Convert.ToSingle(args[1]);
            zoomTimeRemaining = Convert.ToSingle(args[2]);

            @event.CurrentCommand++;
        }

        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Set up skills in GameLaunched to allow ModRegistry to be used here.
            Skills.Init(this.Helper.Events);

            // These are null in mod entry
            GuidebookFont.Fonts.Add("default", Game1.smallFont);
            GuidebookFont.Fonts.Add("tiny", Game1.tinyFont);
            GuidebookFont.Fonts.Add("dialogue", Game1.dialogueFont);

            api = Helper.ModRegistry.GetApi<IApi>(ModManifest.UniqueID);
            api.RegisterCustomProperty(typeof(Farmer), "SpaceCore_ExtraEquippables", typeof(NetStringDictionary<Item, NetRef<Item>>), AccessTools.Method(typeof(FarmerExtData), nameof(FarmerExtData.get_equippables)), AccessTools.Method(typeof(FarmerExtData), nameof(FarmerExtData.set_equippables)));

            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => this.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(this.Config)
                );
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_CustomSkillPage_Name,
                    tooltip: I18n.Config_CustomSkillPage_Tooltip,
                    getValue: () => this.Config.CustomSkillPage,
                    setValue: value => this.Config.CustomSkillPage = value
                );
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_SupportAllProfessionsMod_Name,
                    tooltip: I18n.Config_SupportAllProfessionsMod_Tooltip,
                    getValue: () => this.Config.SupportAllProfessionsMod,
                    setValue: value => this.Config.SupportAllProfessionsMod = value
                );

                configMenu.AddSectionTitle(ModManifest, () => I18n.Config_AdvancedSocialInteractions());
                configMenu.AddBoolOption(ModManifest, () => Config.SocialInteractions_AlwaysTrigger, (val) => Config.SocialInteractions_AlwaysTrigger = val, () => I18n.Config_AlwaysTrigger_Name(), () => I18n.Config_AlwaysTrigger_Description());
                configMenu.AddKeybindList(ModManifest, () => Config.SocialInteractions_TriggerModifier, (val) => Config.SocialInteractions_TriggerModifier = val, () => I18n.Config_TriggerModifier_Name(), () => I18n.Config_TriggerModifier_Description());
            }

            var entoaroxFramework = this.Helper.ModRegistry.GetApi<IEntoaroxFrameworkApi>("Entoarox.EntoaroxFramework");
            if (entoaroxFramework != null)
            {
                Log.Info("Telling EntoaroxFramework to let us handle the serializer");
                entoaroxFramework.HoistSerializerOwnership();
            }


            var cp = Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");
            if (cp != null)
            {
                cp.RegisterToken(ModManifest, "CurrentlyInEvent", () =>
                {
                    if (!Context.IsWorldReady )
                        return null;

                    return new string[] { Game1.CurrentEvent != null ? "true" : "false" };
                });
                cp.RegisterToken(ModManifest, "CurrentEventId", () =>
                {
                    if (!Context.IsWorldReady || Game1.CurrentEvent == null)
                        return null;

                    return new string[] { Game1.CurrentEvent.id.ToString() };
                });
                cp.RegisterToken(ModManifest, "BooksellerInTown", () =>
                {
                    if (!Context.IsWorldReady)
                        return null;

                    return new string[] { Utility.getDaysOfBooksellerThisSeason().Contains(Game1.dayOfMonth)?"true":"false" };
                });
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.CurrentEvent != null)
            {
                foreach (var actor in Game1.CurrentEvent.actors)
                {
                    var spr = actor.Sprite;
                    var extra = spriteExtras.GetOrCreateValue(spr);
                    if ( extra.grad != null )
                        extra.currGradInd = (extra.currGradInd + 1) % extra.grad.Length;
                }
            }

            if ( Game1.CurrentEvent == null && (currZoom != 1 || targetZoom != 1) )
            {
                currZoom = targetZoom = 1;
                zoomTimeRemaining = -1;
                Game1.updateViewportForScreenSizeChange(false, Game1.game1.Window.ClientBounds.Width, Game1.game1.Window.ClientBounds.Height);
            }
            if (currZoom != targetZoom && Game1.CurrentEvent != null)
            {
                float newZoom = currZoom;
                if (zoomTimeRemaining > 0.01)
                {
                    float sec = (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;

                    newZoom = MathF.Exp(Utility.Lerp(MathF.Log(currZoom), MathF.Log(targetZoom), sec / zoomTimeRemaining));
                    newZoom += (targetZoom - currZoom) * (sec / zoomTimeRemaining);
                    zoomTimeRemaining -= sec;
                }
                else
                    newZoom = targetZoom;

                int newW = (int)(Game1.game1.Window.ClientBounds.Width / Game1.options.zoomLevel);
                int newH = (int)(Game1.game1.Window.ClientBounds.Height / Game1.options.zoomLevel);
                Point center = new(Game1.viewport.X + Game1.viewport.Width / 2, Game1.viewport.Y + Game1.viewport.Height / 2);
                Game1.viewport.X = center.X - newW / 2;
                Game1.viewport.Y = center.Y - newH / 2;
                Game1.viewport.Width = newW;
                Game1.viewport.Height = newH;

                currZoom = newZoom;
            }

            if (this.pendingScreenShake > 0)
            {
                this.pendingScreenShake -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                if (this.pendingScreenShake <= 0)
                {
                    Game1.currentViewportTarget = this.preShakeViewportPos;
                }
                else
                {
                    // If the camera moved otherwise we need to update things
                    var vp = new Vector2(Game1.viewport.Location.X, Game1.viewport.Location.Y);
                    if (vp != this.shakeViewportPos)
                        this.preShakeViewportPos += (vp - this.shakeViewportPos);

                    float angle = (float) Game1.random.NextDouble();
                    shakeAmount = new Vector2( (int)(MathF.Cos(angle) * this.screenShakeIntensity), (int)(MathF.Sin(angle) * this.screenShakeIntensity));
                    this.shakeViewportPos = this.preShakeViewportPos + this.shakeAmount;
                    Game1.viewport.X = (int)this.shakeViewportPos.X;
                    Game1.viewport.Y = (int)this.shakeViewportPos.Y;
                }
            }

            if (Game1.shouldTimePass())
            {
                var ext = Game1.player.GetExtData();
                if (ext.StaminaRegen > 0)
                {
                    ext.staminaBuffer += ext.StaminaRegen * (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                    if (ext.staminaBuffer >= 1)
                    {
                        int whole = (int)Math.Truncate(ext.staminaBuffer);
                        ext.staminaBuffer -= whole;
                        Game1.player.Stamina += whole; 
                    }
                }
                if (ext.HealthRegen != 0)
                {
                    ext.healthBuffer += ext.HealthRegen * (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                    if (Math.Abs(ext.healthBuffer) >= 1)
                    {
                        int whole = (int)Math.Truncate(ext.healthBuffer);
                        ext.healthBuffer -= whole;
                        Game1.player.health = Math.Min(Game1.player.health + whole, Game1.player.maxHealth);
                    }
                }
            }

            // disable serializer if not used
            if (this.IsFirstTick && SpaceCore.ModTypes.Count == 0)
            {
                this.IsFirstTick = false;

                Log.Info("Disabling serializer patches (no mods using serializer API)");
                foreach (var method in SaveGamePatcher.GetSaveEnumeratorMethods())
                    this.Harmony.Unpatch(method, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetSaveEnumerator)));
                foreach (var method in SaveGamePatcher.GetLoadEnumeratorMethods())
                    this.Harmony.Unpatch(method, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetLoadEnumerator)));
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                this.LegacyDataMigrator.OnSaveLoaded();
            }
            catch (Exception ex)
            {
                Log.Warn($"Exception migrating legacy save data: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(Options), "get_zoomLevel")]
    public static class OptionsZoomPatch
    {
        public static void Postfix(ref float __result)
        {
            if (Game1.CurrentEvent != null)
            {
                __result *= SpaceCore.Instance.currZoom;
            }
        }
    }
}
