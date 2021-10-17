using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace SpaceCore.Framework
{
    internal static class Commands
    {
        internal static void Register()
        {
            Command.Register("player_giveexp", Commands.ExpCommand);
            Command.Register("asset_invalidate", Commands.InvalidateCommand);
            Command.Register("exttilesheets_dump", Commands.DumpTilesheetsCommand);
            Command.Register("dump_spacecore_skills", Commands.DumpSkills);
            //Command.register( "test", ( args ) => Game1.player.addItemByMenuIfNecessary( new TestObject() ) );
            //SpaceCore.modTypes.Add( typeof( TestObject ) );
        }

        /// <summary>Show a report of currently registered skills and professions.</summary>
        /// <param name="args">The command arguments.</param>
        private static void DumpSkills(string[] args)
        {
            StringBuilder report = new StringBuilder();

            if (Skills.SkillsByName.Any())
            {
                report.AppendLine("The following custom skills are registered with SpaceCore.");
                report.AppendLine();

                foreach (Skills.Skill skill in Skills.SkillsByName.Values)
                {
                    // base skill info
                    report.AppendLine(skill.Id);
                    report.AppendLine("--------------------------------------------------");
                    if (Context.IsWorldReady)
                    {
                        report.AppendLine($"Level: {Skills.GetSkillLevel(Game1.player, skill.Id)}");
                        report.AppendLine($"Experience: {Skills.GetExperienceFor(Game1.player, skill.Id)}");
                    }

                    // level perks
                    {
                        var perks = Enumerable.Range(1, 10)
                            .Select(level => new { level, perks = skill.GetExtraLevelUpInfo(level) })
                            .Where(p => p.perks.Any())
                            .ToArray();

                        if (perks.Any())
                        {
                            report.AppendLine();
                            report.AppendLine("Level perks:");
                            foreach (var entry in perks)
                                report.AppendLine($"   - level {entry.level}: {string.Join(", ", entry.perks)}");
                        }
                    }

                    // professions
                    if (skill.Professions.Any())
                    {
                        report.AppendLine();
                        report.AppendLine("Professions:");

                        foreach (var profession in skill.Professions)
                        {
                            int vanillaId = profession.GetVanillaId();
                            bool hasProfession = Context.IsWorldReady && Game1.player.professions.Contains(vanillaId);

                            report.AppendLine($"   [{(hasProfession ? "X" : " ")}] {profession.GetName()}");
                            report.AppendLine($"        - ID: {profession.Id} ({vanillaId})");
                            report.AppendLine($"        - Description: {profession.GetDescription()}");
                            report.AppendLine();
                        }
                    }
                    else
                        report.AppendLine();

                    report.AppendLine();
                }
            }
            else
                report.AppendLine("There are no custom skills registered with SpaceCore currently.");

            Log.Info(report.ToString());
        }

        private static void ExpCommand(string[] args)
        {
            if (args.Length != 2)
            {
                Log.Info("Usage: player_giveexp <skill> <amt>");
            }

            string skillName = args[0].ToLower();
            int amt = int.Parse(args[1]);

            if (skillName == "farming") Game1.player.gainExperience(Farmer.farmingSkill, amt);
            else if (skillName == "foraging") Game1.player.gainExperience(Farmer.foragingSkill, amt);
            else if (skillName == "mining") Game1.player.gainExperience(Farmer.miningSkill, amt);
            else if (skillName == "fishing") Game1.player.gainExperience(Farmer.fishingSkill, amt);
            else if (skillName == "combat") Game1.player.gainExperience(Farmer.combatSkill, amt);
            else if (skillName == "luck") Game1.player.gainExperience(Farmer.luckSkill, amt);
            else
            {
                var skill = Skills.GetSkill(skillName);
                if (skill == null)
                {
                    Log.Info("No such skill exists");
                }
                else
                {
                    Game1.player.AddCustomSkillExperience(skill, amt);
                }
            }
        }

        private static void InvalidateCommand(string[] args)
        {
            if (args.Length == 0)
            {
                Log.Info("Usage: asset_invalidate <asset1> [asset2] [...]");
            }

            foreach (string arg in args)
            {
                SpaceCore.Instance.Helper.Content.InvalidateCache(arg);
            }
        }

        private static void DumpTilesheetsCommand(string[] args)
        {
            foreach (var asset in TileSheetExtensions.ExtendedTextureAssets)
            {
                Log.Info($"Dumping for asset {asset.Key} (has {asset.Value.Extensions.Count} extensions)");
                Stream stream = File.OpenWrite(Path.GetFileNameWithoutExtension(asset.Key) + "-0.png");
                var tex = Game1.content.Load<Texture2D>(asset.Key);
                tex.SaveAsPng(stream, tex.Width, tex.Height);
                stream.Close();

                for (int i = 0; i < asset.Value.Extensions.Count; ++i)
                {
                    Log.Info("\tDumping extended " + (i + 1));
                    stream = File.OpenWrite(Path.GetFileNameWithoutExtension(asset.Key) + $"-{i + 1}.png");
                    tex = asset.Value.Extensions[i];
                    tex.SaveAsPng(stream, tex.Width, tex.Height);
                    stream.Close();
                }
            }
        }
    }
}
