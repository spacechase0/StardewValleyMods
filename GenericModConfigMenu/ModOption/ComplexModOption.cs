using System;
using GenericModConfigMenu.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GenericModConfigMenu.ModOption
{
    /// <summary>A mod option with custom rendering logic provided by the original mod.</summary>
    internal class ComplexModOption : BaseModOption
    {
        /*********
        ** Fields
        *********/
        /// <summary>Update the component state before drawing if needed. This is called with the pixel position at which to start drawing and the current option value. This should return the new value.</summary>
        private readonly Func<Vector2, object, object> UpdateImpl;

        /// <summary>Draw the option in the config UI. This is called with the sprite batch being rendered, the pixel position at which to start drawing, and the current option value. This should return the new value.</summary>
        private readonly Func<SpriteBatch, Vector2, object, object> DrawImpl;

        /// <summary>Set a new value in the mod config.</summary>
        private readonly Action<object> SaveImpl;

        /// <summary>The cached option value.</summary>
        private object Value;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="update">Update the component state before drawing if needed. This is called with the sprite batch being rendered, the pixel position at which to start drawing, and the current option value. This should return the new value.</param>
        /// <param name="draw">Draw the option in the config UI. This is called with the sprite batch being rendered, the pixel position at which to start drawing, and the current option value. This should return the new value.</param>
        /// <param name="save">Set a new value in the mod config.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        public ComplexModOption(string name, string tooltip, Func<Vector2, object, object> update, Func<SpriteBatch, Vector2, object, object> draw, Action<object> save, ModConfig mod)
            : base(name, tooltip, name, mod)
        {
            this.UpdateImpl = update;
            this.DrawImpl = draw;
            this.SaveImpl = save;
        }

        /// <inheritdoc />
        public override void GetLatest()
        {
            this.Value = null;
        }

        /// <inheritdoc />
        public override void Save()
        {
            this.SaveImpl.Invoke(this.Value);
        }

        /// <summary>Update the component state before drawing if needed.</summary>
        /// <param name="position">The pixel position at which to start drawing.</param>
        public void Update(Vector2 position)
        {
            this.Value = this.UpdateImpl.Invoke(position, this.Value);
        }

        /// <summary>Draw the option to the form.</summary>
        /// <param name="spriteBatch">The sprite batch being rendered.</param>
        /// <param name="position">The pixel position at which to start drawing.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            if (this.Value == null)
                return;

            this.Value = this.DrawImpl.Invoke(spriteBatch, position, this.Value);
        }
    }
}
