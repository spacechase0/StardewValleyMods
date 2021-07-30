using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
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
        public override void Apply(Harmony harmony, IMonitor monitor)
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

            var animalField = Mod.Instance.Helper.Reflection.GetField<FarmAnimal>(__instance, "animal");
            var animal = animalField.GetValue();

            if (location is IAnimalLocation animalLoc)
            {
                animalField.SetValue(animal = Utility.GetBestHarvestableFarmAnimal(animalLoc.Animals.Values, __instance, toolRect));
            }

            if (animal != null && animal.currentProduce.Value > 0 && (animal.age.Value >= animal.ageWhenMature.Value && animal.toolUsedForHarvest.Value.Equals(__instance.BaseName)) && who.couldInventoryAcceptThisObject(animal.currentProduce.Value, 1))
            {
                animal.doEmote(20);
                animal.friendshipTowardFarmer.Value = Math.Min(1000, animal.friendshipTowardFarmer.Value + 5);
                who.currentLocation.localSound("Milking");
                animal.pauseTimer = 1500;
            }
            else if (animal != null && animal.currentProduce.Value > 0 && animal.age.Value >= animal.ageWhenMature.Value)
            {
                if (who != null && Game1.player.Equals(who))
                {
                    if (!animal.toolUsedForHarvest.Value.Equals(__instance.BaseName))
                    {
                        if (animal.toolUsedForHarvest.Value != null && !animal.toolUsedForHarvest.Value.Equals("null"))
                            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14167", animal.toolUsedForHarvest.Value));
                    }
                    else if (!who.couldInventoryAcceptThisObject(animal.currentProduce.Value, 1))
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                }
            }
            else if (who != null && Game1.player.Equals(who))
            {
                DelayedAction.playSoundAfterDelay("fishingRodBend", 300);
                DelayedAction.playSoundAfterDelay("fishingRodBend", 1200);
                string dialogue = "";
                if (animal != null && !animal.toolUsedForHarvest.Value.Equals(__instance.BaseName))
                    dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14175", animal.displayName);
                if (animal != null && animal.isBaby() && animal.toolUsedForHarvest.Value.Equals(__instance.BaseName))
                    dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14176", animal.displayName);
                if (animal != null && animal.age.Value >= animal.ageWhenMature.Value && animal.toolUsedForHarvest.Value.Equals(__instance.BaseName))
                    dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14177", animal.displayName);
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
