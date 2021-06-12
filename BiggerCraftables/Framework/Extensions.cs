namespace BiggerCraftables
{
    internal static class Extensions
    {
        public static string BiggerIndexKey => $"{Mod.Instance.ModManifest.UniqueID}/BiggerIndex";

        public static int GetBiggerIndex(this StardewValley.Object obj)
        {
            return obj.modData.TryGetValue(Extensions.BiggerIndexKey, out string rawIndex)
                ? int.Parse(rawIndex)
                : 0;
        }

        public static void SetBiggerIndex(this StardewValley.Object obj, int index)
        {
            obj.modData[Extensions.BiggerIndexKey] = index.ToString();
        }
    }
}
