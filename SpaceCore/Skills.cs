using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceCore.Events;
using SpaceCore.Interface;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;

namespace SpaceCore
{
    public class Skills
    {
        private static IDataHelper DataApi => SpaceCore.instance.Helper.Data;

        public abstract class Skill
        {
            public abstract class Profession
            {
                public Profession(Skill skill, string id)
                {
                    this.Skill = skill;
                    this.Id = id;
                }

                public Skill Skill { get; }
                public string Id { get; }

                public Texture2D Icon { get; set; }
                public abstract string GetName();
                public abstract string GetDescription();

                public int GetVanillaId()
                {
                    return this.Skill.Id.GetHashCode() ^ this.Id.GetHashCode(); // TODO: Something better
                }

                public virtual void DoImmediateProfessionPerk()
                {
                }
            }

            public class ProfessionPair
            {
                public ProfessionPair(int level, Profession first, Profession second, Profession req = null)
                {
                    this.Level = level;
                    this.First = first;
                    this.Second = second;
                    this.Requires = req;
                }

                public int Level { get; }
                public Profession Requires { get; }
                public Profession First { get; }
                public Profession Second { get; }
            }

            public Skill(string id)
            {
                this.Id = id;
            }

            public string Id { get; }
            public abstract string GetName();
            public Texture2D Icon { get; set; }
            public Texture2D SkillsPageIcon { get; set; }

            public IList<Profession> Professions { get; } = new List<Profession>();

            public int[] ExperienceCurve { get; set; }
            public IList<ProfessionPair> ProfessionsForLevels { get; } = new List<ProfessionPair>();

            public Color ExperienceBarColor { get; set; }

            public virtual List<string> GetExtraLevelUpInfo(int level)
            {
                return new();
            }

            public virtual string GetSkillPageHoverText(int level)
            {
                return "";
            }

            public virtual void DoLevelPerk(int level)
            {
            }
        }

        private static string DataKey = "skills";
        private static string LegacyFilePath => Path.Combine(Constants.CurrentSavePath, "spacecore-skills.json");
        private const string MSG_DATA = "spacechase0.SpaceCore.SkillData";
        private const string MSG_EXPERIENCE = "spacechase0.SpaceCore.SkillExperience";

        internal static Dictionary<string, Skill> skills = new();
        private static Dictionary<long, Dictionary<string, int>> exp = new();
        internal static List<KeyValuePair<string, int>> myNewLevels = new();

        internal static void init(IModEvents events)
        {
            events.GameLoop.SaveLoaded += Skills.onSaveLoaded;
            events.GameLoop.Saving += Skills.onSaving;
            events.GameLoop.Saved += Skills.onSaved;
            events.Display.MenuChanged += Skills.onMenuChanged;
            events.Player.Warped += Skills.onWarped;
            events.Display.RenderedHud += Skills.onRenderedHud;
            SpaceEvents.ShowNightEndMenus += Skills.showLevelMenu;
            SpaceEvents.ServerGotClient += Skills.clientJoined;
            Networking.RegisterMessageHandler(Skills.MSG_DATA, Skills.onDataMessage);
            Networking.RegisterMessageHandler(Skills.MSG_EXPERIENCE, Skills.onExpMessage);
        }

        public static void RegisterSkill(Skill skill)
        {
            Skills.skills.Add(skill.Id, skill);
        }

        public static Skill GetSkill(string name)
        {
            if (Skills.skills.ContainsKey(name))
                return Skills.skills[name];

            foreach (var skill in Skills.skills)
            {
                if (skill.Key.ToLower() == name.ToLower() || skill.Value.GetName().ToLower() == name.ToLower())
                    return skill.Value;
            }

            return null;
        }

        public static string[] GetSkillList()
        {
            return Skills.skills.Keys.ToArray();
        }

        public static int GetExperienceFor(Farmer farmer, string skillName)
        {
            if (!Skills.skills.ContainsKey(skillName))
                return 0;
            Skills.validateSkill(farmer, skillName);

            return Skills.exp[farmer.UniqueMultiplayerID][skillName];
        }

        public static int GetSkillLevel(Farmer farmer, string skillName)
        {
            if (!Skills.skills.ContainsKey(skillName))
                return 0;
            Skills.validateSkill(farmer, skillName);

            var skill = Skills.skills[skillName];
            for (int i = skill.ExperienceCurve.Length - 1; i >= 0; --i)
            {
                if (Skills.GetExperienceFor(farmer, skillName) >= skill.ExperienceCurve[i])
                {
                    return i + 1;
                }
            }

            return 0;
        }

        public static void AddExperience(Farmer farmer, string skillName, int amt)
        {
            if (!Skills.skills.ContainsKey(skillName))
                return;
            Skills.validateSkill(farmer, skillName);

            int prevLevel = Skills.GetSkillLevel(farmer, skillName);
            Skills.exp[farmer.UniqueMultiplayerID][skillName] += amt;
            if (farmer == Game1.player && prevLevel != Skills.GetSkillLevel(farmer, skillName))
                for (int i = prevLevel + 1; i <= Skills.GetSkillLevel(farmer, skillName); ++i)
                    Skills.myNewLevels.Add(new KeyValuePair<string, int>(skillName, i));

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(skillName);
                writer.Write(Skills.exp[farmer.UniqueMultiplayerID][skillName]);
                Networking.BroadcastMessage(Skills.MSG_EXPERIENCE, stream.ToArray());
            }
        }

        private static void validateSkill(Farmer farmer, string skillName)
        {
            if (!Skills.exp.ContainsKey(farmer.UniqueMultiplayerID))
                Skills.exp.Add(farmer.UniqueMultiplayerID, new Dictionary<string, int>());
            if (!Skills.exp[farmer.UniqueMultiplayerID].ContainsKey(skillName))
                Skills.exp[farmer.UniqueMultiplayerID].Add(skillName, 0);
        }

        private static void clientJoined(object sender, EventArgsServerGotClient args)
        {
            if (!Skills.exp.ContainsKey(args.FarmerID))
                Skills.exp.Add(args.FarmerID, new Dictionary<string, int>());
            foreach (var skill in Skills.skills)
                if (!Skills.exp[args.FarmerID].ContainsKey(skill.Key))
                    Skills.exp[args.FarmerID].Add(skill.Key, 0);

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Skills.exp.Count);
                foreach (var data in Skills.exp)
                {
                    writer.Write(data.Key);
                    writer.Write(data.Value.Count);
                    foreach (var skill in data.Value)
                    {
                        writer.Write(skill.Key);
                        writer.Write(skill.Value);
                    }
                }

                var server = (GameServer)sender;
                Log.trace("Sending skill data to " + args.FarmerID);
                Networking.ServerSendTo(args.FarmerID, Skills.MSG_DATA, stream.ToArray());
            }
        }

        private static void onExpMessage(IncomingMessage msg)
        {
            Skills.exp[msg.FarmerID][msg.Reader.ReadString()] = msg.Reader.ReadInt32();
        }

        private static void onDataMessage(IncomingMessage msg)
        {
            Log.trace("Got experience data!");
            int count = msg.Reader.ReadInt32();
            for (int ie = 0; ie < count; ++ie)
            {
                long id = msg.Reader.ReadInt64();
                Log.trace("\t" + id + ":");
                int count2 = msg.Reader.ReadInt32();
                for (int isk = 0; isk < count2; ++isk)
                {
                    string skill = msg.Reader.ReadString();
                    int amt = msg.Reader.ReadInt32();
                    if (!Skills.exp.ContainsKey(id))
                        Skills.exp.Add(id, new Dictionary<string, int>());
                    Skills.exp[id][skill] = amt;
                    Log.trace("\t" + skill + "=" + amt);
                }
            }
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                Skills.exp = Skills.DataApi.ReadSaveData<Dictionary<long, Dictionary<string, int>>>(Skills.DataKey);
                if (Skills.exp == null && File.Exists(Skills.LegacyFilePath))
                    Skills.exp = JsonConvert.DeserializeObject<Dictionary<long, Dictionary<string, int>>>(File.ReadAllText(Skills.LegacyFilePath));
                if (Skills.exp == null)
                    Skills.exp = new Dictionary<long, Dictionary<string, int>>();
            }
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onSaving(object sender, SavingEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                Log.trace("Saving custom data");
                Skills.DataApi.WriteSaveData(Skills.DataKey, Skills.exp);
            }
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onSaved(object sender, SavedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                if (File.Exists(Skills.LegacyFilePath))
                {
                    Log.trace($"Deleting legacy data file at {Skills.LegacyFilePath}");
                    File.Delete(Skills.LegacyFilePath);
                }
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is GameMenu gm)
            {
                if (SpaceCore.instance.Config.CustomSkillPage && Skills.skills.Count > 0)
                {
                    gm.pages[GameMenu.skillsTab] = new NewSkillsPage(gm.xPositionOnScreen, gm.yPositionOnScreen, gm.width + (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru ? 64 : 0), gm.height);
                }
            }
        }
        private static void showLevelMenu(object sender, EventArgsShowNightEndMenus args)
        {
            Log.debug("Doing skill menus");

            if (Game1.endOfNightMenus.Count == 0)
                Game1.endOfNightMenus.Push(new SaveGameMenu());

            if (Skills.myNewLevels.Count() > 0)
            {
                for (int i = Skills.myNewLevels.Count() - 1; i >= 0; --i)
                {
                    string skill = Skills.myNewLevels[i].Key;
                    int level = Skills.myNewLevels[i].Value;
                    Log.trace("Doing " + i + ": " + skill + " level " + level + " screen");

                    Game1.endOfNightMenus.Push(new SkillLevelUpMenu(skill, level));
                }
                Skills.myNewLevels.Clear();
            }
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer && SpaceCore.instance.Helper.ModRegistry.IsLoaded("cantorsdust.AllProfessions"))
            {
                foreach (var skill in Skills.skills)
                {
                    int level = Game1.player.GetCustomSkillLevel(skill.Key);
                    foreach (var profPair in skill.Value.ProfessionsForLevels)
                    {
                        if (level >= profPair.Level)
                        {
                            if (!Game1.player.professions.Contains(profPair.First.GetVanillaId()))
                                Game1.player.professions.Add(profPair.First.GetVanillaId());
                            if (!Game1.player.professions.Contains(profPair.Second.GetVanillaId()))
                                Game1.player.professions.Add(profPair.Second.GetVanillaId());
                        }
                    }
                }
            }
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.activeClickableMenu != null || Game1.eventUp)
                return;

            // draw exp bars
            foreach (var skillPair in Skills.skills)
            {
                var skill = skillPair.Value;
                int level = Game1.player.GetCustomSkillLevel(skillPair.Key);
                int exp = Game1.player.GetCustomSkillExperience(skillPair.Key);

                int prevReq = 0, nextReq = 1;
                if (level == 0)
                {
                    nextReq = skill.ExperienceCurve[0];
                }
                else if (level < skill.ExperienceCurve.Length)
                {
                    prevReq = skill.ExperienceCurve[level - 1];
                    nextReq = skill.ExperienceCurve[level];
                }

                int haveExp = exp - prevReq;
                int needExp = nextReq - prevReq;
                float progress = (float)haveExp / needExp;
                if (level == 10)
                {
                    progress = -1;
                }

                var api = SpaceCore.instance.Helper.ModRegistry.GetApi<ExperienceBarsAPI>("spacechase0.ExperienceBars");
                if (api == null)
                {
                    Log.warn("No experience bars API? Turning off");
                    SpaceCore.instance.Helper.Events.Display.RenderedHud -= Skills.onRenderedHud;
                    return;
                }
                api.DrawExperienceBar(skill.Icon ?? Game1.staminaRect, level, progress, skill.ExperienceBarColor);
            }
        }

        internal static Skill.Profession getProfessionFor(Skill skill, int level)
        {
            foreach (var profPair in skill.ProfessionsForLevels)
            {
                if (level == profPair.Level)
                {
                    if (Game1.player.HasCustomProfession(profPair.First))
                        return profPair.First;
                    else if (Game1.player.HasCustomProfession(profPair.Second))
                        return profPair.Second;
                }
            }

            return null;
        }
    }

    public static class SkillExtensions
    {
        public static int GetCustomSkillExperience(this Farmer farmer, Skills.Skill skill)
        {
            return Skills.GetExperienceFor(farmer, skill.Id);
        }

        public static int GetCustomSkillExperience(this Farmer farmer, string skill)
        {
            return Skills.GetExperienceFor(farmer, skill);
        }

        public static int GetCustomSkillLevel(this Farmer farmer, Skills.Skill skill)
        {
            return Skills.GetSkillLevel(farmer, skill.Id);
        }

        public static int GetCustomSkillLevel(this Farmer farmer, string skill)
        {
            return Skills.GetSkillLevel(farmer, skill);
        }

        public static void AddCustomSkillExperience(this Farmer farmer, Skills.Skill skill, int amt)
        {
            Skills.AddExperience(farmer, skill.Id, amt);
        }

        public static void AddCustomSkillExperience(this Farmer farmer, string skill, int amt)
        {
            Skills.AddExperience(farmer, skill, amt);
        }

        public static bool HasCustomProfession(this Farmer farmer, Skills.Skill.Profession prof)
        {
            return farmer.professions.Contains(prof.GetVanillaId());
        }
    }
}
