using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace RealtimeMinimap
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Configuration Config;

        public const int BORDER_WIDTH = 12;

        private static PerScreen<State> _state = new();
        public static State State
        {
            get
            {
                if ( _state.Value == null )
                    _state.Value = new State() { ShowMinimap = Config.ShowByDefault };
                return _state.Value;
            }
        }
        private static Timer timer;

        public override void Entry( StardewModdingAPI.IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;

            Config = Helper.ReadConfig<Configuration>();

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Player.Warped += OnWarped;
            Helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
            Helper.Events.Display.RenderedHud += OnRenderedHud;

            timer = new Timer();
            timer.Elapsed += (s, e) => { foreach ( var state in _state.GetActiveValues() ) state.Value.RenderQueue.Add( Game1.currentLocation.NameOrUniqueName ); };
            timer.AutoReset = true;
            ResetTimer();
        }

        private void OnGameLaunched( object sender, GameLaunchedEventArgs e )
        {
            var gmcm = Helper.ModRegistry.GetApi< IGenericModConfigMenuApi >( "spacechase0.GenericModConfigMenu" );
            if ( gmcm != null )
            {
                gmcm.RegisterModConfig( ModManifest, () => Config = new Configuration(), () => Helper.WriteConfig( Config ) );
                gmcm.SetDefaultIngameOptinValue( ModManifest, true );

                gmcm.RegisterSimpleOption( ModManifest, "Show by default", "Whether or not the minimap should be shown by default.\nYou must restart the game for this to take effect.", () => Config.ShowByDefault, ( v ) => Config.ShowByDefault = v );
                gmcm.RegisterSimpleOption( ModManifest, "Toggle shown key", "Key to toggle showing the minimap.", () => Config.ToggleShowKey, ( v ) => Config.ToggleShowKey = v );
                gmcm.RegisterSimpleOption( ModManifest, "Update Interval", "The interval, in milliseconds, that the minimap will update. 0 will be every frame. -1 will only do it when entering a new location. (Markers update every frame regardless.)", () => Config.UpdateInterval, ( v ) => { Config.UpdateInterval = v; ResetTimer(); } );

                gmcm.RegisterLabel( ModManifest, "Positioning & Size", "Options pertaining to the placement of the minimap." );
                gmcm.RegisterClampedOption( ModManifest, "Minimap Anchor X", "The percentage of the screen's width where the top-left of the minimap will be placed.", () => Config.MinimapAnchorX, ( v ) => Config.MinimapAnchorX = v, 0, 1 );
                gmcm.RegisterClampedOption( ModManifest, "Minimap Anchor Y", "The percentage of the screen's height where the top-left of the minimap will be placed.", () => Config.MinimapAnchorY, ( v ) => Config.MinimapAnchorY = v, 0, 1 );
                gmcm.RegisterSimpleOption( ModManifest, "Minimap Offset X", "The X offset from the anchor that the minimap will be placed at.", () => Config.MinimapOffsetX, ( v ) => Config.MinimapOffsetX = v );
                gmcm.RegisterSimpleOption( ModManifest, "Minimap Offset Y", "The Y offset from the anchor that the minimap will be placed at.", () => Config.MinimapOffsetY, ( v ) => Config.MinimapOffsetY = v );
                gmcm.RegisterSimpleOption( ModManifest, "Minimap Size", "The size of the minimap, in pixels (before UI scale).", () => Config.MinimapSize, ( v ) => Config.MinimapSize = v );

                gmcm.RegisterLabel( ModManifest, "Markers", "Options pertaining to rendering markers on the map." );
                gmcm.RegisterClampedOption( ModManifest, "Player Heads", "Render scale for the head of a player. 0 disables it.", () => Config.RenderHeads, ( v ) => Config.RenderHeads = v, 0, 4 );
                gmcm.RegisterClampedOption( ModManifest, "NPC Heads", "Render scale for the head of an NPC. 0 disables it.", () => Config.RenderNpcs, ( v ) => Config.RenderNpcs = v, 0, 4 );
                gmcm.RegisterClampedOption( ModManifest, "Wood Signs", "Render scale for items held on wooden signs . 0 disables it.", () => Config.RenderWoodSigns, ( v ) => Config.RenderWoodSigns = v, 0, 4 );
                gmcm.RegisterClampedOption( ModManifest, "Stone Signs", "Render scale for items held on stone signs. 0 disables it.", () => Config.RenderStoneSigns, ( v ) => Config.RenderStoneSigns = v, 0, 4 );
                gmcm.RegisterClampedOption( ModManifest, "Dark Signs", "Render scale for items held on dark signs. 0 disables it.", () => Config.RenderDarkSigns, ( v ) => Config.RenderDarkSigns = v, 0, 4 );
            }
        }

        private void OnWarped( object sender, WarpedEventArgs e )
        {
            if ( !State.ShowMinimap )
                return;
            State.RenderQueue.Add( e.NewLocation.NameOrUniqueName );
        }

        private void OnUpdateTicking( object sender, UpdateTickingEventArgs e )
        {
            if ( !Context.IsWorldReady || Game1.currentLocation == null )
                return;

            if ( Config.ToggleShowKey.JustPressed() )
            {
                State.ShowMinimap = !State.ShowMinimap;
                Helper.Input.SuppressActiveKeybinds( Config.ToggleShowKey );
            }
            else /*if ( Config.MapKey.JustPressed() )
            {
                if ( Game1.activeClickableMenu == null )
                    Game1.activeClickableMenu = new MyMapMenu();
                else if ( Game1.activeClickableMenu is MyMapMenu )
                    Game1.activeClickableMenu = null;
                Helper.Input.SuppressActiveKeybinds( Config.MapKey );
            }*/

            if ( Config.UpdateInterval == 0 && !State.RenderQueue.Contains( Game1.currentLocation.NameOrUniqueName ) )
            {
                State.RenderQueue.Add( Game1.currentLocation.NameOrUniqueName );
            }

            if ( State.RenderQueue.Count == 0 )
                return;
            string mapName = State.RenderQueue[ 0 ];
            State.RenderQueue.RemoveAt( 0 );
            var map = Game1.getLocationFromName( mapName );

            if ( State.MinimapTarget == null || State.MinimapTarget.Width != map.Map.DisplayWidth || State.MinimapTarget.Height != map.Map.DisplayHeight )
            {
                var Game1__lightmap = Helper.Reflection.GetField< RenderTarget2D >( typeof( Game1 ), "_lightmap" );
                var Game1_allocateLightmap = Helper.Reflection.GetMethod( typeof( Game1 ), "allocateLightmap" );

                var oldLightmap = Game1__lightmap.GetValue();
                Game1__lightmap.SetValue( null );

                State.MinimapTarget?.Dispose();
                State.MinimapTarget = new RenderTarget2D( Game1.graphics.GraphicsDevice, map.Map.DisplayWidth, map.Map.DisplayHeight, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents );
                Game1_allocateLightmap.Invoke( map.Map.DisplayWidth, map.Map.DisplayHeight );
                State.MinimapLightmap = Game1__lightmap.GetValue();

                Game1__lightmap.SetValue( oldLightmap );
            }
            RenderMap( map );
        }

        [EventPriority(EventPriority.High)] // So we run before XP bars
        private void OnRenderedHud( object sender, RenderedHudEventArgs e )
        {
            if ( !State.ShowMinimap || Game1.eventUp || !Context.IsPlayerFree )
                return;

            if ( !State.Locations.ContainsKey( Game1.currentLocation.NameOrUniqueName ) )
            {
                if ( !State.RenderQueue.Contains( Game1.currentLocation.NameOrUniqueName ) )
                    State.RenderQueue.Add( Game1.currentLocation.NameOrUniqueName );
                return;
            }

            var b = e.SpriteBatch;

            int x = ( int )( Game1.game1.localMultiplayerWindow.Width * Config.MinimapAnchorX ) + Config.MinimapOffsetX;
            int y = ( int )( Game1.game1.localMultiplayerWindow.Height * Config.MinimapAnchorY ) + Config.MinimapOffsetY;

            IClickableMenu.drawTextureBox( b, x, y, Config.MinimapSize, Config.MinimapSize, Color.White );

            Texture2D mapTex = State.Locations[ Game1.currentLocation.NameOrUniqueName ];

            float actualSize = Config.MinimapSize - BORDER_WIDTH * 2;
            float scale = actualSize / Math.Max( mapTex.Width, mapTex.Height );
            var rect = new Rectangle( x + BORDER_WIDTH, y + BORDER_WIDTH, ( int )( mapTex.Width * scale ), ( int )( mapTex.Height * scale ) );
            if ( rect.Width < rect.Height )
                rect.X += ( int ) ( actualSize - rect.Width ) / 2;
            else if ( rect.Width > rect.Height )
                rect.Y += ( int ) ( actualSize - rect.Height ) / 2;
            b.Draw( Game1.staminaRect, new Rectangle( x + BORDER_WIDTH, y + BORDER_WIDTH, ( int ) actualSize, ( int ) actualSize ), Color.Black );
            b.Draw( mapTex, rect, Color.White );

            foreach ( var obj in Game1.currentLocation.objects.Values.Where( o => o.bigCraftable && ( o.ParentSheetIndex == 37 || o.ParentSheetIndex == 38 || o.ParentSheetIndex == 39 ) && ( o as StardewValley.Objects.Sign ).displayItem.Value != null ) )
            {
                var pos = obj.TileLocation * Game1.tileSize;
                float s = Config.RenderWoodSigns;
                if ( obj.ParentSheetIndex == 38 )
                    s = Config.RenderStoneSigns;
                else if ( obj.ParentSheetIndex == 39 )
                    s = Config.RenderDarkSigns;
                if ( s <= 0 )
                    continue;
                ( obj as StardewValley.Objects.Sign ).displayItem.Value.drawInMenu( b, new Vector2( rect.X + ( pos.X / Game1.currentLocation.map.DisplayWidth ) * rect.Width - 12 * s, rect.Y + ( pos.Y / Game1.currentLocation.map.DisplayHeight ) * rect.Height - 12 * s ), s / 4f );
            }
            if ( Config.RenderNpcs > 0 )
            {
                foreach ( var character in Game1.currentLocation.characters.OfType<NPC>() )
                {
                    if ( !character.isVillager() )
                        continue;
                    b.Draw( character.Sprite.Texture, new Vector2( rect.X + ( character.getStandingPosition().X / Game1.currentLocation.map.DisplayWidth ) * rect.Width - 8 * Config.RenderHeads, rect.Y + ( character.getStandingPosition().Y / Game1.currentLocation.map.DisplayHeight ) * rect.Height - 10 * Config.RenderHeads ), new Rectangle( 0, 3, 16, 16 ), Color.White, 0, Vector2.Zero, Config.RenderNpcs, SpriteEffects.None, 1 );
                }
            }
            if ( Config.RenderHeads > 0 )
            {
                foreach ( var farmer in Game1.currentLocation.farmers )
                {
                    farmer.FarmerRenderer.drawMiniPortrat( b, new Vector2( rect.X + ( farmer.getStandingPosition().X / Game1.currentLocation.map.DisplayWidth ) * rect.Width - 8 * Config.RenderHeads, rect.Y + ( farmer.getStandingPosition().Y / Game1.currentLocation.map.DisplayHeight ) * rect.Height - 10 * Config.RenderHeads ), 1, Config.RenderHeads, 0, farmer );
                }
            }
        }

        private void ResetTimer()
        {
            if ( Config.UpdateInterval > 0 )
            {
                timer.Interval = Config.UpdateInterval;
                timer.Enabled = true;
            }
            else
                timer.Enabled = false;
        }

        private void RenderMap( GameLocation map )
        {
            var oldTarget = Game1.graphics.GraphicsDevice.GetRenderTargets()?.Length == 0 ? null : Game1.graphics.GraphicsDevice.GetRenderTargets()[ 0 ].RenderTarget;
            var oldLoc = Game1.currentLocation;

            Game1.currentLocation = map;

            var Game1__lightmap = Helper.Reflection.GetField< RenderTarget2D >( typeof( Game1 ), "_lightmap" );
            var Game1__draw = Helper.Reflection.GetMethod( Game1.game1, "_draw" );

            xTile.Dimensions.Rectangle old_viewport = Game1.viewport;
            bool old_display_hud = Game1.displayHUD;
            Game1.game1.takingMapScreenshot = true;
            float old_zoom_level = Game1.options.baseZoomLevel;
            Game1.options.baseZoomLevel = 1f;
            RenderTarget2D cached_lightmap = Game1__lightmap.GetValue();
            Game1__lightmap.SetValue( null );
            bool fail = false;
            try
            {
                Game1__lightmap.SetValue( State.MinimapLightmap );
                Game1.viewport = new xTile.Dimensions.Rectangle( 0, 0, Game1.currentLocation.Map.DisplayWidth, Game1.currentLocation.Map.DisplayHeight );
                Game1__draw.Invoke( Game1.currentGameTime, State.MinimapTarget );
            }
            catch ( Exception e )
            {
                Console.WriteLine( "Error rendering map: " + e.ToString() );
                fail = true;
            }
            if ( Game1__lightmap.GetValue() != null )
            {
                Game1__lightmap.SetValue( null );
            }
            Game1__lightmap.SetValue( cached_lightmap );
            Game1.options.baseZoomLevel = old_zoom_level;
            Game1.game1.takingMapScreenshot = false;
            Game1.displayHUD = old_display_hud;
            Game1.viewport = old_viewport;
            Game1.currentLocation = oldLoc;
            Game1.game1.GraphicsDevice.SetRenderTarget( ( RenderTarget2D ) oldTarget );

            Texture2D tex = null;
            if ( State.Locations.ContainsKey( map.NameOrUniqueName ) )
                tex = State.Locations[ map.NameOrUniqueName ];
            else
            {
                tex = new Texture2D( Game1.graphics.GraphicsDevice, map.Map.DisplayWidth, map.map.DisplayHeight );
                State.Locations.Add( map.NameOrUniqueName, tex );
            }

            var colors = new Color[ State.MinimapTarget.Width * State.MinimapTarget.Height ];
            State.MinimapTarget.GetData( colors );
            tex.SetData( colors );
        }
    }
}
