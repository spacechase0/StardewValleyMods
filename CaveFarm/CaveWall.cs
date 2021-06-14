using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace CaveFarm
{
    public class CaveWall : TerrainFeature
    {
        public readonly NetInt Health = new();

        public CaveWall()
            : base(false)
        {
            this.Health.Value = 3;
            this.NetFields.AddField(this.Health);
        }

        public override Rectangle getBoundingBox(Vector2 tileLocation)
        {
            return new((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
        }

        public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location)
        {
            if (t is Pickaxe pickaxe)
            {
                location.playSound("hammer");
                this.Health.Value -= ((pickaxe.UpgradeLevel + 1) / 2) + 1;
            }
            else if (damage > 0)
            {
                this.Health.Value -= damage;
            }

            if (this.Health.Value > 0)
                return true;
            return false;
        }

        public override void draw(SpriteBatch b, Vector2 tileLocation)
        {
            int x = (int)tileLocation.X;
            int y = (int)tileLocation.Y;
            b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, new Rectangle(x * Game1.tileSize, y * Game1.tileSize - Game1.tileSize * 2, Game1.tileSize, Game1.tileSize * 3)), null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, (y + 1) * 64 / 10000f - x * 64 / 1000000f);
        }
    }
}
