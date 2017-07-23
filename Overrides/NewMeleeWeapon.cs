using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Utilities;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Reflection;
using SFarmer = StardewValley.Farmer;

namespace SpaceCore.Overrides
{
    class NewMeleeWeapon
    {
        internal static void hijack( HarmonyInstance harmony )
        {
            Hijack.hijack(typeof(   MeleeWeapon).GetMethod("drawDuringUse", BindingFlags.Static | BindingFlags.Public),
                          typeof(NewMeleeWeapon).GetMethod("drawDuringUse", BindingFlags.Static | BindingFlags.Public));
        }

        public static void drawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, SFarmer f, Rectangle sourceRect, int type, bool isOnSpecial)
        {
            if (f.CurrentTool is ICustomWeaponDraw)
            {
                (f.CurrentTool as ICustomWeaponDraw).draw(frameOfFarmerAnimation, facingDirection, spriteBatch, playerPosition, f, sourceRect, type, isOnSpecial);
                return;
            }

            var MeleeWeapon_center = new Vector2(1f, 15f);

            Tool currentTool = f.CurrentTool;
            if (type != 1)
            {
                if (isOnSpecial)
                {
                    if (type == 3)
                    {
                        switch (f.FacingDirection)
                        {
                            case 0:
                                spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 8.0), playerPosition.Y - 44f), new Rectangle?(sourceRect), Color.White, -1.767146f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                                break;
                            case 1:
                                spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 8.0), playerPosition.Y - 4f), new Rectangle?(sourceRect), Color.White, -3f * (float)Math.PI / 16f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 1) / 10000f));
                                break;
                            case 2:
                                spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 52.0), playerPosition.Y + 4f), new Rectangle?(sourceRect), Color.White, -5.105088f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
                                break;
                            case 3:
                                spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 56.0), playerPosition.Y - 4f), new Rectangle?(sourceRect), Color.White, -0.9817477f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 1) / 10000f));
                                break;
                        }
                    }
                    else
                    {
                        if (type != 2)
                            return;
                        if (facingDirection == 1)
                        {
                            switch (frameOfFarmerAnimation)
                            {
                                case 0:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X - (double)(Game1.tileSize / 2) - 12.0), playerPosition.Y - (float)(Game1.tileSize * 5 / 4)), new Rectangle?(sourceRect), Color.White, -3f * (float)Math.PI / 8f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 1:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize, (float)((double)playerPosition.Y - (double)Game1.tileSize - 48.0)), new Rectangle?(sourceRect), Color.White, 0.3926991f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 2:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)(Game1.tileSize * 2) - (float)(Game1.pixelZoom * 4), playerPosition.Y - (float)Game1.tileSize - (float)(Game1.pixelZoom * 3)), new Rectangle?(sourceRect), Color.White, 3f * (float)Math.PI / 8f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 3:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 72f, (float)((double)playerPosition.Y - (double)Game1.tileSize + (double)(Game1.tileSize / 4) - 32.0)), new Rectangle?(sourceRect), Color.White, 0.3926991f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 4:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)(Game1.tileSize * 3 / 2), (float)((double)playerPosition.Y - (double)Game1.tileSize + (double)(Game1.tileSize / 4) - 16.0)), new Rectangle?(sourceRect), Color.White, 0.7853982f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 5:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)(Game1.tileSize * 3 / 2) - 12.0), playerPosition.Y - (float)Game1.tileSize + (float)(Game1.tileSize / 4)), new Rectangle?(sourceRect), Color.White, 0.7853982f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 6:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)(Game1.tileSize * 3 / 2) - 16.0), (float)((double)playerPosition.Y - (double)Game1.tileSize + (double)Game1.tileSize * 0.625 - 8.0)), new Rectangle?(sourceRect), Color.White, 0.7853982f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 7:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)(Game1.tileSize * 3 / 2) - 8.0), playerPosition.Y + (float)Game1.tileSize * 0.625f), new Rectangle?(sourceRect), Color.White, 0.9817477f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                            }
                        }
                        else if (facingDirection == 3)
                        {
                            switch (frameOfFarmerAnimation)
                            {
                                case 0:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 4.0 + 8.0), playerPosition.Y - 56f - (float)Game1.tileSize), new Rectangle?(sourceRect), Color.White, 0.3926991f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 1:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - (float)(Game1.tileSize / 2), playerPosition.Y - (float)(Game1.tileSize / 2)), new Rectangle?(sourceRect), Color.White, -1.963495f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 2:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 12f, playerPosition.Y + (float)(Game1.pixelZoom * 2)), new Rectangle?(sourceRect), Color.White, -2.748894f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 3:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X - (double)(Game1.tileSize / 2) - 4.0), playerPosition.Y + (float)(Game1.pixelZoom * 2)), new Rectangle?(sourceRect), Color.White, -2.356194f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 4:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X - (double)(Game1.tileSize / 4) - 24.0), (float)((double)playerPosition.Y + (double)Game1.tileSize + 12.0) - (float)Game1.tileSize), new Rectangle?(sourceRect), Color.White, 4.31969f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 5:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 20f, (float)((double)playerPosition.Y + (double)Game1.tileSize + 40.0) - (float)Game1.tileSize), new Rectangle?(sourceRect), Color.White, 3.926991f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 6:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 16f, (float)((double)playerPosition.Y + (double)Game1.tileSize + 56.0)), new Rectangle?(sourceRect), Color.White, 3.926991f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                                case 7:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 8f, (float)((double)playerPosition.Y + (double)Game1.tileSize + 64.0)), new Rectangle?(sourceRect), Color.White, 3.730641f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                                    break;
                            }
                        }
                        else
                        {
                            switch (frameOfFarmerAnimation)
                            {
                                case 0:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 24f, (float)((double)playerPosition.Y - (double)(Game1.tileSize / 3) - 8.0) - (float)Game1.tileSize), new Rectangle?(sourceRect), Color.White, -0.7853982f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                    break;
                                case 1:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 16f, playerPosition.Y - (float)(Game1.tileSize / 3) - (float)Game1.tileSize + (float)Game1.pixelZoom), new Rectangle?(sourceRect), Color.White, -0.7853982f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                    break;
                                case 2:
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 16f, (float)((double)playerPosition.Y - (double)(Game1.tileSize / 3) + 20.0) - (float)Game1.tileSize), new Rectangle?(sourceRect), Color.White, -0.7853982f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                    break;
                                case 3:
                                    if (facingDirection == 2)
                                    {
                                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize + (float)(Game1.pixelZoom * 2), playerPosition.Y + (float)(Game1.tileSize / 2)), new Rectangle?(sourceRect), Color.White, -3.926991f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                        break;
                                    }
                                    spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 16f, (float)((double)playerPosition.Y - (double)(Game1.tileSize / 3) + 32.0) - (float)Game1.tileSize), new Rectangle?(sourceRect), Color.White, -0.7853982f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                    break;
                                case 4:
                                    if (facingDirection == 2)
                                    {
                                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize + (float)(Game1.pixelZoom * 2), playerPosition.Y + (float)(Game1.tileSize / 2)), new Rectangle?(sourceRect), Color.White, -3.926991f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                        break;
                                    }
                                    break;
                                case 5:
                                    if (facingDirection == 2)
                                    {
                                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize + 12.0), (float)((double)playerPosition.Y + (double)Game1.tileSize - 20.0)), new Rectangle?(sourceRect), Color.White, 2.356194f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                        break;
                                    }
                                    break;
                                case 6:
                                    if (facingDirection == 2)
                                    {
                                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize + 12.0), (float)((double)playerPosition.Y + (double)Game1.tileSize + 54.0)), new Rectangle?(sourceRect), Color.White, 2.356194f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                        break;
                                    }
                                    break;
                                case 7:
                                    if (facingDirection == 2)
                                    {
                                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize + 12.0), (float)((double)playerPosition.Y + (double)Game1.tileSize + 58.0)), new Rectangle?(sourceRect), Color.White, 2.356194f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                        break;
                                    }
                                    break;
                            }
                            if (f.facingDirection != 0)
                                return;
                            f.FarmerRenderer.draw(spriteBatch, f.FarmerSprite, f.FarmerSprite.SourceRect, f.getLocalPosition(Game1.viewport), new Vector2(0.0f, (float)(((double)f.yOffset + (double)(Game1.tileSize * 2) - (double)(f.GetBoundingBox().Height / 2)) / 4.0 + 4.0)), Math.Max(0.0f, (float)((double)f.getStandingY() / 10000.0 + 0.00989999994635582)), Color.White, 0.0f, f);
                        }
                    }
                }
                else if (facingDirection == 1)
                {
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 40f, (float)((double)playerPosition.Y - (double)Game1.tileSize + 8.0)), new Rectangle?(sourceRect), Color.White, -0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 56f, (float)((double)playerPosition.Y - (double)Game1.tileSize + 28.0)), new Rectangle?(sourceRect), Color.White, 0.0f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                            break;
                        case 2:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)Game1.pixelZoom, playerPosition.Y - (float)(4 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, 0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                            break;
                        case 3:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)Game1.pixelZoom, playerPosition.Y - (float)Game1.pixelZoom), new Rectangle?(sourceRect), Color.White, 1.570796f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 4:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)(7 * Game1.pixelZoom), playerPosition.Y + (float)Game1.pixelZoom), new Rectangle?(sourceRect), Color.White, 1.963495f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 5:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)(12 * Game1.pixelZoom), playerPosition.Y + (float)Game1.pixelZoom), new Rectangle?(sourceRect), Color.White, 2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 6:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)(12 * Game1.pixelZoom), playerPosition.Y + (float)Game1.pixelZoom), new Rectangle?(sourceRect), Color.White, 2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 7:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 16.0), (float)((double)playerPosition.Y + (double)Game1.tileSize + 12.0)), new Rectangle?(sourceRect), Color.White, 1.963495f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                    }
                }
                else if (facingDirection == 3)
                {
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - (float)(4 * Game1.pixelZoom), playerPosition.Y - (float)Game1.tileSize - (float)(4 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, 0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - (float)(12 * Game1.pixelZoom), playerPosition.Y - (float)Game1.tileSize + (float)(5 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, 0.0f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                            break;
                        case 2:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - (float)Game1.tileSize + (float)(8 * Game1.pixelZoom), playerPosition.Y + (float)(4 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                            break;
                        case 3:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.pixelZoom, playerPosition.Y + (float)(11 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -1.570796f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 4:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)(11 * Game1.pixelZoom), playerPosition.Y + (float)(13 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -1.963495f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 5:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)(20 * Game1.pixelZoom), playerPosition.Y + (float)(10 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 6:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)(20 * Game1.pixelZoom), playerPosition.Y + (float)(10 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 7:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 44f, playerPosition.Y + 96f), new Rectangle?(sourceRect), Color.White, -5.105088f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipVertically, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                    }
                }
                else if (facingDirection == 0)
                {
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 32f), new Rectangle?(sourceRect), Color.White, -2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 48f), new Rectangle?(sourceRect), Color.White, -1.570796f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 2:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), new Rectangle?(sourceRect), Color.White, -3f * (float)Math.PI / 8f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 3:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), new Rectangle?(sourceRect), Color.White, -0.3926991f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 4:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 8.0), playerPosition.Y - 40f), new Rectangle?(sourceRect), Color.White, 0.0f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 5:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize, playerPosition.Y - 40f), new Rectangle?(sourceRect), Color.White, 0.3926991f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 6:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize, playerPosition.Y - 40f), new Rectangle?(sourceRect), Color.White, 0.3926991f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 7:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 44.0), playerPosition.Y + (float)Game1.tileSize), new Rectangle?(sourceRect), Color.White, -1.963495f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                    }
                }
                else
                {
                    if (facingDirection != 2)
                        return;
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 16f), new Rectangle?(sourceRect), Color.White, 0.3926991f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 52f, playerPosition.Y - 8f), new Rectangle?(sourceRect), Color.White, 1.570796f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                            break;
                        case 2:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 40f, playerPosition.Y), new Rectangle?(sourceRect), Color.White, 1.570796f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                            break;
                        case 3:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 16f, playerPosition.Y + 4f), new Rectangle?(sourceRect), Color.White, 2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                            break;
                        case 4:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 8f, playerPosition.Y + 8f), new Rectangle?(sourceRect), Color.White, 3.141593f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                            break;
                        case 5:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 12f, playerPosition.Y), new Rectangle?(sourceRect), Color.White, 3.534292f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                            break;
                        case 6:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 12f, playerPosition.Y), new Rectangle?(sourceRect), Color.White, 3.534292f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                            break;
                        case 7:
                            spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 44f, playerPosition.Y + (float)Game1.tileSize), new Rectangle?(sourceRect), Color.White, -5.105088f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                            break;
                    }
                }
            }
            else
            {
                frameOfFarmerAnimation %= 2;
                if (facingDirection == 1)
                {
                    if (frameOfFarmerAnimation != 0)
                    {
                        if (frameOfFarmerAnimation != 1)
                            return;
                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)(2 * Game1.pixelZoom), playerPosition.Y - 24f), new Rectangle?(sourceRect), Color.White, 0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                    }
                    else
                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)(4 * Game1.pixelZoom), playerPosition.Y - 16f), new Rectangle?(sourceRect), Color.White, 0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                }
                else if (facingDirection == 3)
                {
                    if (frameOfFarmerAnimation != 0)
                    {
                        if (frameOfFarmerAnimation != 1)
                            return;
                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)(2 * Game1.pixelZoom), playerPosition.Y - 24f), new Rectangle?(sourceRect), Color.White, -2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                    }
                    else
                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)(4 * Game1.pixelZoom), playerPosition.Y - 16f), new Rectangle?(sourceRect), Color.White, -2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                }
                else if (facingDirection == 0)
                {
                    if (frameOfFarmerAnimation != 0)
                    {
                        if (frameOfFarmerAnimation != 1)
                            return;
                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 16.0), playerPosition.Y - (float)(12 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2) / 10000f));
                    }
                    else
                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 4.0), playerPosition.Y - (float)(10 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2) / 10000f));
                }
                else
                {
                    if (facingDirection != 2)
                        return;
                    if (frameOfFarmerAnimation != 0)
                    {
                        if (frameOfFarmerAnimation != 1)
                            return;
                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)(Game1.tileSize / 3), playerPosition.Y), new Rectangle?(sourceRect), Color.White, 2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                    }
                    else
                        spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + (float)(Game1.tileSize / 2), playerPosition.Y - 12f), new Rectangle?(sourceRect), Color.White, 2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                }
            }
        }
    }
}
