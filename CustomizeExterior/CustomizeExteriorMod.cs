using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static Microsoft.Xna.Framework.Input.ButtonState;
using StardewValley.Locations;
using StardewValley.Buildings;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace CustomizeExterior
{
    public class CustomizeExteriorMod : Mod
    {
        public static CustomizeExteriorMod instance;
        public static Config config;
        public static ContentManager content;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            config = Helper.ReadConfig<Config>();

            GameEvents.LoadContent += onLoad;
            GameEvents.UpdateTick += onUpdate;
        }

        private void onLoad(object sender, EventArgs args)
        {
            content = new ContentManager(Game1.content.ServiceProvider, Path.Combine(Helper.DirectoryPath, "Buildings"));
        }

        private MouseState prevMouse;
        private void onUpdate( object sender, EventArgs args)
        {
            MouseState mouse = Mouse.GetState();
            
            if ( prevMouse != null && mouse.RightButton == Pressed && prevMouse.RightButton != Pressed)
            {
                Point pos = new Point(Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y);

                if (Game1.currentLocation is BuildableGameLocation)
                {
                    var loc = Game1.currentLocation as BuildableGameLocation;
                    
                    foreach ( var building in loc.buildings )
                    {
                        Rectangle tileBounds = new Rectangle(building.tileX * Game1.tileSize, building.tileY * Game1.tileSize, building.tilesWide * Game1.tileSize, building.tilesHigh * Game1.tileSize);
                        if ( tileBounds.Contains( pos.X, pos.Y ) )
                        {
                            Log.trace("Right clicked a building: " + building.nameOfIndoors);
                            checkBuildingClick(building.nameOfIndoors, building.buildingType);
                        }
                    }
                }
            }

            prevMouse = mouse;
        }
        
        private DateTime recentClickTime;
        private string recentClickTarget = null;
        private string recentClickTargetType = null;
        private void checkBuildingClick( string target, string type )
        {
            if (Game1.activeClickableMenu != null) return;

            if (recentClickTarget != target)
            {
                recentClickTarget = target;
                recentClickTargetType = type;
                recentClickTime = DateTime.Now;
            }
            else
            {
                if (DateTime.Now - recentClickTime < config.clickWindow)
                    todoRenameFunction( target, type );
                else recentClickTime = DateTime.Now;
            }
        }

        private void todoRenameFunction( string target, string type )
        {
            Log.debug("Clicked soon enough");
            Log.debug("Target: " + target + " " + type);

            if (!config.choices.ContainsKey(type))
                return;

            foreach ( var choice in config.choices[ type ] )
            {
                Log.debug("Choice: " + choice);
            }

            Game1.activeClickableMenu = new SelectDisplayMenu(type, "");
        }
    }
}
