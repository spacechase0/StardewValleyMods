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
        public override int Height { get => ModOption.Height(); }


        /*********
        ** Public methods
        *********/
        public ComplexModOptionWidget(ComplexModOption modOption)
        {
            this.ModOption = modOption;
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            // intentionally not calling Element.Update
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            this.ModOption.Draw(b, this.Position);
        }
    }
}
