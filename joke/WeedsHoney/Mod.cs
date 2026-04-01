using HarmonyLib;
using Microsoft.Xna.Framework;
using SpaceShared;
using SpaceShared.Attributes;
using StardewValley;

namespace WeedsHoney;

[HasHarmony]
public partial class Mod : BaseMod<Mod>
{
    protected override void ModEntry()
    {
    }
}

[HarmonyPatch(typeof(MachineDataUtility), nameof(MachineDataUtility.GetNearbyFlowerItemId))]
public static class NearbyFlowerPatch
{
    public static void Postfix(StardewValley.Object machine, ref string __result)
    {
        int range = 5;

        Queue<Vector2> vector2Queue = new Queue<Vector2>();
        HashSet<Vector2> vector2Set = new HashSet<Vector2>();
        vector2Queue.Enqueue(machine.TileLocation);
        for (int index = 0; (range >= 0 || range < 0 && index <= 150) && vector2Queue.Count > 0; ++index)
        {
            Vector2 vector2 = vector2Queue.Dequeue();
            if (machine.Location.getObjectAtTile((int)vector2.X, (int)vector2.Y) is StardewValley.Object { Name: "Weeds" } obj)
            {
                __result = obj.ItemId;
                return;
            }

            foreach (Vector2 adjacentTileLocation in Utility.getAdjacentTileLocations(vector2))
            {
                if (!vector2Set.Contains(adjacentTileLocation) && (range < 0 || (double)Math.Abs(adjacentTileLocation.X - machine.TileLocation.X) + (double)Math.Abs(adjacentTileLocation.Y - machine.TileLocation.Y) <= (double)range))
                    vector2Queue.Enqueue(adjacentTileLocation);
            }
            vector2Set.Add(vector2);
        }
    }
}
