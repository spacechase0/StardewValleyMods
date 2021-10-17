using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>A mod option with custom rendering logic provided by the original mod.</summary>
    internal class ComplexModOption : BaseModOption
    {
        /*********
        ** Fields
        *********/
        /// <summary>Draw the option in the config UI. This is called with the sprite batch being rendered, the pixel position at which to start drawing, and the current option value. This should return the new value.</summary>
        private readonly Action<SpriteBatch, Vector2> DrawImpl;

        /// <summary>Set a new value in the mod config.</summary>
        private readonly Action SaveImpl;


        /*********
        ** Accessors
        *********/
        /// <summary>The height to allocate for the option in the form.</summary>
        public Func<int> Height { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fieldId">The unique field ID used when raising field-changed events, or <c>null</c> to generate a random one.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        /// <param name="height">The height to allocate for the option in the form. This is called once before the form is drawn and can't be changed dynamically.</param>
        /// <param name="draw">Draw the option in the config UI. This is called with the sprite batch being rendered, the pixel position at which to start drawing, and the current option value. This should return the new value.</param>
        /// <param name="save">Set a new value in the mod config.</param>
        public ComplexModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod, Func<int> height, Action<SpriteBatch, Vector2> draw, Action save)
            : base(fieldId, name, tooltip, mod)
        {
            height ??= () => 0; // UI will ignore values below the minimum one row

            this.Height = height;
            this.DrawImpl = draw;
            this.SaveImpl = save;
        }

        /// <inheritdoc />
        public override void GetLatest() { }

        /// <inheritdoc />
        public override void Save()
        {
            this.SaveImpl();
        }

        /// <summary>Draw the option to the form.</summary>
        /// <param name="spriteBatch">The sprite batch being rendered.</param>
        /// <param name="position">The pixel position at which to start drawing.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            this.DrawImpl(spriteBatch, position);
        }
    }
}
