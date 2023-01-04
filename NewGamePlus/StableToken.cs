using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;

namespace NewGamePlus
{
    [XmlType("Mods_spacechase0_NewGamePlus_StableToken" )]
    public class StableToken : StardewValley.Object
    {
        public StableToken()
        : base(74, 1)
        {
        }

        public override string DisplayName { get => I18n.Item_StableToken_Name(); set { } }

        public override string getDescription()
        {
            return I18n.Item_StableToken_Description();
        }

        public override bool canStackWith(ISalable other)
        {
            return false;
        }

        public override Item getOne()
        {
            var ret = new StableToken();
            ret._GetOneFrom(this);
            return ret;
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(Mod.instance.stableTokenTex, location + new Vector2(32), null, Color.White * transparency, 0, new Vector2(8), 4 * scaleSize, SpriteEffects.None, layerDepth);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            spriteBatch.Draw(Mod.instance.stableTokenTex, objectPosition, null, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, (f.getStandingY() + 3) / 10000f);
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            return l is BuildableGameLocation;
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            if (location is not BuildableGameLocation bgl)
                return false;

            bool ret = bgl.buildStructure(new BluePrint("Stable"), new Vector2(x / 64 - 3, y / 64 - 1), who);
            if (ret)
            {
                var b = bgl.getBuildingAt(new Vector2(x / 64 - 3, y / 64 - 1));
                while (b.daysOfConstructionLeft.Value > 0 )
                    b.dayUpdate(0);
            }
            return ret;
        }

        public override void drawPlacementBounds(SpriteBatch b, GameLocation location)
        {
            if (location is not BuildableGameLocation)
                return;


            int x = (int)Game1.GetPlacementGrabTile().X * 64;
            int y = (int)Game1.GetPlacementGrabTile().Y * 64;

            Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
            if (Game1.isCheckingNonMousePlacement)
            {
                Vector2 placementPosition = Utility.GetNearbyValidPlacementPosition(Game1.player, location, this, x, y);
                x = (int)placementPosition.X;
                y = (int)placementPosition.Y;
            }

            Game1.isCheckingNonMousePlacement = false;

            var CurrentBlueprint = new BluePrint("Stable");

            Vector2 mousePositionTile2 = new Vector2(x / 64 - 3, y / 64 - 1);
            for (int y4 = 0; y4 < CurrentBlueprint.tilesHeight; y4++)
            {
                for (int x3 = 0; x3 < CurrentBlueprint.tilesWidth; x3++)
                {
                    int sheetIndex3 = CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(x3, y4);
                    Vector2 currentGlobalTilePosition3 = new Vector2(mousePositionTile2.X + (float)x3, mousePositionTile2.Y + (float)y4);
                    if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(currentGlobalTilePosition3))
                    {
                        sheetIndex3++;
                    }
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition3 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex3 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
                }
            }
            foreach (Point additionalPlacementTile in CurrentBlueprint.additionalPlacementTiles)
            {
                int x4 = additionalPlacementTile.X;
                int y3 = additionalPlacementTile.Y;
                int sheetIndex4 = CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(x4, y3);
                Vector2 currentGlobalTilePosition4 = new Vector2(mousePositionTile2.X + (float)x4, mousePositionTile2.Y + (float)y3);
                if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(currentGlobalTilePosition4))
                {
                    sheetIndex4++;
                }
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition4 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex4 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
            }
        }
    }
}
