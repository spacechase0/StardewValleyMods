using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static Microsoft.Xna.Framework.Input.ButtonState;
using StardewValley.Locations;
using StardewValley.Buildings;

namespace CustomizeExterior
{
    public class CustomizeExteriorMod : Mod
    {
        public static CustomizeExteriorMod instance;
        public static Config config;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            config = Helper.ReadConfig<Config>();

            GameEvents.UpdateTick += onUpdate;

            Log.info("MEOW");
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
                            checkBuildingClick(building.nameOfIndoors);
                        }
                    }
                }
            }

            prevMouse = mouse;
        }

        private DateTime recentClickTime;
        private string recentClickTarget = null;
        private void checkBuildingClick( string target )
        {
            if (recentClickTarget != target)
            {
                recentClickTarget = target;
                recentClickTime = DateTime.Now;
            }
            else
            {
                if (DateTime.Now - recentClickTime < config.clickWindow)
                    todoRenameFunction();
                else recentClickTime = DateTime.Now;
            }
        }

        private void todoRenameFunction()
        {
            Log.debug("Clicked soon enough");
        }
    }
}
