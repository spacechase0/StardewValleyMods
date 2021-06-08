using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using Spacechase.Shared.Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace BuildableLocationsFramework.Patches
{
    /// <summary>Applies Harmony patches to <see cref="MilkPail"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class MilkPailPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<MilkPail>(nameof(MilkPail.beginUsing)),
                prefix: this.GetHarmonyMethod(nameof(Before_BeginUsing))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="MilkPail.beginUsing"/>.</summary>
        private static bool Before_BeginUsing(Shears __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            x = (int)who.GetToolLocation(false).X;
            y = (int)who.GetToolLocation(false).Y;
            Rectangle toolRect = new Rectangle(x - 32, y - 32, 64, 64);
            var __instance_animal = Mod.instance.Helper.Reflection.GetField<FarmAnimal>(__instance, "animal");
            if (location is IAnimalLocation animalLoc)
                __instance_animal.SetValue(Utility.GetBestHarvestableFarmAnimal(animalLoc.Animals.Values, __instance, toolRect));
            if (__instance_animal.GetValue() != null && (int)(NetFieldBase<int, NetInt>)__instance_animal.GetValue().currentProduce > 0 && ((int)(NetFieldBase<int, NetInt>)__instance_animal.GetValue().age >= (int)(byte)(NetFieldBase<byte, NetByte>)__instance_animal.GetValue().ageWhenMature && __instance_animal.GetValue().toolUsedForHarvest.Equals((object)__instance.BaseName)) && who.couldInventoryAcceptThisObject((int)(NetFieldBase<int, NetInt>)__instance_animal.GetValue().currentProduce, 1, 0))
            {
                __instance_animal.GetValue().doEmote(20, true);
                __instance_animal.GetValue().friendshipTowardFarmer.Value = Math.Min(1000, (int)(NetFieldBase<int, NetInt>)__instance_animal.GetValue().friendshipTowardFarmer + 5);
                who.currentLocation.localSound("Milking");
                __instance_animal.GetValue().pauseTimer = 1500;
            }
            else if (__instance_animal.GetValue() != null && (int)(NetFieldBase<int, NetInt>)__instance_animal.GetValue().currentProduce > 0 && (int)(NetFieldBase<int, NetInt>)__instance_animal.GetValue().age >= (int)(byte)(NetFieldBase<byte, NetByte>)__instance_animal.GetValue().ageWhenMature)
            {
                if (who != null && Game1.player.Equals((object)who))
                {
                    if (!__instance_animal.GetValue().toolUsedForHarvest.Equals((object)__instance.BaseName))
                    {
                        if (!((NetFieldBase<string, NetString>)__instance_animal.GetValue().toolUsedForHarvest == (NetString)null) && !__instance_animal.GetValue().toolUsedForHarvest.Equals((object)"null"))
                            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14167", (object)__instance_animal.GetValue().toolUsedForHarvest));
                    }
                    else if (!who.couldInventoryAcceptThisObject((int)(NetFieldBase<int, NetInt>)__instance_animal.GetValue().currentProduce, 1, 0))
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                }
            }
            else if (who != null && Game1.player.Equals((object)who))
            {
                DelayedAction.playSoundAfterDelay("fishingRodBend", 300, (GameLocation)null, -1);
                DelayedAction.playSoundAfterDelay("fishingRodBend", 1200, (GameLocation)null, -1);
                string dialogue = "";
                if (__instance_animal.GetValue() != null && !__instance_animal.GetValue().toolUsedForHarvest.Equals((object)__instance.BaseName))
                    dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14175", (object)__instance_animal.GetValue().displayName);
                if (__instance_animal.GetValue() != null && __instance_animal.GetValue().isBaby() && __instance_animal.GetValue().toolUsedForHarvest.Equals((object)__instance.BaseName))
                    dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14176", (object)__instance_animal.GetValue().displayName);
                if (__instance_animal.GetValue() != null && (int)(NetFieldBase<int, NetInt>)__instance_animal.GetValue().age >= (int)(byte)(NetFieldBase<byte, NetByte>)__instance_animal.GetValue().ageWhenMature && __instance_animal.GetValue().toolUsedForHarvest.Equals((object)__instance.BaseName))
                    dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14177", (object)__instance_animal.GetValue().displayName);
                if (dialogue.Length > 0)
                    DelayedAction.showDialogueAfterDelay(dialogue, 1000);
            }
            who.Halt();
            int currentFrame = who.FarmerSprite.CurrentFrame;
            who.FarmerSprite.animateOnce(287 + who.FacingDirection, 50f, 4);
            who.FarmerSprite.oldFrame = currentFrame;
            who.UsingTool = true;
            who.CanMove = false;
            return true;
        }
    }
}
