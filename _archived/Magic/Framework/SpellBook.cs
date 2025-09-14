using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Magic.Framework.Schools;
using Magic.Framework.Spells;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewValley;

namespace Magic.Framework
{
    /// <summary>A self-updating view of the player's magic metadata.</summary>
    internal class SpellBook
    {
        /*********
        ** Fields
        *********/
        /// <summary>The player's underlying data.</summary>
        private readonly SpellBookData Data;


        /*********
        ** Accessors
        *********/
        /// <summary>The underlying player.</summary>
        public Farmer Player => this.Data.Player;

        /// <summary>The number of spell points available to spend.</summary>
        public int FreePoints => this.GetUpdatedData().FreePoints;

        /// <summary>The player's learned spells.</summary>
        public IDictionary<string, PreparedSpell> KnownSpells => this.GetUpdatedData().KnownSpells;

        /// <summary>The player's spell hotbars.</summary>
        public IList<PreparedSpellBar> Prepared => this.GetUpdatedData().Prepared;

        /// <summary>The currently selected hotbar, as an index in the <see cref="Prepared"/> list.</summary>
        public int SelectedPrepared => this.GetUpdatedData().SelectedPrepared;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="player">The underlying player.</param>
        public SpellBook(Farmer player)
        {
            this.Data = new SpellBookData(player);
        }

        /// <summary>Change the underlying spell data and save when done.</summary>
        /// <param name="mutate">Apply changes to the spell data.</param>
        public void Mutate(Action<SpellBookData> mutate)
        {
            this.Data.UpdateIfNeeded();
            mutate(this.Data);
            this.Data.Save();
        }

        /// <summary>Get the current spell hotbar, if any.</summary>
        public PreparedSpellBar GetPreparedSpells()
        {
            var data = this.GetUpdatedData();

            return data.SelectedPrepared < data.Prepared.Count
                ? data.Prepared[data.SelectedPrepared]
                : null;
        }

        /// <summary>Swap the prepared hotbars.</summary>
        public void SwapPreparedSet()
        {
            var data = this.GetUpdatedData();

            if (data.Prepared.Any())
            {
                data.SelectedPrepared = (data.SelectedPrepared + 1) % data.Prepared.Count;
                data.Save();
            }
        }

        /// <summary>Forget a known spell.</summary>
        /// <param name="spellId">The spell ID.</param>
        /// <param name="level">The level to forget.</param>
        public void ForgetSpell(string spellId, int level)
        {
            var data = this.GetUpdatedData();

            // skip if spell level isn't known
            if (!data.KnownSpells.TryGetValue(spellId, out PreparedSpell spell) || spell.Level < level)
                return;
            Log.Debug($"Forgetting spell {spellId}, level {level + 1}");

            // forget spell
            int diff = (spell.Level + 1) - level;
            if (level == 0)
                data.KnownSpells.Remove(spellId);
            else if (spell.Level >= level)
                spell.Level = level - 1;

            // regain spell points
            data.FreePoints = Math.Max(0, data.FreePoints + diff);

            // save changes
            data.Save();
        }

        /// <summary>Forget a known spell.</summary>
        /// <param name="spell">The spell ID.</param>
        /// <param name="level">The level to forget.</param>
        public void ForgetSpell(Spell spell, int level)
        {
            this.ForgetSpell(spell.FullId, level);
        }

        /// <summary>Reduce the number of spell points by the given number (or increase if negative).</summary>
        /// <param name="points">The number of points.</param>
        public void UseSpellPoints(int points)
        {
            var data = this.GetUpdatedData();

            data.FreePoints = Math.Max(0, data.FreePoints - points);

            data.Save();
        }

        /// <summary>Get whether the player knows a given spell.</summary>
        /// <param name="spellId">The spell ID.</param>
        /// <param name="level">The minimum spell level.</param>
        public bool KnowsSpell(string spellId, int level)
        {
            var data = this.GetUpdatedData();

            return
                data.KnownSpells.TryGetValue(spellId, out PreparedSpell spell)
                && spell?.Level >= level;
        }

        /// <summary>Get whether the player knows a given spell.</summary>
        /// <param name="spell">The spell.</param>
        /// <param name="level">The minimum spell level.</param>
        public bool KnowsSpell(Spell spell, int level)
        {
            return this.KnowsSpell(spell.FullId, level);
        }

        /// <summary>Get whether the player knows any spells in this school.</summary>
        public bool KnowsSchool(School school)
        {
            return school.GetAllSpellTiers()
                .SelectMany(tier => tier)
                .Any(spell => this.KnowsSpell(spell.FullId, 0));
        }

        /// <summary>Add a spell to the player's list of known spells.</summary>
        /// <param name="spellId">The spell ID.</param>
        /// <param name="level">The spell level.</param>
        /// <param name="free">Whether the spell is free, so it shouldn't decrease the available spell points.</param>
        public void LearnSpell(string spellId, int level, bool free = false)
        {
            var data = this.GetUpdatedData();

            // add spell
            if (!data.KnownSpells.TryGetValue(spellId, out PreparedSpell spell))
                data.KnownSpells[spellId] = spell = new PreparedSpell(spellId, 0);

            // upgrade level
            int previousLevel = spell.Level;
            int diff = level - previousLevel;
            if (diff > 0)
            {
                if (!free)
                    data.FreePoints = Math.Max(0, data.FreePoints - diff);
                data.KnownSpells[spellId].Level = level;

                Log.Debug($"Learned spell {spellId}, level {level + 1}");
            }

            // save changes
            data.Save();
        }

        /// <summary>Add a spell to the player's list of known spells.</summary>
        /// <param name="spell">The spell.</param>
        /// <param name="level">The spell level.</param>
        /// <param name="free">Whether the spell is free, so it shouldn't decrease the available spell points.</param>
        public void LearnSpell(Spell spell, int level, bool free = false)
        {
            this.LearnSpell(spell.FullId, level, free);
        }

        /// <summary>Get whether the player can cast the given spell.</summary>
        /// <param name="spell">The spell to cast.</param>
        /// <param name="level">The spell level.</param>
        public bool CanCastSpell(Spell spell, int level)
        {
            return spell.CanCast(this.Player, level);
        }

        /// <summary>Cast a spell.</summary>
        /// <param name="spellId">The spell ID to cast.</param>
        /// <param name="level">The spell level.</param>
        /// <param name="x">The X coordinate on which to cast the spell.</param>
        /// <param name="y">The Y coordinate on which to cast the spell.</param>
        public IActiveEffect CastSpell(string spellId, int level, int x = int.MinValue, int y = int.MinValue)
        {
            return this.CastSpell(SpellManager.Get(spellId), level, x, y);
        }

        /// <summary>Cast a spell.</summary>
        /// <param name="spell">The spell to cast.</param>
        /// <param name="level">The spell level.</param>
        /// <param name="x">The X coordinate on which to cast the spell.</param>
        /// <param name="y">The Y coordinate on which to cast the spell.</param>
        public IActiveEffect CastSpell(Spell spell, int level, int x = int.MinValue, int y = int.MinValue)
        {
            if (this.Player == Game1.player)
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

            return spell.OnCast(this.Player, level, pos.X, pos.Y);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the underlying magic data, updating it if needed.</summary>
        private SpellBookData GetUpdatedData()
        {
            this.Data.UpdateIfNeeded();
            return this.Data;
        }
    }
}
