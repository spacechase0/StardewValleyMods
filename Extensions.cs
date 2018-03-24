using Microsoft.Xna.Framework;
using Magic.Spells;
using StardewValley;
using System;
using static Magic.Mod;
using SFarmer = StardewValley.Farmer;

namespace Magic
{
    public static class Extensions
    {
        public static int getCurrentMana(this SFarmer player)
        {
            if (player != Game1.player || Data == null)
                return 0;

            return Data.mana;
        }

        public static void addMana(this SFarmer player, int amt)
        {
            if (player != Game1.player || Data == null)
                return;

            Data.mana = Math.Max(0, Math.Min(player.getCurrentMana() + amt, player.getMaxMana()));
        }

        public static int getMaxMana(this SFarmer player)
        {
            if (player != Game1.player || Data == null)
                return 0;

            return Data.manaCap;
        }

        public static void setMaxMana(this SFarmer player, int newCap )
        {
            if (player != Game1.player || Data == null)
                return;

            Data.manaCap = newCap;
        }

        public static int getMagicLevel(this SFarmer player)
        {
            if (player != Game1.player || Data == null)
                return 0;

            return Data.magicLevel;
        }

        public static int getMagicExp(this SFarmer player)
        {
            if (player != Game1.player || Data == null)
                return 0;

            return Data.magicExp;
        }

        public static void addMagicExp(this SFarmer player, int exp)
        {
            if (player != Game1.player || Data == null)
                return;

            if (Data.magicLevel >= 50)
                return;

            Data.magicExp += exp;
            
            while (Data.magicExp >= player.getMagicExpForNextLevel() )
            {
                Data.magicExp -= player.getMagicExpForNextLevel();
                Data.magicLevel++;
                if ( Data.magicLevel % 2 == 1 )
                    Data.freePoints++;
                player.setMaxMana(player.getMagicLevel() == 1 ? 50 : player.getMaxMana() + 10);
                Magic.newMagicLevels.Add(Data.magicLevel);
            }
        }

        public static int getMagicExpForNextLevel(this SFarmer player)
        {
            if (player != Game1.player || Data == null)
                return 50;

            return 50 + Data.magicLevel * 50;
        }

        public static int getFreeSpellPoints(this SFarmer player)
        {
            if (player != Game1.player || Data == null)
                return 0;

            return Data.freePoints;
        }

        public static void useSpellPoints(this SFarmer player, int amt)
        {
            if (player != Game1.player || Data == null)
                return;
            Data.freePoints -= amt;
        }

        public static SpellBook getSpellBook(this SFarmer player)
        {
            if (player != Game1.player || Data == null)
                return null;
            return Data.spellBook;
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
                useSpellPoints(player, diff);
            player.getSpellBook().knownSpells[spellId] = level;
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
            useSpellPoints(player, -diff);
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

        public static void castSpell(this SFarmer player, string spellId, int level)
        {
            Point pos = new Point(Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y);
            SpellBook.get(spellId).onCast(player, level, pos.X, pos.Y);
        }

        public static void castSpell(this SFarmer player, Spell spell, int level)
        {
            Point pos = new Point(Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y);
            spell.onCast(player, level, pos.X, pos.Y);
        }
    }
}
