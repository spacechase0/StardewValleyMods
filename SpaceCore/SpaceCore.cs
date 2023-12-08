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
using static SpaceCore.SpaceCore;

namespace SpaceCore
{
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

    [HarmonyPatch(typeof(NPC), "draw", new Type[] { typeof(SpriteBatch), typeof(float) })]
    public static class AnimatedSpriteDrawExtrasPatch3
    {
        public static void getExtraValues(NPC who, ref Vector2 both, ref float x, ref float y, ref Color grad)
        {
            var extras = SpaceCore.spriteExtras.GetOrCreateValue(who.Sprite);
            both = extras.scale;
            x = extras.scale.X;
            y = extras.scale.Y;
            grad = (extras.grad != null ? extras.grad[extras.currGradInd] : Color.White);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            LocalBuilder scale = generator.DeclareLocal(typeof(Vector2));
            LocalBuilder scaleX = generator.DeclareLocal(typeof(float));
            LocalBuilder scaleY = generator.DeclareLocal(typeof(float));
            LocalBuilder gradColor = generator.DeclareLocal(typeof(Color));

            var funcIL = new List<CodeInstruction>(instructions);

            // add the call to GetOrCreateValue to the start of the function
            // and save the values to locals
            var preload = new List<CodeInstruction>(){
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloca, scale),
                new(OpCodes.Ldloca, scaleX),
                new(OpCodes.Ldloca, scaleY),
                new(OpCodes.Ldloca, gradColor),
                new(OpCodes.Call, typeof(AnimatedSpriteDrawExtrasPatch3).GetMethod("getExtraValues", BindingFlags.Public | BindingFlags.Static)),
            };
            funcIL.InsertRange(0, preload);

            int ptr;
            int num = 0;
            bool complete = false;

            // from the start, the first three *4 operations all need to mul
            // by the scale values. Y, X, Y, in that order.
            for (ptr = 0; ptr < funcIL.Count - 1; ++ptr) {
                if (funcIL[ptr].opcode == OpCodes.Ldc_I4_4 && funcIL[ptr+1].opcode == OpCodes.Mul) {
                    Log.Trace($"NPC.draw: inserting mul at {ptr}");
                    funcIL.InsertRange(ptr+2, new List<CodeInstruction>(){
                        new(OpCodes.Ldloc, (num == 2 ? scaleX : scaleY)),
                        new(OpCodes.Mul),
                    });
                    if (++num == 3) {
                        complete = true;
                        break;
                    }
                    ptr += 4;
                }
            }
            if (!complete) {
                Log.Error($"Aborting NPC.draw transpiler: expected 3 multiply instructions but found {num}");
                return instructions;
            }

            num = 0;
            complete = false;
            // replace uses of Color.White with gradColor. there are three, but
            // only the first and third ones are edited
            for (; ptr < funcIL.Count; ++ptr) {
                if (funcIL[ptr].opcode == OpCodes.Call && funcIL[ptr].operand.Equals(typeof(Microsoft.Xna.Framework.Color).GetMethod("get_White", BindingFlags.Public | BindingFlags.Static))) {
                    if (++num == 2) {
                        continue;
                    }
                    Log.Trace($"NPC.draw: replacing White at {ptr}");
                    funcIL[ptr] = new CodeInstruction(OpCodes.Ldloc, gradColor);
                    if (num == 3) {
                        complete = true;
                        break;
                    }
                }
            }
            if (!complete) {
                Log.Error($"Aborting NPC.draw transpiler: expected 3 uses of Color.White but found {num}");
                return instructions;
            }

            complete = false;
            // inject one multiply by the vector2 scale. this also requires
            // calling a different overload of SpriteBatch.Draw
            for (; ptr < funcIL.Count - 1; ++ptr) {
                if (funcIL[ptr].opcode == OpCodes.Ldc_R4 && funcIL[ptr].operand.Equals(4f) && funcIL[ptr+1].opcode == OpCodes.Mul) {
                    Log.Trace($"NPC.draw: vector mul at {ptr}");
                    funcIL.InsertRange(ptr+2, new List<CodeInstruction>(){
                        new(OpCodes.Ldloc, scale),
                        new(OpCodes.Call, typeof(Microsoft.Xna.Framework.Vector2).GetMethod("op_Multiply", BindingFlags.Public | BindingFlags.Static, null, new Type[]{typeof(float), typeof(Microsoft.Xna.Framework.Vector2)}, null)),
                    });
                    ptr += 4;
                    complete = true;
                    break;
                }
            }
            if (!complete) {
                Log.Error($"Aborting NPC.draw transpiler: failed to find expected scale multiply");
                return instructions;
            }
            complete = false;
            // (change the call)
            for (; ptr < funcIL.Count; ++ptr) {
                if (funcIL[ptr].opcode == OpCodes.Callvirt && ((MethodInfo)funcIL[ptr].operand).Name.Equals("Draw")) {
                    Log.Trace($"NPC.draw: replacing Draw at {ptr}");
                    funcIL[ptr] = new CodeInstruction(OpCodes.Callvirt, typeof(Microsoft.Xna.Framework.Graphics.SpriteBatch).GetMethod("Draw", BindingFlags.Public | BindingFlags.Instance, null, new Type[]{typeof(Microsoft.Xna.Framework.Graphics.Texture2D), typeof(Microsoft.Xna.Framework.Vector2), typeof(Microsoft.Xna.Framework.Rectangle), typeof(Microsoft.Xna.Framework.Color), typeof(float), typeof(Microsoft.Xna.Framework.Vector2), typeof(Microsoft.Xna.Framework.Vector2), typeof(Microsoft.Xna.Framework.Graphics.SpriteEffects), typeof(float)}, null));
                    complete = true;
                    break;
                }
            }
            if (!complete) {
                Log.Error($"Aborting NPC.draw transpiler: failed to find expected call to SpriteBatch.Draw");
                return instructions;
            }

            return funcIL;
        }

    }

    [HarmonyPatch(typeof(NPC), "DrawBreathing", new Type[] { typeof(SpriteBatch), typeof(float) })]
    public static class AnimatedSpriteDrawExtrasPatch4
    {
        public static void getExtraValues(NPC who, ref Vector2 scale, ref Color grad)
        {
            var extras = SpaceCore.spriteExtras.GetOrCreateValue(who.Sprite);
            scale = extras.scale;
            grad = (extras.grad != null ? extras.grad[extras.currGradInd] : Color.White);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            LocalBuilder scale = generator.DeclareLocal(typeof(Vector2));
            LocalBuilder gradColor = generator.DeclareLocal(typeof(Color));

            var funcIL = new List<CodeInstruction>(instructions);

            // add the call to GetOrCreateValue to the start of the function
            // and save the values to locals
            var preload = new List<CodeInstruction>(){
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloca, scale),
                new(OpCodes.Ldloca, gradColor),
                new(OpCodes.Call, typeof(AnimatedSpriteDrawExtrasPatch4).GetMethod("getExtraValues", BindingFlags.Public | BindingFlags.Static)),
            };
            funcIL.InsertRange(0, preload);

            int ptr;
            bool complete = false;
            // find the one use of Color.White and replace with gradColor
            for (ptr = 4; ptr < funcIL.Count; ++ptr) {
                if (funcIL[ptr].opcode == OpCodes.Call && funcIL[ptr].operand.Equals(typeof(Microsoft.Xna.Framework.Color).GetMethod("get_White", BindingFlags.Public | BindingFlags.Static))) {
                    Log.Trace($"NPC.DrawBreathing: replacing White at {ptr}");
                    funcIL[ptr] = new CodeInstruction(OpCodes.Ldloc, gradColor);
                    complete = true;
                    break;
                }
            }
            if (!complete) {
                Log.Error($"Aborting NPC.DrawBreathing transpiler: failed to find expected use of Color.White");
                return instructions;
            }

            complete = false;
            // inject scale multiply. also have to add a vector instead of just
            // the single float
            for (; ptr < funcIL.Count - 2; ++ptr) {
                if (funcIL[ptr].opcode == OpCodes.Ldc_R4 && funcIL[ptr].operand.Equals(4f) && funcIL[ptr+1].opcode == OpCodes.Mul && funcIL[ptr+2].opcode == OpCodes.Ldloc_2) {
                    Log.Trace($"NPC.DrawBreathing: vector mul/add at {ptr}");
                    funcIL.RemoveRange(ptr+2, 2);
                    funcIL.InsertRange(ptr+2, new List<CodeInstruction>(){
                        new(OpCodes.Ldloc, scale),
                        new(OpCodes.Call, typeof(Microsoft.Xna.Framework.Vector2).GetMethod("op_Multiply", BindingFlags.Public | BindingFlags.Static, null, new Type[]{typeof(float), typeof(Microsoft.Xna.Framework.Vector2)}, null)),
                        new(OpCodes.Ldloc_2),
                        new(OpCodes.Ldloc_2),
                        new(OpCodes.Newobj, typeof(Microsoft.Xna.Framework.Vector2).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.HasThis, new Type[]{typeof(float), typeof(float)}, null)),
                        new(OpCodes.Call, typeof(Microsoft.Xna.Framework.Vector2).GetMethod("op_Addition", BindingFlags.Public | BindingFlags.Static, null, new Type[]{typeof(Microsoft.Xna.Framework.Vector2), typeof(Microsoft.Xna.Framework.Vector2)}, null)),
                    });
                    ptr += 8;
                    complete = true;
                    break;
                }
            }
            if (!complete) {
                Log.Error($"Aborting NPC.DrawBreathing transpiler: failed to find expected scale multiply");
                return instructions;
            }

            complete = false;
            // change the call
            for (; ptr < funcIL.Count; ++ptr) {
                if (funcIL[ptr].opcode == OpCodes.Callvirt && ((MethodInfo)funcIL[ptr].operand).Name.Equals("Draw")) {
                    Log.Trace($"NPC.DrawBreathing: replacing Draw at {ptr}");
                    funcIL[ptr] = new CodeInstruction(OpCodes.Callvirt, typeof(Microsoft.Xna.Framework.Graphics.SpriteBatch).GetMethod("Draw", BindingFlags.Public | BindingFlags.Instance, null, new Type[]{typeof(Microsoft.Xna.Framework.Graphics.Texture2D), typeof(Microsoft.Xna.Framework.Vector2), typeof(Microsoft.Xna.Framework.Rectangle), typeof(Microsoft.Xna.Framework.Color), typeof(float), typeof(Microsoft.Xna.Framework.Vector2), typeof(Microsoft.Xna.Framework.Vector2), typeof(Microsoft.Xna.Framework.Graphics.SpriteEffects), typeof(float)}, null));
                    complete = true;
                    break;
                }
            }
            if (!complete) {
                Log.Error($"Aborting NPC.DrawBreathing transpiler: failed to find expected call to SpriteBatch.Draw");
                return instructions;
            }

            return funcIL;
        }
    }

    [HarmonyPatch(typeof(NPC), nameof(NPC.isMarried))]
    public static class NpcIsMarriedNotReallyInSomeCasesPatch
    {
        public static void Postfix(NPC __instance, ref bool __result)
        {
            var dict = Game1.content.Load<Dictionary<string, NpcExtensionData>>("spacechase0.SpaceCore/NpcExtensionData");
            if (!dict.TryGetValue(__instance.Name, out var npcEntry))
                return;

            if (!npcEntry.IgnoreMarriageSchedule)
                return;

            MethodBase[] meths = new[]
            {
                typeof(NPC).GetMethod(nameof(NPC.reloadData)),
                typeof(NPC).GetMethod(nameof(NPC.reloadSprite)),
                typeof(NPC).GetMethod(nameof(NPC.getHome)),
                typeof(NPC).GetMethod("prepareToDisembarkOnNewSchedulePath"),
                typeof(NPC).GetMethod(nameof(NPC.parseMasterSchedule)),
                typeof(NPC).GetMethod(nameof(NPC.TryLoadSchedule), new Type[ 0 ]),
                typeof(NPC).GetMethod(nameof(NPC.resetForNewDay)),
                typeof(NPC).GetMethod(nameof(NPC.dayUpdate)),
            };

            var st = new System.Diagnostics.StackTrace();
            for (int i = 0; i < st.FrameCount; ++i) // Originally had 7 instead of FrameCount, but some mods interfere so we need to check further
            {
                var meth = st.GetFrame(i).GetMethod();
                foreach (var checkMeth in meths)
                {
                    // When someone patches a method the method name changes due to SMAPI's custom fork of Harmony, and so the methodinfo doesn't match.
                    // This is a workaround
                    // Excuse the liberal use of ? - I was tired and frustrated
                    if ((meth?.DeclaringType == checkMeth?.DeclaringType || ( meth?.Name?.Contains( checkMeth?.DeclaringType?.FullName ?? "qwerqwer" ) ?? false ) ) && ( meth?.Name?.Contains( checkMeth?.Name ?? "asdfasdf" ) ?? false ) )
                    {
                        __result = false;
                        return;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(NPC), "loadCurrentDialogue")]
    public static class NpcLoadCurrentDialogueFakeNotMarriedPatch
    {
        public static void Prefix(NPC __instance, ref string __state)
        {
            var dict = Game1.content.Load<Dictionary<string, NpcExtensionData>>("spacechase0.SpaceCore/NpcExtensionData");
            if (!dict.TryGetValue(__instance.Name, out var npcEntry))
                return;

            if (!npcEntry.IgnoreMarriageSchedule)
                return;

            __state = null;
            if (Game1.player.spouse == __instance.Name)
            {
                __state = Game1.player.spouse;
                Game1.player.spouse = "";
            }
        }
        public static void Postfix(NPC __instance, ref string __state)
        {
            if (__state != null)
            {
                Game1.player.spouse = __state;
            }
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
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;

            Event.RegisterCustomCommand("damageFarmer", DamageFarmerEventCommand);
            Event.RegisterCustomCommand("giveHat", GiveHatEventCommand);
            Event.RegisterCustomCommand("setDating", SetDatingEventCommand);
            Event.RegisterCustomCommand("totemWarpEffect", TotemWarpEventCommand);
            Event.RegisterCustomCommand("setActorScale", SetActorScale);
            Event.RegisterCustomCommand("cycleActorColors", CycleActorColors);
            Event.RegisterCustomCommand("flash", FlashEventCommand); 

            Commands.Register();
            VanillaAssetExpansion.VanillaAssetExpansion.Init();

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
                new HoeDirtPatcher()
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
                    f.Status = FriendshipStatus.Dating;
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

        /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is StardewValley.Menus.ForgeMenu)
                Game1.activeClickableMenu = new NewForgeMenu();
        }
    }
}
