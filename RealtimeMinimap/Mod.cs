using System;
using System.Linq;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealtimeMinimap.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace RealtimeMinimap
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Configuration Config;

        public const int BORDER_WIDTH = 12;

        private static readonly PerScreen<State> _state = new();

        public static State State
        {
            get
            {
                if (Mod._state.Value == null)
                    Mod._state.Value = new State() { ShowMinimap = Mod.Config.ShowByDefault };
                return Mod._state.Value;
            }
        }

        private static Timer timer;

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            Mod.Config = this.Helper.ReadConfig<Configuration>();

            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            this.Helper.Events.Player.Warped += this.OnWarped;
            this.Helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            this.Helper.Events.Display.RenderedHud += this.OnRenderedHud;

            Mod.timer = new Timer();
            Mod.timer.Elapsed += (s, e) => { foreach (var state in Mod._state.GetActiveValues()) state.Value.RenderQueue.Add(Game1.currentLocation.NameOrUniqueName); };
            Mod.timer.AutoReset = true;
            this.ResetTimer();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => Mod.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(Mod.Config)
                );

                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => "Show by default",
                    tooltip: () => "Whether or not the minimap should be shown by default.\nYou must restart the game for this to take effect.",
                    getValue: () => Mod.Config.ShowByDefault,
                    setValue: value => Mod.Config.ShowByDefault = value
                );
                configMenu.AddKeybindList(
                    mod: this.ModManifest,
                    name: () => "Toggle shown key",
                    tooltip: () => "Key to toggle showing the minimap.",
                    getValue: () => Mod.Config.ToggleShowKey,
                    setValue: value => Mod.Config.ToggleShowKey = value
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "Update Interval",
                    tooltip: () => "The interval, in milliseconds, that the minimap will update. 0 will be every frame. -1 will only do it when entering a new location. (Markers update every frame regardless.)",
                    getValue: () => Mod.Config.UpdateInterval,
                    setValue: value =>
                    {
                        Mod.Config.UpdateInterval = value;
                        this.ResetTimer();
                    }
                );

                configMenu.AddSectionTitle(
                    mod: this.ModManifest,
                    text: () => "Positioning & Size",
                    tooltip: () => "Options pertaining to the placement of the minimap."
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "Minimap Anchor X",
                    tooltip: () => "The percentage of the screen's width where the top-left of the minimap will be placed.",
                    getValue: () => Mod.Config.MinimapAnchorX,
                    setValue: value => Mod.Config.MinimapAnchorX = value,
                    min: 0,
                    max: 1
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "Minimap Anchor Y",
                    tooltip: () => "The percentage of the screen's height where the top-left of the minimap will be placed.",
                    getValue: () => Mod.Config.MinimapAnchorY,
                    setValue: value => Mod.Config.MinimapAnchorY = value,
                    min: 0,
                    max: 1
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "Minimap Offset X",
                    tooltip: () => "The X offset from the anchor that the minimap will be placed at.",
                    getValue: () => Mod.Config.MinimapOffsetX,
                    setValue: value => Mod.Config.MinimapOffsetX = value
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "Minimap Offset Y",
                    tooltip: () => "The Y offset from the anchor that the minimap will be placed at.",
                    getValue: () => Mod.Config.MinimapOffsetY,
                    setValue: value => Mod.Config.MinimapOffsetY = value
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "Minimap Size",
                    tooltip: () => "The size of the minimap, in pixels (before UI scale).",
                    getValue: () => Mod.Config.MinimapSize,
                    setValue: value => Mod.Config.MinimapSize = value
                );

                configMenu.AddSectionTitle(
                    mod: this.ModManifest,
                    text: () => "Markers",
                    tooltip: () => "Options pertaining to rendering markers on the map."
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "Player Heads",
                    tooltip: () => "Render scale for the head of a player. 0 disables it.",
                    getValue: () => Mod.Config.RenderHeads,
                    setValue: value => Mod.Config.RenderHeads = value,
                    min: 0,
                    max: 4
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "NPC Heads",
                    tooltip: () => "Render scale for the head of an NPC. 0 disables it.",
                    getValue: () => Mod.Config.RenderNpcs,
                    setValue: value => Mod.Config.RenderNpcs = value,
                    min: 0,
                    max: 4
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "Wood Signs",
                    tooltip: () => "Render scale for items held on wooden signs . 0 disables it.",
                    getValue: () => Mod.Config.RenderWoodSigns,
                    setValue: value => Mod.Config.RenderWoodSigns = value,
                    min: 0,
                    max: 4
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "Stone Signs",
                    tooltip: () => "Render scale for items held on stone signs. 0 disables it.",
                    getValue: () => Mod.Config.RenderStoneSigns,
                    setValue: value => Mod.Config.RenderStoneSigns = value,
                    min: 0,
                    max: 4

                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "Dark Signs",
                    tooltip: () => "Render scale for items held on dark signs. 0 disables it.",
                    getValue: () => Mod.Config.RenderDarkSigns,
                    setValue: value => Mod.Config.RenderDarkSigns = value,
                    min: 0,
                    max: 4
                );
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!Mod.State.ShowMinimap)
                return;
            Mod.State.RenderQueue.Add(e.NewLocation.NameOrUniqueName);
        }

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.currentLocation == null)
                return;

            if (Mod.Config.ToggleShowKey.JustPressed())
            {
                Mod.State.ShowMinimap = !Mod.State.ShowMinimap;
                this.Helper.Input.SuppressActiveKeybinds(Mod.Config.ToggleShowKey);
            }
            else /*if ( Config.MapKey.JustPressed() )
            {
                if ( Game1.activeClickableMenu == null )
                    Game1.activeClickableMenu = new MyMapMenu();
                else if ( Game1.activeClickableMenu is MyMapMenu )
                    Game1.activeClickableMenu = null;
                Helper.Input.SuppressActiveKeybinds( Config.MapKey );
            }*/

            if (Mod.Config.UpdateInterval == 0 && !Mod.State.RenderQueue.Contains(Game1.currentLocation.NameOrUniqueName))
            {
                Mod.State.RenderQueue.Add(Game1.currentLocation.NameOrUniqueName);
            }

            if (Mod.State.RenderQueue.Count == 0)
                return;
            string mapName = Mod.State.RenderQueue[0];
            Mod.State.RenderQueue.RemoveAt(0);
            var map = Game1.getLocationFromName(mapName);

            if (Mod.State.MinimapTarget == null || Mod.State.MinimapTarget.Width != map.Map.DisplayWidth || Mod.State.MinimapTarget.Height != map.Map.DisplayHeight)
            {
                var Game1__lightmap = this.Helper.Reflection.GetField<RenderTarget2D>(typeof(Game1), "_lightmap");
                var Game1_allocateLightmap = this.Helper.Reflection.GetMethod(typeof(Game1), "allocateLightmap");

                var oldLightmap = Game1__lightmap.GetValue();
                Game1__lightmap.SetValue(null);

                Mod.State.MinimapTarget?.Dispose();
                Mod.State.MinimapTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, map.Map.DisplayWidth, map.Map.DisplayHeight, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                Game1_allocateLightmap.Invoke(map.Map.DisplayWidth, map.Map.DisplayHeight);
                Mod.State.MinimapLightmap = Game1__lightmap.GetValue();

                Game1__lightmap.SetValue(oldLightmap);
            }

            this.RenderMap(map);
        }

        [EventPriority(EventPriority.High)] // So we run before XP bars
        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (!Mod.State.ShowMinimap || Game1.eventUp || !Context.IsPlayerFree)
                return;

            if (!Mod.State.Locations.ContainsKey(Game1.currentLocation.NameOrUniqueName))
            {
                if (!Mod.State.RenderQueue.Contains(Game1.currentLocation.NameOrUniqueName))
                    Mod.State.RenderQueue.Add(Game1.currentLocation.NameOrUniqueName);
                return;
            }

            var b = e.SpriteBatch;

            int x = (int)(Game1.game1.localMultiplayerWindow.Width * Mod.Config.MinimapAnchorX) + Mod.Config.MinimapOffsetX;
            int y = (int)(Game1.game1.localMultiplayerWindow.Height * Mod.Config.MinimapAnchorY) + Mod.Config.MinimapOffsetY;

            IClickableMenu.drawTextureBox(b, x, y, Mod.Config.MinimapSize, Mod.Config.MinimapSize, Color.White);

            Texture2D mapTex = Mod.State.Locations[Game1.currentLocation.NameOrUniqueName];

            float actualSize = Mod.Config.MinimapSize - Mod.BORDER_WIDTH * 2;
            float scale = actualSize / Math.Max(mapTex.Width, mapTex.Height);
            var rect = new Rectangle(x + Mod.BORDER_WIDTH, y + Mod.BORDER_WIDTH, (int)(mapTex.Width * scale), (int)(mapTex.Height * scale));
            if (rect.Width < rect.Height)
                rect.X += (int)(actualSize - rect.Width) / 2;
            else if (rect.Width > rect.Height)
                rect.Y += (int)(actualSize - rect.Height) / 2;
            b.Draw(Game1.staminaRect, new Rectangle(x + Mod.BORDER_WIDTH, y + Mod.BORDER_WIDTH, (int)actualSize, (int)actualSize), Color.Black);
            b.Draw(mapTex, rect, Color.White);

            foreach (var obj in Game1.currentLocation.objects.Values.Where(o => o.bigCraftable && (o.ParentSheetIndex is 37 or 38 or 39) && (o as StardewValley.Objects.Sign).displayItem.Value != null))
            {
                var pos = obj.TileLocation * Game1.tileSize;
                float s = Mod.Config.RenderWoodSigns;
                if (obj.ParentSheetIndex == 38)
                    s = Mod.Config.RenderStoneSigns;
                else if (obj.ParentSheetIndex == 39)
                    s = Mod.Config.RenderDarkSigns;
                if (s <= 0)
                    continue;
                (obj as StardewValley.Objects.Sign).displayItem.Value.drawInMenu(b, new Vector2(rect.X + (pos.X / Game1.currentLocation.map.DisplayWidth) * rect.Width - 12 * s, rect.Y + (pos.Y / Game1.currentLocation.map.DisplayHeight) * rect.Height - 12 * s), s / 4f);
            }
            if (Mod.Config.RenderNpcs > 0)
            {
                foreach (var character in Game1.currentLocation.characters.OfType<NPC>())
                {
                    if (!character.isVillager())
                        continue;
                    b.Draw(character.Sprite.Texture, new Vector2(rect.X + (character.getStandingPosition().X / Game1.currentLocation.map.DisplayWidth) * rect.Width - 8 * Mod.Config.RenderHeads, rect.Y + (character.getStandingPosition().Y / Game1.currentLocation.map.DisplayHeight) * rect.Height - 10 * Mod.Config.RenderHeads), new Rectangle(0, 3, 16, 16), Color.White, 0, Vector2.Zero, Mod.Config.RenderNpcs, SpriteEffects.None, 1);
                }
            }
            if (Mod.Config.RenderHeads > 0)
            {
                foreach (var farmer in Game1.currentLocation.farmers)
                {
                    farmer.FarmerRenderer.drawMiniPortrat(b, new Vector2(rect.X + (farmer.getStandingPosition().X / Game1.currentLocation.map.DisplayWidth) * rect.Width - 8 * Mod.Config.RenderHeads, rect.Y + (farmer.getStandingPosition().Y / Game1.currentLocation.map.DisplayHeight) * rect.Height - 10 * Mod.Config.RenderHeads), 1, Mod.Config.RenderHeads, 0, farmer);
                }
            }
        }

        private void ResetTimer()
        {
            if (Mod.Config.UpdateInterval > 0)
            {
                Mod.timer.Interval = Mod.Config.UpdateInterval;
                Mod.timer.Enabled = true;
            }
            else
                Mod.timer.Enabled = false;
        }

        private void RenderMap(GameLocation map)
        {
            var oldTarget = Game1.graphics.GraphicsDevice.GetRenderTargets()?.Length == 0 ? null : Game1.graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget;
            var oldLoc = Game1.currentLocation;

            Game1.currentLocation = map;

            var Game1__lightmap = this.Helper.Reflection.GetField<RenderTarget2D>(typeof(Game1), "_lightmap");
            var Game1__draw = this.Helper.Reflection.GetMethod(Game1.game1, "_draw");

            xTile.Dimensions.Rectangle old_viewport = Game1.viewport;
            bool old_display_hud = Game1.displayHUD;
            Game1.game1.takingMapScreenshot = true;
            float old_zoom_level = Game1.options.baseZoomLevel;
            Game1.options.baseZoomLevel = 1f;
            RenderTarget2D cached_lightmap = Game1__lightmap.GetValue();
            Game1__lightmap.SetValue(null);
            bool fail = false;
            try
            {
                Game1__lightmap.SetValue(Mod.State.MinimapLightmap);
                Game1.viewport = new xTile.Dimensions.Rectangle(0, 0, Game1.currentLocation.Map.DisplayWidth, Game1.currentLocation.Map.DisplayHeight);
                Game1__draw.Invoke(Game1.currentGameTime, Mod.State.MinimapTarget);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error rendering map: " + e.ToString());
                fail = true;
            }
            if (Game1__lightmap.GetValue() != null)
            {
                Game1__lightmap.SetValue(null);
            }
            Game1__lightmap.SetValue(cached_lightmap);
            Game1.options.baseZoomLevel = old_zoom_level;
            Game1.game1.takingMapScreenshot = false;
            Game1.displayHUD = old_display_hud;
            Game1.viewport = old_viewport;
            Game1.currentLocation = oldLoc;
            Game1.game1.GraphicsDevice.SetRenderTarget((RenderTarget2D)oldTarget);

            Texture2D tex = null;
            if (Mod.State.Locations.ContainsKey(map.NameOrUniqueName))
                tex = Mod.State.Locations[map.NameOrUniqueName];
            else
            {
                tex = new Texture2D(Game1.graphics.GraphicsDevice, map.Map.DisplayWidth, map.map.DisplayHeight);
                Mod.State.Locations.Add(map.NameOrUniqueName, tex);
            }

            var colors = new Color[Mod.State.MinimapTarget.Width * Mod.State.MinimapTarget.Height];
            Mod.State.MinimapTarget.GetData(colors);
            tex.SetData(colors);
        }
    }
}
