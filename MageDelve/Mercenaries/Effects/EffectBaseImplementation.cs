using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace MageDelve.Mercenaries.Effects
{
    internal static class EffectBaseImplementation
    {
        internal static ConditionalWeakTable<Character, List<EffectData>> pending = new();
        public static void ApplyEffect(this Character character, EffectData effectData)
        {
            pending.GetOrCreateValue(character).Add((EffectData) effectData.Clone());
        }
        public static void UpdateEffects(this Character character, float delta)
        {
            var pendingEffects = pending.GetOrCreateValue(character);
            foreach (var effectData in pendingEffects.ToList())
            {
                effectData.InitialDelay -= delta;
                if (effectData.InitialDelay <= 0)
                {
                    if (EffectData.EffectTypes.TryGetValue(effectData.EffectType, out var action))
                    {
                        action(character, effectData);
                        if (effectData.Occurrences > 1)
                        {
                            effectData.Occurrences--;
                            effectData.InitialDelay += effectData.RecurringDelay;
                        }
                        else
                            pendingEffects.Remove(effectData);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.update), new Type[] { typeof( GameTime ), typeof( GameLocation ), typeof( long ), typeof( bool ) } )]
    public static class CharacterUpdateEffectsPatch
    {
        public static void Prefix(Character __instance, GameTime time)
        {
            __instance.UpdateEffects((float)time.ElapsedGameTime.TotalSeconds);
        }
    }

    // Character.update doesn't get called when the farmer is updated since they are updated separately
    // (Farmers aren't in GameLocation.characters)
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.Update))]
    public static class FarmerUpdateEffectsPatch
    {
        public static void Prefix(Farmer __instance, GameTime time)
        {
            __instance.UpdateEffects((float)time.ElapsedGameTime.TotalSeconds);
        }
    }
}
