using GenericModConfigMenu.ModOption;
using Microsoft.Xna.Framework.Graphics;

namespace GenericModConfigMenu.Framework.UI
{
    internal class ComplexModOptionWidget : Element
    {
        /*********
        ** Accessors
        *********/
        public ComplexModOption ModOption { get; }

        /// <inheritdoc />
        public override int Width => 0;

        /// <inheritdoc />
        public override int Height => 0;


        /*********
        ** Public methods
        *********/
        public ComplexModOptionWidget(ComplexModOption modOption)
        {
            this.ModOption = modOption;
        }

        /// <inheritdoc />
        public override void Update(bool hidden = false)
        {
            // intentionally not calling Element.Update
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            this.ModOption.Draw(b, this.Position);
        }
    }
}
