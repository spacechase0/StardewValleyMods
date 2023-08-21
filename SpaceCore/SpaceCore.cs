using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
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
using SpaceCore.Framework.Schedules;
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

namespace SpaceCore
{
    /*
    public static class Fix1_5NetCodeBugPatch
    {
        public static void Prefix(
            NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>> __instance,
            string key,
            ref object __state
        )
        {
            __state = __instance is ModDataDictionary && __instance.ContainsKey(key);
        }
        public static void Postfix(
            NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>> __instance,
            string key,
            string value,
            object __state,
            System.Collections.IList ___outgoingChanges,
            Dictionary<string, NetVersion> ___dictReassigns
        )
        {
            if(__instance is ModDataDictionary)
            if (__state as bool? == true)
            {
                var field = __instance.FieldDict[key];
                var ogts = __instance.GetType().BaseType.BaseType.BaseType.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                var ogt = ogts.First(t => t.Name.StartsWith("OutgoingChange"));
                ogt = ogt.MakeGenericType(new Type[] { typeof( string ), typeof( string ), typeof( NetString ), typeof( SerializableDictionary<string, string> ), typeof( NetStringDictionary<string,NetString> ) });
                var ogc = ogt.GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0];
                object og = ogc.Invoke(new object[] { false, key, field, ___dictReassigns[ key ] });
                ___outgoingChanges.Add(og);
                if (key.Contains("spacechase0"))
                    Log.Debug("oc:" + ___outgoingChanges.Count);
            }
        }
    }
    */

    [HarmonyPatch(typeof(AnimatedSprite), "draw", new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float) })]
    public static class AnimatedSpriteDrawExtrasPatch1
    {
        public static bool Prefix(AnimatedSprite __instance, SpriteBatch b, Vector2 screenPosition, float layerDepth)
        {
            if (__instance.Texture != null)
            {
                var extras = SpaceCore.spriteExtras.GetOrCreateValue(__instance);
                b.Draw(__instance.Texture, screenPosition, __instance.sourceRect, extras.grad == null ? Color.White : extras.grad[ extras.currGradInd ], 0f, Vector2.Zero, 4f * extras.scale, (__instance.CurrentAnimation != null && __instance.CurrentAnimation[__instance.currentAnimationIndex].flip) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(AnimatedSprite), "draw", new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof( int ), typeof( int ), typeof( Color ), typeof( bool ), typeof( float), typeof( float ), typeof( bool ) })]
    public static class AnimatedSpriteDrawExtrasPatch2
    {
        public static bool Prefix(AnimatedSprite __instance, SpriteBatch b, Vector2 screenPosition, float layerDepth, int xOffset, int yOffset, Color c, bool flip = false, float scale = 1f, float rotation = 0f, bool characterSourceRectOffset = false)
        {
            if (__instance.Texture != null)
            {
                var extras = SpaceCore.spriteExtras.GetOrCreateValue(__instance);
                b.Draw(__instance.Texture, screenPosition, new Rectangle(__instance.sourceRect.X + xOffset, __instance.sourceRect.Y + yOffset, __instance.sourceRect.Width, __instance.sourceRect.Height), Color.Lerp( c, (extras.grad == null ? Color.White : extras.grad[extras.currGradInd]), 0.5f), rotation, characterSourceRectOffset ? new Vector2(__instance.SpriteWidth / 2, (float)__instance.SpriteHeight * 3f / 4f) : Vector2.Zero, scale * extras.scale, (flip || (__instance.CurrentAnimation != null && __instance.CurrentAnimation[__instance.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
            }
            return false;
        }
    }

    // TODO Transpiler
    [HarmonyPatch(typeof(NPC), "draw", new Type[] { typeof(SpriteBatch), typeof(float) })]
    public static class AnimatedSpriteDrawExtrasPatch3
    {
        public static bool Prefix(NPC __instance, SpriteBatch b, float alpha, int ___shakeTimer, NetVector2 ___defaultPosition)
        {
            var extras = SpaceCore.spriteExtras.GetOrCreateValue(__instance.Sprite);
            if (__instance.Sprite == null || __instance.IsInvisible || (!Utility.isOnScreen(__instance.Position, 128) && (!__instance.eventActor || !(__instance.currentLocation is Summit))))
            {
                return false;
            }
            if ((bool)__instance.swimming)
            {
                b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(32f, 80 + __instance.yJumpOffset * 2) + ((___shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero) - new Vector2(0f, __instance.yOffset), new Microsoft.Xna.Framework.Rectangle(__instance.Sprite.SourceRect.X, __instance.Sprite.SourceRect.Y, __instance.Sprite.SourceRect.Width, __instance.Sprite.SourceRect.Height / 2 - (int)(__instance.yOffset / 4f)), Color.White, __instance.rotation, new Vector2(32f, 96f) / 4f, Math.Max(0.2f, __instance.scale) * 4f, __instance.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, __instance.drawOnTop ? 0.991f : ((float)__instance.getStandingY() / 10000f)));
                Vector2 localPosition = __instance.getLocalPosition(Game1.viewport);
                b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)localPosition.X + (int)__instance.yOffset + 8, (int)localPosition.Y - 128 + __instance.Sprite.SourceRect.Height * 4 + 48 + __instance.yJumpOffset * 2 - (int)__instance.yOffset, __instance.Sprite.SourceRect.Width * 4 - (int)__instance.yOffset * 2 - 16, 4), Game1.staminaRect.Bounds, Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, (float)__instance.getStandingY() / 10000f + 0.001f);
            }
            else
            {
                b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(__instance.GetSpriteWidthForPositioning() * 4 / 2, __instance.GetBoundingBox().Height / 2) + ((___shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), __instance.Sprite.SourceRect, (extras.grad != null ? extras.grad[extras.currGradInd]:Color.White)* alpha, __instance.rotation, new Vector2(__instance.Sprite.SpriteWidth / 2, (float)__instance.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, __instance.scale) * 4f * extras.scale, (__instance.flip || (__instance.Sprite.CurrentAnimation != null && __instance.Sprite.CurrentAnimation[__instance.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, __instance.drawOnTop ? 0.991f : ((float)__instance.getStandingY() / 10000f)));
            }
            if (__instance.Breather && ___shakeTimer <= 0 && !__instance.swimming && __instance.Sprite.currentFrame < 16 && !__instance.farmerPassesThrough)
            {
                Microsoft.Xna.Framework.Rectangle chestBox = __instance.Sprite.SourceRect;
                chestBox.Y += __instance.Sprite.SpriteHeight / 2 + __instance.Sprite.SpriteHeight / 32;
                chestBox.Height = __instance.Sprite.SpriteHeight / 4;
                chestBox.X += __instance.Sprite.SpriteWidth / 4;
                chestBox.Width = __instance.Sprite.SpriteWidth / 2;
                Vector2 chestPosition = new Vector2(__instance.Sprite.SpriteWidth * 4 / 2, 8f);
                if (__instance.Age == 2)
                {
                    chestBox.Y += __instance.Sprite.SpriteHeight / 6 + 1;
                    chestBox.Height /= 2;
                    chestPosition.Y += __instance.Sprite.SpriteHeight / 8 * 4;
                    if (__instance is Child)
                    {
                        if ((__instance as Child).Age == 0)
                        {
                            chestPosition.X -= 12f;
                        }
                        else if ((__instance as Child).Age == 1)
                        {
                            chestPosition.X -= 4f;
                        }
                    }
                }
                else if (__instance.Gender == 1)
                {
                    chestBox.Y++;
                    chestPosition.Y -= 4f;
                    chestBox.Height /= 2;
                }
                float breathScale = Math.Max(0f, (float)Math.Ceiling(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 600.0 + (double)(___defaultPosition.X * 20f))) / 4f);
                b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + chestPosition + ((___shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), chestBox, (extras.grad != null ? extras.grad[extras.currGradInd] : Color.White) * alpha, __instance.rotation, new Vector2(chestBox.Width / 2, chestBox.Height / 2 + 1), Math.Max(0.2f, __instance.scale) * 4f * extras.scale + new Vector2(breathScale,breathScale), __instance.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, __instance.drawOnTop ? 0.992f : ((float)__instance.getStandingY() / 10000f + 0.001f)));
            }
            if (__instance.isGlowing)
            {
                b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(__instance.GetSpriteWidthForPositioning() * 4 / 2, __instance.GetBoundingBox().Height / 2) + ((___shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), __instance.Sprite.SourceRect, __instance.glowingColor * __instance.glowingTransparency, __instance.rotation, new Vector2(__instance.Sprite.SpriteWidth / 2, (float)__instance.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, __instance.scale) * 4f * extras.scale, __instance.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, __instance.drawOnTop ? 0.99f : ((float)__instance.getStandingY() / 10000f + 0.001f)));
            }
            if (__instance.IsEmoting && !Game1.eventUp && !(__instance is Child) && !(__instance is Pet))
            {
                Vector2 emotePosition = __instance.getLocalPosition(Game1.viewport);
                emotePosition.Y -= 32 + __instance.Sprite.SpriteHeight * 4;
                b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(__instance.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, __instance.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)__instance.getStandingY() / 10000f);
            }
            return false;
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

        /// <summary>A queue of textures to dispose, with the <see cref="Game1.ticks"/> value when they were queued.</summary>
        private readonly Queue<KeyValuePair<Texture2D, int>> TextureDisposalQueue = new();


        /*********
        ** Accessors
        *********/
        public Configuration Config { get; set; }
        internal static SpaceCore Instance;
        internal static IReflectionHelper Reflection;
        internal static List<Type> ModTypes = new();
        internal static Dictionary<Type, Dictionary<string, CustomPropertyInfo>> CustomProperties = new();
        internal static Dictionary<GameLocation.LocationContext, CustomLocationContext> CustomLocationContexts = new();


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
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.GameLoop.Saved += this.OnSaved;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;

            helper.Events.Content.AssetRequested += this.Content_AssetRequested;

            SpaceEvents.ActionActivated += this.SpaceEvents_ActionActivated;

            EventPatcher.CustomCommands.Add("damageFarmer", AccessTools.Method(this.GetType(), "DamageFarmerEventCommand"));
            EventPatcher.CustomCommands.Add("giveHat", AccessTools.Method(this.GetType(), "GiveHatEventCommand"));
            EventPatcher.CustomCommands.Add("setDating", AccessTools.Method(this.GetType(), "SetDatingEventCommand"));
            EventPatcher.CustomCommands.Add("totemWarpEffect", AccessTools.Method(this.GetType(), nameof(TotemWarpEventCommand)));
            EventPatcher.CustomCommands.Add("setActorScale", AccessTools.Method(this.GetType(), nameof(SetActorScale)));
            EventPatcher.CustomCommands.Add("cycleActorColors", AccessTools.Method(this.GetType(), nameof(CycleActorColors)));
            EventPatcher.CustomCommands.Add("flash", AccessTools.Method(this.GetType(), nameof(FlashEventCommand))); 
            // Remove this one in 1.6
            EventPatcher.CustomCommands.Add("temporaryAnimatedSprite", AccessTools.Method(this.GetType(), nameof(AddTemporarySprite16)));

            SpaceEvents.AfterGiftGiven += this.SpaceEvents_AfterGiftGiven;

            Commands.Register();
            TileSheetExtensions.Init();
            ScheduleExpansion.Init();

            var serializerManager = new SerializerManager(helper.ModRegistry);

            this.Harmony = HarmonyPatcher.Apply(this,
                new EnumPatcher(),
                new EventPatcher(),
                new CraftingRecipePatcher(),
                new FarmerPatcher(),
                new ForgeMenuPatcher(),
                new Game1Patcher(),
                new GameLocationPatcher(),
                new GameMenuPatcher(),
                new GameServerPatcher(),
                new LoadGameMenuPatcher(serializerManager),
                new MeleeWeaponPatcher(),
                new MultiplayerPatcher(),
                new NpcPatcher(),
                new SaveGamePatcher(serializerManager),
                new SerializationPatcher(),
                new SpriteBatchPatcher(),
                new UtilityPatcher(),
                new HoeDirtPatcher(),

                // I've started organizing by purpose instead of class patched
                new PortableCarpenterPatcher()
            );
            /*
            var ps = typeof(NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>).GetProperties();
            MethodBase m = null;
            foreach (var p in ps)
            {
                if (p.GetIndexParameters() == null || p.GetIndexParameters().Length == 0)
                    continue;
                if (p.GetSetMethod() == null)
                    continue;
                m = p.GetSetMethod();
                break;
            }
            Harmony.Patch(m,
                prefix: new HarmonyMethod(typeof(Fix1_5NetCodeBugPatch).GetMethod("Prefix", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)),
                postfix: new HarmonyMethod(typeof(Fix1_5NetCodeBugPatch).GetMethod("Postfix", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)));
            */
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return new Api();
        }

        private static void DamageFarmerEventCommand(Event evt, GameLocation loc, GameTime time, string[] args)
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

        private static void GiveHatEventCommand(Event evt, GameLocation loc, GameTime time, string[] args)
        {
            try
            {
                Game1.player.addItemByMenuIfNecessary(new Hat(int.Parse(args[1])));
            }
            finally
            {
                evt.CurrentCommand++;
            }
        }

        private static void SetDatingEventCommand(Event evt, GameLocation loc, GameTime time, string[] args)
        {
            try
            {
                if (!Game1.player.friendshipData.TryGetValue(args[1], out Friendship f))
                {
                    Log.Warn("Could not find NPC " + args[1] + " to mark as dating");
                }
                else
                {
                    f.Status = FriendshipStatus.Dating;
                }
            }
            finally
            {
                evt.CurrentCommand++;
            }
        }
        private static void TotemWarpEventCommand(Event evt, GameLocation loc, GameTime time, string[] args)
        {
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
        private static void SetActorScale(Event evt, GameLocation loc, GameTime time, string[] args)
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
        private static void CycleActorColors(Event evt, GameLocation loc, GameTime time, string[] args)
        {
            try
            {
                string actorName = args[1];
                NPC actor = evt.getActorByName(actorName);
                var extras = spriteExtras.GetOrCreateValue(actor.Sprite);
                if (args[2] == "null")
                {
                    extras.grad = null;
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
                }
                cols.Add(cols[0]);

                int perSection = (int)(60 * interval / (cols.Count - 1));
                Color[] grad = new Color[(perSection * (cols.Count - 1)) + 1];
                for (int i = 0; i < cols.Count - 1; ++i)
                {
                    Color[] gradpart = Util.GetColorGradient(cols[i], cols[1 + 1], perSection).ToArray();
                    Array.Copy(gradpart, 0, grad, perSection * i, gradpart.Length);
                }

                extras.grad = grad;
            }
            finally
            {
                evt.CurrentCommand++;
            }
        }
        private static void FlashEventCommand(Event @event, GameLocation loc, GameTime time, string[] args)
        {
            try
            {
                float duration = float.Parse(args[1]);
                Game1.flashAlpha = 1 + duration * 60;
            }
            finally
            {
                @event.CurrentCommand++;
            }
        }

        [SuppressMessage("Style", "IDE0008", Justification = "copy pasted from vanilla with as few changes as possible")]
        public static void AddTemporarySprite16(Event @event, GameLocation loc, GameTime time, string[] args)
        {
            try
            {
                string ArgUtility_GetMissingRequiredIndexError(string[] array, int index)
                {
                    switch (array.Length)
                    {
                        case 0:
                            {
                                return $"required index {index} not found (list is empty)";
                            }
                        case 1:
                            {
                                return $"required index {index} not found (list has a single value at index 0)";
                            }
                        default:
                            {
                                return $"required index {index} not found (list has indexes 0 through {array.Length - 1})";
                            }
                    }
                }
                bool ArgUtility_TryGet(string[] array, int index, out string value, out string error, bool allowBlank = true)
                {
                    if (array == null)
                    {
                        value = null;
                        error = "argument list is null";
                        return false;
                    }
                    if (index < 0 || index >= array.Length)
                    {
                        value = null;
                        error = ArgUtility_GetMissingRequiredIndexError(array, index);
                        return false;
                    }
                    value = array[index];
                    if (!allowBlank && string.IsNullOrWhiteSpace(value))
                    {
                        value = null;
                        error = $"required index {index} has a blank value";
                        return false;
                    }
                    error = null;
                    return true;
                }
                string ArgUtility_GetValueParseError(string[] array, int index, bool required, string typeSummary)
                {
                    return required ? "required" : "optional" + $" index {index} has value '{array[index]}', which can't be parsed as {typeSummary}";
                }
                bool ArgUtility_TryGetInt(string[] array, int index, out int value, out string error)
                {
                    if (!ArgUtility_TryGet(array, index, out string raw, out error, allowBlank: false))
                    {
                        value = 0;
                        return false;
                    }
                    if (!int.TryParse(raw, out value))
                    {
                        value = 0;
                        error = ArgUtility_GetValueParseError(array, index, required: true, "an integer");
                        return false;
                    }
                    error = null;
                    return true;
                }
                bool ArgUtility_TryGetRectangle(string[] array, int index, out Rectangle value, out string error)
                {
                    if (!ArgUtility_TryGetInt(array, index, out int x, out error) || !ArgUtility_TryGetInt(array, index + 1, out int y, out error) || !ArgUtility_TryGetInt(array, index + 2, out var width, out error) || !ArgUtility_TryGetInt(array, index + 3, out var height, out error))
                    {
                        value = Rectangle.Empty;
                        return false;
                    }
                    error = null;
                    value = new Rectangle(x, y, width, height);
                    return true;
                }
                bool ArgUtility_TryGetFloat(string[] array, int index, out float value, out string error)
                {
                    if (!ArgUtility_TryGet(array, index, out var raw, out error, allowBlank: false))
                    {
                        value = 0f;
                        return false;
                    }
                    if (!float.TryParse(raw, out value))
                    {
                        value = 0f;
                        error = ArgUtility_GetValueParseError(array, index, required: true, "a number");
                        return false;
                    }
                    error = null;
                    return true;
                }
                bool ArgUtility_TryGetVector2(string[] array, int index, out Vector2 value, out string error, bool integerOnly = false)
                {
                    float x;
                    float y;
                    if (integerOnly)
                    {
                        if (ArgUtility_TryGetInt(array, index, out var x2, out error) && ArgUtility_TryGetInt(array, index + 1, out var y2, out error))
                        {
                            value = new Vector2(x2, y2);
                            return true;
                        }
                    }
                    else if (ArgUtility_TryGetFloat(array, index, out x, out error) && ArgUtility_TryGetFloat(array, index + 1, out y, out error))
                    {
                        value = new Vector2(x, y);
                        return true;
                    }
                    value = Vector2.Zero;
                    return false;
                }
                bool ArgUtility_TryGetBool(string[] array, int index, out bool value, out string error)
                {
                    if (!ArgUtility_TryGet(array, index, out var raw, out error, allowBlank: false))
                    {
                        value = false;
                        return false;
                    }
                    if (!bool.TryParse(raw, out value))
                    {
                        value = false;
                        error = ArgUtility_GetValueParseError(array, index, required: true, "a boolean (should be 'true' or 'false')");
                        return false;
                    }
                    error = null;
                    return true;
                }
                if (!ArgUtility_TryGet(args, 1, out var textureName, out var error) || !ArgUtility_TryGetRectangle(args, 2, out var sourceRect, out error) || !ArgUtility_TryGetFloat(args, 6, out var animationInterval, out error) || !ArgUtility_TryGetInt(args, 7, out var animationLength, out error) || !ArgUtility_TryGetInt(args, 8, out var numberOfLoops, out error) || !ArgUtility_TryGetVector2(args, 9, out var tile, out error, integerOnly: true) || !ArgUtility_TryGetBool(args, 11, out var flicker, out error) || !ArgUtility_TryGetBool(args, 12, out var flip, out error) || !ArgUtility_TryGetFloat(args, 13, out var layerDepth, out error) || !ArgUtility_TryGetFloat(args, 14, out var alphaFade, out error) || !ArgUtility_TryGetInt(args, 15, out var scale, out error) || !ArgUtility_TryGetFloat(args, 16, out var scaleChange, out error) || !ArgUtility_TryGetFloat(args, 17, out var rotation, out error) || !ArgUtility_TryGetFloat(args, 18, out var rotationChange, out error))
                {
                    throw new Exception(error);
                    return;
                }
                TemporaryAnimatedSprite tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, animationInterval, animationLength, numberOfLoops, @event.OffsetPosition(tile * 64f), flicker, flip, @event.OffsetPosition(new Vector2(0f, layerDepth) * 64f).Y / 10000f, alphaFade, Color.White, 4 * scale, scaleChange, rotation, rotationChange);
                for (int i = 19; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "hold_last_frame":
                            tempSprite.holdLastFrame = true;
                            break;
                        case "ping_pong":
                            tempSprite.pingPong = true;
                            break;
                        case "motion":
                            {
                                if (!ArgUtility_TryGetVector2(args, i + 1, out var value, out error))
                                {
                                    throw new Exception(error);
                                    break;
                                }
                                tempSprite.motion = value;
                                i += 2;
                                break;
                            }
                        case "acceleration":
                            {
                                if (!ArgUtility_TryGetVector2(args, i + 1, out var value2, out error))
                                {
                                    throw new Exception(error);
                                    break;
                                }
                                tempSprite.acceleration = value2;
                                i += 2;
                                break;
                            }
                        case "acceleration_change":
                            {
                                if (!ArgUtility_TryGetVector2(args, i + 1, out var value3, out error))
                                {
                                    throw new Exception(error);
                                    break;
                                }
                                tempSprite.accelerationChange = value3;
                                i += 2;
                                break;
                            }
                        default:
                            throw new Exception("unknown option '" + args[i] + "'");
                            break;
                    }
                }
                loc.TemporarySprites.Add(tempSprite);
            }
            finally
            {
                @event.CurrentCommand++;
            }
        }

        // TODO: In 1.6 move to vanilla asset expansion part of the code
        // Also make it use ItemId instead
        // Also make it change to use PlayEvent
        private void SpaceEvents_AfterGiftGiven(object sender, EventArgsGiftGiven e)
        {
            var farmer = sender as Farmer;
            if (farmer != Game1.player) return;

            var dict = Game1.content.Load<Dictionary<string, NpcExtensionData>>("spacechase0.SpaceCore/NpcExtensionData");
            if (!dict.TryGetValue(e.Npc.Name, out var npcEntry))
                return;

            if (!npcEntry.GiftEventTriggers.TryGetValue(e.Gift.ParentSheetIndex.ToString(), out string eventStr))
                return;

            string[] data = eventStr.Split('/');

            var events = Game1.player.currentLocation.GetLocationEvents();
            if (events.ContainsKey(eventStr))
            {
                if (Game1.activeClickableMenu is DialogueBox db)
                {
                    db.dialogueFinished = true;
                    db.closeDialogue();
                    Game1.activeClickableMenu = null;
                    Game1.dialogueUp = false;
                }
                else return; // In case someone else is doing something unusual

                int eid = Convert.ToInt32(data[0]);
                Game1.player.eventsSeen.Add(eid);
                Game1.player.currentLocation.startEvent(new Event(events[eventStr], eid));
            }
        }

        public class NpcExtensionData
        {
            public Dictionary<string, string> GiftEventTriggers = new();
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/NpcExtensionData"))
            {
                e.LoadFrom(() => new Dictionary<string, NpcExtensionData>(), AssetLoadPriority.Low);
            }
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

            // update tilesheet references
            foreach (Texture2D oldTexture in TileSheetExtensions.UpdateReferences())
            {
                if (this.Config.DisposeOldTextures)
                    this.TextureDisposalQueue.Enqueue(new(oldTexture, Game1.ticks));
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

            // dispose old textures
            if (e.IsOneSecond)
            {
                while (this.TextureDisposalQueue.Count != 0)
                {
                    const int delayTicks = 60; // sixty ticks per second

                    var next = this.TextureDisposalQueue.Peek();
                    Texture2D asset = next.Key;
                    int queuedTicks = next.Value;

                    if (Game1.ticks - queuedTicks <= delayTicks)
                        break;

                    this.TextureDisposalQueue.Dequeue();
                    if (!asset.IsDisposed)
                        asset.Dispose();
                }
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

            if ( Game1.IsMasterGame )
            {
                DoLoadCustomLocationWeather();
            }
        }

        private void OnSaving( object sender, SavingEventArgs e )
        {
            // This had to be moved to a harmony patch to fix an issue from saving in a custom location context location
            /*
            if ( Game1.IsMasterGame )
            {
                var lws = SaveGame.GetSerializer( typeof( LocationWeather ) );
                Dictionary<int, string> customLocWeathers = new();
                foreach ( int context in Game1.netWorldState.Value.LocationWeather.Keys )
                {
                    if ( !Enum.IsDefined( ( GameLocation.LocationContext ) context ) )
                    {
                        SpaceShared.Log.Debug( "doing ctx " + context );
                        using MemoryStream ms = new();
                        lws.Serialize( ms, Game1.netWorldState.Value.LocationWeather[ context ] );
                        customLocWeathers.Add( context, Encoding.ASCII.GetString( ms.ToArray() ) );
                    }
                }
                foreach ( int key in customLocWeathers.Keys )
                    Game1.netWorldState.Value.LocationWeather.Remove( key );
                Helper.Data.WriteSaveData( "CustomLocationWeathers", customLocWeathers );
            }
            */
        }

        private void OnSaved( object sender, SavedEventArgs e )
        {
            if ( Game1.IsMasterGame )
            {
                DoLoadCustomLocationWeather();
            }
        }

        /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is StardewValley.Menus.ForgeMenu)
                Game1.activeClickableMenu = new NewForgeMenu();
        }

        private void SpaceEvents_ActionActivated(object sender, EventArgsAction e)
        {
            if (e.Action == "CarpenterMenu")
            {
                bool magic = e.ActionString.Split(' ')[1] == "true";
                Game1.activeClickableMenu = new StardewValley.Menus.CarpenterMenu(magic);
            }
        }

        private void DoLoadCustomLocationWeather()
        {
            var lws = SaveGame.GetSerializer( typeof( LocationWeather ) );
            var customLocWeathers = Helper.Data.ReadSaveData< Dictionary<int, string> >( "CustomLocationWeathers" );
            if ( customLocWeathers == null )
                return;
            foreach ( var kvp in customLocWeathers )
            {
                using MemoryStream ms = new( Encoding.Unicode.GetBytes( kvp.Value ) );
                LocationWeather lw = ( LocationWeather )lws.Deserialize( ms );
                if ( Game1.netWorldState.Value.LocationWeather.ContainsKey( kvp.Key ) )
                    Game1.netWorldState.Value.LocationWeather.Remove( kvp.Key );
                Game1.netWorldState.Value.LocationWeather.Add( kvp.Key, lw );
            }
        }
    }
}
