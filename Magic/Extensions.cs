using System.IO;
using Magic.Spells;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewValley;

namespace Magic
{
    public static class Extensions
    {
        private static void dataCheck(Farmer player)
        {
            if (!Mod.Data.players.ContainsKey(player.UniqueMultiplayerID))
                Mod.Data.players.Add(player.UniqueMultiplayerID, new MultiplayerSaveData.PlayerData());
        }

        public static int getCurrentMana(this Farmer player)
        {
            return Mod.mana.GetMana(player);
        }

        public static void addMana(this Farmer player, int amt)
        {
            Mod.mana.AddMana(player, amt);
        }

        public static int getMaxMana(this Farmer player)
        {
            return Mod.mana.GetMaxMana(player);
        }

        public static void setMaxMana(this Farmer player, int newCap)
        {
            Mod.mana.SetMaxMana(player, newCap);
        }

        public static int getFreeSpellPoints(this Farmer player)
        {
            Extensions.dataCheck(player);
            return Mod.Data.players[player.UniqueMultiplayerID].freePoints;
        }

        public static void useSpellPoints(this Farmer player, int amt, bool sync = true)
        {
            Extensions.dataCheck(player);
            Mod.Data.players[player.UniqueMultiplayerID].freePoints -= amt;
            if (player == Game1.player)
                Mod.Data.syncMineFull();
        }

        public static SpellBook getSpellBook(this Farmer player)
        {
            Extensions.dataCheck(player);
            return Mod.Data.players[player.UniqueMultiplayerID].spellBook;
        }

        public static bool knowsSpell(this Farmer player, string spellId, int level)
        {
            if (player != Game1.player || Mod.Data == null)
                return false;
            return player.getSpellBook().knownSpells.ContainsKey(spellId) &&
                   player.getSpellBook().knownSpells[spellId] >= level;
        }

        public static bool knowsSpell(this Farmer player, Spell spell, int level)
        {
            return Extensions.knowsSpell(player, spell.FullId, level);
        }

        public static int knowsSpellLevel(this Farmer player, string spellId)
        {
            if (player != Game1.player || Mod.Data == null)
                return -1;
            if (!player.getSpellBook().knownSpells.ContainsKey(spellId))
                return -1;
            return player.getSpellBook().knownSpells[spellId];
        }

        public static int knowsSpellLevel(this Farmer player, Spell spell)
        {
            return Extensions.knowsSpellLevel(player, spell.FullId);
        }

        public static void learnSpell(this Farmer player, string spellId, int level, bool free = false)
        {
            int known = Extensions.knowsSpellLevel(player, spellId);
            int diff = level - known;

            if (diff <= 0 || Extensions.getFreeSpellPoints(player) < diff && !free)
                return;

            Log.debug($"Learning spell {spellId}, level {level + 1}");
            if (!free)
                Extensions.useSpellPoints(player, diff, false);
            player.getSpellBook().knownSpells[spellId] = level;

            Mod.Data.syncMineFull();
        }

        public static void learnSpell(this Farmer player, Spell spell, int level, bool free = false)
        {
            Extensions.learnSpell(player, spell.FullId, level, free);
        }

        public static void forgetSpell(this Farmer player, string spellId, int level, bool sync = true)
        {
            int known = Extensions.knowsSpellLevel(player, spellId);
            if (level > known)
                return;
            int diff = (known + 1) - level;

            Log.debug($"Forgetting spell {spellId}, level {level + 1}");
            if (level == 0)
                Game1.player.getSpellBook().knownSpells.Remove(spellId);
            else if (Game1.player.getSpellBook().knownSpells[spellId] >= level)
                Game1.player.getSpellBook().knownSpells[spellId] = level - 1;
            Extensions.useSpellPoints(player, -diff, false);

            Mod.Data.syncMineFull();
        }

        public static void forgetSpell(this Farmer player, Spell spell, int level, bool sync = true)
        {
            Extensions.forgetSpell(player, spell.FullId, level, sync);
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
            return Extensions.castSpell(player, SpellBook.get(spellId), level, x, y);
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
                    writer.Write(Game1.getMouseX() + Game1.viewport.X);
                    writer.Write(Game1.getMouseY() + Game1.viewport.Y);
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
