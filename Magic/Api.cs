using System;
using System.Collections.Generic;
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

            SpellBook spells = Game1.player.GetSpellBook();
            foreach (var spell in new Dictionary<string, int>(spells.KnownSpells))
            {
                if (spell.Value > 0)
                    Game1.player.ForgetSpell(spell.Key, 1, sync: false);
            }
            Game1.player.UseSpellPoints(10, sync: true);
        }

        /// <inheritdoc />
        public void UseSpellPoints(IManifest manifest, int count)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            Mod.Instance.Monitor.Log($"{manifest.Name} {(count < 0 ? "restored" : "used")} {count} spell points on behalf of the current player.");

            Game1.player.UseSpellPoints(-count);
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
