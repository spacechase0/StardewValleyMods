using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Patches;
using SpaceCore.VanillaAssetExpansion;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace SpaceCore
{
    public interface IApi
    {
        /// <summary>
        /// Returns a list of all currently loaded skill's string IDs
        /// </summary>
        /// <returns>An array of skill IDs</returns>
        string[] GetCustomSkills();

        /// <summary>
        /// Gets the Base level of the custom skill for the farmer.
        /// </summary>
        /// <param name="farmer"> The farmer who you want to get the skill level for.</param>
        /// <param name="skill"> The string ID of the skill you want the level of.</param>
        /// <returns>Int</returns>
        int GetLevelForCustomSkill(Farmer farmer, string skill);

        /// <summary>
        /// Gets the Buff level of the custom skill for the farmer.
        /// </summary>
        /// <param name="farmer"> The farmer who you want to get the skill level for.</param>
        /// <param name="skill"> The string ID of the skill you want the level of.</param>
        /// <returns>Int</returns>
        int GetBuffLevelForCustomSkill(Farmer farmer, string skill);

        /// <summary>
        /// Gets the Base + Buff level of the custom skill for the farmer.
        /// </summary>
        /// <param name="farmer"> The farmer who you want to get the skill level for.</param>
        /// <param name="skill"> The string ID of the skill you want the level of.</param>
        /// <returns>Int</returns>
        int GetTotalLevelForCustomSkill(Farmer farmer, string skill);

        /// <summary>
        /// Gets the total exp the skill has
        /// </summary>
        /// <param name="farmer"> The farmer who you want to get the skill level for.</param>
        /// <param name="skill"> The string ID of the skill you want the level of.</param>
        /// <returns>Int</returns>
        int GetExperienceForCustomSkill(Farmer farmer, string skill);

        /// <summary>
        /// Get a list of all the skills with the ID, current xp value in them, and current level
        /// </summary>
        /// <param name="farmer"> The farmer you want the list for</param>
        /// <returns>List<Tuple<skill, XP value, Level>></returns>
        List<Tuple<string, int, int>> GetExperienceAndLevelsForCustomSkill(Farmer farmer);

        /// <summary>
        /// Add EXP to a custom skill
        /// </summary>
        /// <param name="farmer"> The farmer who you want to give exp to</param>
        /// <param name="skill"> The string ID of the custom skill</param>
        /// <param name="amt"> The int Amount you want to give</param>
        void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt);

        /// <summary>
        /// Gets the display name of the skill
        /// </summary>
        /// <param name="skill"> The ID of the skill you want to get</param>
        /// <returns>Texture2D</returns>
        string GetDisplayNameOfCustomSkill(string skill);

        /// <summary>
        /// Gets the 10x10 icon of the skill that shows up on the skill page
        /// </summary>
        /// <param name="skill"> The ID of the skill you want to get</param>
        /// <returns>Texture2D</returns>
        Texture2D GetSkillPageIconForCustomSkill(string skill);

        /// <summary>
        /// Gets the 16x16 icon of the skill that shows up in the level up menu.
        /// </summary>
        /// <param name="skill"> The ID of the skill you want to get</param>
        /// <returns>Texture2D</returns>
        Texture2D GetSkillIconForCustomSkill(string skill);

        /// <summary>
        /// Get the profession ID for the custom skill
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="profession"></param>
        /// <returns>int profession ID</returns>
        int GetProfessionId(string skill, string profession);

        /// Must have [XmlType("Mods_SOMETHINGHERE")] attribute (required to start with "Mods_")
        void RegisterSerializerType(Type type);

        void RegisterCustomProperty( Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter );

        void RegisterSpawnableMonster(string id, Func<Vector2, Dictionary<string, object>, Monster> monsterSpawner);

        List<int> GetLocalIndexForMethod(MethodBase meth, string local);

        public event EventHandler<Action<string, Action>> AdvancedInteractionStarted;

        public List<string> GetVirtualCurrencyList();
        public bool IsVirtualCurrencyTeamWide(string currency);
        public int GetVirtualCurrencyAmount(Farmer who, string currency);
        public void AddToVirtualCurrency(Farmer who, string currency, int amount); // supports negative numbers

        public void RegisterEquipmentSlot(IManifest modManifest, string globalId, Func<Item, bool> slotValidator, Func<string> slotDisplayName, Texture2D bgTex, Rectangle? bgRect = null);
        public Item GetItemInEquipmentSlot(Farmer farmer, string globalId);
        public void SetItemInEquipmentSlot(Farmer farmer, string globalId, Item item);
        public bool CanItemGoInEquipmentSlot(string globalId, Item item);
    }

    public class Api : IApi
    {
        public string[] GetCustomSkills()
        {
            return Skills.GetSkillList();
        }

        public int GetLevelForCustomSkill(Farmer farmer, string skill)
        {
            return Skills.GetSkillLevel(farmer, skill);
        }

        public int GetBuffLevelForCustomSkill(Farmer farmer, string skill)
        {
            return Skills.GetSkillBuffLevel(farmer, skill);
        }

        public int GetTotalLevelForCustomSkill(Farmer farmer, string skill)
        {
            return Skills.GetSkillBuffLevel(farmer, skill) + Skills.GetSkillLevel(farmer, skill);
        }

        public int GetExperienceForCustomSkill(Farmer farmer, string skill)
        {
            return farmer.GetCustomSkillExperience(skill);
        }

        public List<Tuple<string, int, int>> GetExperienceAndLevelsForCustomSkill(Farmer farmer)
        {
            return farmer.GetCustomSkillExperienceAndLevels();
        }

        public void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt)
        {
            farmer.AddCustomSkillExperience(skill, amt);
        }

        public string GetDisplayNameOfCustomSkill(string skill)
        {
            return Skills.GetSkill(skill).GetName();
        }

        public Texture2D GetSkillPageIconForCustomSkill(string skill)
        {
            return Skills.GetSkillPageIcon(skill);
        }

        public Texture2D GetSkillIconForCustomSkill(string skill)
        {
            return Skills.GetSkillIcon(skill);
        }

        public int GetProfessionId(string skill, string profession)
        {
            return Skills.GetSkill(skill).Professions.Single(p => p.Id == profession).GetVanillaId();
        }

        public void RegisterSerializerType(Type type)
        {
            if (type.GetCustomAttribute<XmlTypeAttribute>()?.TypeName?.StartsWith("Mods_") != true)
            {
                throw new ArgumentException("Custom types must have an [XmlType] attribute with the TypeName starting with \"Mods_\"");
            }
            SpaceCore.ModTypes.Add(type);
        }

        public void RegisterCustomProperty( Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter )
        {
            if ( !SpaceCore.CustomProperties.ContainsKey( declaringType ) )
                SpaceCore.CustomProperties.Add( declaringType, new() );

            SpaceCore.CustomProperties[ declaringType ].Add( name, new Framework.CustomPropertyInfo()
            {
                DeclaringType = declaringType,
                Name = name,
                PropertyType = propType,
                Getter = getter,
                Setter = setter,
            } );
        }

        public void RegisterSpawnableMonster(string id, Func<Vector2, Dictionary<string, object>, Monster> monsterSpawner)
        {
            Dungeons.SpawnableImpl.MonsterFactory.Add(id, monsterSpawner);
        }

        public List<int> GetLocalIndexForMethod(MethodBase meth, string local)
        {
            return SpaceCore.GetLocalIndexForMethod(meth, local);
        }

        public event EventHandler<Action<string, Action>> AdvancedInteractionStarted;

        internal void InvokeASI(NPC npc, Action<string, Action> addCallback)
        {
            AdvancedInteractionStarted?.Invoke(npc, addCallback);
        }

        public List<string> GetVirtualCurrencyList()
        {
            return VanillaAssetExpansion.VanillaAssetExpansion.virtualCurrencies.Keys.ToList();
        }

        public bool IsVirtualCurrencyTeamWide(string currency)
        {
            return VanillaAssetExpansion.VanillaAssetExpansion.virtualCurrencies.TryGetValue(currency, out var data) ? data.TeamWide : false;
        }
        public int GetVirtualCurrencyAmount(Farmer who, string currency)
        {
            if (!VanillaAssetExpansion.VanillaAssetExpansion.virtualCurrencies.TryGetValue(currency, out var data))
                return 0;

            return data.TeamWide ? who.team.GetVirtualCurrencyAmount(currency) : who.GetVirtualCurrencyAmount(currency);
        }
        public void AddToVirtualCurrency(Farmer who, string currency, int amt)
        {
            if (!VanillaAssetExpansion.VanillaAssetExpansion.virtualCurrencies.TryGetValue(currency, out var data))
                return;

            if (data.TeamWide)
                who.team.AddVirtualCurrencyAmount(currency, amt);
            else
                who.AddVirtualCurrencyAmount(currency, amt);
        }

        public void RegisterEquipmentSlot(IManifest modManifest, string globalId, Func<Item, bool> slotValidator, Func<string> slotDisplayName, Texture2D bgTex, Rectangle? bgRect = null)
        {
            SpaceCore.EquipmentSlots.Add(globalId, new()
            {
                CorrespondingMod = modManifest,
                SlotValidator = slotValidator,
                DisplayName = slotDisplayName,
                BackgroundTex = bgTex,
                BackgroundRect = bgRect
            });
        }

        public Item GetItemInEquipmentSlot(Farmer farmer, string globalId)
        {
            if (farmer.GetExtData().ExtraEquippables.TryGetValue(globalId, out var item))
                return item;
            return null;
        }

        public void SetItemInEquipmentSlot(Farmer farmer, string globalId, Item item)
        {
            if (SpaceCore.EquipmentSlots.TryGetValue(globalId, out var data) && data.SlotValidator( item ) )
            {
                farmer.GetExtData().ExtraEquippables[globalId] = item;
            }
        }

        public bool CanItemGoInEquipmentSlot(string globalId, Item item)
        {
            var data = SpaceCore.EquipmentSlots.GetOrDefault(globalId);
            return data?.SlotValidator?.Invoke(item) ?? false;
        }
    }
}
