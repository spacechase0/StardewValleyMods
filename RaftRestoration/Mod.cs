using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace RaftRestoration
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.Display.MenuChanged += onMenuChanged;

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            harmony.Patch(AccessTools.Method(typeof(Tool), nameof(Tool.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }), prefix: new HarmonyMethod(this.GetType().GetMethod("Prefix_Tool_drawInMenu")));
            harmony.Patch(AccessTools.Method(typeof(Raft), nameof(Raft.DoFunction)), prefix: new HarmonyMethod(this.GetType().GetMethod("Prefix_Raft_DoFunction")));
            harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.moveRaft)), postfix: new HarmonyMethod(this.GetType().GetMethod("Postfix_Farmer_moveRaft")));
            harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) }), transpiler: new HarmonyMethod(this.GetType().GetMethod("Transpiler_Farmer_draw")));
        }

        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu shop)
            {
                if (shop.portraitPerson?.Name != "Pierre")
                    return;

                var raft = new Raft();
                shop.forSale.Add(raft);
                shop.itemPriceAndStock.Add(raft, new int[] { 5000, 1 });
            }
        }

        public static Rectangle MakeRectangleHelper(int x, int y, int w, int h)
        {
            return new Rectangle(x, y, w, h);
        }

        public static bool Prefix_Tool_drawInMenu(Tool __instance,
            SpriteBatch spriteBatch,
            Vector2 location,
            float scaleSize,
            float transparency,
            float layerDepth,
            StackDrawType drawStackNumber,
            Color color,
            bool drawShadow)
        {
            if (!(__instance is Raft))
                return true;

            spriteBatch.Draw(Game1.toolSpriteSheet, location + new Vector2(32f, 32f), new Rectangle(16, 0, 16, 16), color * transparency, 0.0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
            return false;
        }

        public static bool Prefix_Raft_DoFunction(Raft __instance, GameLocation location, ref int x, ref int y, int power, Farmer who)
        {
            //x = (int) who.lastClick.X;
            //y = (int) who.lastClick.Y;
            who.CanMove = true;
            /*Rectangle position = new Rectangle(x - 32, y - 32, 64, 64);
            if (location.isCollidingPosition( position, Game1.viewport, true ))
            {
                Log.debug( "was colliding" );
                switch ( who.FacingDirection )
                {
                    case 0: y -= 64; break;
                    case 1: x += 64; break;
                    case 2: y += 64; break;
                    case 3: x -= 64; break;
                }
            }*/

            if (!who.isRafting && location.doesTileHaveProperty(x / 64, y / 64, "Water", "Back") != null)
            {
                who.isRafting = true;
                Rectangle position = new Rectangle(x - 32, y - 32, 64, 64);
                /*
                if ( location.isCollidingPosition( position, Game1.viewport, true ) )
                {
                    who.isRafting = false;
                    return false;
                }
                */
                int xo = 0, yo = 0;
                /*switch ( who.FacingDirection )
                {
                    case 0: yo -= 64; break;
                    case 1: xo += 64; break;
                    case 2: yo += 64; break;
                    case 3: xo -= 64; break;
                }*/
                who.xVelocity = who.FacingDirection == 1 ? 3f : (who.FacingDirection == 3 ? -3f : 0.0f);
                who.yVelocity = who.FacingDirection == 2 ? 3f : (who.FacingDirection == 0 ? -3f : 0.0f);
                who.Position = new Vector2((float)(x - 32) + xo, (float)(y - 32 - 32 - (y < who.getStandingY() ? 64 : 0)) + yo);
                Game1.playSound("dropItemInWater");
            }
            __instance.CurrentParentTileIndex = __instance.IndexOfMenuItemView;

            return false;
        }

        public static void Postfix_Farmer_moveRaft(Farmer __instance, GameLocation currentLocation, GameTime time)
        {
            __instance.position.X += __instance.xVelocity;
            __instance.position.Y += __instance.yVelocity;
            Rectangle r = new Rectangle((int)(__instance.Position.X), (int)(__instance.Position.Y + 16), 64, 64);

            // 2 - down, 0 - up, 3 - left, 1 - right
            //Log.trace( "meow: " + ( __instance.movementDirections.Count > 0 ?__instance.movementDirections[0]:-1));
            if (__instance.movementDirections.Contains(2) || __instance.movementDirections.Contains(0))
            {
                Vector2 pos = new Vector2(r.X + r.Width / 2, r.Y + r.Height);
                var xPos = new xTile.Dimensions.Location((int)pos.X, (int)pos.Y);
                xTile.ObjectModel.PropertyValue propVal = null;
                currentLocation.map.GetLayer("Back").PickTile(xPos, Game1.viewport.Size)?.TileIndexProperties.TryGetValue("Water", out propVal);
                if (propVal == null)
                {
                    if (currentLocation.isTileLocationOpen(xPos))
                    {
                        Game1.player.isRafting = false;
                        Game1.player.Position = pos;
                        //Game1.player.position.Y -= 64 + 16;
                        Game1.player.setTrajectory(0, 0);
                    }
                    return;
                }

                pos = new Vector2(r.X + r.Width / 2, r.Y);
                xPos = new xTile.Dimensions.Location((int)pos.X, (int)pos.Y);
                propVal = null;
                currentLocation.map.GetLayer("Back").PickTile(xPos, Game1.viewport.Size)?.TileIndexProperties.TryGetValue("Water", out propVal);
                if (propVal == null)
                {
                    if (currentLocation.isTileLocationOpen(xPos))
                    {
                        Game1.player.isRafting = false;
                        Game1.player.Position = pos;
                        Game1.player.position.Y -= 64 + 16;
                        Game1.player.setTrajectory(0, 0);
                    }
                    return;
                }
            }

            if (__instance.movementDirections.Contains(3))
            {
                Vector2 pos = new Vector2(r.X, r.Y + r.Height / 2);
                var xPos = new xTile.Dimensions.Location((int)pos.X, (int)pos.Y);
                xTile.ObjectModel.PropertyValue propVal = null;
                currentLocation.map.GetLayer("Back").PickTile(xPos, Game1.viewport.Size)?.TileIndexProperties.TryGetValue("Water", out propVal);
                if (propVal == null)
                {
                    if (currentLocation.isTileLocationOpen(xPos))
                    {
                        Game1.player.isRafting = false;
                        Game1.player.Position = pos;
                        Game1.player.position.Y -= 64 + 16;
                        Game1.player.setTrajectory(0, 0);
                    }
                    return;
                }

                pos = new Vector2(r.X + r.Width, r.Y + r.Height / 2);
                xPos = new xTile.Dimensions.Location((int)pos.X, (int)pos.Y);
                propVal = null;
                currentLocation.map.GetLayer("Back").PickTile(xPos, Game1.viewport.Size)?.TileIndexProperties.TryGetValue("Water", out propVal);
                if (propVal == null)
                {
                    if (currentLocation.isTileLocationOpen(xPos))
                    {
                        Game1.player.isRafting = false;
                        Game1.player.Position = pos;
                        Game1.player.position.Y -= 64 + 16;
                        Game1.player.setTrajectory(0, 0);
                    }
                    return;
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler_Farmer_draw(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            var ret = new List<CodeInstruction>();
            bool foundIt = false;
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Call && insn.operand is MethodInfo meth && meth == AccessTools.Method(typeof(Game1), nameof(Game1.getSourceRectForStandardTileSheet)))
                {
                    foundIt = true;
                    ret.RemoveRange(ret.Count - 4, 4);
                    ret.Add(new CodeInstruction(OpCodes.Ldc_I4, 16));
                    ret.Add(new CodeInstruction(OpCodes.Ldc_I4, 0));
                    ret.Add(new CodeInstruction(OpCodes.Ldc_I4, 16));
                    ret.Add(new CodeInstruction(OpCodes.Ldc_I4, 16));
                    insn.operand = AccessTools.Method(typeof(Mod), nameof(MakeRectangleHelper));
                }
                if (foundIt && insn.opcode == OpCodes.Ldc_R4 && ((float)insn.operand) == 1f)
                {
                    insn.operand = 4f;
                    foundIt = false;
                }
                ret.Add(insn);
            }
            return ret;
        }
    }
}
