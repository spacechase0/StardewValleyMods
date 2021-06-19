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
            int curLevel = player.KnowsSpellLevel(spellId);
            return curLevel > -1 && curLevel >= level;
        }

        public static bool KnowsSpell(this Farmer player, Spell spell, int level)
        {
            return player.KnowsSpell(spell.FullId, level);
        }

        public static int KnowsSpellLevel(this Farmer player, string spellId)
        {
            return player == Game1.player && Mod.Data != null && player.GetSpellBook().KnownSpells.TryGetValue(spellId, out int level)
                ? level
                : -1;
        }

        public static void LearnSpell(this Farmer player, string spellId, int level, bool free = false)
        {
            int known = player.KnowsSpellLevel(spellId);
            int diff = level - known;

            if (diff <= 0 || player.GetFreeSpellPoints() < diff && !free)
                return;

            Log.Debug($"Learning spell {spellId}, level {level + 1}");
            if (!free)
                player.UseSpellPoints(diff, false);
            player.GetSpellBook().KnownSpells[spellId] = level;

            Mod.Data.SyncMineFull();
        }

        public static void LearnSpell(this Farmer player, Spell spell, int level, bool free = false)
        {
            player.LearnSpell(spell.FullId, level, free);
        }

        public static void ForgetSpell(this Farmer player, string spellId, int level, bool sync = true)
        {
            int known = player.KnowsSpellLevel(spellId);
            if (level > known)
                return;
            int diff = (known + 1) - level;

            Log.Debug($"Forgetting spell {spellId}, level {level + 1}");
            if (level == 0)
                Game1.player.GetSpellBook().KnownSpells.Remove(spellId);
            else if (Game1.player.GetSpellBook().KnownSpells[spellId] >= level)
                Game1.player.GetSpellBook().KnownSpells[spellId] = level - 1;
            player.UseSpellPoints(-diff, false);

            Mod.Data.SyncMineFull();
        }

        public static void ForgetSpell(this Farmer player, Spell spell, int level, bool sync = true)
        {
            player.ForgetSpell(spell.FullId, level, sync);
        }

        public static bool CanCastSpell(this Farmer player, Spell spell, int level)
        {
            return spell.CanCast(player, level);
        }

        public static IActiveEffect CastSpell(this Farmer player, string spellId, int level, int x = int.MinValue, int y = int.MinValue)
        {
            return player.CastSpell(SpellBook.Get(spellId), level, x, y);
        }

        public static IActiveEffect CastSpell(this Farmer player, Spell spell, int level, int x = int.MinValue, int y = int.MinValue)
        {
            if (player == Game1.player)
            {
                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);
                writer.Write(spell.FullId);
                writer.Write(level);
                writer.Write(Game1.getMouseX() + Game1.viewport.X);
                writer.Write(Game1.getMouseY() + Game1.viewport.Y);
                SpaceCore.Networking.BroadcastMessage(Magic.MsgCast, stream.ToArray());
            }
            Point pos = new Point(x, y);
            if (x == int.MinValue && y == int.MinValue)
                pos = new Point(Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y);

            return spell.OnCast(player, level, pos.X, pos.Y);
        }
    }
}
