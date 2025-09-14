using StardewValley;
using StardewValley.Buildings;

namespace CustomizeExterior.Framework
{
    /// <summary>Encapsulates reading and writing values in <see cref="Building.modData"/> and <see cref="GameLocation.modData"/>.</summary>
    internal static class ModDataManager
    {
        /*********
        ** Fields
        *********/
        /// <summary>The prefix added to mod data keys.</summary>
        private const string Prefix = "spacechase0.CustomizeExterior";

        /// <summary>The data key in <see cref="Building.modData"/> for the asset pack folder name containing a building's texture path.</summary>
        private const string BuildingAssetPackKey = ModDataManager.Prefix + "/AssetPack";

        /// <summary>The data key in <see cref="GameLocation.modData"/> for the asset pack folder name containing the farmhouse's texture path.</summary>
        private const string FarmhouseTextureKey = ModDataManager.Prefix + "/FarmhouseAssetPack";


        /*********
        ** Public methods
        *********/
        /// <summary>Get the asset pack folder name containing the building's texture.</summary>
        /// <param name="building">The building instance.</param>
        public static string GetAssetPackName(this Building building)
        {
            return building.modData.TryGetValue(ModDataManager.BuildingAssetPackKey, out string path)
                ? path
                : null;
        }

        /// <summary>Set the asset pack folder name containing the building's texture.</summary>
        /// <param name="building">The building instance.</param>
        /// <param name="folderName">The asset pack's folder name.</param>
        public static void SetAssetPack(this Building building, string folderName)
        {
            building.modData[ModDataManager.BuildingAssetPackKey] = folderName;
        }

        /// <summary>Get the asset pack folder name containing the farmhouse's texture.</summary>
        /// <param name="farm">The farm instance.</param>
        public static string GetFarmhouseAssetPackName(this Farm farm)
        {
            return farm.modData.TryGetValue(ModDataManager.FarmhouseTextureKey, out string path)
                ? path
                : null;
        }

        /// <summary>Set the asset pack folder name containing the farmhouse's texture.</summary>
        /// <param name="farm">The farm instance.</param>
        /// <param name="folderName">The asset pack's folder name.</param>
        public static void SetFarmhouseAssetPackName(this Farm farm, string folderName)
        {
            farm.modData[ModDataManager.FarmhouseTextureKey] = folderName;
        }
    }
}
