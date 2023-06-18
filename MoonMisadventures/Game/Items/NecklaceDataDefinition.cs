using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace MoonMisadventures.Game.Items
{
    public class NecklaceDataDefinition : BaseItemDataDefinition
    {
        public NecklaceDataDefinition()
        {
        }

        private NecklaceData GetNecklaceData(string id)
        {
            return Game1.content.Load<Dictionary<string, NecklaceData>>("spacechase0.MoonMisadventures/Necklaces")[ id ];
        }

        public override string Identifier => "(SC0_MM_N)";

        public override string StandardDescriptor => "SC0_MM_N";


        public override IEnumerable<string> GetAllIds()
        {
            return Game1.content.Load<Dictionary<string, NecklaceData>>("spacechase0.MoonMisadventures/Necklaces").Keys;
        }

        public override bool Exists(string itemId)
        {
            return Game1.content.Load<Dictionary<string, NecklaceData>>("spacechase0.MoonMisadventures/Necklaces").ContainsKey(itemId);
        }

        public override ParsedItemData GetData(string itemId)
        {
            var nd = GetNecklaceData(itemId);
            return new ParsedItemData(this, itemId, nd.TextureIndex, nd.Texture, "Necklace." + itemId, nd.DisplayName, nd.Description, StardewValley.Object.equipmentCategory, null, nd, false);
        }

        public override Item CreateItem(ParsedItemData data)
        {
            return new Necklace(data.ItemId);
        }

        public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
        {
            int w = texture.Width / 16;
            return new Rectangle(spriteIndex % w * 16, spriteIndex / w * 16, 16, 16);
        }
    }
}
