using System.IO;
using Magic.Framework.Spells;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewValley;

namespace Magic.Framework
{
    internal static class Extensions
    {
        private static void DataCheck(Farmer player)
        {
            if (!Mod.Data.Players.ContainsKey(player.UniqueMultiplayerID))
                Mod.Data.Players.Add(player.UniqueMultiplayerID, new MultiplayerSaveData.PlayerData());
        }

        public static int GetCurrentMana(this Farmer player)
        {
            return Mod.Mana.GetMana(player);
        }

        public static void AddMana(this Farmer player, int amt)
        {
            Mod.Mana.AddMana(player, amt);
        }

        public static int GetMaxMana(this Farmer player)
        {
            return Mod.Mana.GetMaxMana(player);
        }

        public static void SetMaxMana(this Farmer player, int newCap)
        {
            Mod.Mana.SetMaxMana(player, newCap);
        }

        public static int GetFreeSpellPoints(this Farmer player)
        {
            Extensions.DataCheck(player);
            return Mod.Data.Players[player.UniqueMultiplayerID].FreePoints;
        }

        public static void UseSpellPoints(this Farmer player, int amt, bool sync = true)
        {
            Extensions.DataCheck(player);
            Mod.Data.Players[player.UniqueMultiplayerID].FreePoints -= amt;
            if (player == Game1.player)
                Mod.Data.SyncMineFull();
        }

        public static SpellBook GetSpellBook(this Farmer player)
        {
            Extensions.DataCheck(player);
            return Mod.Data.Players[player.UniqueMultiplayerID].SpellBook;
        }

        public static bool KnowsSpell(this Farmer player, string spellId, int level)
        {
            if (player != Game1.player || Mod.Data == null)
                return false;
            return player.GetSpellBook().KnownSpells.ContainsKey(spellId) &&
                   player.GetSpellBook().KnownSpells[spellId] >= level;
        }

        public static bool KnowsSpell(this Farmer player, Spell spell, int level)
        {
            return Extensions.KnowsSpell(player, spell.FullId, level);
        }

        public static int KnowsSpellLevel(this Farmer player, string spellId)
        {
            if (player != Game1.player || Mod.Data == null)
                return -1;
            if (!player.GetSpellBook().KnownSpells.ContainsKey(spellId))
                return -1;
            return player.GetSpellBook().KnownSpells[spellId];
        }

        public static int KnowsSpellLevel(this Farmer player, Spell spell)
        {
            return Extensions.KnowsSpellLevel(player, spell.FullId);
        }

        public static void LearnSpell(this Farmer player, string spellId, int level, bool free = false)
        {
            int known = Extensions.KnowsSpellLevel(player, spellId);
            int diff = level - known;

            if (diff <= 0 || Extensions.GetFreeSpellPoints(player) < diff && !free)
                return;

            Log.Debug($"Learning spell {spellId}, level {level + 1}");
            if (!free)
                Extensions.UseSpellPoints(player, diff, false);
            player.GetSpellBook().KnownSpells[spellId] = level;

            Mod.Data.SyncMineFull();
        }

        public static void LearnSpell(this Farmer player, Spell spell, int level, bool free = false)
        {
            Extensions.LearnSpell(player, spell.FullId, level, free);
        }

        public static void ForgetSpell(this Farmer player, string spellId, int level, bool sync = true)
        {
            int known = Extensions.KnowsSpellLevel(player, spellId);
            if (level > known)
                return;
            int diff = (known + 1) - level;

            Log.Debug($"Forgetting spell {spellId}, level {level + 1}");
            if (level == 0)
                Game1.player.GetSpellBook().KnownSpells.Remove(spellId);
            else if (Game1.player.GetSpellBook().KnownSpells[spellId] >= level)
                Game1.player.GetSpellBook().KnownSpells[spellId] = level - 1;
            Extensions.UseSpellPoints(player, -diff, false);

            Mod.Data.SyncMineFull();
        }

        public static void ForgetSpell(this Farmer player, Spell spell, int level, bool sync = true)
        {
            Extensions.ForgetSpell(player, spell.FullId, level, sync);
        }

        public static bool CanCastSpell(this Farmer player, string spellId, int level)
        {
            return SpellBook.Get(spellId).CanCast(player, level);
        }

        public static bool CanCastSpell(this Farmer player, Spell spell, int level)
        {
            return spell.CanCast(player, level);
        }

        public static IActiveEffect CastSpell(this Farmer player, string spellId, int level, int x = int.MinValue, int y = int.MinValue)
        {
            return Extensions.CastSpell(player, SpellBook.Get(spellId), level, x, y);
        }

        public static IActiveEffect CastSpell(this Farmer player, Spell spell, int level, int x = int.MinValue, int y = int.MinValue)
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
                    SpaceCore.Networking.BroadcastMessage(Magic.MsgCast, stream.ToArray());
                }
            }
            Point pos = new Point(x, y);
            if (x == int.MinValue && y == int.MinValue)
                pos = new Point(Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y);

            return spell.OnCast(player, level, pos.X, pos.Y);
        }
    }
}