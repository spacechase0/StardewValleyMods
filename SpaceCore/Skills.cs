using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
        private static IDataHelper DataApi => SpaceCore.Instance.Helper.Data;

        public abstract class Skill
        {
            public abstract class Profession
            {
                public Skill Skill { get; }
                public string Id { get; }

                public Texture2D Icon { get; set; }
                public abstract string GetName();
                public abstract string GetDescription();

                public int GetVanillaId()
                {
                    return this.Skill.Id.GetDeterministicHashCode() ^ this.Id.GetDeterministicHashCode(); // TODO: Something better
                }

                public virtual void DoImmediateProfessionPerk() { }
                public virtual void UndoImmediateProfessionPerk() { }

                protected Profession(Skill skill, string id)
                {
                    this.Skill = skill;
                    this.Id = id;
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

            public string Id { get; }
            public abstract string GetName();
            public Texture2D Icon { get; set; }
            public Texture2D SkillsPageIcon { get; set; }

            public IList<Profession> Professions { get; } = new List<Profession>();

            public int[] ExperienceCurve { get; set; }
            public IList<ProfessionPair> ProfessionsForLevels { get; } = new List<ProfessionPair>();

            public Color ExperienceBarColor { get; set; }

            public virtual IDictionary<int, IList<string>> GetSkillLevelUpCraftingRecipes(int level)
            {
                return new Dictionary<int, IList<string>>();
            }

            public virtual IDictionary<int, IList<string>> GetSkillLevelUpCookingRecipes(int level)
            {
                return new Dictionary<int, IList<string>>();
            }


            public virtual List<string> GetExtraLevelUpInfo(int level)
            {
                return new();
            }

            public virtual string GetSkillPageHoverText(int level)
            {
                return "";
            }

            public virtual void DoLevelPerk(int level) { }

            public virtual bool ShouldShowOnSkillsPage => true;

            protected Skill(string id)
            {
                this.Id = id;
            }
        }

        public class SkillBuff : Buff
        {
            public Dictionary<string, int> SkillLevelIncreases { get; set; } = new Dictionary<string, int>();

            private const string SkillBuffField = "spacechase.SpaceCore.SkillBuff.";

            public SkillBuff(Buff buff, string id, Dictionary<string, string> customFields) : base(id, buff.source, buff.displaySource, buff.millisecondsDuration, buff.iconTexture, buff.iconSheetIndex, buff.effects, false, buff.displayName, buff.description)
            {
                foreach (var entry in ParseCustomFields(customFields))
                {
                    SkillLevelIncreases[entry.Key] = entry.Value;
                }
            }

            public static IEnumerable<KeyValuePair<string, int>> ParseCustomFields(Dictionary<string, string> customFields)
            {
                foreach (KeyValuePair<string, string> entry in customFields)
                {
                    if (!entry.Key.StartsWith(SkillBuffField))
                    {
                        continue;
                    }

                    string skillId = entry.Key.Substring(SkillBuffField.Length);
                    if (!int.TryParse(entry.Value, out int level))
                    {
                        Log.Error($"Could not parse int {entry.Value} from buff custom field {entry.Key}");
                        continue;
                    }

                    yield return KeyValuePair.Create(skillId, level);
                }
            }

            public override void OnAdded()
            {
                base.OnAdded();

                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);
                writer.Write(this.id);
                writer.Write(this.SkillLevelIncreases.Count);
                foreach (var skill in this.SkillLevelIncreases)
                {
                    ValidateSkill(Game1.player, skill.Key);
                    writer.Write(skill.Key);
                    writer.Write(skill.Value);
                    Skills.Buffs[Game1.player.UniqueMultiplayerID][skill.Key][this.id] = skill.Value;
                    Log.Info($"Adding buff for skill {skill.Key} from source {this.id} at level {skill.Value}");
                }

                Networking.BroadcastMessage(Skills.MsgExperience, stream.ToArray());
            }

            public override void OnRemoved()
            {
                base.OnRemoved();

                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);
                writer.Write(this.id);
                writer.Write(this.SkillLevelIncreases.Count);
                foreach (var skill in this.SkillLevelIncreases)
                {
                    ValidateSkill(Game1.player, skill.Key);
                    writer.Write(skill.Key);
                    writer.Write(0);
                    Skills.Buffs[Game1.player.UniqueMultiplayerID][skill.Key].Remove(this.id);
                    Log.Info($"Removing buff for skill {skill.Key} from source {this.id} at level {skill.Value}");
                }

                Networking.BroadcastMessage(Skills.MsgExperience, stream.ToArray());
            }
        }

        private static readonly string DataKey = "skills";
        private static string LegacyFilePath => Path.Combine(Constants.CurrentSavePath, "spacecore-skills.json");
        private const string MsgData = "spacechase0.SpaceCore.SkillData";
        private const string MsgExperience = "spacechase0.SpaceCore.SkillExperience";
        private const string MsgBuffs = "spacechase0.SpaceCore.SkillBuffs";

        internal static Dictionary<string, Skill> SkillsByName = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<long, Dictionary<string, int>> Exp = new();
        // MultiplayerID => SkillID => BuffID => Level
        private static Dictionary<long, Dictionary<string, Dictionary<string, int>>> Buffs = new();
        internal static List<KeyValuePair<string, int>> NewLevels = new();

        private static IExperienceBarsApi? BarsApi;

        internal static void Init(IModEvents events)
        {
            events.GameLoop.SaveLoaded += Skills.OnSaveLoaded;
            events.GameLoop.Saving += Skills.OnSaving;
            events.GameLoop.Saved += Skills.OnSaved;
            events.Display.MenuChanged += Skills.OnMenuChanged;
            SpaceEvents.ShowNightEndMenus += Skills.ShowLevelMenu;
            SpaceEvents.ServerGotClient += Skills.ClientJoined;
            Networking.RegisterMessageHandler(Skills.MsgData, Skills.OnDataMessage);
            Networking.RegisterMessageHandler(Skills.MsgExperience, Skills.OnExpMessage);
            Networking.RegisterMessageHandler(Skills.MsgBuffs, Skills.OnBuffsMessage);

            if (SpaceCore.Instance.Helper.ModRegistry.IsLoaded("cantorsdust.AllProfessions"))
                events.Player.Warped += Skills.OnWarped;

            BarsApi = SpaceCore.Instance.Helper.ModRegistry.GetApi<IExperienceBarsApi>("spacechase0.ExperienceBars");
            if (BarsApi is not null)
                events.Display.RenderedHud += Skills.OnRenderedHud;
        }

        public static void RegisterSkill(Skill skill)
        {
            Skills.SkillsByName.Add(skill.Id, skill);

            GameStateQuery.Register("PLAYER_" + skill.Id.ToUpper() + "_LEVEL", (args, ctx) =>
            {
                return GameStateQuery.Helpers.PlayerSkillLevelImpl(args, ctx.Player, (f) => f.GetCustomSkillLevel(skill));
            });
        }

        public static Skill GetSkill(string name)
        {
            if (Skills.SkillsByName.TryGetValue(name, out Skill found))
                return found;

            foreach (var skill in Skills.SkillsByName)
            {
                if (skill.Key.Equals(name, StringComparison.OrdinalIgnoreCase)
                    || skill.Value.GetName().Equals(name, StringComparison.OrdinalIgnoreCase))
                    return skill.Value;
            }

            return null;
        }

        public static string[] GetSkillList()
        {
            return Skills.SkillsByName.Keys.ToArray();
        }

        public static List<Tuple<string, int, int>> GetExperienceAndLevels(Farmer farmer)
        {
            List<Tuple<string, int, int>> forFarmer = new();
            if (!Skills.Exp.ContainsKey(farmer.UniqueMultiplayerID))
                return forFarmer;

            forFarmer.AddRange(
                from skillName in Skills.Exp[farmer.UniqueMultiplayerID].Keys
                let xp = Skills.GetExperienceFor(farmer, skillName)
                let level = Skills.GetSkillLevel(farmer, skillName)
                select new Tuple<string, int, int>(skillName, xp, level)
                );

            return forFarmer;
        }

        public static int GetExperienceFor(Farmer farmer, string skillName)
        {
            if (!Skills.SkillsByName.ContainsKey(skillName))
                return 0;

            Skills.ValidateSkill(farmer, skillName);

            return Skills.Exp[farmer.UniqueMultiplayerID][skillName];
        }

        public static int GetSkillLevel(Farmer farmer, string skillName)
        {
            if (!Skills.SkillsByName.ContainsKey(skillName))
                return 0;
            Skills.ValidateSkill(farmer, skillName);

            var skill = Skills.SkillsByName[skillName];
            for (int i = skill.ExperienceCurve.Length - 1; i >= 0; --i)
            {
                if (Skills.GetExperienceFor(farmer, skillName) >= skill.ExperienceCurve[i])
                {
                    return i + 1;
                }
            }

            return 0;
        }

        public static int GetSkillBuffLevel(Farmer farmer, string skillName, string? buffName = null)
        {
            if (!Skills.SkillsByName.ContainsKey(skillName))
            {
                return 0;
            }
            Skills.ValidateSkill(farmer, skillName);

            if (buffName is not null)
            {
                if (!Skills.Buffs[farmer.UniqueMultiplayerID][skillName].TryGetValue(buffName, out int level))
                {
                    level = 0;
                }
                return level;
            }

            int totalLevel = 0;
            foreach (var buff in Skills.Buffs[farmer.UniqueMultiplayerID][skillName])
            {
                totalLevel += buff.Value;
            }
            return totalLevel;
        }

        public static void AddExperience(Farmer farmer, string skillName, int amt)
        {
            if (!Skills.SkillsByName.ContainsKey(skillName))
                return;
            Skills.ValidateSkill(farmer, skillName);

            int prevLevel = Skills.GetSkillLevel(farmer, skillName);
            Skills.Exp[farmer.UniqueMultiplayerID][skillName] += amt;
            if (farmer == Game1.player && prevLevel != Skills.GetSkillLevel(farmer, skillName))
                for (int i = prevLevel + 1; i <= Skills.GetSkillLevel(farmer, skillName); ++i)
                    Skills.NewLevels.Add(new KeyValuePair<string, int>(skillName, i));

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(skillName);
            writer.Write(Skills.Exp[farmer.UniqueMultiplayerID][skillName]);
            Networking.BroadcastMessage(Skills.MsgExperience, stream.ToArray());
        }

        private static void ValidateSkill(Farmer farmer, string skillName)
        {
            ValidateSkill(farmer.UniqueMultiplayerID, skillName);
        }

        private static void ValidateSkill(long uniqueMultiplayerId, string skillName)
        {
            if (!Skills.Exp.TryGetValue(uniqueMultiplayerId, out var skillExp))
            {
                skillExp = new();
                Skills.Exp.Add(uniqueMultiplayerId, skillExp);
            }
            if (!Skills.Buffs.TryGetValue(uniqueMultiplayerId, out var skillBuffs))
            {
                skillBuffs = new();
                Skills.Buffs.Add(uniqueMultiplayerId, skillBuffs);
            }

            _ = skillExp.TryAdd(skillName, 0);
            _ = skillBuffs.TryAdd(skillName, new());
        }

        private static void ClientJoined(object sender, EventArgsServerGotClient args)
        {
            foreach (var skill in Skills.SkillsByName)
            {
                ValidateSkill(args.FarmerID, skill.Key);
            }

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(Skills.Exp.Count);
            foreach (var data in Skills.Exp)
            {
                writer.Write(data.Key);
                writer.Write(data.Value.Count);
                foreach (var skill in data.Value)
                {
                    writer.Write(skill.Key);
                    writer.Write(skill.Value);
                }
            }

            writer.Write(Skills.Buffs.Count);
            foreach (var data in Skills.Buffs)
            {
                writer.Write(data.Key);
                writer.Write(data.Value.Count);
                foreach (var skill in data.Value)
                {
                    writer.Write(skill.Key);
                    writer.Write(skill.Value.Count);
                    foreach (var buff in skill.Value)
                    {
                        writer.Write(buff.Key);
                        writer.Write(buff.Value);
                    }
                }
            }

            Log.Trace("Sending skill data to " + args.FarmerID);
            Networking.ServerSendTo(args.FarmerID, Skills.MsgData, stream.ToArray());
        }

        private static void OnBuffsMessage(IncomingMessage msg)
        {
            string buffId = msg.Reader.ReadString();
            int count = msg.Reader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                string skill = msg.Reader.ReadString();
                int level = msg.Reader.ReadInt32();

                if (level == 0)
                {
                    Skills.Buffs[msg.FarmerID][skill].Remove(buffId);
                }
                else
                {
                    Skills.Buffs[msg.FarmerID][skill][buffId] = level;
                }
            }
        }

        private static void OnExpMessage(IncomingMessage msg)
        {
            Skills.Exp[msg.FarmerID][msg.Reader.ReadString()] = msg.Reader.ReadInt32();
        }

        private static void OnDataMessage(IncomingMessage msg)
        {
            Log.Trace("Got experience data!");
            int count = msg.Reader.ReadInt32();
            for (int ie = 0; ie < count; ++ie)
            {
                long id = msg.Reader.ReadInt64();
                Log.Trace("\t" + id + ":");
                int count2 = msg.Reader.ReadInt32();
                for (int isk = 0; isk < count2; ++isk)
                {
                    string skill = msg.Reader.ReadString();
                    int amt = msg.Reader.ReadInt32();

                    if (!Skills.Exp.TryGetValue(id, out var skillExp))
                    {
                        skillExp = new();
                        Skills.Exp.Add(id, skillExp);
                    }
                    skillExp[skill] = amt;
                    Log.Trace($"\t{skill}={amt}");
                }
            }

            Log.Trace("Got buff data!");
            int playerCount = msg.Reader.ReadInt32();
            for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
            {
                long playerId = msg.Reader.ReadInt64();
                Log.Trace($"\t{playerId}:");
                int skillCount = msg.Reader.ReadInt32();
                for (int skillIndex = 0; skillIndex < skillCount; ++skillIndex)
                {
                    if (!Skills.Buffs.TryGetValue(playerId, out var playerSkills))
                    {
                        playerSkills = new();
                        Skills.Buffs.Add(playerId, playerSkills);
                    }

                    string skillId = msg.Reader.ReadString();
                    Log.Trace($"\t\t{skillId}:");
                    int buffCount = msg.Reader.ReadInt32();

                    for (int buffIndex = 0; buffIndex < buffCount; ++buffIndex)
                    {
                        if (!playerSkills.TryGetValue(skillId, out var playerSkillBuffs))
                        {
                            playerSkillBuffs = new();
                            playerSkills.Add(skillId, playerSkillBuffs);
                        }

                        string buffId = msg.Reader.ReadString();
                        int level = msg.Reader.ReadInt32();
                        playerSkillBuffs[buffId] = level;
                        Log.Trace($"\t\t{buffId}={level}");
                    }
                }
            }
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                Skills.Exp = Skills.DataApi.ReadSaveData<Dictionary<long, Dictionary<string, int>>>(Skills.DataKey);
                if (Skills.Exp == null && File.Exists(Skills.LegacyFilePath))
                    Skills.Exp = JsonConvert.DeserializeObject<Dictionary<long, Dictionary<string, int>>>(File.ReadAllText(Skills.LegacyFilePath));
                Skills.Exp ??= new Dictionary<long, Dictionary<string, int>>();
            }
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnSaving(object sender, SavingEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                Log.Trace("Saving custom data");
                Skills.DataApi.WriteSaveData(Skills.DataKey, Skills.Exp);
            }

            SpaceCore.Instance.Helper.Events.GameLoop.Saved -= OnSaved;
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnSaved(object sender, SavedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                if (File.Exists(Skills.LegacyFilePath))
                {
                    Log.Trace($"Deleting legacy data file at {Skills.LegacyFilePath}");
                    File.Delete(Skills.LegacyFilePath);
                }
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is GameMenu gm)
            {
                if (SpaceCore.Instance.Config.CustomSkillPage && ( Skills.SkillsByName.Count > 0 || SpaceEvents.HasAddWalletItemEventHandlers() ) )
                {
                    //gm.pages[GameMenu.skillsTab] = new NewSkillsPage(gm.xPositionOnScreen, gm.yPositionOnScreen, gm.width + (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru ? 64 : 0), gm.height);
                }
            }
        }

        [SuppressMessage("Reliability", "CA2000", Justification = DiagnosticMessages.DisposableOutlivesScope)]
        private static void ShowLevelMenu(object sender, EventArgsShowNightEndMenus args)
        {
            Log.Debug("Doing skill menus");

            if (Game1.endOfNightMenus.Count == 0)
                Game1.endOfNightMenus.Push(new SaveGameMenu());

            if (Skills.NewLevels.Any())
            {
                for (int i = Skills.NewLevels.Count - 1; i >= 0; --i)
                {
                    string skill = Skills.NewLevels[i].Key;
                    int level = Skills.NewLevels[i].Value;
                    Log.Trace("Doing " + i + ": " + skill + " level " + level + " screen");

                    Game1.endOfNightMenus.Push(new SkillLevelUpMenu(skill, level));
                }
                Skills.NewLevels.Clear();
            }
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        /// <remarks>Used to set all professions for All Professions, matching their code.</remarks>
        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                foreach (var skill in Skills.SkillsByName)
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
        private static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.activeClickableMenu != null || Game1.eventUp)
                return;

            // draw exp bars
            foreach (var skillPair in Skills.SkillsByName)
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

                BarsApi.DrawExperienceBar(skill.Icon ?? Game1.staminaRect, level, progress, skill.ExperienceBarColor);
            }
        }

        internal static Skill.Profession GetProfessionFor(Skill skill, int level)
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
        internal static bool CanRespecAnyCustomSkill()
        {
            foreach (string s in GetSkillList())
            {
                if (CanRespecCustomSkill(s))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool CanRespecCustomSkill(string skillId)
        {
            if (Game1.player.GetCustomSkillLevel(skillId) < 5)
            {
                return false;
            }
            foreach (KeyValuePair<string, int> newLevel in NewLevels)
            {
                if (newLevel.Key == skillId && newLevel.Value == 5 || newLevel.Value == 10)
                {
                    return false;
                }
            }
            return true;
        }

        internal static List<Response> GetRespecCustomResponses()
        {
            List<Response> responses = new List<Response>();
            foreach (string skill in Skills.GetSkillList())
            {
                if (Skills.CanRespecCustomSkill(skill))
                {
                    responses.Add(new Response(skill, Skills.GetSkill(skill).GetName()));
                }
            }
            return responses;
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

        public static List<Tuple<string, int, int>> GetCustomSkillExperienceAndLevels(this Farmer farmer)
        {
            return Skills.GetExperienceAndLevels(farmer);
        }

        public static int GetCustomBuffedSkillLevel(this Farmer farmer, Skills.Skill skill)
        {
            return Skills.GetSkillLevel(farmer, skill.Id) + Skills.GetSkillBuffLevel(farmer, skill.Id);
        }

        public static int GetCustomBuffedSkillLevel(this Farmer farmer, string skill)
        {
            return Skills.GetSkillLevel(farmer, skill) + Skills.GetSkillBuffLevel(farmer, skill);
        }

        public static int GetCustomSkillBuffAmount(this Farmer farmer, Skills.Skill skill, string buffId = null)
        {
            return Skills.GetSkillBuffLevel(farmer, skill.Id, buffId);
        }

        public static int GetCustomSkillBuffAmount(this Farmer farmer, string skill, string buffId = null)
        {
            return Skills.GetSkillBuffLevel(farmer, skill, buffId);
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
