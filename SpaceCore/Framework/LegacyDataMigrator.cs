using System.IO;
using StardewModdingAPI;

namespace SpaceCore.Framework
{
    /// <summary>Handles migrating legacy data for a save file.</summary>
    internal class LegacyDataMigrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>An API for reading and storing local mod data.</summary>
        private readonly IDataHelper DataHelper;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="dataHelper"><inheritdoc cref="DataHelper" path="/summary"/></param>
        /// <param name="monitor"><inheritdoc cref="Monitor" path="/summary"/></param>
        public LegacyDataMigrator(IDataHelper dataHelper, IMonitor monitor)
        {
            this.DataHelper = dataHelper;
            this.Monitor = monitor;
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        public void OnSaveLoaded()
        {
            if (!Context.IsMainPlayer)
                return;

            // delete legacy SleepyEye data
            if (Context.IsMainPlayer)
            {
                this.DataHelper.WriteSaveData("sleepy-eye", null as object);

                FileInfo legacyFile = new FileInfo(Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json"));
                if (legacyFile.Exists)
                    legacyFile.Delete();
            }
        }

        /// <summary>Get the vanilla ID for a profession.</summary>
        /// <param name="skillId">The skill ID.</param>
        /// <param name="professionId">The profession ID.</param>
        /// <remarks>Derived from <see cref="Skills.Skill.Profession.GetVanillaId"/>.</remarks>
        private int GetId(string skillId, string professionId)
        {
            return skillId.GetHashCode() ^ professionId.GetHashCode();
        }
    }
}
