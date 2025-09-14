using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using SObject = StardewValley.Object;

namespace MoreGrassStarters
{
    [XmlType("Mods_spacechase0_GrassStarterItem")]
    public class GrassStarterItem : SObject
    {
        internal const int grassID = 297;

        private static readonly Texture2D Tex = Game1.content.Load<Texture2D>("TerrainFeatures\\grass");
        public static Texture2D Tex2;
        public static int ExtraGrassTypes => GrassStarterItem.Tex2 == null ? 0 : GrassStarterItem.Tex2.Height / 20;

        public int WhichGrass { get; set; }

        public GrassStarterItem()
        {
        }

        public GrassStarterItem(int which)
        {
            this.WhichGrass = which;
            this.name = $"Grass ({which})";
            this.Price = 100;
            this.ParentSheetIndex = grassID;
        }

        public override Item getOne()
        {
            return new GrassStarterItem(this.WhichGrass);
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
            Vector2 index1 = new Vector2(x / Game1.tileSize, y / Game1.tileSize);
            this.health = 10;
            this.owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;

            if (location.objects.ContainsKey(index1) || location.terrainFeatures.ContainsKey(index1))
                return false;
            location.terrainFeatures.Add(index1, new CustomGrass(this.WhichGrass, 4));
            Game1.playSound("dirtyHit");

            return true;
        }

        public override void draw(SpriteBatch b, int x, int y, float alpha = 1)
        {
            Texture2D tex = GrassStarterItem.Tex;
            int texOffset = 20 + this.WhichGrass * 20;
            if (this.WhichGrass >= 5)
            {
                tex = GrassStarterItem.Tex2;
                texOffset = 20 * (this.WhichGrass - 5);
            }
            b.Draw(tex, new Rectangle(x, y, 16, 20), new Rectangle(0, texOffset, 16, 20), Color.White);
        }

        public override void drawWhenHeld(SpriteBatch b, Vector2 pos, Farmer f)
        {
            Texture2D tex = GrassStarterItem.Tex;
            int texOffset = 20 + this.WhichGrass * 20;
            if (this.WhichGrass >= 5)
            {
                tex = GrassStarterItem.Tex2;
                texOffset = 20 * (this.WhichGrass - 5);
            }
            b.Draw(tex, pos - new Vector2(-4, 24), new Rectangle(0, texOffset, 16, 20), Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, (f.getStandingY() + 3) / 10000f);
        }

        public override void drawInMenu(SpriteBatch b, Vector2 pos, float scale, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            Texture2D tex = GrassStarterItem.Tex;
            int texOffset = 20 + this.WhichGrass * 20;
            if (this.WhichGrass >= 5)
            {
                tex = GrassStarterItem.Tex2;
                texOffset = 20 * (this.WhichGrass - 5);
            }
            b.Draw(tex, pos + new Vector2(4, 0), new Rectangle(0, texOffset, 16, 20), Color.White, 0, Vector2.Zero, 4 * scale, SpriteEffects.None, layerDepth);

            if ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1 || drawStackNumber == StackDrawType.Draw_OneInclusive) && scale > 0.3 && this.Stack != int.MaxValue)
                Utility.drawTinyDigits(this.Stack, b, pos + new Vector2(Game1.tileSize - Utility.getWidthOfTinyDigitString(this.Stack, 3f * scale) + 3f * scale, (float)(Game1.tileSize - 18.0 * scale + 2.0)), 3f * scale, 1f, Color.White);
        }
    }
}
