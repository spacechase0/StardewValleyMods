using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using SpaceCore;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;

namespace SkillTraining
{
    public class Mod : StardewModdingAPI.Mod
    {
        public class SkillInfo
        {
            public string Display { get; set; }
            public object Indicator { get; set; }
        }

        public static Mod Instance;
        internal Configuration Config { get; set; }
        private Dictionary<string, SkillInfo> TrainingMap { get; set; }

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Log.Monitor = Monitor;
            Config = Helper.ReadConfig<Configuration>();
            I18n.Init(Helper.Translation);

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm != null)
            {
                gmcm.Register(ModManifest, () => Config = new(), () => Helper.WriteConfig(Config));
                gmcm.AddNumberOption(ModManifest, () => Config.PricePerExperiencePoint, (int val) => Config.PricePerExperiencePoint = val, () => I18n.Config_PricePerExpPoint_Name(), () => I18n.Config_PricePerExpPoint_Description());
                gmcm.AddNumberOption(ModManifest, () => Config.MaxTrainableLevel, (int val) => Config.MaxTrainableLevel = val, () => I18n.Config_MaxTrainableLevel_Name(), () => I18n.Config_MaxTrainableLevel_Description());
            }

            var asi = Helper.ModRegistry.GetApi<IAdvancedSocialInteractionsApi>("spacechase0.AdvancedSocialInteractions");
            asi.AdvancedInteractionStarted += this.Asi_AdvancedInteractionStarted;
        }

        private void Asi_AdvancedInteractionStarted(object sender, Action<string, Action> e)
        {
            if (TrainingMap == null)
                TrainingMap = GetTrainingMap();

            var npc = sender as NPC;
            if (!TrainingMap.ContainsKey(npc.Name))
                return;
            var info = TrainingMap[npc.Name];

            int exp = 0;
            int[] expCurve = new[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };
            Action<int> addExp = null;
            if (info.Indicator is int vanilla)
            {
                exp = Game1.player.experiencePoints[vanilla];
                addExp = (amt) => Game1.player.gainExperience(vanilla, amt);
            }
            else
            {
                Skills.Skill skill = Skills.GetSkill(info.Indicator as string);
                exp = Game1.player.GetCustomSkillExperience(skill);
                expCurve = skill.ExperienceCurve;
                addExp = (amt) => Game1.player.AddCustomSkillExperience(skill, amt);
            }

            int currLevel = -1;
            for (int i = 0; i < expCurve.Length; ++i)
            {
                if (exp < expCurve[i])
                {
                    currLevel = i;
                    break;
                }
            }

            if (currLevel >= Config.MaxTrainableLevel || currLevel >= expCurve.Length || currLevel == -1)
                return;

            int pointsNeeded = expCurve[currLevel] - exp;
            int price = pointsNeeded * Config.PricePerExperiencePoint;

            e(I18n.Prompt_Question(currLevel + 1, info.Display, price), () =>
            {
                if (Game1.player.Money >= price)
                {
                    Game1.player.Money -= price;
                    addExp(pointsNeeded);
                    Game1.drawObjectDialogue(I18n.Prompt_Trained(currLevel + 1, info.Display));
                }
                else
                {
                    Game1.drawObjectDialogue(I18n.Prompt_Broke());
                }
            });
        }

        private Dictionary<string, SkillInfo> GetTrainingMap()
        {
            Dictionary<string, SkillInfo> ret = new();

            ret.Add("Marnie", new() { Display = Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1991"), Indicator = Farmer.farmingSkill });
            ret.Add("Clint", new() { Display = Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1992"), Indicator = Farmer.miningSkill });
            ret.Add("Willy", new() { Display = Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1993"), Indicator = Farmer.fishingSkill });
            ret.Add("Leah", new() { Display = Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1994"), Indicator = Farmer.foragingSkill });
            ret.Add("Kent", new() { Display = Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1996"), Indicator = Farmer.combatSkill });

            if ( Helper.ModRegistry.IsLoaded( "spacechase0.LuckSkill" ) )
                ret.Add("Emily", new() { Display = Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1995"), Indicator = Farmer.luckSkill });

            if ( Helper.ModRegistry.IsLoaded( "spacechase0.Magic" ))
                ret.Add("Wizard", new() { Display = Skills.GetSkill( "Magic" ).GetName(), Indicator = "Magic" });

            // Hopefully nobody has both installed...
            // Preferring blueberry's because his mod is just better 
            if (Helper.ModRegistry.IsLoaded("blueberry.LoveOfCooking"))
                ret.Add("Gus", new() { Display = Skills.GetSkill("blueberry.LoveOfCooking.CookingSkill").GetName(), Indicator = "blueberry.LoveOfCooking.CookingSkill" });
            else if (Helper.ModRegistry.IsLoaded("spacechase0.CookingSkill"))
                ret.Add("Gus", new() { Display = Skills.GetSkill("Cooking").GetName(), Indicator = "Cooking" });

            if (Helper.ModRegistry.IsLoaded("drbirbdev.SocializingSkill"))
                ret.Add("Haley", new() { Display = Skills.GetSkill("drbirbdev.Socializing").GetName(), Indicator = "drbirbdev.Socializing" });
            
            if (Helper.ModRegistry.IsLoaded("drbirbdev.BinningSkill"))
                ret.Add("Linus", new() { Display = Skills.GetSkill("drbirbdev.Binning").GetName(), Indicator = "drbirbdev.Binning" });
            
            return ret;
        }
    }
}
