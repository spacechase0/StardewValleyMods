using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

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
            {
                this.DataHelper.WriteSaveData("sleepy-eye", null as object);

                FileInfo legacyFile = new FileInfo(Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json"));
                if (legacyFile.Exists)
                    legacyFile.Delete();
            }

            // migrate old profession IDs to the new algorithm
            {
                IDictionary<int, KnownProfession> oldProfessions = this.GetKnownProfessions().ToDictionary(p => p.OldVanillaId);

                string forGameVersionLabel = SemanticVersion.TryParse(Game1.version, out ISemanticVersion gameVersion) && gameVersion.IsOlderThan("1.5.5")
                    ? "the upcoming Stardew Valley 1.5.5"
                    : $"Stardew Valley {Game1.version}";

                foreach (Farmer player in Game1.getAllFarmers())
                {
                    IList<int> professions = player.professions.ToList();

                    for (int i = 0; i < professions.Count; i++)
                    {
                        int id = professions[i];
                        if (oldProfessions.TryGetValue(id, out KnownProfession profession))
                        {
                            this.Monitor.LogOnce($"Custom profession IDs changed for compatibility with {forGameVersionLabel}.", LogLevel.Info);
                            this.Monitor.LogOnce($"Migrating professions for {player.Name}...", LogLevel.Info);
                            this.Monitor.Log($"    - '{profession.SkillId}.{profession.ProfessionId}': changed from {id} to {profession.NewVanillaId}", LogLevel.Info);
                            professions[i] = profession.NewVanillaId;
                            player.professions.Remove(i);
                            player.professions.Add(profession.NewVanillaId);
                        }
                    }
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the custom professions which may need to be migrated.</summary>
        private IEnumerable<KnownProfession> GetKnownProfessions()
        {
            return new KnownProfession[]
            {
                // Cooking Skill
                new("spacechase0.Cooking", "BuffLevel", 1523108199),     // Intense Flavors
                new("spacechase0.Cooking", "BuffPlain", 2070445932),     // Secret Spices
                new("spacechase0.Cooking", "BuffTime", 1996298040),      // Satisfying
                new("spacechase0.Cooking", "Conservation", -2118257575), // Efficient
                new("spacechase0.Cooking", "SellPrice", -1590995229),    // Gourmet
                new("spacechase0.Cooking", "Silver", 1124912015),        // Professional Chef

                // Equivalent Exchange
                new("EquivalentExchange.Alchemy", "Adept", -191196008),      // Adept
                new("EquivalentExchange.Alchemy", "Aurumancer", 807555225),  // Aurumancer
                new("EquivalentExchange.Alchemy", "Conduit", 2072845900),    // Conduit
                new("EquivalentExchange.Alchemy", "Sage", -362266530),       // Sage
                new("EquivalentExchange.Alchemy", "Shaper", 62801863),       // Shaper
                new("EquivalentExchange.Alchemy", "Transmuter", 1651221136), // Transmuter
                
                // Magic
                new("spacechase0.Magic", "FifthSpellSlot", -1668120241), // Memory
                new("spacechase0.Magic", "ManaCap", -6997631),           // Mana Reserve
                new("spacechase0.Magic", "ManaRegen1", 861299334),       // Mana Regen I
                new("spacechase0.Magic", "ManaRegen2", 861495942),       // Mana Regen II
                new("spacechase0.Magic", "UpgradePoints1", 269425385),   // Potential
                new("spacechase0.Magic", "UpgradePoints2", 269228777),   // Prodigy

                // The Love of Cooking
                new("blueberry.LoveOfCooking.CookingSkill", "menu.cooking_skill.tier1_path1", 1574400322),  // Sous Chef
                new("blueberry.LoveOfCooking.CookingSkill", "menu.cooking_skill.tier1_path2", 1574334786),  // Big Eater
                new("blueberry.LoveOfCooking.CookingSkill", "menu.cooking_skill.tier2_path1a", 1831162724), // Head Chef
                new("blueberry.LoveOfCooking.CookingSkill", "menu.cooking_skill.tier2_path1b", 88076595),   // Five Star Cook
                new("blueberry.LoveOfCooking.CookingSkill", "menu.cooking_skill.tier2_path2a", 1831097188), // Gourmet
                new("blueberry.LoveOfCooking.CookingSkill", "menu.cooking_skill.tier2_path2b", 88011059),   // Glutton
            };
        }

        /// <summary>A profession that may have been registered through SpaceCore 1.6.2 or earlier.</summary>
        private class KnownProfession
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The unique ID for the custom skill.</summary>
            public string SkillId { get; }

            /// <summary>The unique ID for the custom profession.</summary>
            public string ProfessionId { get; }

            /// <summary>The save ID for the profession as generated by SpaceCore 1.6.2 or earlier.</summary>
            public int OldVanillaId { get; }

            /// <summary>The save ID for the profession as generated by SpaceCore 1.6.3 or later.</summary>
            /// <remarks>Derived from <see cref="Skills.Skill.Profession.GetVanillaId"/>.</remarks>
            public int NewVanillaId => this.SkillId.GetDeterministicHashCode() ^ this.ProfessionId.GetDeterministicHashCode();


            /*********
            ** Public methods
            *********/
            /// <summary>Construct an instance.</summary>
            /// <param name="skillId">The unique ID for the custom skill.</param>
            /// <param name="professionId">The unique ID for the custom profession.</param>
            /// <param name="oldVanillaId">The save ID for the profession as generated by SpaceCore 1.6.2 or earlier.</param>
            public KnownProfession(string skillId, string professionId, int oldVanillaId)
            {
                this.SkillId = skillId;
                this.ProfessionId = professionId;
                this.OldVanillaId = oldVanillaId;
            }
        }
    }
}
