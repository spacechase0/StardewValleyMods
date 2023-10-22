using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using SpaceCore.Interface;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using static SpaceCore.SpaceCore;

namespace SpaceCore.VanillaAssetExpansion
{
    internal class VanillaAssetExpansion
    {
        public static void Init()
        {
            SpaceCore.Instance.Helper.Events.Content.AssetRequested += Content_AssetRequested;

            SpaceEvents.AddWalletItems += SpaceEvents_AddWalletItems;
            SpaceEvents.AfterGiftGiven += SpaceEvents_AfterGiftGiven;
        }


        private static void SpaceEvents_AfterGiftGiven(object sender, EventArgsGiftGiven e)
        {
            var farmer = sender as Farmer;
            if (farmer != Game1.player) return;

            var dict = Game1.content.Load<Dictionary<string, NpcExtensionData>>("spacechase0.SpaceCore/NpcExtensionData");
            if (!dict.TryGetValue(e.Npc.Name, out var npcEntry))
                return;

            if (!npcEntry.GiftEventTriggers.TryGetValue(e.Gift.ItemId, out string eventStr))
                return;

            string[] data = eventStr.Split('/');
            string eid = data[0];

            Game1.PlayEvent(eid, checkPreconditions: false);
        }

        private static void SpaceEvents_AddWalletItems(object sender, EventArgs e)
        {
            var page = sender as NewSkillsPage;

            var dict = Game1.content.Load<Dictionary<string, WalletItem>>("spacechase0.SpaceCore/WalletItems");
            foreach (var entry in dict)
            {
                if (Game1.player.hasOrWillReceiveMail(entry.Key))
                {
                    var tex = Game1.content.Load<Texture2D>(entry.Value.TexturePath);
                    page.specialItems.Add(new ClickableTextureComponent(
                        name: "", bounds: new Rectangle(-1, -1, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                        label: null, hoverText: entry.Value.HoverText,
                        texture: tex, sourceRect: Game1.getSquareSourceRectForNonStandardTileSheet( tex, 16, 16, entry.Value.SpriteIndex ), scale: 4f, drawShadow: true));
                }
            }
        }

        private static void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/ObjectExtensionData"))
                e.LoadFrom(() => new Dictionary<string, ObjectExtensionData>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/WalletItems"))
                e.LoadFrom(() => new Dictionary<string, WalletItem>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/NpcExtensionData"))
                e.LoadFrom(() => new Dictionary<string, NpcExtensionData>(), AssetLoadPriority.Low);
        }
    }
}
