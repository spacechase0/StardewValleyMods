using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

namespace Magic.Framework.Spells.Effects
{
    internal class TendrilGroup : List<Tendril>, IActiveEffect
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Update the effect state if needed.</summary>
        /// <param name="e">The update tick event args.</param>
        /// <returns>Returns true if the effect is still active, or false if it can be discarded.</returns>
        public bool Update(UpdateTickedEventArgs e)
        {
            for (int i = this.Count - 1; i >= 0; i--)
            {
                Tendril tendril = this[i];
                if (!tendril.Update(e))
                    this.RemoveAt(i);
            }

            return this.Any();
        }

        /// <summary>Draw the effect to the screen if needed.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Tendril tendril in this)
                tendril.Draw(spriteBatch);
        }
    }
}
