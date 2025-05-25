using System;

namespace StardewHack.WearMoreRings
{
    /*
     * If you're looking for the old API methods, those were implemented as follows.
     * You can copy the code and use that instead of the old API.
     * These work regardless of whether Wear More Rings is installed. 
     * If Wear More rings is installed, GetAllRings will include the ring slots that are added by that mod.
     * For combined rings, GetAllRings will return the rings used to create that combined ring, rather than the combined ring itself.
     * If you need access to the combined rings, or want to manipulate the modded ring slots directly, that's only possible with the new API.
     * 
     * CountEquippedRings's which argument probably needs to be changed into a string for SDV 1.6.
     */
#if false
    public int CountEquippedRings(Farmer f, int which) {
        int res = 0;
        foreach (var r in GetAllRings(f)) {
            if (r.GetsEffectOfRing(which)) {
                res++;
            }
        }
        return res;
    }

    public IEnumerable<Ring> GetAllRings(Farmer f) {
        if (f == null) throw new ArgumentNullException(nameof(f));
        var stack = new Stack<Ring>();
        stack.Push(f.leftRing.Value);
        stack.Push(f.rightRing.Value);
        while (stack.Count > 0) {
            var ring = stack.Pop();
            if (ring is CombinedRing) {
                foreach (var cr in ((CombinedRing)ring).combinedRings) {
                    stack.Push(cr);
                }
            } else if (ring != null) {
                yield return ring;
            }
        }
    }
#endif


    public interface IWearMoreRingsAPI_2 {
        /// <summary>
        /// Get the mod's config setting for how many rings can be equipped.
        /// 
        /// Note that this value is not synchronized in multiplayer, so its only valid for the current player (Game1.player).
        /// </summary>
        /// <returns>Config setting for how many rings the local player can wear.</returns>
        int RingSlotCount();

        /// <summary>
        /// Get the ring that the local player has equipped in the given slot. 
        /// </summary>
        /// <param name="slot">The ring equipment slot being queried. Ranging from 0 to RingSlotCount()-1.</param>
        /// <returns>The ring equipped in the given slot or null if its empty.</returns>
        StardewValley.Objects.Ring GetRing(int slot);

        /// <summary>
        /// Equip a new ring in the given slot. Note that this overwrites the previous ring in that slot. Use null to remove the ring.
        /// </summary>
        /// <param name="slot">The ring equipment slot being queried. Ranging from 0 to RingSlotCount()-1.</param>
        /// <param name="ring">The new ring being equipped. Can be null to unequip the current ring.</param>
        void SetRing(int slot, StardewValley.Objects.Ring ring);

    }
}
