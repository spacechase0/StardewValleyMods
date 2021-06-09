using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.TerrainFeatures;
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
            this.whichGrass = which;
            this.name = $"Grass ({which})";
            this.Price = 100;
            this.ParentSheetIndex = 297;
        }

        public override Item getOne()
        {
            return new GrassStarterItem(this.whichGrass);
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            return !l.objects.ContainsKey(tile) && !l.terrainFeatures.ContainsKey(tile);
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Vector2 index1 = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            this.health = 10;
            this.owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;

            if (location.objects.ContainsKey(index1) || location.terrainFeatures.ContainsKey(index1))
                return false;
            location.terrainFeatures.Add(index1, (TerrainFeature)new CustomGrass(this.whichGrass, 4));
            Game1.playSound("dirtyHit");

            return true;
        }

        public override void draw(SpriteBatch b, int x, int y, float alpha = 1)
        {
            Texture2D tex = GrassStarterItem.tex;
            int texOffset = 20 + this.whichGrass * 20;
            if (this.whichGrass >= 5)
            {
                tex = tex2;
                texOffset = 20 * (this.whichGrass - 5);
            }
            b.Draw(tex, new Rectangle(x, y, 16, 20), new Rectangle(0, texOffset, 16, 20), Color.White);
        }

        public override void drawWhenHeld(SpriteBatch b, Vector2 pos, Farmer f)
        {
            Texture2D tex = GrassStarterItem.tex;
            int texOffset = 20 + this.whichGrass * 20;
            if (this.whichGrass >= 5)
            {
                tex = tex2;
                texOffset = 20 * (this.whichGrass - 5);
            }
            b.Draw(tex, pos - new Vector2(-4, 24), new Rectangle(0, texOffset, 16, 20), Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, (f.getStandingY() + 3) / 10000f);
        }

        public override void drawInMenu(SpriteBatch b, Vector2 pos, float scale, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            Texture2D tex = GrassStarterItem.tex;
            int texOffset = 20 + this.whichGrass * 20;
            if (this.whichGrass >= 5)
            {
                tex = tex2;
                texOffset = 20 * (this.whichGrass - 5);
            }
            b.Draw(tex, pos + new Vector2(4, 0), new Rectangle(0, texOffset, 16, 20), Color.White, 0, Vector2.Zero, 4 * scale, SpriteEffects.None, layerDepth);

            if ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1 || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scale > 0.3 && this.Stack != int.MaxValue)
                Utility.drawTinyDigits(this.Stack, b, pos + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(this.stack, 3f * scale)) + 3f * scale, (float)((double)Game1.tileSize - 18.0 * (double)scale + 2.0)), 3f * scale, 1f, Color.White);
        }

        // Custom Element Handler
        public object getReplacement()
        {
            return new SObject(297, this.stack);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>
            {
                ["whichGrass"] = this.whichGrass.ToString()
            };
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            this.whichGrass = int.Parse(additionalSaveData["whichGrass"]);
            this.name = $"Grass ({this.whichGrass})";
            this.Price = 100;
            this.ParentSheetIndex = 297;
        }
    }
}
