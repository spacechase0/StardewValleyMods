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

namespace SpaceCore
{
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


        /*********
        ** Accessors
        *********/
        public Configuration Config { get; set; }
        internal static SpaceCore Instance;
        internal static IReflectionHelper Reflection;
        internal static List<Type> ModTypes = new();
        internal static Dictionary<Type, Dictionary<string, CustomPropertyInfo>> CustomProperties = new();


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

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

            var manualContext = new TriggerActionContext("Manual", Array.Empty<object>(), null); ;
            GameLocation.RegisterTileAction("spacechase0.SpaceCore_TriggerAction", (loc, args, farmer, pos) =>
            {
                var triggers = TriggerActionManager.GetActionsForTrigger("Manual");
                var trigger = triggers.FirstOrDefault(t => t.Data.Id == args[0]);
                if (trigger != null)
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
                var triggers = TriggerActionManager.GetActionsForTrigger("Manual");
                var trigger = triggers.FirstOrDefault(t => t.Data.Id == args[0]);
                if (trigger != null)
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

                error = null;
                Game1.addHUDMessage(new HUDMessage(args[0]));
                return true;
            });

            GameStateQuery.Register("NEARBY_CROP", (string[] query, GameStateQueryContext ctx) =>
            {
                if (!ArgUtility.TryGetInt(query, 1, out int radius, out string error) || !ArgUtility.TryGet(query, 2, out string cropSeedId, out error))
                {
                    Log.Warn($"Error for NEARBY_CROP: {error}");
                    return false;
                }
                if (ctx.CustomFields == null || !ctx.CustomFields.TryGetValue("Tile", out object tileObj) || tileObj is not Vector2 tile)
                {
                    Log.Warn("No tile for NEARBY_CROP GSQ");
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
                        hd.crop.growCompletely();
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

            Commands.Register();
            VanillaAssetExpansion.VanillaAssetExpansion.Init();

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
                new SaveGamePatcher(serializerManager),
                new SerializationPatcher(),
                new UtilityPatcher(),
                new HoeDirtPatcher(),
                new SkillBuffPatcher(),
                new SpriteBatchPatcher()
            );
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

        private Api api;
        /// <inheritdoc />
        public override object GetApi()
        {
            return api ??= new Api();
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
                        Game1.Multiplayer.globalChatInfoMessage("Dating", Game1.player.Name, Game1.getCharacterFromName(args[1]).GetTokenizedDisplayName());
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
                else if (!int.TryParse(args[2], out int weddingOffset) || weddingOffset < 1)
                {
                    Log.Warn("Could not parse weddingOffset as positive integer");
                }
                else
                {
                    WorldDate weddingDate = new WorldDate(Game1.Date);
                    weddingDate.TotalDays += weddingOffset;
                    while (!Game1.canHaveWeddingOnDay(weddingDate.DayOfMonth, weddingDate.Season))
                        ++weddingDate.TotalDays;

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
            Game1.netWorldState.Value.GetWeatherForLocation(args[1]).IsRaining = Convert.ToBoolean(args[2]);
            if (args[1] == "Default")
                Game1.isRaining = true;

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

        public class NpcExtensionData
        {
            public Dictionary<string, string> GiftEventTriggers = new();
            public bool IgnoreMarriageSchedule { get; set; } = false;
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
