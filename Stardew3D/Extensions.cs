using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;

namespace Stardew3D
{
    public static class Extensions
    {
        private static float GetHeightAtTile( Point pt )
        {
            var aloc = Mod.State.ActiveLocation;
            if (aloc == null || aloc.Heightmap == null)
                return 0;

            if (pt.X < 0 || pt.Y < 0 || pt.X >= aloc.HeightmapTex.Width || pt.Y >= aloc.HeightmapTex.Height)
                return 0;

            int pi = pt.X + pt.Y * aloc.HeightmapTex.Width;
            return aloc.HeightmapMin + (aloc.Heightmap[pi].R - aloc.HeightmapMinColor.R) / ( float ) aloc.HeightmapMaxColor.R * ( aloc.HeightmapMax - aloc.HeightmapMin );
        }

        private static float GetHeightAtPoint(Vector2 vec)
        {
            float offsetY = 0;
            var aloc = Mod.State.ActiveLocation;
            if (aloc == null || aloc.Heightmap == null)
                return 0;

            float x = vec.X / Game1.tileSize;
            float y = vec.Y / Game1.tileSize;

            Vector2 spot = Vector2.Zero;
            float tl, tr, bl, br;
            if (x % 1 < 0.5)
            {
                if (y % 1 < 0.5)
                {
                    spot = new Vector2(x % 1 * 2, y % 1 * 2);
                    tl = GetHeightAtTile(new Point((int)x - 1, (int)y - 1));
                    tr = GetHeightAtTile(new Point((int)x - 0, (int)y - 1));
                    bl = GetHeightAtTile(new Point((int)x - 1, (int)y - 0));
                    br = GetHeightAtTile(new Point((int)x - 0, (int)y - 0));
                }
                else
                {
                    spot = new Vector2(x % 1 * 2, y % 1 * 2 - 1);
                    tl = GetHeightAtTile(new Point((int)x - 1, (int)y - 0));
                    tr = GetHeightAtTile(new Point((int)x - 0, (int)y - 0));
                    bl = GetHeightAtTile(new Point((int)x - 1, (int)y + 1));
                    br = GetHeightAtTile(new Point((int)x - 0, (int)y + 1));
                }
            }
            else
            {
                if (y % 1 < 0.5)
                {
                    spot = new Vector2(x % 1 * 2 - 1, y % 1 * 2);
                    tl = GetHeightAtTile(new Point((int)x - 0, (int)y - 1));
                    tr = GetHeightAtTile(new Point((int)x + 1, (int)y - 1));
                    bl = GetHeightAtTile(new Point((int)x - 0, (int)y - 0));
                    br = GetHeightAtTile(new Point((int)x + 1, (int)y - 0));
                }
                else
                {
                    spot = new Vector2(x % 1 * 2 - 1, y % 1 * 2 - 1);
                    tl = GetHeightAtTile(new Point((int)x - 0, (int)y - 0));
                    tr = GetHeightAtTile(new Point((int)x + 1, (int)y - 0));
                    bl = GetHeightAtTile(new Point((int)x - 0, (int)y + 1));
                    br = GetHeightAtTile(new Point((int)x + 1, (int)y + 1));
                }
            }

            if (vec.X >= 14 && vec.X < 15)
            {
                //Console.WriteLine("hmm");
            }

            return Utility.Lerp(Utility.Lerp(tl, tr, spot.X), Utility.Lerp(bl, br, spot.X), spot.Y);
        }

        public static Vector3 GetPosition3D( this Character character )
        {
            return new Vector3(character.GetBoundingBox().Center.X / (float) Game1.tileSize, GetHeightAtPoint( character.GetBoundingBox().Center.ToVector2() ) + -character.yJumpOffset / 64f, character.GetBoundingBox().Center.Y / ( float ) Game1.tileSize);
        }

        public static float GetFacing3D(this Character character)
        {
            switch (character.FacingDirection)
            {
                case Game1.down: return 0;
                case Game1.up: return MathF.PI;
                case Game1.left: return -MathF.PI / 2;
                case Game1.right: return MathF.PI / 2;
            }
            return 0;
        }

        public static Vector3 To3D(this Vector2 v2, bool doHeight = true)
        {
            return new Vector3(v2.X, doHeight ? GetHeightAtPoint( v2 * Game1.tileSize ) : 0, v2.Y);
        }

        public static string GetLogSummary(this Exception exception)
        {
            return (string)AccessTools.Method("StardewModdingAPI.Internal.ExceptionHelper:GetLogSummary").Invoke(null, new object[] { exception });
        }
        public static string GetMenuChainLabel(this IClickableMenu menu)
        {
            return (string)AccessTools.Method("StardewModdingAPI.Framework.InternalExtensions:GetMenuChainLabel").Invoke(null, new object[] { menu });
        }
    }
}
