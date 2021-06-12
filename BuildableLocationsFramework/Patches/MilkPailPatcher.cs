using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace BuildableLocationsFramework.Patches
{
    /// <summary>Applies Harmony patches to <see cref="MilkPail"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
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
            x = (int)who.GetToolLocation().X;
            y = (int)who.GetToolLocation().Y;
            Rectangle toolRect = new Rectangle(x - 32, y - 32, 64, 64);
            var __instance_animal = Mod.Instance.Helper.Reflection.GetField<FarmAnimal>(__instance, "animal");
            if (location is IAnimalLocation animalLoc)
                __instance_animal.SetValue(Utility.GetBestHarvestableFarmAnimal(animalLoc.Animals.Values, __instance, toolRect));
            if (__instance_animal.GetValue() != null && (int)__instance_animal.GetValue().currentProduce > 0 && ((int)__instance_animal.GetValue().age >= (byte)__instance_animal.GetValue().ageWhenMature && __instance_animal.GetValue().toolUsedForHarvest.Equals(__instance.BaseName)) && who.couldInventoryAcceptThisObject((int)__instance_animal.GetValue().currentProduce, 1))
            {
                __instance_animal.GetValue().doEmote(20);
                __instance_animal.GetValue().friendshipTowardFarmer.Value = Math.Min(1000, (int)__instance_animal.GetValue().friendshipTowardFarmer + 5);
                who.currentLocation.localSound("Milking");
                __instance_animal.GetValue().pauseTimer = 1500;
            }
            else if (__instance_animal.GetValue() != null && (int)__instance_animal.GetValue().currentProduce > 0 && (int)__instance_animal.GetValue().age >= (byte)__instance_animal.GetValue().ageWhenMature)
            {
                if (who != null && Game1.player.Equals(who))
                {
                    if (!__instance_animal.GetValue().toolUsedForHarvest.Equals(__instance.BaseName))
                    {
                        if (!(__instance_animal.GetValue().toolUsedForHarvest == null) && !__instance_animal.GetValue().toolUsedForHarvest.Equals("null"))
                            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14167", __instance_animal.GetValue().toolUsedForHarvest));
                    }
                    else if (!who.couldInventoryAcceptThisObject((int)__instance_animal.GetValue().currentProduce, 1))
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                }
            }
            else if (who != null && Game1.player.Equals(who))
            {
                DelayedAction.playSoundAfterDelay("fishingRodBend", 300);
                DelayedAction.playSoundAfterDelay("fishingRodBend", 1200);
                string dialogue = "";
                if (__instance_animal.GetValue() != null && !__instance_animal.GetValue().toolUsedForHarvest.Equals(__instance.BaseName))
                    dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14175", __instance_animal.GetValue().displayName);
                if (__instance_animal.GetValue() != null && __instance_animal.GetValue().isBaby() && __instance_animal.GetValue().toolUsedForHarvest.Equals(__instance.BaseName))
                    dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14176", __instance_animal.GetValue().displayName);
                if (__instance_animal.GetValue() != null && (int)__instance_animal.GetValue().age >= (byte)__instance_animal.GetValue().ageWhenMature && __instance_animal.GetValue().toolUsedForHarvest.Equals(__instance.BaseName))
                    dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14177", __instance_animal.GetValue().displayName);
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
