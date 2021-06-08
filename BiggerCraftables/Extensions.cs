namespace BiggerCraftables
{
    public static class Extensions
    {
        public static string BiggerIndexKey => $"{Mod.instance.ModManifest.UniqueID}/BiggerIndex";

        public static int GetBiggerIndex(this StardewValley.Object obj)
        {
            return obj.modData.ContainsKey(BiggerIndexKey) ? int.Parse(obj.modData[BiggerIndexKey]) : 0;
        }

        public static void SetBiggerIndex(this StardewValley.Object obj, int index)
        {
            obj.modData[BiggerIndexKey] = index.ToString();
        }
    }
}
