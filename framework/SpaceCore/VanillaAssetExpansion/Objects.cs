using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.Triggers;

namespace SpaceCore.VanillaAssetExpansion
{
    public class ObjectExtensionData
    {
        public string CategoryTextOverride { get; set; } = null;
        public Color CategoryColorOverride { get; set; } = new Color( 0, 0, 0, 0);

        public bool CanBeTrashed { get; set; } = true;
        public bool CanBeShipped { get; set; } = true;

        public int? EatenHealthRestoredOverride { get; set; } = null;
        public int? EatenStaminaRestoredOverride { get; set; } = null;

        public int? MaxStackSizeOverride { get; set; } = null;

        public class TotemWarpData
        {
            public string Location { get; set; }
            public Vector2 Position { get; set; }
            public Color Color { get; set; }
            public bool ConsumedOnUse { get; set; } = true;
        }
        public TotemWarpData TotemWarp { get; set; }

        public bool UseForTriggerAction { get; set; } = false;
        public bool ConsumeForTriggerAction { get; set; } = false;

        public string GiftedToNotOnAllowListMessage { get; set; }
        public Dictionary<string, bool> GiftableToNpcAllowList { get; set; }
        public Dictionary<string, string> GiftableToNpcDisallowList { get; set; }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.getCategoryName))]
    public static class ObjectCategoryNamePatch
    {
        public static void Postfix(StardewValley.Object __instance, ref string __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (!__instance.bigCraftable.Value && dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].CategoryTextOverride != null)
            {
                __result = dict[__instance.ItemId].CategoryTextOverride;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.getCategoryColor))]
    public static class ObjectCategoryColorPatch
    {
        public static void Postfix(StardewValley.Object __instance, ref Color __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (!__instance.bigCraftable.Value && dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].CategoryColorOverride.A != 0)
            {
                __result = dict[__instance.ItemId].CategoryColorOverride;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.countsForShippedCollection))]
    public static class ObjectHiddenInShippingCollectionPatch
    {
        public static void Postfix(StardewValley.Object __instance, ref bool __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && !dict[__instance.ItemId].CanBeShipped )
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.canBeTrashed))]
    public static class ObjectTrashablePatch
    {
        public static void Postfix(StardewValley.Object __instance, ref bool __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && !dict[__instance.ItemId].CanBeTrashed)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Item), nameof(StardewValley.Item.canBeDropped))]
    public static class ObjectDroppablePatch
    {
        public static void Postfix(StardewValley.Item __instance, ref bool __result)
        {
            if (__instance is StardewValley.Object obj)
            ObjectTrashablePatch.Postfix(obj, ref __result);
        }
    }


    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.canBeShipped))]
    public static class ObjectShippablePatch
    {
        public static void Postfix(StardewValley.Object __instance, ref bool __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && !dict[__instance.ItemId].CanBeShipped)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.healthRecoveredOnConsumption))]
    public static class ObjectHealthRecoveredPatch
    {
        public static void Postfix(StardewValley.Object __instance, ref int __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].EatenHealthRestoredOverride.HasValue)
            {
                __result = dict[__instance.ItemId].EatenHealthRestoredOverride.Value;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.staminaRecoveredOnConsumption))]
    public static class ObjectStaminaRecoveredPatch
    {
        public static void Postfix(StardewValley.Object __instance, ref int __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].EatenStaminaRestoredOverride.HasValue)
            {
                __result = dict[__instance.ItemId].EatenStaminaRestoredOverride.Value;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.maximumStackSize))]
    public static class ObjectMaxStackPatch
    {
        public static void Postfix(StardewValley.Object __instance, ref int __result )
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].MaxStackSizeOverride.HasValue)
            {
                __result = dict[__instance.ItemId].MaxStackSizeOverride.Value;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.performUseAction))]
    public static class ObjectUsePatch
    {
        public static bool Prefix(StardewValley.Object __instance, GameLocation location, ref bool __result)
        {
            bool didStuff = false;
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].UseForTriggerAction)
            {
                if (!Game1.player.canMove || __instance.isTemporarilyInvisible)
                {
                    __result = false;
                    return false;
                }
                TriggerActionManager.Raise("spacechase0.SpaceCore_OnItemUsed", location: Game1.player.currentLocation, player: Game1.player, inputItem: __instance);

                __result = dict[__instance.ItemId].ConsumeForTriggerAction;
                didStuff = true;
            }
            if (dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].TotemWarp != null)
            {
                if (!Game1.player.canMove || __instance.isTemporarilyInvisible)
                {
                    __result = false;
                    return false;
                }
                bool normal_gameplay = !Game1.eventUp && !Game1.isFestival() && !Game1.fadeToBlack && !Game1.player.swimming.Value && !Game1.player.bathingClothes.Value && !Game1.player.onBridge.Value;
                if (normal_gameplay)
                {
                    Game1.player.jitterStrength = 1f;
                    Color sprinkleColor = dict[__instance.ItemId].TotemWarp.Color;
                    location.playSound("warrior");
                    Game1.player.faceDirection(2);
                    Game1.player.CanMove = false;
                    Game1.player.temporarilyInvincible = true;
                    Game1.player.temporaryInvincibilityTimer = -4000;
                    Game1.changeMusicTrack("silence");
                    Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
                    {
                        new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
                        new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, (Farmer who) => { DoTotemWarp( __instance, who ); }, behaviorAtEndOfFrame: true)
                    });
                    TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
                    {
                        motion = new Vector2(0f, -1f),
                        scaleChange = 0.01f,
                        alpha = 1f,
                        alphaFade = 0.0075f,
                        shakeIntensity = 1f,
                        initialPosition = Game1.player.Position + new Vector2(0f, -96f),
                        xPeriodic = true,
                        xPeriodicLoopTime = 1000f,
                        xPeriodicRange = 4f,
                        layerDepth = 1f
                    };
                    sprite.CopyAppearanceFromItemId(__instance.QualifiedItemId);
                    Game1.Multiplayer.broadcastSprites(location, sprite);
                    sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(-64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
                    {
                        motion = new Vector2(0f, -0.5f),
                        scaleChange = 0.005f,
                        scale = 0.5f,
                        alpha = 1f,
                        alphaFade = 0.0075f,
                        shakeIntensity = 1f,
                        delayBeforeAnimationStart = 10,
                        initialPosition = Game1.player.Position + new Vector2(-64f, -96f),
                        xPeriodic = true,
                        xPeriodicLoopTime = 1000f,
                        xPeriodicRange = 4f,
                        layerDepth = 0.9999f
                    };
                    sprite.CopyAppearanceFromItemId(__instance.QualifiedItemId);
                    Game1.Multiplayer.broadcastSprites(location, sprite);
                    sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
                    {
                        motion = new Vector2(0f, -0.5f),
                        scaleChange = 0.005f,
                        scale = 0.5f,
                        alpha = 1f,
                        alphaFade = 0.0075f,
                        delayBeforeAnimationStart = 20,
                        shakeIntensity = 1f,
                        initialPosition = Game1.player.Position + new Vector2(64f, -96f),
                        xPeriodic = true,
                        xPeriodicLoopTime = 1000f,
                        xPeriodicRange = 4f,
                        layerDepth = 0.9988f
                    };
                    sprite.CopyAppearanceFromItemId(__instance.QualifiedItemId);
                    Game1.Multiplayer.broadcastSprites(location, sprite);
                    Game1.screenGlowOnce(sprinkleColor, hold: false);
                    Utility.addSprinklesToLocation(location, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, 16, 16, 1300, 20, Color.White, null, motionTowardCenter: true);
                    __result = dict[__instance.ItemId].TotemWarp.ConsumedOnUse;
                    didStuff = true;
                }
            }

            if (didStuff)
            {
                if (__instance.Category == -102 || __instance.Category == -103)
                {
                    SpaceCore.Instance.Helper.Reflection.GetMethod(__instance, "readBook").Invoke(location);
                }
            }

            return !didStuff;
        }

        private static void DoTotemWarp(StardewValley.Object __instance, Farmer who)
        {
            GameLocation location = who.currentLocation;
            for (int i = 0; i < 12; i++)
            {
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, Game1.random.NextBool()));
            }
            who.playNearbySoundAll("wand");
            Game1.displayFarmer = false;
            Game1.player.temporarilyInvincible = true;
            Game1.player.temporaryInvincibilityTimer = -2000;
            Game1.player.freezePause = 1000;
            Game1.flashAlpha = 1f;
            DelayedAction.fadeAfterDelay(() => totemWarpForReal( __instance ), 1000);
            Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
            new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64).Inflate(192, 192);
            int j = 0;
            Point playerTile = who.TilePoint;
            for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
            {
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(6, new Vector2(x, playerTile.Y) * 64f, Color.White, 8, flipped: false, 50f)
                {
                    layerDepth = 1f,
                    delayBeforeAnimationStart = j * 25,
                    motion = new Vector2(-0.25f, 0f)
                });
                j++;
            }
        }
        private static void totemWarpForReal( StardewValley.Object __instance)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].TotemWarp != null)
            {
                var warp = dict[__instance.ItemId].TotemWarp;
                Game1.warpFarmer(warp.Location, (int) warp.Position.X, (int)warp.Position.Y, flip: false);
            }
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
        }
    }
}
