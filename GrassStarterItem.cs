using PyTK.CustomElementHandler;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MoreGrassStarters
{
    public class GrassStarterItem : StardewValley.Object, ISaveElement
    {
        private static Texture2D tex = Game1.content.Load<Texture2D>("TerrainFeatures\\grass");
        public static Texture2D tex2;
        private int whichGrass = 1;
        public static int ExtraGrassTypes => tex2 == null ? 0 : tex2.Height / 20;

        public GrassStarterItem()
        {
        }

        public GrassStarterItem(int which)
        {
            whichGrass = which;
            name = "Grass (" + which + ")";
            price = 100;
            ParentSheetIndex = 297;
        }

        public override Item getOne()
        {
            return new GrassStarterItem(whichGrass);
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            return !l.objects.ContainsKey(tile) && !l.terrainFeatures.ContainsKey(tile);
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool placementAction(GameLocation location, int x, int y, StardewValley.Farmer who = null)
        {
            Vector2 index1 = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            this.health = 10;
            this.owner = who == null ? Game1.player.uniqueMultiplayerID : who.uniqueMultiplayerID;

            if (location.objects.ContainsKey(index1) || location.terrainFeatures.ContainsKey(index1))
                return false;
            location.terrainFeatures.Add(index1, (TerrainFeature)new CustomGrass(whichGrass, 4));
            Game1.playSound("dirtyHit");

            return true;
        }

        public override void draw(SpriteBatch b, int x, int y, float alpha = 1)
        {
            Texture2D tex = GrassStarterItem.tex;
            int texOffset = 20 + whichGrass * 20;
            if (whichGrass >= 5)
            {
                tex = tex2;
                texOffset = 20 * (whichGrass - 5);
            }
            b.Draw(tex, new Rectangle(x, y, 16, 20), new Rectangle(0, texOffset, 16, 20), Color.White);
        }

        public override void drawWhenHeld(SpriteBatch b, Vector2 pos, StardewValley.Farmer f)
        {
            Texture2D tex = GrassStarterItem.tex;
            int texOffset = 20 + whichGrass * 20;
            if (whichGrass >= 5)
            {
                tex = tex2;
                texOffset = 20 * (whichGrass - 5);
            }
            b.Draw(tex, pos - new Vector2(-4, 24), new Rectangle(0, texOffset, 16, 20), Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, (f.getStandingY() + 3) / 10000f);
        }

        public override void drawInMenu(SpriteBatch b, Vector2 pos, float scale, float transparency, float layerDepth, bool drawStackNumber)
        {
            Texture2D tex = GrassStarterItem.tex;
            int texOffset = 20 + whichGrass * 20;
            if (whichGrass >= 5)
            {
                tex = tex2;
                texOffset = 20 * (whichGrass - 5);
            }
            b.Draw(tex, pos + new Vector2( 4, 0 ), new Rectangle(0, texOffset, 16, 20), Color.White, 0, Vector2.Zero, 4 * scale, SpriteEffects.None, layerDepth);

            if (drawStackNumber && this.maximumStackSize() > 1 && ((double)scale > 0.3 && this.Stack != int.MaxValue) && this.Stack > 1)
                Utility.drawTinyDigits(this.stack, b, pos + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(this.stack, 3f * scale)) + 3f * scale, (float)((double)Game1.tileSize - 18.0 * (double)scale + 2.0)), 3f * scale, 1f, Color.White);
        }

        // Custom Element Handler
        public object getReplacement()
        {
            return new StardewValley.Object(297, stack);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            var dict = new Dictionary<string, string>();
            dict["whichGrass"] = whichGrass.ToString();
            return dict;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            whichGrass = int.Parse(additionalSaveData["whichGrass"]);
            name = "Grass (" + whichGrass + ")";
            price = 100;
            ParentSheetIndex = 297;
        }
    }
}
