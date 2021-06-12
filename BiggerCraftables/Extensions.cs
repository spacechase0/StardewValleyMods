namespace BiggerCraftables
{
    public static class Extensions
    {
        public static string BiggerIndexKey => $"{Mod.Instance.ModManifest.UniqueID}/BiggerIndex";

        public static int GetBiggerIndex(this StardewValley.Object obj)
        {
            return obj.modData.ContainsKey(Extensions.BiggerIndexKey) ? int.Parse(obj.modData[Extensions.BiggerIndexKey]) : 0;
        }

        public static void SetBiggerIndex(this StardewValley.Object obj, int index)
        {
            obj.modData[Extensions.BiggerIndexKey] = index.ToString();
        }
    }
}
