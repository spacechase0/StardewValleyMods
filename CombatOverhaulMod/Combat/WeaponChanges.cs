using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace CombatOverhaulMod.Combat
{
    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.damageMonster), new Type[] { typeof( Rectangle ), typeof( int ), typeof( int ), typeof( bool ), typeof( float ), typeof( int ), typeof( float ), typeof( float ), typeof( bool ), typeof( Farmer ) } )]
    public static class CritConsecutiveMonsterHitsAndCombatTimerPatch
    {
        public static void ModifyCrit(Monster monster, ref bool crit)
        {
            CombatEngine.ResetCombatTimer();

            if (monster != CombatEngine.lastHitMonster)
            {
                CombatEngine.lastHitMonster = monster;
                CombatEngine.hitCount = 1;
                return;
            }

            if (Game1.random.NextDouble() < (++CombatEngine.hitCount - 1) * 0.05)
            {
                CombatEngine.hitCount = 0;

                crit = true;
                Game1.currentLocation.playSound("crit");
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
        {
            List<CodeInstruction> ret = new();

            bool foundFirstCrit = false;
            foreach (var insn in insns)
            {
                ret.Add(insn);

                if (!foundFirstCrit && insn.opcode == OpCodes.Stloc_S && ((LocalBuilder)insn.operand).LocalIndex == 7)
                {
                    foundFirstCrit = true;
                    ret.Add(new CodeInstruction(OpCodes.Ldloc_S, 2)); // monster
                    ret.Add(new CodeInstruction(OpCodes.Ldloca_S, 7)); // crit
                    ret.Add(CodeInstruction.Call(typeof(CritConsecutiveMonsterHitsAndCombatTimerPatch), nameof(ModifyCrit)));
                }
            }

            return ret;
        }
    }

    [HarmonyPatch(typeof(Farmer), nameof(Farmer.takeDamage))]
    public static class CombatTimerPatch2
    {
        public static void Prefix(Farmer __instance, int damage, Monster damager)
        {
            if (damager != null && __instance.CanBeDamaged())
            {
                CombatEngine.ResetCombatTimer();
            }
        }
    }

    [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.setFarmerAnimating))]
    public static class WeaponFunctionalityPatch1
    {
        public static bool Prefix(
            MeleeWeapon __instance, Farmer who,
            /*ref bool ___hasBegunWeaponEndPause,*/ ref float ___swipeSpeed, ref bool ___anotherClick)
        {
            Impl(__instance, who/*, ref ___hasBegunWeaponEndPause*/, ref ___swipeSpeed, ref ___anotherClick);
            return false;
        }

        private static void Impl(MeleeWeapon weapon, Farmer who/*, ref bool hasBegunWeaponEndPause*/, ref float swipeSpeed, ref bool anotherClick)
        {
            var weaponType = WeaponTypeManager.GetWeaponType(weapon.type.Value);

            if (!weaponType.StopsPlayer)
                who.forceCanMove();

            who.FarmerSprite.PauseForSingleAnimation = false;
            who.FarmerSprite.StopAnimation();
            //hasBegunWeaponEndPause = false;
            swipeSpeed = weaponType.BaseSwipeSpeed - weapon.speed.Value * weaponType.SwipeSpeedModifier - who.addedSpeed * weaponType.SwipeSpeedModifier;
            swipeSpeed *= 1f - who.buffs.WeaponSpeedMultiplier;
            if (swipeSpeed < 1) // Fail-safe
                swipeSpeed = 1;

            if (who.IsLocalPlayer)
            {
                foreach (var ench in weapon.enchantments)
                {
                    if (ench is BaseWeaponEnchantment weaponEnch)
                        weaponEnch.OnSwing(weapon, who);
                }
                weapon.FireProjectile(who);
            }

            weaponType.StartSwipe(weapon, who);
            anotherClick = false;

            if (who.CurrentTool == null)
            {
                who.completelyStopAnimatingOrDoingAction();
                who.forceCanMove();
            }
        }
    }

    [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.drawDuringUse), new Type[] { typeof( int ), typeof( int ), typeof( SpriteBatch ), typeof( Vector2 ), typeof( Farmer ), typeof( string ), typeof( int ), typeof( bool ) } )]
    public static class MeleeWeaponAnimationPatch
    {
        public static bool Prefix(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, string weapon_item_id, int type, bool isOnSpecial)
        {
            Impl(frameOfFarmerAnimation, facingDirection, spriteBatch, playerPosition, f, weapon_item_id, type, isOnSpecial);
            return false;
        }

        private static void Impl(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, string weapon_item_id, int type, bool isOnSpecial)
        {
            var itemTypeFromIdentifier = ItemRegistry.GetTypeDefinition("(W)");
            Texture2D texture = null;
            var itemDataForItemID = ItemRegistry.GetData(weapon_item_id);
            texture = itemDataForItemID.GetTexture();
            if (texture == null)
            {
                texture = Tool.weaponsTexture;
            }
            Rectangle sourceRect = itemDataForItemID.GetSourceRect(0);
            float drawLayer = f.getDrawLayer();
            FarmerRenderer.FarmerSpriteLayers weapon_sort_layer = FarmerRenderer.FarmerSpriteLayers.TOOL_IN_USE_SIDE;
            if (f.facingDirection.Value == 0)
            {
                weapon_sort_layer = FarmerRenderer.FarmerSpriteLayers.ToolUp;
            }
            else if (f.facingDirection.Value == 2)
            {
                weapon_sort_layer = FarmerRenderer.FarmerSpriteLayers.ToolDown;
            }
            float sort_behind_layer = FarmerRenderer.GetLayerDepth(drawLayer, FarmerRenderer.FarmerSpriteLayers.ToolUp);
            float sort_layer = FarmerRenderer.GetLayerDepth(drawLayer, weapon_sort_layer);

            var wtype = WeaponTypeManager.GetWeaponType(type);
            wtype.DrawDuringUse(frameOfFarmerAnimation, facingDirection, spriteBatch, playerPosition, f, isOnSpecial, texture, sourceRect, sort_behind_layer, sort_layer);
        }
    }

    [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.getAreaOfEffect))]
    public static class MeleeWeaponAreaOfEffectPatch
    {
        public static void Postfix(MeleeWeapon __instance,
            int x, int y, int facingDirection, ref Vector2 tileLocation1, ref Vector2 tileLocation2, Rectangle wielderBoundingBox, int indexInCurrentAnimation,
            ref Rectangle __result)
        {
            var wtype = WeaponTypeManager.GetWeaponType(__instance.type.Value);
            __result = wtype.GetNormalDamageArea(__instance, x, y, facingDirection, wielderBoundingBox, indexInCurrentAnimation);
        }
    }

    [HarmonyPatch(typeof(Farmer), nameof(Farmer.showSwordSwipe))]
    public static class FarmerSwipeAnimationPatch
    {
        public static bool Prefix(Farmer who)
        {
            Impl(who);
            return false;
        }

        private static void Impl(Farmer who)
        {
            // TODO - swipe stuff
            TemporaryAnimatedSprite tempSprite = null;
            bool dagger = who.CurrentTool != null && who.CurrentTool is MeleeWeapon && (int)(who.CurrentTool as MeleeWeapon).type == 1;
            Vector2 actionTile = who.GetToolLocation(ignoreClick: true);
            if (who.CurrentTool != null && who.CurrentTool is MeleeWeapon && !dagger)
            {
                (who.CurrentTool as MeleeWeapon).DoDamage(who.currentLocation, (int)actionTile.X, (int)actionTile.Y, who.FacingDirection, 1, who);
            }
            int min_swipe_interval = 20;
            switch (who.FacingDirection)
            {
                case 2:
                    switch (who.FarmerSprite.currentAnimationIndex)
                    {
                        case 0:
                            if (dagger)
                            {
                                who.yVelocity = -0.6f;
                            }
                            break;
                        case 1:
                            who.yVelocity = (dagger ? 0.5f : (-0.5f));
                            break;
                        case 5:
                            who.yVelocity = 0.3f;
                            tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(503, 256, 42, 17), who.Position + new Vector2(-16f, -2f) * 4f, flipped: false, 0.07f, Color.White)
                            {
                                scale = 4f,
                                animationLength = 1,
                                interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
                                alpha = 0.5f,
                                layerDepth = (who.Position.Y + 64f) / 10000f
                            };
                            break;
                    }
                    break;
                case 1:
                    switch (who.FarmerSprite.currentAnimationIndex)
                    {
                        case 0:
                            if (dagger)
                            {
                                who.xVelocity = 0.6f;
                            }
                            break;
                        case 1:
                            who.xVelocity = (dagger ? (-0.5f) : 0.5f);
                            break;
                        case 5:
                            who.xVelocity = -0.3f;
                            tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(4f, -12f) * 4f, flipped: false, 0.07f, Color.White)
                            {
                                scale = 4f,
                                animationLength = 1,
                                interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
                                alpha = 0.5f
                            };
                            break;
                    }
                    break;
                case 3:
                    switch (who.FarmerSprite.currentAnimationIndex)
                    {
                        case 0:
                            if (dagger)
                            {
                                who.xVelocity = -0.6f;
                            }
                            break;
                        case 1:
                            who.xVelocity = (dagger ? 0.5f : (-0.5f));
                            break;
                        case 5:
                            who.xVelocity = 0.3f;
                            tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(-15f, -12f) * 4f, flipped: false, 0.07f, Color.White)
                            {
                                scale = 4f,
                                animationLength = 1,
                                interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
                                flipped = true,
                                alpha = 0.5f
                            };
                            break;
                    }
                    break;
                case 0:
                    switch (who.FarmerSprite.currentAnimationIndex)
                    {
                        case 0:
                            if (dagger)
                            {
                                who.yVelocity = 0.6f;
                            }
                            break;
                        case 1:
                            who.yVelocity = (dagger ? (-0.5f) : 0.5f);
                            break;
                        case 5:
                            who.yVelocity = -0.3f;
                            tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(0f, -32f) * 4f, flipped: false, 0.07f, Color.White)
                            {
                                scale = 4f,
                                animationLength = 1,
                                interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
                                alpha = 0.5f,
                                rotation = 3.926991f
                            };
                            break;
                    }
                    break;
            }
            if (tempSprite != null)
            {
                if (who.CurrentTool != null && who.CurrentTool is MeleeWeapon && who.CurrentTool.InitialParentTileIndex == 4)
                {
                    tempSprite.color = Color.HotPink;
                }
                who.currentLocation.temporarySprites.Add(tempSprite);
            }
        }
    }
}
