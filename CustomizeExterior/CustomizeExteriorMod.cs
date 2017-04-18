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
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CustomizeExterior
{
    public class CustomizeExteriorMod : Mod
    {
        public static CustomizeExteriorMod instance;
        public static Config config;
        public static ContentManager content;

        public static Dictionary<string, List<string>> choices = new Dictionary<string, List<string>>();

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
            compileChoices();
        }

        public MouseState prevMouse;
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

        private void compileChoices()
        {
            Log.info("Creating list of building choices...");
            var choices = Directory.GetDirectories(Path.Combine(Helper.DirectoryPath, "Buildings"));
            foreach ( var choice in choices )
            {
                var types = Directory.GetFiles(choice);
                foreach ( var type in types )
                {
                    Log.trace(Path.GetExtension(type));
                    if (Path.GetExtension( type ) != ".xnb")
                        continue;

                    string choiceStr = Path.GetFileName(choice);
                    string typeStr = Path.GetFileNameWithoutExtension(type);
                    List<string> forType = CustomizeExteriorMod.choices.ContainsKey(typeStr) ? CustomizeExteriorMod.choices[typeStr] : new List<string>();
                    forType.Add(choiceStr);
                    if (!CustomizeExteriorMod.choices.ContainsKey(typeStr))
                        CustomizeExteriorMod.choices.Add(typeStr, forType);

                    Log.trace("Added choice: " + typeStr + "::" + choiceStr);
                }
            }
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
            Log.debug("Target: " + target + " " + type);

            if (!choices.ContainsKey(type))
                return;

            foreach ( var choice in choices[ type ] )
            {
                Log.debug("Choice: " + choice);
            }

            recentTarget = target;
            var menu = new SelectDisplayMenu(type, "/");
            menu.onSelected = onExteriorSelected;
            Game1.activeClickableMenu = menu;
        }

        private string recentTarget = null;
        private void onExteriorSelected( string type, string choice )
        {
            Log.debug("onExteriorSelected: " + recentTarget + " " + type + " " + choice);

            foreach ( Building building in Game1.getFarm().buildings )
            {
                if ( building.buildingType == type && building.nameOfIndoors == recentTarget )
                {
                    Texture2D tex = getTextureForChoice( type, choice );
                    if ( tex == null )
                        Log.warn("Failed to load chosen texture '" + choice + "' for building type '" + type + "'.");
                    else
                        building.texture = tex;

                    break;
                }
            }
        }

        public static Texture2D getTextureForChoice(string type, string choice)
        {
            if (choice == "/")
                return Game1.content.Load<Texture2D>("Buildings/" + type);
            else
                return content.Load<Texture2D>(choice + "/" + type);
        }
    }
}
