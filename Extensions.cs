using Microsoft.Xna.Framework;
using Magic.Spells;
using StardewValley;
using System;
using static Magic.Mod;
using SFarmer = StardewValley.Farmer;
using System.IO;

namespace Magic
{
    public static class Extensions
    {
        private static void dataCheck( SFarmer player )
        {
            if (!Data.players.ContainsKey(player.UniqueMultiplayerID))
                Data.players.Add(player.UniqueMultiplayerID, new MultiplayerSaveData.PlayerData());
        }

        public static int getCurrentMana(this SFarmer player)
        {
            dataCheck(player);
            return Data.players[ player.UniqueMultiplayerID ].mana;
        }

        public static void addMana(this SFarmer player, int amt)
        {
            dataCheck(player);
            Data.players[player.UniqueMultiplayerID].mana = Math.Max(0, Math.Min(player.getCurrentMana() + amt, player.getMaxMana()));
            if (player == Game1.player)
                Data.syncMineMini();
        }

        public static int getMaxMana(this SFarmer player)
        {
            dataCheck(player);
            return Data.players[player.UniqueMultiplayerID].manaCap;
        }

        public static void setMaxMana(this SFarmer player, int newCap )
        {
            dataCheck(player);
            Data.players[player.UniqueMultiplayerID].manaCap = newCap;
            if (player == Game1.player)
                Data.syncMineMini();
        }

        public static int getMagicLevel(this SFarmer player)
        {
            dataCheck(player);
            return Data.players[player.UniqueMultiplayerID].magicLevel;
        }

        public static int getMagicExp(this SFarmer player)
        {
            dataCheck(player);
            return Data.players[player.UniqueMultiplayerID].magicExp;
        }

        public static void addMagicExp(this SFarmer player, int exp)
        {
            dataCheck(player);
            if (Data.players[player.UniqueMultiplayerID].magicLevel >= 50)
                return;

            Data.players[player.UniqueMultiplayerID].magicExp += exp;
            if (player == Game1.player)
                Data.syncMineMini();

            while (Data.players[player.UniqueMultiplayerID].magicExp >= player.getMagicExpForNextLevel() )
            {
                Data.players[player.UniqueMultiplayerID].magicExp -= player.getMagicExpForNextLevel();
                Data.players[player.UniqueMultiplayerID].magicLevel++;
                //if ( Data.magicLevel % 2 == 1 )
                    Data.players[player.UniqueMultiplayerID].freePoints++;
                player.setMaxMana(player.getMagicLevel() == 1 ? 50 : player.getMaxMana() + 10);
                Magic.newMagicLevels.Add(Data.players[player.UniqueMultiplayerID].magicLevel);
                Data.syncMineFull();
            }
        }

        public static int getMagicExpForNextLevel(this SFarmer player)
        {
            dataCheck(player);
            return 50 + Data.players[player.UniqueMultiplayerID].magicLevel * 50;
        }

        public static int getFreeSpellPoints(this SFarmer player)
        {
            dataCheck(player);
            return Data.players[player.UniqueMultiplayerID].freePoints;
        }

        public static void useSpellPoints(this SFarmer player, int amt, bool sync = true)
        {
            dataCheck(player);
            Data.players[player.UniqueMultiplayerID].freePoints -= amt;
            if (player == Game1.player)
                Data.syncMineFull();
        }

        public static SpellBook getSpellBook(this SFarmer player)
        {
            dataCheck(player);
            return Data.players[player.UniqueMultiplayerID].spellBook;
        }

        public static bool knowsSchool(this SFarmer player, string school)
        {
            if (player != Game1.player || Data == null)
                return false;
            return player.getSpellBook().knownSchools.Contains(school);
        }

        public static void learnSchool(this SFarmer player, string school)
        {
            if (!knowsSchool(player, school))
                player.getSpellBook().knownSchools.Add(school);
        }

        public static bool knowsSpell(this SFarmer player, string spellId, int level)
        {
            if (player != Game1.player || Data == null)
                return false;
            return player.getSpellBook().knownSpells.ContainsKey(spellId) &&
                   player.getSpellBook().knownSpells[spellId] >= level;
        }

        public static bool knowsSpell(this SFarmer player, Spell spell, int level)
        {
            return knowsSpell(player, spell.FullId, level);
        }

        public static int knowsSpellLevel(this SFarmer player, string spellId)
        {
            if (player != Game1.player || Data == null)
                return -1;
            if (!player.getSpellBook().knownSpells.ContainsKey(spellId))
                return -1;
            return player.getSpellBook().knownSpells[spellId];
        }

        public static int knowsSpellLevel(this SFarmer player, Spell spell)
        {
            return knowsSpellLevel(player, spell.FullId);
        }

        public static void learnSpell(this SFarmer player, string spellId, int level, bool free = false)
        {
            int known = knowsSpellLevel(player, spellId);
            int diff = level - known;
            
            if (diff <= 0 || getFreeSpellPoints(player) < diff && !free)
                return;

            Log.debug($"Learning spell {spellId}, level {level + 1}");
            if ( !free )
                useSpellPoints(player, diff, false);
            player.getSpellBook().knownSpells[spellId] = level;

            Data.syncMineFull();
        }

        public static void learnSpell(this SFarmer player, Spell spell, int level, bool free = false)
        {
            learnSpell(player, spell.FullId, level, free);
        }

        public static void forgetSpell(this SFarmer player, string spellId, int level)
        {
            int known = knowsSpellLevel(player, spellId);
            if (level > known)
                return;
            int diff = (known + 1) - level;

            Log.debug($"Forgetting spell {spellId}, level {level + 1}");
            if (level == 0)
                Game1.player.getSpellBook().knownSpells.Remove(spellId);
            else if (Game1.player.getSpellBook().knownSpells[spellId] >= level)
                Game1.player.getSpellBook().knownSpells[spellId] = level - 1;
            useSpellPoints(player, -diff, false);

            Data.syncMineFull();
        }

        public static void forgetSpell(this SFarmer player, Spell spell, int level)
        {
            forgetSpell(player, spell.FullId, level);
        }

        public static bool canCastSpell(this SFarmer player, string spellId, int level)
        {
            return SpellBook.get(spellId).canCast(player, level);
        }
        
        public static bool canCastSpell(this SFarmer player, Spell spell, int level)
        {
            return spell.canCast(player, level);
        }

        public static void castSpell(this SFarmer player, string spellId, int level, int x = int.MinValue, int y = int.MinValue)
        {
            castSpell(player, SpellBook.get(spellId), level, x, y);
        }

        public static void castSpell(this SFarmer player, Spell spell, int level, int x = int.MinValue, int y = int.MinValue)
        {
            if (player == Game1.player)
            {
                using (var stream = new MemoryStream())
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(spell.FullId);
                    writer.Write(level);
                    writer.Write((int)(Game1.getMouseX() + Game1.viewport.X));
                    writer.Write((int)(Game1.getMouseY() + Game1.viewport.Y));
                    SpaceCore.Networking.BroadcastMessage(Magic.MSG_CAST, stream.ToArray());
                }
            }
            Point pos = new Point(x, y);
            if (x == int.MinValue && y == int.MinValue)
                pos = new Point(Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y);
            spell.onCast(player, level, pos.X, pos.Y);
        }
    }
}
