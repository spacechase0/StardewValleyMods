using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericModConfigMenu.ModOption;
using Microsoft.Xna.Framework.Graphics;

namespace GenericModConfigMenu.UI
{
    internal class ComplexModOptionWidget : Element
    {
        public ComplexModOption ModOption { get; }

        public ComplexModOptionWidget(ComplexModOption modOption)
        {
            ModOption = modOption;
        }

        public override int Width => 0;
        public override int Height => 0;

        public override void Update(bool hidden = false)
        {
            // intentionally not calling Element.Update
            ModOption.Update(Position);
        }

        public override void Draw(SpriteBatch b)
        {
            ModOption.Draw(b, Position);
        }
    }
}
