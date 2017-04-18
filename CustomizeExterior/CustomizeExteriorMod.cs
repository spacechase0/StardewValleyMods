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
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Config config;
        public static ContentManager content;

        public static Dictionary<string, List<string>> choices = new Dictionary<string, List<string>>();

        public override void Entry(IModHelper helper)
        {
            instance = this;

            GameEvents.LoadContent += onContentLoad;
            GameEvents.UpdateTick += onUpdate;
            SaveEvents.AfterLoad += afterLoad;
            SaveEvents.AfterSave += afterSave;
        }

        private void onContentLoad(object sender, EventArgs args)
        {
            content = new ContentManager(Game1.content.ServiceProvider, Path.Combine(Helper.DirectoryPath, "Buildings"));
            compileChoices();
        }

        private void afterLoad(object sender, EventArgs args)
        {
            string path = Path.Combine(Constants.SaveFolderName, "building-exteriors.json");
            Log.info("Loading per-save config file (\"" + path + "\")...");
            config = Helper.ReadJsonFile<Config>(path) ?? new Config();

            foreach ( var choice in config.chosen )
            {
                recentTarget = choice.Key;
                Log.debug("Saved choice: " + choice.Key + " " + choice.Value);

                string type = null;
                if (recentTarget == "FarmHouse" || recentTarget == "Greenhouse")
                {
                    type = "houses";
                }
                else
                {
                    foreach (Building building in Game1.getFarm().buildings)
                    {
                        if ( building.nameOfIndoors == choice.Key )
                        {
                            type = building.buildingType;
                        }
                    }
                }

                if (type != null)
                    onExteriorSelected(type, choice.Value, false);
            }
        }

        private void afterSave(object sender, EventArgs args)
        {
            Log.info("Saving per-save config file...");
            Helper.WriteJsonFile(Path.Combine(Constants.SaveFolderName, "building-exteriors.json"), config);
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
                if ( Game1.currentLocation is Farm )
                {
                    Rectangle house = new Rectangle(59 * Game1.tileSize, 11 * Game1.tileSize, 9 * Game1.tileSize, 6 * Game1.tileSize);
                    Rectangle greenhouse = new Rectangle(25 * Game1.tileSize, 10 * Game1.tileSize, 7 * Game1.tileSize, 6 * Game1.tileSize);

                    if ( house.Contains( pos.X, pos.Y ) )
                    {
                        Log.trace("Right clicked the house.");
                        checkBuildingClick("FarmHouse", "houses");
                    }
                    else if ( greenhouse.Contains( pos.X, pos.Y ) )
                    {
                        Log.trace("Right clicked the greenhouse.");
                        checkBuildingClick("Greenhouse", "houses");
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
                    if (Path.GetExtension( type ) != ".xnb")
                        continue;

                    string choiceStr = Path.GetFileName(choice);
                    string typeStr = Path.GetFileNameWithoutExtension(type);
                    List<string> forType = Mod.choices.ContainsKey(typeStr) ? Mod.choices[typeStr] : new List<string>();
                    forType.Add(choiceStr);
                    if (!Mod.choices.ContainsKey(typeStr))
                        Mod.choices.Add(typeStr, forType);

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
        private void onExteriorSelected(string type, string choice) { onExteriorSelected(type, choice, true); }
        private void onExteriorSelected( string type, string choice, bool updateChosen )
        {
            Log.debug("onExteriorSelected: " + recentTarget + " " + type + " " + choice);
            
            Texture2D tex = getTextureForChoice(type, choice);
            if (tex == null)
            {
                Log.warn("Failed to load chosen texture '" + choice + "' for building type '" + type + "'.");
                return;
            }
            if ( updateChosen )
                config.chosen[recentTarget] = choice;

            if ( recentTarget == "FarmHouse" || recentTarget == "Greenhouse" )
            {
                housesHybrid = null;
                Game1.getFarm().houseTextures = getHousesTexture();
            }
            else
            {
                foreach ( Building building in Game1.getFarm().buildings )
                {
                    if (building.buildingType == type && building.nameOfIndoors == recentTarget)
                    {
                        building.texture = tex;
                        break;
                    }
                }
            }
        }

        public static string getChosenTexture( string type )
        {
            return config.chosen.ContainsKey(type) ? config.chosen[type] : "/";
        }

        public static Texture2D getTextureForChoice(string type, string choice)
        {
            if (choice == "/")
                return Game1.content.Load<Texture2D>("Buildings/" + type);
            else
                return content.Load<Texture2D>(choice + "/" + type);
        }
        
        private static Texture2D housesHybrid = null;
        private static Texture2D getHousesTexture()
        {
            if (housesHybrid != null)
                return housesHybrid;

            Log.trace("Creating hybrid farmhouse/greenhouse texture");

            Farm farm = Game1.getFarm();
            Texture2D baseTex = farm.houseTextures;
            Rectangle houseRect = new Rectangle( 0, 0, 160, baseTex.Height );// instance.Helper.Reflection.GetPrivateValue<Rectangle>(farm, "houseSource");
            Rectangle greenhouseRect = new Rectangle(160, 0, 112, baseTex.Height);// instance.Helper.Reflection.GetPrivateValue<Rectangle>(farm, "greenhouseSource");

            GraphicsDevice dev = Game1.graphics.GraphicsDevice;
            RenderTarget2D ret = new RenderTarget2D(dev, baseTex.Width, baseTex.Height);
            SpriteBatch b = Game1.spriteBatch;
            dev.SetRenderTarget(ret);
            {
                dev.Clear(Color.Transparent);
                b.Begin();
                b.Draw(getTextureForChoice("houses", getChosenTexture("FarmHouse")), houseRect, houseRect, Color.White);
                b.Draw(getTextureForChoice("houses", getChosenTexture("Greenhouse")), greenhouseRect, greenhouseRect, Color.White);
                b.End();
            }
            dev.SetRenderTarget(null);

            housesHybrid = ret;
            return ret;
        }
    }
}
