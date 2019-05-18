using Microsoft.Xna.Framework;
using Magic.Spells;
using StardewValley;
using System;
using static Magic.Mod;
using System.IO;

namespace Magic
{
    public static class Extensions
    {
        private static void dataCheck( Farmer player )
        {
            if (!Data.players.ContainsKey(player.UniqueMultiplayerID))
                Data.players.Add(player.UniqueMultiplayerID, new MultiplayerSaveData.PlayerData());
        }

        public static int getCurrentMana(this Farmer player)
        {
            dataCheck(player);
            return Data.players[ player.UniqueMultiplayerID ].mana;
        }

        public static void addMana(this Farmer player, int amt)
        {
            dataCheck(player);
            Data.players[player.UniqueMultiplayerID].mana = Math.Max(0, Math.Min(player.getCurrentMana() + amt, player.getMaxMana()));
            if (player == Game1.player)
                Data.syncMineMini();
        }

        public static int getMaxMana(this Farmer player)
        {
            dataCheck(player);
            return Data.players[player.UniqueMultiplayerID].manaCap;
        }

        public static void setMaxMana(this Farmer player, int newCap )
        {
            dataCheck(player);
            Data.players[player.UniqueMultiplayerID].manaCap = newCap;
            if (player == Game1.player)
                Data.syncMineMini();
        }

        public static int getFreeSpellPoints(this Farmer player)
        {
            dataCheck(player);
            return Data.players[player.UniqueMultiplayerID].freePoints;
        }

        public static void useSpellPoints(this Farmer player, int amt, bool sync = true)
        {
            dataCheck(player);
            Data.players[player.UniqueMultiplayerID].freePoints -= amt;
            if (player == Game1.player)
                Data.syncMineFull();
        }

        public static SpellBook getSpellBook(this Farmer player)
        {
            dataCheck(player);
            return Data.players[player.UniqueMultiplayerID].spellBook;
        }

        public static bool knowsSpell(this Farmer player, string spellId, int level)
        {
            if (player != Game1.player || Data == null)
                return false;
            return player.getSpellBook().knownSpells.ContainsKey(spellId) &&
                   player.getSpellBook().knownSpells[spellId] >= level;
        }

        public static bool knowsSpell(this Farmer player, Spell spell, int level)
        {
            return knowsSpell(player, spell.FullId, level);
        }

        public static int knowsSpellLevel(this Farmer player, string spellId)
        {
            if (player != Game1.player || Data == null)
                return -1;
            if (!player.getSpellBook().knownSpells.ContainsKey(spellId))
                return -1;
            return player.getSpellBook().knownSpells[spellId];
        }

        public static int knowsSpellLevel(this Farmer player, Spell spell)
        {
            return knowsSpellLevel(player, spell.FullId);
        }

        public static void learnSpell(this Farmer player, string spellId, int level, bool free = false)
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

        public static void learnSpell(this Farmer player, Spell spell, int level, bool free = false)
        {
            learnSpell(player, spell.FullId, level, free);
        }

        public static void forgetSpell(this Farmer player, string spellId, int level, bool sync = true)
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

        public static void forgetSpell(this Farmer player, Spell spell, int level, bool sync = true)
        {
            forgetSpell(player, spell.FullId, level, sync);
        }

        public static bool canCastSpell(this Farmer player, string spellId, int level)
        {
            return SpellBook.get(spellId).canCast(player, level);
        }
        
        public static bool canCastSpell(this Farmer player, Spell spell, int level)
        {
            return spell.canCast(player, level);
        }

        public static IActiveEffect castSpell(this Farmer player, string spellId, int level, int x = int.MinValue, int y = int.MinValue)
        {
            return castSpell(player, SpellBook.get(spellId), level, x, y);
        }

        public static IActiveEffect castSpell(this Farmer player, Spell spell, int level, int x = int.MinValue, int y = int.MinValue)
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

            return spell.onCast(player, level, pos.X, pos.Y);
        }
    }
}
