using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Overrides;
using SpaceShared;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore
{
    public interface IApi
    {
        string[] GetCustomSkills();
        int GetLevelForCustomSkill(Farmer farmer, string skill);
        void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt);
        int GetProfessionId(string skill, string profession);

        // Must take (Event, GameLocation, GameTime, string[])
        void AddEventCommand( string command, MethodInfo info );
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

        public void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt)
        {
            farmer.AddCustomSkillExperience(skill, amt);
        }

        public int GetProfessionId(string skill, string profession)
        {
            return Skills.GetSkill(skill).Professions.Single(p => p.Id == profession).GetVanillaId();
        }

        public void AddEventCommand( string command, MethodInfo info )
        {
            if ( info.GetParameters().Length != 4 )
                throw new ArgumentException( "Custom event method must take Must take (Event, GameLocation, GameTime, string[])" );
            if ( info.GetParameters()[ 0 ].ParameterType != typeof( Event ) ||
                 info.GetParameters()[ 1 ].ParameterType != typeof( GameLocation ) ||
                 info.GetParameters()[ 2 ].ParameterType != typeof( GameTime ) ||
                 info.GetParameters()[ 3 ].ParameterType != typeof( string[] ) )
                throw new ArgumentException( "Custom event method must take Must take (Event, GameLocation, GameTime, string[])" );

            Log.debug( "Adding event command: " + command + " = " + info );
            EventTryCommandPatch.customCommands.Add( command, info );
        }
    }
}
