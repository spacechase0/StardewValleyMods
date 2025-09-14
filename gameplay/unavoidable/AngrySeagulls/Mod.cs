using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace AngrySeagulls
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public static ConditionalWeakTable<Bat, NetRef<Item>> extraDrops = new();

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            Helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            Helper.Events.GameLoop.DayEnding += this.GameLoop_DayEnding;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            var beach = Game1.getLocationFromName("Beach");
            Rectangle range = new Rectangle(18, 7, 43 - 18, 19 - 7);

            int amt = 1 + Game1.random.Next(3);
            for (int i = 0; i < amt; ++i)
            {
                Point tile = new(range.X + Game1.random.Next(range.Width), range.Y + Game1.random.Next(range.Height));

                Bat seagull = new(tile.ToVector2() * Game1.tileSize);
                seagull.reloadSprite();
                seagull.Sprite.textureName.Value = Helper.ModContent.GetInternalAssetName("assets/seagull.png").BaseName;
                seagull.canLunge.Value = true;
                seagull.DamageToFarmer = 1;
                seagull.Health = seagull.MaxHealth = 3;

                beach.characters.Add(seagull);
            }
        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            var beach = Game1.getLocationFromName("Beach");

            List<NPC> toRemove = new();
            foreach (var npc in beach.characters)
            {
                if (npc is Bat bat && bat.Sprite.textureName.Value == Helper.ModContent.GetInternalAssetName("assets/seagull.png").BaseName)
                {
                    toRemove.Add(npc);
                }
            }

            foreach (var npc in toRemove)
                beach.characters.Remove(npc);
        }
    }

    [HarmonyPatch(typeof(Bat), "initNetFields")]
    public static class BatExtraDropFieldPatch
    {
        public static void Postfix(Bat __instance)
        {
            __instance.NetFields.AddField(Mod.extraDrops.GetOrCreateValue(__instance), "extraDrop");
        }
    }

    [HarmonyPatch(typeof(Bat), nameof(Bat.onDealContactDamage))]
    public static class BatStealFoodPatch
    {
        public static void Postfix(Bat __instance, Farmer who)
        {
            if (__instance.Sprite.textureName.Value != Mod.instance.Helper.ModContent.GetInternalAssetName("assets/seagull.png").BaseName)
                return;

            var extraDrop = Mod.extraDrops.GetOrCreateValue(__instance);
            //if (extraDrop.Value == null)
            {
                List<Weighted<Item>> potential = new();
                foreach (var item in who.Items)
                {
                    if (item is StardewValley.Object obj && obj.Edibility > 0)
                    {
                        if ( extraDrop.Value == null || obj.canStackWith( extraDrop.Value ) )
                            potential.Add(new(obj.Edibility, obj));
                    }
                }

                if (potential.Count == 0)
                    return;

                var choice = potential.Choose();
                if (extraDrop.Value == null)
                {
                    extraDrop.Value = choice.getOne();
                }
                else
                {
                    extraDrop.Value.Stack++;
                }
                if (choice.Stack > 1)
                    choice.Stack--;
                else
                {
                    who.Items[who.Items.IndexOf(choice)] = null;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Bat), nameof(Bat.getExtraDropItems))]
    public static class BatExtraDropsPatch
    {
        public static void Postfix(Bat __instance, List<Item> __result)
        {
            var extraDrop = Mod.extraDrops.GetOrCreateValue(__instance);
            if (extraDrop.Value != null)
            {
                var tmp = extraDrop.Value;
                extraDrop.Value = null;
                __result.Add(tmp);
            }
        }
    }

    [HarmonyPatch(typeof(Bat), nameof(Bat.drawAboveAllLayers))]
    public static class BatDrawLootPatch
    {
        public static void Postfix(Bat __instance, SpriteBatch b)
        {
            var extraDrop = Mod.extraDrops.GetOrCreateValue(__instance);
            if (extraDrop.Value != null)
            {
                extraDrop.Value.drawInMenu(b, __instance.getLocalPosition(Game1.viewport) + new Vector2(0, 32), 1, 1, 0.93f, StackDrawType.Hide);
            }
        }
    }
}
