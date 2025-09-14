using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using SpaceShared;
using StardewValley;
using StardewValley.GameData.Characters;
using static SpaceCore.SpaceCore;

namespace SpaceCore.VanillaAssetExpansion
{
    public class NpcExtensionData
    {
        public Dictionary<string, string> GiftEventTriggers = new();
        public bool IgnoreMarriageSchedule { get; set; } = false;
        public bool SeparateDatability { get; set; } = false;
    }

    public class ScheduleAnimationExtensionData
    {
        public Vector2 SourceRectSize { get; set; } = new Vector2(16, 32);
        public Vector2 DrawOffset { get; set; } = new Vector2(0, 0);

        public string AppearanceOverride { get; set; } = null;

        public class FrameExtData
        {
            public int? Emote { get; set; }
            public string PlaySound { get; set; }
        }

        public FrameExtData OnStart { get; set; } = null;
        public FrameExtData OnEnd { get; set; } = null;
        public Dictionary<int, FrameExtData> OnFrame { get; set; } = new();
    }

    [HarmonyPatch(typeof(NPC), "initNetFields")]
    public static class NpcNetFieldsPatch
    {
        public static void Postfix(NPC __instance)
        {
            /*
            if (Game1.IsMasterGame)
                return;
            */
            __instance.datable.fieldChangeEvent += (field, oldVal, newVal) =>
            {
                var dict = Game1.content.Load<Dictionary<string, NpcExtensionData>>("spacechase0.SpaceCore/NpcExtensionData");
                if (__instance.Name == null || __instance.GetData() == null || !dict.TryGetValue(__instance.Name, out var data) || !data.SeparateDatability)
                    return;

                bool newNewVal = __instance.GetData().CanBeRomanced;
                //SpaceCore.Instance.Monitor.Log("meow1, setting to: " + newNewVal, StardewModdingAPI.LogLevel.Alert);
                if (newNewVal == newVal)
                    return;

                SpaceCore.Instance.Helper.Reflection.GetField<bool>(field, "value").SetValue(newNewVal);
                SpaceCore.Instance.Helper.Reflection.GetField<bool>(field, "previousValue").SetValue(newNewVal);
                SpaceCore.Instance.Helper.Reflection.GetField<bool>(field, "targetValue").SetValue(newNewVal);
            };
        }
    }

    [HarmonyPatch(typeof(NPC), nameof(NPC.reloadSprite))]
    public static class NpcReloadDatabilityPatch
    {
        public static void Postfix(NPC __instance)
        {
            /*
            if (Game1.IsMasterGame)
                return;
            */

            var dict = Game1.content.Load<Dictionary<string, NpcExtensionData>>("spacechase0.SpaceCore/NpcExtensionData");
            if (__instance.Name == null || __instance.GetData() == null || !dict.TryGetValue(__instance.Name, out var data) || !data.SeparateDatability)
                return;

            bool newNewVal = __instance.GetData().CanBeRomanced;
            //SpaceCore.Instance.Monitor.Log("meow2, setting to: " + newNewVal, StardewModdingAPI.LogLevel.Alert);
            if (newNewVal == __instance.datable.Value)
                return;

            var field = __instance.datable;

            SpaceCore.Instance.Helper.Reflection.GetField<bool>(field, "value").SetValue(newNewVal);
            SpaceCore.Instance.Helper.Reflection.GetField<bool>(field, "previousValue").SetValue(newNewVal);
            SpaceCore.Instance.Helper.Reflection.GetField<bool>(field, "targetValue").SetValue(newNewVal);
        }
    }


    [HarmonyPatch(typeof(NPC), "startRouteBehavior")]
    public static class NpcAnimationStartPatch
    {
        public static bool Prefix(NPC __instance, string behaviorName)
        {
            var dict = Game1.content.Load<Dictionary<string, ScheduleAnimationExtensionData>>("spacechase0.SpaceCore/ScheduleAnimationExtensionData");
            if (!dict.TryGetValue(behaviorName, out var data))
            {
                return true;
            }

            if (data.AppearanceOverride != null)
            {
                var appearance = __instance.GetData().Appearance.FirstOrDefault(app => app.Id == data.AppearanceOverride);
                if (appearance == null)
                {
                    Log.Error($"Appearance \"{data.AppearanceOverride}\" does not exist for NPC \"{__instance.Name}\" (used in schedule animation extension data \"{behaviorName}\"");
                }
                else
                {
                    __instance.portraitOverridden = __instance.spriteOverridden = true;
                    if (!__instance.TryLoadPortraits(appearance.Portrait, out string err))
                    {
                        Log.Error($"Appearance \"{data.AppearanceOverride}\" has invalid portrait \"{appearance.Portrait}\" for NPC \"{__instance.Name}\" (used in schedule animation extension data \"{behaviorName}\": {err}");
                        __instance.portraitOverridden = false;
                    }
                    if (!__instance.TryLoadSprites(appearance.Sprite, out err))
                    {
                        Log.Error($"Appearance \"{data.AppearanceOverride}\" has invalid sprite \"{appearance.Sprite}\" for NPC \"{__instance.Name}\" (used in schedule animation extension data \"{behaviorName}\": {err}");
                        __instance.spriteOverridden = false;
                    }
                }
            }

            var srs = data.SourceRectSize.ToPoint();
            if (srs != new Point(16, 32))
            {
                __instance.extendSourceRect(srs.X - 16, srs.Y - 32);
                __instance.Sprite.SpriteWidth = srs.X;
                __instance.Sprite.tempSpriteHeight = srs.Y;
            }

            __instance.Sprite.currentFrame = __instance.Sprite.CurrentAnimation[0].frame;
            __instance.drawOffset = data.DrawOffset;
            __instance.Sprite.ignoreSourceRectUpdates = false;

            if (data.OnStart != null)
            {
                if (data.OnStart.Emote.HasValue)
                    __instance.doEmote(data.OnStart.Emote.Value);
                if (data.OnStart.PlaySound != null /*&& Utility.isOnScreen(Utility.Vector2ToPoint(__instance.Position), 64, __instance.currentLocation)*/)
                    __instance.currentLocation.playSound(data.OnStart.PlaySound, __instance.Tile);
            }

            if (data.OnFrame.Count > 0)
            {
                foreach (var entry in data.OnFrame)
                {
                    var frameData = entry.Value;
                    var existing = __instance.Sprite.CurrentAnimation[entry.Key];
                    __instance.Sprite.CurrentAnimation[entry.Key] = new(existing.frame, existing.milliseconds, 0, secondaryArm: false, flip: false, (_) =>
                    {
                        if (frameData.PlaySound != null)
                            __instance.currentLocation.playSound(frameData.PlaySound, __instance.Tile);
                        if (frameData.Emote.HasValue)
                            __instance.doEmote(frameData.Emote.Value);
                    });
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(NPC), "finishRouteBehavior")]
    public static class NpcAnimationEndPatch
    {
        public static bool Prefix(NPC __instance, string behaviorName)
        {
            var dict = Game1.content.Load<Dictionary<string, ScheduleAnimationExtensionData>>("spacechase0.SpaceCore/ScheduleAnimationExtensionData");
            if (!dict.TryGetValue(behaviorName, out var data))
            {
                return true;
            }

            if (data.AppearanceOverride != null)
            {
                __instance.portraitOverridden = __instance.spriteOverridden = false;
            }

            if (data.OnEnd != null)
            {
                if (data.OnEnd.Emote.HasValue)
                    __instance.doEmote(data.OnEnd.Emote.Value);
                if (data.OnEnd.PlaySound != null /*&& Utility.isOnScreen(Utility.Vector2ToPoint(__instance.Position), 64, __instance.currentLocation)*/)
                    __instance.currentLocation.playSound(data.OnEnd.PlaySound, __instance.Tile);
            }

            __instance.reloadSprite();
            CharacterData cdata = __instance.GetData();
            __instance.Sprite.SpriteWidth = cdata?.Size.X ?? 16;
            __instance.Sprite.SpriteHeight = cdata?.Size.Y ?? 32;
            __instance.Sprite.UpdateSourceRect();
            __instance.drawOffset = Vector2.Zero;
            __instance.Halt();
            __instance.movementPause = 1;

            return false;
        }
    }
}
