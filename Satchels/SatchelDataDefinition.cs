using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace Satchels
{
    public class SatchelDataDefinition : BaseItemDataDefinition
    {
        public SatchelDataDefinition()
        {
        }

        internal static SatchelData GetSpecificData(string id)
        {
            return Game1.content.Load<Dictionary<string, SatchelData>>("spacechase0.Satchels/Satchels")[ id ];
        }

        public override string Identifier => "(SC0_S_S)";

        public override string StandardDescriptor => "SC0_S_S";


        public override IEnumerable<string> GetAllIds()
        {
            return Game1.content.Load<Dictionary<string, SatchelData>>("spacechase0.Satchels/Satchels").Keys;
        }

        public override bool Exists(string itemId)
        {
            return Game1.content.Load<Dictionary<string, SatchelData>>("spacechase0.Satchels/Satchels").ContainsKey(itemId);
        }

        public override ParsedItemData GetData(string itemId)
        {
            var data = GetSpecificData(itemId);
            return new ParsedItemData(this, itemId, data.TextureIndex, data.Texture, "Satchel." + itemId, data.DisplayName, data.Description, StardewValley.Object.equipmentCategory, null, data, false);
        }

        public override Item CreateItem(ParsedItemData data)
        {
            return new Satchel(data.ItemId);
        }

        public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
        {
            int w = texture.Width / 16;
            return new Rectangle(spriteIndex % w * 16, spriteIndex / w * 16, 16, 16);
        }
    }
}
