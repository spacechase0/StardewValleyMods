using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.UI;

namespace NewGamePlus
{
    internal class LegacySlot : StaticContainer
    {
        public class SlotEntry
        {
            public Texture2D Texture;
            public Rectangle TexRect;
            public string Text;
            public int PointCost;

            public Action Apply;
        }

        public SlotEntry[] slots;
        public int active = 0;

        private Element display;
        private Label cost;

        private bool pendingRefresh = false;

        public LegacySlot(SlotEntry[] slots)
        {
            this.slots = slots;
            Size = new Vector2(100, 128);

            cost = new Label()
            {
                Callback = (e) => { active = active - 1; if (active < 0) active += slots.Length; pendingRefresh = true; }
            };
            AddChild(cost);

            Refresh();
        }

        public override void Update(bool isOffScreen = false)
        {
            if (pendingRefresh)
                Refresh();
            base.Update(isOffScreen);
        }

        public void Refresh()
        {
            pendingRefresh = false;

            if ( display != null )
                RemoveChild(display);
            if (slots[active].Texture != null)
            {
                display = new Image()
                {
                    Texture = slots[active].Texture,
                    TexturePixelArea = slots[active].TexRect,
                    Scale = 100 / slots[active].TexRect.Width,
                    Callback = (e) => { active = (active + 1) % slots.Length; pendingRefresh = true; }
                };
                display.LocalPosition = new Vector2((100 - display.Width) / 2, 0);
            }
            else
            {
                display = new Label()
                {
                    String = slots[active].Text,
                    Callback = (e) => { active = (active + 1) % slots.Length; pendingRefresh = true; }
                };
                display.LocalPosition = new Vector2((100 - display.Width) / 2, (100 - display.Height) / 2);
            }
            AddChild(display);

            cost.String = (active == slots.Length - 1 ? slots[ active ].PointCost.ToString() : ( slots[ active ].PointCost + " -> " + slots[ active + 1 ].PointCost ));
            cost.LocalPosition = new Vector2((125 - cost.Width) / 2, 108);
        }

        public void ApplyCurrent()
        {
            if (slots[active].Apply != null)
                slots[active].Apply();
        }
    }
}
