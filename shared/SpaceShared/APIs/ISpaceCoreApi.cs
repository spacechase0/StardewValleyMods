using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace SpaceShared.APIs
{
    public interface ISpaceCoreApi
    {
        string[] GetCustomSkills();
        int GetLevelForCustomSkill(Farmer farmer, string skill);
        void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt);
        int GetProfessionId(string skill, string profession);

        // Must have [XmlType("Mods_SOMETHINGHERE")] attribute (required to start with "Mods_")
        void RegisterSerializerType(Type type);
        void RegisterCustomProperty( Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter );

        void RegisterSpawnableMonster(string id, Func<Vector2, Dictionary<string, object>, Monster> monsterSpawner);

        List<int> GetLocalIndexForMethod(MethodBase meth, string local);

        event EventHandler<Action<string, Action>> AdvancedInteractionStarted;

        void RegisterEquipmentSlot(IManifest modManifest, string globalId, Func<Item, bool> slotValidator, Func<string> slotDisplayName, Texture2D bgTex, Rectangle? bgRect = null);
        Item GetItemInEquipmentSlot(Farmer farmer, string globalId);
        void SetItemInEquipmentSlot(Farmer farmer, string globalId, Item item);
        bool CanItemGoInEquipmentSlot(string globalId, Item item);
    }
}
