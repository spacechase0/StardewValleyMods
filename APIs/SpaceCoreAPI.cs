using StardewValley;
using System;
using System.Reflection;

namespace SpaceShared.APIs
{
    public interface SpaceCoreAPI
    {
        string[] GetCustomSkills();
        int GetLevelForCustomSkill( Farmer farmer, string skill );
        void AddExperienceForCustomSkill( Farmer farmer, string skill, int amt );
        int GetProfessionId( string skill, string profession );

        // Must take (Event, GameLocation, GameTime, string[])
        void AddEventCommand( string command, MethodInfo info );

        // Must have [XmlType("Mods_SOMETHINGHERE")] attribute (required to start with "Mods_")
        void RegisterSerializerType( Type type );
    }
}
