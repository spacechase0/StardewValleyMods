using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace QiChest
{
    [XmlType( "Mods_spacechase0_QiChest_Contents" )]
    public class QiChestContents : INetObject<NetFields>
    {
        public NetFields NetFields { get; }  = new(nameof(QiChestContents));

        public readonly NetInt Code = new();
        public readonly NetObjectList<Item> Items = new();

        public QiChestContents()
        {
            NetFields.SetOwner( this )
                .AddField(Code)
                .AddField(Items);
        }
    }

    [HarmonyPatch(typeof(NetWorldState), MethodType.Constructor)]
    public static class NetWorldStateFieldPatch
    {
        internal static ConditionalWeakTable<NetWorldState, NetObjectList<QiChestContents>> values = new();

        public static void Postfix(NetWorldState __instance)
        {
            __instance.NetFields.AddField(values.GetOrCreateValue(__instance));
        }

        public static NetObjectList<QiChestContents> GetQiChests(this NetWorldState team)
        {
            return values.GetOrCreateValue(team);
        }
    }

    public static class SaveGameField
    {
        private static ConditionalWeakTable< SaveGame, NetObjectList<QiChestContents>> values = new();

        public static void set_qiChests(this SaveGame farmer, NetObjectList<QiChestContents> newVal)
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetObjectList<QiChestContents> get_qiChests(this SaveGame farmer)
        {
            var val = values.GetOrCreateValue(farmer);

            var st = new System.Diagnostics.StackTrace();
            for (int i = 0; i < st.FrameCount; ++i)
            {
                if (st.GetFrame(i).GetMethod().Name.Contains("getSaveEnumerator"))
                {
                    val.CopyFrom(Game1.netWorldState.Value.GetQiChests());
                    break;
                }
            }


            return val;
        }
    }

    [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.loadDataToFarmer))]
    public static class LoadQiChestsPatch
    {
        public static void Postfix()
        {
            if (SaveGame.loaded == null)
                return;

            Game1.netWorldState.Value.GetQiChests().CopyFrom(SaveGame.loaded.get_qiChests());
            foreach (var chest in Game1.netWorldState.Value.GetQiChests())
            {
                string key = "QiChest_" + chest.Code.ToString();

                if (!SaveGame.loaded.globalInventories.ContainsKey(key))
                {
                    List<Item> inv = new();
                    foreach (var item in chest.Items)
                    {
                        inv.Add(item);
                    }
                    SaveGame.loaded.globalInventories.Add(key, inv);

                    Mod.migrated = true;
                }
            }
        }
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public const string ModDataKey = "spacechase0.QiChest/NetworkCode";

        public static Texture2D TextureChestBase;
        public static Texture2D TextureChestCode0;
        public static Texture2D TextureChestCode1;
        public static Texture2D TextureChestCode2;

        public static bool migrated = false;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            TextureChestBase = Helper.ModContent.Load<Texture2D>("assets/chest-base.png");
            TextureChestCode0 = Helper.ModContent.Load<Texture2D>("assets/chest-code-0.png");
            TextureChestCode1 = Helper.ModContent.Load<Texture2D>("assets/chest-code-1.png");
            TextureChestCode2 = Helper.ModContent.Load<Texture2D>("assets/chest-code-2.png");

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Helper.Events.Display.RenderedActiveMenu += this.Display_RenderedActiveMenu;
            Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            Helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;

            Harmony harmony = new(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (migrated)
            {
                Utility.ForEachItem((item) =>
                {
                    if ( item is Chest c && c.SpecialChestType == Chest.SpecialChestTypes.JunimoChest && c.modData.ContainsKey(ModDataKey) )
                    {
                        c.GlobalInventoryId = $"QiChest_{c.modData[ModDataKey]}";
                    }
                    return true;
                });
                migrated = false;
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton() && Game1.input.GetKeyboardState().IsKeyDown(Keys.LeftShift))
            {
                var ca = Game1.netWorldState.Value.GetQiChests();
                Vector2 tile = e.Cursor.GrabTile;
                if (Game1.currentLocation.objects.TryGetValue(tile, out var o) && o is Chest c)
                {
                    if (c.SpecialChestType == Chest.SpecialChestTypes.JunimoChest)
                    {
                        if (!c.modData.ContainsKey(ModDataKey))
                            c.modData.Add(ModDataKey, 0.ToString());
                    }
                }
            }
        }

        private MouseState oldMouseState;
        private MouseState oldMouseState2;
        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            oldMouseState2 = oldMouseState;
            oldMouseState = Game1.input.GetMouseState();
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(QiChestContents));
            sc.RegisterCustomProperty(typeof(SaveGame), "qiChests", typeof(NetObjectList<QiChestContents>), AccessTools.Method(typeof(SaveGameField), nameof(SaveGameField.get_qiChests)), AccessTools.Method(typeof(SaveGameField), nameof(SaveGameField.set_qiChests)));
        }

        private void Display_RenderedActiveMenu(object sender, StardewModdingAPI.Events.RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is ItemGrabMenu igm && igm.context is Chest c)
            {
                if (c.SpecialChestType != Chest.SpecialChestTypes.JunimoChest || !c.modData.ContainsKey(ModDataKey))
                    return;
                c.GlobalInventoryId = $"QiChest_{c.modData[ModDataKey]}";

                Vector2 size = Game1.dialogueFont.MeasureString(I18n.Ui_ChangeCode());
                Rectangle spot = new( igm.xPositionOnScreen, igm.yPositionOnScreen - (int)size.Y - IClickableMenu.borderWidth * 2, (int)size.X + IClickableMenu.borderWidth * 2, (int)size.Y + IClickableMenu.borderWidth * 2 );
                IClickableMenu.drawTextureBox(e.SpriteBatch, spot.X, spot.Y, spot.Width, spot.Height, Color.White);
                e.SpriteBatch.DrawString( Game1.dialogueFont, I18n.Ui_ChangeCode(), new Vector2( spot.X + IClickableMenu.borderWidth, spot.Y + IClickableMenu.borderWidth ), Color.Black );

                if (spot.Contains(Game1.getMouseX(), Game1.getMouseY()))
                {
                    if (oldMouseState2.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released &&
                        Game1.input.GetMouseState().LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                    {
                        Game1.activeClickableMenu = new QiCodeMenu( igm.context as Chest );
                    }
                }

                Game1.activeClickableMenu.drawMouse(e.SpriteBatch);
            }
        }

        private void AddQiChest(string cmd, string[] args)
        {
            if (!Context.IsPlayerFree)
                return;

            Chest item = new(true, Vector2.Zero, "256");
            item.SpecialChestType = Chest.SpecialChestTypes.JunimoChest;
            item.modData.Add(ModDataKey, Extensions.MakeQiCode(0, 0, 0).ToString());
            item.GlobalInventoryId = "QiChest_" + Extensions.MakeQiCode(0, 0, 0).ToString();
            Game1.player.addItemByMenuIfNecessary(item);
        }
    }

    public static class Extensions
    {
        public static int ExtractQiCode(this int i, int num)
        {
            return (i >> (num * 4)) & 0xF;
        }

        public static int AdjustQiCode(this int i, int num, int code)
        {
            int ret = 0;
            ret |= ((num == 0 ? code : i.ExtractQiCode(0)) << 0) & 0x00F;
            ret |= ((num == 1 ? code : i.ExtractQiCode(1)) << 4) & 0x0F0;
            ret |= ((num == 2 ? code : i.ExtractQiCode(2)) << 8) & 0xF00;
            return ret;
        }

        public static int MakeQiCode(int a, int b, int c)
        {
            return 0.AdjustQiCode(0, a).AdjustQiCode(1, b).AdjustQiCode(2, c);
        }
        /*
        public static QiChestContents GetChestFor(this NetWorldState team, int code)
        {
            var chest = team.GetQiChests().FirstOrDefault(qc => qc.Code == code);
            if (chest == null)
            {
                chest = new() { Code = { code } };
                team.GetQiChests().Add(chest);
            }
            return chest;
        }
        */
    }

    [HarmonyPatch(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
    public static class ChestDrawPatch
    {
        public static bool Prefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!__instance.modData.TryGetValue(Mod.ModDataKey, out string val))
                return true;

            try
            {
                int code = int.Parse(val);
                int code0 = code.ExtractQiCode(0);
                int code1 = code.ExtractQiCode(1);
                int code2 = code.ExtractQiCode(2);

                Color[] mapping = new Color[]
                {
                    new Color(119, 191, 255),
                    new Color(85, 85, 255),
                    new Color(0, 170, 170),
                    new Color(0, 234, 175),
                    new Color(0, 170, 0),
                    new Color(159, 236, 0),
                    new Color(255, 234, 18),
                    new Color(255, 167, 18),
                    new Color(255, 105, 18),
                    new Color(255, 0, 0),
                    new Color(135, 0, 35),
                    new Color(255, 173, 199),
                    new Color(255, 117, 195),
                    new Color(172, 0, 198),
                    new Color(143, 0, 255),
                    new Color(89, 11, 142),
                };

                int __instance_currentLidFrame = Mod.instance.Helper.Reflection.GetField< int >(__instance, "currentLidFrame").GetValue();
                __instance_currentLidFrame -= __instance.startingLidFrame;

                float draw_x = x;
                float draw_y = y;
                if (__instance.localKickStartTile.HasValue)
                {
                    draw_x = Utility.Lerp(__instance.localKickStartTile.Value.X, draw_x, __instance.kickProgress);
                    draw_y = Utility.Lerp(__instance.localKickStartTile.Value.Y, draw_y, __instance.kickProgress);
                }
                float base_sort_order = Math.Max(0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;
                if (__instance.localKickStartTile.HasValue)
                {
                    spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((draw_x + 0.5f) * 64f, (draw_y + 0.5f) * 64f)), Game1.shadowTexture.Bounds, Color.Black * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.0001f);
                    draw_y -= (float)Math.Sin((double)__instance.kickProgress * Math.PI) * 0.5f;
                }

                spriteBatch.Draw(Mod.TextureChestBase, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Mod.TextureChestBase, 0, 16, 32), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
                spriteBatch.Draw(Mod.TextureChestBase, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Mod.TextureChestBase, __instance_currentLidFrame, 16, 32), Color.White * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
                spriteBatch.Draw(Mod.TextureChestCode0, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Mod.TextureChestBase, 0, 16, 32), mapping[ code0 ] * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
                spriteBatch.Draw(Mod.TextureChestCode0, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Mod.TextureChestBase, __instance_currentLidFrame, 16, 32), mapping[code0] * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
                spriteBatch.Draw(Mod.TextureChestCode1, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Mod.TextureChestBase, 0, 16, 32), mapping[code1] * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
                spriteBatch.Draw(Mod.TextureChestCode1, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Mod.TextureChestBase, __instance_currentLidFrame, 16, 32), mapping[code1] * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
                spriteBatch.Draw(Mod.TextureChestCode2, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Mod.TextureChestBase, 0, 16, 32), mapping[code2] * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
                spriteBatch.Draw(Mod.TextureChestCode2, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Mod.TextureChestBase, __instance_currentLidFrame, 16, 32), mapping[code2] * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
                return false;
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e);
                return true;
            }
        }
    }
}
