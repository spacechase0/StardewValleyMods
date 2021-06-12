using System.IO;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;

namespace SpaceCore
{
    /*
    [XmlType( "Mods_Test" )]
    public class TestObject : StardewValley.Object
    {
        public TestObject()
            : base(74, 1)
        {
            this.Quality = 4;
        }
        public override string DisplayName { get => "Test Custom Object"; }
    }
    //*/

    public static class Commands
    {
        internal static void register()
        {
            Command.register("player_giveexp", Commands.expCommand);
            Command.register("asset_invalidate", Commands.invalidateCommand);
            Command.register("exttilesheets_dump", Commands.dumpTilesheetsCommand);
            //Command.register( "test", ( args ) => Game1.player.addItemByMenuIfNecessary( new TestObject() ) );
            //SpaceCore.modTypes.Add( typeof( TestObject ) );
        }

        private static void expCommand(string[] args)
        {
            if (args.Length != 2)
            {
                Log.info("Usage: player_giveexp <skill> <amt>");
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
                    Log.info("No such skill exists");
                }
                else
                {
                    Game1.player.AddCustomSkillExperience(skill, amt);
                }
            }
        }

        private static void invalidateCommand(string[] args)
        {
            if (args.Length == 0)
            {
                Log.info("Usage: asset_invalidate <asset1> [asset2] [...]");
            }

            foreach (string arg in args)
            {
                SpaceCore.instance.Helper.Content.InvalidateCache(arg);
            }
        }

        private static void dumpTilesheetsCommand(string[] args)
        {
            foreach (var asset in TileSheetExtensions.extendedTextureAssets)
            {
                Log.info($"Dumping for asset {asset.Key} (has {asset.Value.Extensions.Count} extensions)");
                Stream stream = File.OpenWrite(Path.GetFileNameWithoutExtension(asset.Key) + "-0.png");
                var tex = Game1.content.Load<Texture2D>(asset.Key);
                tex.SaveAsPng(stream, tex.Width, tex.Height);
                stream.Close();

                for (int i = 0; i < asset.Value.Extensions.Count; ++i)
                {
                    Log.info("\tDumping extended " + (i + 1));
                    stream = File.OpenWrite(Path.GetFileNameWithoutExtension(asset.Key) + $"-{i + 1}.png");
                    tex = asset.Value.Extensions[i];
                    tex.SaveAsPng(stream, tex.Width, tex.Height);
                    stream.Close();
                }
            }
        }
    }
}
