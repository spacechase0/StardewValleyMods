using PyTK.CustomElementHandler;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using SObject = StardewValley.Object;

namespace MoreGrassStarters
{
    public class GrassStarterItem : SObject, ISaveElement
    {
        private static readonly Texture2D tex = Game1.content.Load<Texture2D>("TerrainFeatures\\grass");
        public static Texture2D tex2;
        private int whichGrass = 1;
        public static int ExtraGrassTypes => tex2 == null ? 0 : tex2.Height / 20;

        public GrassStarterItem()
        {
        }

        public GrassStarterItem(int which)
        {
            whichGrass = which;
            name = $"Grass ({which})";
            Price = 100;
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
            this.owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;

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

        public override void drawInMenu(SpriteBatch b, Vector2 pos, float scale, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
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
                Utility.drawTinyDigits(this.Stack, b, pos + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(this.stack, 3f * scale)) + 3f * scale, (float)((double)Game1.tileSize - 18.0 * (double)scale + 2.0)), 3f * scale, 1f, Color.White);
        }

        // Custom Element Handler
        public object getReplacement()
        {
            return new SObject(297, stack);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>
            {
                ["whichGrass"] = whichGrass.ToString()
            };
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            whichGrass = int.Parse(additionalSaveData["whichGrass"]);
            name = $"Grass ({whichGrass})";
            Price = 100;
            ParentSheetIndex = 297;
        }
    }
}
