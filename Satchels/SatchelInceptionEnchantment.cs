using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using StardewValley;
using StardewValley.Enchantments;

namespace Satchels
{
    [XmlType("Mods_spacechase0_Satchels_SatchelInceptionEnchantment")]
    public class SatchelInceptionEnchantment : BaseEnchantment
    {
        public override bool CanApplyTo(Item item)
        {
            return (item is Satchel);
        }

        public override string GetName()
        {
            return "Satchel Inception";
        }
    }
}
