using GenericModConfigMenu.Framework.ModOption;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared.UI;

namespace GenericModConfigMenu.Framework
{
    internal class ComplexModOptionWidget : Element
    {
        /*********
        ** Accessors
        *********/
        public ComplexModOption ModOption { get; }

        /// <inheritdoc />
        public override int Width { get; } = 0;

        /// <inheritdoc />
        public override int Height { get; }


        /*********
        ** Public methods
        *********/
        public ComplexModOptionWidget(ComplexModOption modOption)
        {
            this.ModOption = modOption;
            this.Height = modOption.Height();
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
