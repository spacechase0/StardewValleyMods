using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MoonMisadventures.Game.Items
{
    public class NecklaceDataDefinition : ItemDataDefinition
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

        public override Item CreateItem(string item_id, int amount, int quality)
        {
            return new Necklace(item_id);
        }

        public override bool DataExists(string item_id)
        {
            return Game1.content.Load<Dictionary<string, NecklaceData>>("spacechase0.MoonMisadventures/Necklaces").ContainsKey(item_id);
        }

        public override IEnumerable<string> GetAllItemIDs()
        {
            return Game1.content.Load<Dictionary<string, NecklaceData>>("spacechase0.MoonMisadventures/Necklaces").Keys;
        }

        public override string GetDefaultTexturePath()
        {
            return null;
        }

        public override string GetDescription(string item_id)
        {
            return GetNecklaceData(item_id).Description;
        }

        public override string GetDisplayName(string item_id)
        {
            return GetNecklaceData(item_id).DisplayName;
        }

        public override string GetOverrideTexturePath(string item_id)
        {
            return GetNecklaceData(item_id).Texture;
        }

        public override int GetParentSheetIndex(string item_id)
        {
            return GetNecklaceData(item_id).TextureIndex;
        }

        public override Rectangle GetSourceRect(string item_id, Texture2D texture, int offset, int? parent_sheet_index)
        {
            return Game1.getSourceRectForStandardTileSheet(texture, base._CalculateParentSheetIndex( item_id, offset, parent_sheet_index ), 16, 16);
        }

        public override string GetItemName(string item_id)
        {
            return item_id;
        }

        public override int GetItemCategory(string item_id)
        {
            return StardewValley.Object.equipmentCategory;
        }
    }
}
