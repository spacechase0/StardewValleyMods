using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatOverhaulMod.Buffs
{
    public class DummyBuff : Buff
    {
        public CustomBuffInstance CustomBuff { get; }

        public DummyBuff(CustomBuffInstance customBuff)
        : base(customBuff.GetData().Name,
                  buff_source: customBuff.GetData().Name,
                  display_source: customBuff.GetData().Name,
                  duration: (int)((customBuff.Duration - customBuff.DurationUsed) * 1000),
                  icon_texture: Game1.content.Load<Texture2D>(customBuff.GetData().Icon),
                  is_debuff: customBuff.GetData().IsConsideredDebuff,
                  display_name: customBuff.GetData().Name,
                  description: customBuff.GetData().Description )
        {
            CustomBuff = customBuff;
        }
    }
}
