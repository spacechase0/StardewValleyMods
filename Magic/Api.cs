using System;
using System.Linq;
using Magic.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace Magic
{
    /// <inheritdoc />
    public class Api : IApi
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public event EventHandler OnAnalyzeCast;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public void ResetProgress(IManifest manifest)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            Mod.Instance.Monitor.Log($"{manifest.Name} reset the current player's magic progress.", LogLevel.Info);

            SpellBook book = Game1.player.GetSpellBook();
            foreach (var spell in book.KnownSpells.Values.ToArray())
            {
                if (spell.Level > 0)
                    book.ForgetSpell(spell.SpellId, 1);
            }
            book.UseSpellPoints(book.FreePoints);
        }

        /// <inheritdoc />
        public void UseSpellPoints(IManifest manifest, int count)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            Mod.Instance.Monitor.Log($"{manifest.Name} {(count < 0 ? "restored" : "used")} {count} spell points on behalf of the current player.");

            SpellBook book = Game1.player.GetSpellBook();
            book.UseSpellPoints(-count);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raise the <see cref="OnAnalyzeCast"/> event.</summary>
        /// <param name="player">The player who cast the analyze spell.</param>
        internal void InvokeOnAnalyzeCast(Farmer player)
        {
            Log.Trace("Event: OnAnalyzeCast");
            if (this.OnAnalyzeCast == null)
                return;
            Util.InvokeEvent("Magic.Api.OnAnalyzeCast", this.OnAnalyzeCast.GetInvocationList(), player);
        }
    }
}
