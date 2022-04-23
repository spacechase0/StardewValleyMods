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
        public static Vector3 GetPosition3D( this Character character )
        {
            // TODO: Map elevation
            return new Vector3(character.Position.X / Game1.tileSize, -character.yJumpOffset / 64f, character.Position.Y / Game1.tileSize);
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

        public static Vector3 To3D(this Vector2 v2)
        {
            return new Vector3(v2.X, 0, v2.Y);
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
