using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using StardewValley;

namespace TerraformingHoe
{
    [XmlType("Mods_spacechase0_TerraformingHoe_TerraformEnchantment")]
    public class TerraformEnchantment : HoeEnchantment
    {
        public TerraformEnchantment()
        {
        }

        public override string GetName()
        {
            return "Terraform";
        }
    }
}
