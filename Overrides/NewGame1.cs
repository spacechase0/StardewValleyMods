using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpaceCore.Events;
using SpaceCore.Locations;
using SpaceCore.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace SpaceCore.Overrides
{
    class NewGame1
    {
        internal static void hijack()
        {
            Hijack.hijack(typeof(   Game1).GetMethod("showEndOfNightStuff",  BindingFlags.Static   | BindingFlags.   Public ),
                          typeof(NewGame1).GetMethod("showEndOfNightStuff",  BindingFlags.Static   | BindingFlags.   Public ));
            Hijack.hijack(typeof(   Game1).GetMethod("setGraphicsForSeason", BindingFlags.Static   | BindingFlags.   Public ),
                          typeof(NewGame1).GetMethod("setGraphicsForSeason", BindingFlags.Static   | BindingFlags.   Public ));
            Hijack.hijack(typeof(   Game1).GetMethod("UpdateControlInput",   BindingFlags.Instance | BindingFlags.NonPublic ),
                          typeof(NewGame1).GetMethod("UpdateControlInput",   BindingFlags.Static   | BindingFlags.   Public ));
        }
        
        public static void showEndOfNightStuff()
        {
            var ev = new EventArgsShowNightEndMenus();
            ev.Stage = EventStage.Before;
            SpaceEvents.InvokeShowNightEndMenus(ev);

            bool flag1 = false;
            if (ev.ProcessShippedItems && Game1.getFarm().shippingBin.Count > 0)
            {
                Game1.endOfNightMenus.Push((IClickableMenu)new ShippingMenu(Game1.getFarm().shippingBin));
                Game1.getFarm().shippingBin.Clear();
                flag1 = true;
            }
            bool flag2 = false;
            if (Game1.player.newLevels.Count > 0 && !flag1)
                Game1.endOfNightMenus.Push((IClickableMenu)new SaveGameMenu());
            while (Game1.player.newLevels.Count > 0)
            {
                Game1.endOfNightMenus.Push((IClickableMenu)new LevelUpMenu(Game1.player.newLevels.Last<Point>().X, Game1.player.newLevels.Last<Point>().Y));
                Game1.player.newLevels.RemoveAt(Game1.player.newLevels.Count - 1);
                flag2 = true;
            }

            ev.Stage = EventStage.After;
            SpaceEvents.InvokeShowNightEndMenus(ev);

            if (flag2)
                Game1.playSound("newRecord");
            if (Game1.endOfNightMenus.Count > 0)
            {
                Game1.showingEndOfNightStuff = true;
                Game1.activeClickableMenu = Game1.endOfNightMenus.Pop();
            }
            else if (Game1.saveOnNewDay)
            {
                Game1.showingEndOfNightStuff = true;
                Game1.activeClickableMenu = (IClickableMenu)new SaveGameMenu();
            }
            else
            {
                Game1.currentLocation.resetForPlayerEntry();
                Game1.globalFadeToClear(new Game1.afterFadeFunction(Game1.playMorningSong), 0.02f);
            }
        }

        public static void setGraphicsForSeason()
        {
            // All I've done is add checks relating to ISeasonalLocation
            foreach (GameLocation location in Game1.locations)
            {
                location.seasonUpdate(Game1.currentSeason, true);
                if (location.IsOutdoors)
                {
                    var seasonalLoc = location as ISeasonalLocation;
                    if (!location.Name.Equals("Desert") && seasonalLoc == null)
                    {
                        for (int index = 0; index < location.Map.TileSheets.Count; ++index)
                        {
                            if (!location.Map.TileSheets[index].ImageSource.Contains('_'))
                                continue;
                            if (!location.Map.TileSheets[index].ImageSource.Contains("path") && !location.Map.TileSheets[index].ImageSource.Contains("object"))
                            {
                                location.Map.TileSheets[index].ImageSource = "Maps\\" + Game1.currentSeason + "_" + location.Map.TileSheets[index].ImageSource.Split('_')[1];
                                location.Map.DisposeTileSheets(Game1.mapDisplayDevice);
                                location.Map.LoadTileSheets(Game1.mapDisplayDevice);
                            }
                        }
                    }
                    if (Game1.currentSeason.Equals("spring") || (seasonalLoc != null && seasonalLoc.Season == "spring"))
                    {
                        foreach (KeyValuePair<Vector2, SObject> keyValuePair in (Dictionary<Vector2, SObject>)location.Objects)
                        {
                            if ((keyValuePair.Value.Name.Contains("Stump") || keyValuePair.Value.Name.Contains("Boulder") || (keyValuePair.Value.Name.Equals("Stick") || keyValuePair.Value.Name.Equals("Stone"))) && (keyValuePair.Value.ParentSheetIndex >= 378 && keyValuePair.Value.ParentSheetIndex <= 391))
                                keyValuePair.Value.ParentSheetIndex -= 376;
                        }
                        Game1.eveningColor = new Color((int)byte.MaxValue, (int)byte.MaxValue, 0);
                    }
                    else if (Game1.currentSeason.Equals("summer") || (seasonalLoc != null && seasonalLoc.Season == "summer"))
                    {
                        foreach (KeyValuePair<Vector2, SObject> keyValuePair in (Dictionary<Vector2, SObject>)location.Objects)
                        {
                            if (keyValuePair.Value.Name.Contains("Weed"))
                            {
                                if (keyValuePair.Value.parentSheetIndex == 792)
                                    ++keyValuePair.Value.ParentSheetIndex;
                                else if (Game1.random.NextDouble() < 0.3)
                                    keyValuePair.Value.ParentSheetIndex = 676;
                                else if (Game1.random.NextDouble() < 0.3)
                                    keyValuePair.Value.ParentSheetIndex = 677;
                            }
                        }
                        Game1.eveningColor = new Color((int)byte.MaxValue, (int)byte.MaxValue, 0);
                    }
                    else if (Game1.currentSeason.Equals("fall") || (seasonalLoc != null && seasonalLoc.Season == "fall"))
                    {
                        foreach (KeyValuePair<Vector2, SObject> keyValuePair in (Dictionary<Vector2, SObject>)location.Objects)
                        {
                            if (keyValuePair.Value.Name.Contains("Weed"))
                            {
                                if (keyValuePair.Value.parentSheetIndex == 793)
                                    ++keyValuePair.Value.ParentSheetIndex;
                                else
                                    keyValuePair.Value.ParentSheetIndex = Game1.random.NextDouble() >= 0.5 ? 679 : 678;
                            }
                        }
                        Game1.eveningColor = new Color((int)byte.MaxValue, (int)byte.MaxValue, 0);
                        foreach (WeatherDebris weatherDebris in Game1.debrisWeather)
                            weatherDebris.which = 2;
                    }
                    else if (Game1.currentSeason.Equals("winter") || (seasonalLoc != null && seasonalLoc.Season == "winter"))
                    {
                        for (int index = location.Objects.Count - 1; index >= 0; --index)
                        {
                            SObject @object = location.Objects[location.Objects.Keys.ElementAt<Vector2>(index)];
                            if (@object.Name.Contains("Weed"))
                                location.Objects.Remove(location.Objects.Keys.ElementAt<Vector2>(index));
                            else if ((!@object.Name.Contains("Stump") && !@object.Name.Contains("Boulder") && (!@object.Name.Equals("Stick") && !@object.Name.Equals("Stone")) || @object.ParentSheetIndex > 100) && (location.IsOutdoors && !@object.isHoedirt))
                                @object.name.Equals("HoeDirt");
                        }
                        foreach (WeatherDebris weatherDebris in Game1.debrisWeather)
                            weatherDebris.which = 3;
                        Game1.eveningColor = new Color(245, 225, 170);
                    }
                }
            }
        }

        public static void UpdateControlInput(Game1 game, GameTime time)
        {
            if (Game1.paused)
                return;
            KeyboardState state1 = Keyboard.GetState();
            MouseState state2 = Mouse.GetState();
            GamePadState state3 = GamePad.GetState(Game1.playerOneIndex);
            if (state3.IsConnected && !Game1.options.gamepadControls)
            {
                Game1.options.gamepadControls = true;
                Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2574"));
                if (Game1.activeClickableMenu != null && Game1.options.SnappyMenus)
                {
                    Game1.activeClickableMenu.populateClickableComponentList();
                    Game1.activeClickableMenu.snapToDefaultClickableComponent();
                }
            }
            else if (!state3.IsConnected && Game1.options.gamepadControls)
            {
                Game1.options.gamepadControls = false;
                Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2575"));
                if (Game1.activeClickableMenu == null)
                    Game1.activeClickableMenu = (IClickableMenu)new GameMenu();
            }
            bool flag1 = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            bool flag7 = false;
            bool flag8 = false;
            bool flag9 = false;
            bool flag10 = false;
            bool flag11 = false;
            bool flag12 = false;
            bool flag13 = false;
            bool flag14 = false;
            bool flag15 = false;
            bool flag16 = false;
            bool flag17 = false;
            bool flag18 = false;
            bool flag19 = false;
            bool flag20 = false;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.actionButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.actionButton) || state2.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && Game1.oldMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                flag1 = true;
                Game1.rightClickPolling = 250;
            }
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.useToolButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.useToolButton) || state2.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && Game1.oldMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                flag3 = true;
            if (Game1.areAllOfTheseKeysUp(state1, Game1.options.useToolButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton) || state2.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && Game1.oldMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                flag4 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.toolSwapButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.toolSwapButton) || state2.ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue)
                flag2 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.cancelButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.cancelButton) || state2.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && Game1.oldMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                flag6 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.moveUpButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveUpButton))
                flag7 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.moveRightButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveRightButton))
                flag8 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.moveDownButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveDownButton))
                flag10 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.moveLeftButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveLeftButton))
                flag9 = true;
            if (Game1.areAllOfTheseKeysUp(state1, Game1.options.moveUpButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton))
                flag11 = true;
            if (Game1.areAllOfTheseKeysUp(state1, Game1.options.moveRightButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton))
                flag12 = true;
            if (Game1.areAllOfTheseKeysUp(state1, Game1.options.moveDownButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton))
                flag13 = true;
            if (Game1.areAllOfTheseKeysUp(state1, Game1.options.moveLeftButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton))
                flag14 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.moveUpButton))
                flag15 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.moveRightButton))
                flag16 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.moveDownButton))
                flag17 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.moveLeftButton))
                flag18 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.useToolButton) || state2.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                flag19 = true;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.chatButton))
                flag20 = true;
            TimeSpan elapsedGameTime1;
            if (Game1.isOneOfTheseKeysDown(state1, Game1.options.actionButton) || state2.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                int rightClickPolling = Game1.rightClickPolling;
                elapsedGameTime1 = time.ElapsedGameTime;
                int milliseconds = elapsedGameTime1.Milliseconds;
                Game1.rightClickPolling = rightClickPolling - milliseconds;
                if (Game1.rightClickPolling <= 0)
                {
                    Game1.rightClickPolling = 100;
                    flag1 = true;
                }
            }
            if (Game1.options.gamepadControls)
            {
                GamePadThumbSticks thumbSticks;
                if (Game1.isGamePadThumbstickInMotion(0.2))
                {
                    double x1 = (double)state2.X;
                    thumbSticks = state3.ThumbSticks;
                    double num1 = (double)thumbSticks.Right.X * (double)Reflect.getProperty<float>(typeof(Game1), "thumbstickToMouseModifier");
                    int x2 = (int)(x1 + num1);
                    double y1 = (double)state2.Y;
                    thumbSticks = state3.ThumbSticks;
                    double num2 = (double)thumbSticks.Right.Y * (double)Reflect.getProperty<float>(typeof(Game1), "thumbstickToMouseModifier");
                    int y2 = (int)(y1 - num2);
                    Mouse.SetPosition(x2, y2);
                    Game1.lastCursorMotionWasMouse = false;
                }
                if (Game1.getMouseX() == Game1.getOldMouseX() && Game1.getMouseY() == Game1.getOldMouseY() || (Game1.getMouseX() == 0 || Game1.getMouseY() == 0))
                {
                    thumbSticks = state3.ThumbSticks;
                    if ((double)Math.Abs(thumbSticks.Right.X) <= 0.0)
                    {
                        thumbSticks = state3.ThumbSticks;
                        if ((double)Math.Abs(thumbSticks.Right.Y) <= 0.0)
                            goto label_60;
                    }
                }
                if (Game1.timerUntilMouseFade <= 0 && game.IsActive)
                {
                    Mouse.SetPosition(Game1.lastMousePositionBeforeFade.X, Game1.lastMousePositionBeforeFade.Y);
                    Game1.lastCursorMotionWasMouse = false;
                }
                else if (!Game1.isGamePadThumbstickInMotion(0.0))
                    Game1.lastCursorMotionWasMouse = true;
                Game1.timerUntilMouseFade = 4000;
            label_60:
                if (state1.GetPressedKeys().Length != 0 || state2.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed || state2.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                    Game1.timerUntilMouseFade = 4000;
                if (state3.IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))
                {
                    flag1 = true;
                    Game1.lastCursorMotionWasMouse = false;
                    Game1.rightClickPolling = 250;
                }
                if (state3.IsButtonDown(Buttons.X) && !Game1.oldPadState.IsButtonDown(Buttons.X))
                {
                    flag3 = true;
                    Game1.lastCursorMotionWasMouse = false;
                }
                if (!state3.IsButtonDown(Buttons.X) && Game1.oldPadState.IsButtonDown(Buttons.X))
                    flag4 = true;
                if (state3.IsButtonDown(Buttons.RightTrigger) && !Game1.oldPadState.IsButtonDown(Buttons.RightTrigger))
                {
                    flag2 = true;
                    Game1.triggerPolling = 300;
                }
                else if (state3.IsButtonDown(Buttons.LeftTrigger) && !Game1.oldPadState.IsButtonDown(Buttons.LeftTrigger))
                {
                    flag2 = true;
                    Game1.triggerPolling = 300;
                }
                if (state3.IsButtonDown(Buttons.X))
                    flag19 = true;
                if (state3.IsButtonDown(Buttons.A))
                {
                    int rightClickPolling = Game1.rightClickPolling;
                    elapsedGameTime1 = time.ElapsedGameTime;
                    int milliseconds = elapsedGameTime1.Milliseconds;
                    Game1.rightClickPolling = rightClickPolling - milliseconds;
                    if (Game1.rightClickPolling <= 0)
                    {
                        Game1.rightClickPolling = 100;
                        flag1 = true;
                    }
                }
                if (state3.IsButtonDown(Buttons.RightTrigger) || state3.IsButtonDown(Buttons.LeftTrigger))
                {
                    int triggerPolling = Game1.triggerPolling;
                    elapsedGameTime1 = time.ElapsedGameTime;
                    int milliseconds = elapsedGameTime1.Milliseconds;
                    Game1.triggerPolling = triggerPolling - milliseconds;
                    if (Game1.triggerPolling <= 0)
                    {
                        Game1.triggerPolling = 100;
                        flag2 = true;
                    }
                }
                if (state3.IsButtonDown(Buttons.RightShoulder) && !Game1.oldPadState.IsButtonDown(Buttons.RightShoulder))
                    Game1.player.shiftToolbar(true);
                if (state3.IsButtonDown(Buttons.LeftShoulder) && !Game1.oldPadState.IsButtonDown(Buttons.LeftShoulder))
                    Game1.player.shiftToolbar(false);
                if (state3.IsButtonDown(Buttons.DPadUp) && !Game1.oldPadState.IsButtonDown(Buttons.DPadUp))
                    flag7 = true;
                else if (!state3.IsButtonDown(Buttons.DPadUp) && Game1.oldPadState.IsButtonDown(Buttons.DPadUp))
                    flag11 = true;
                if (state3.IsButtonDown(Buttons.DPadRight) && !Game1.oldPadState.IsButtonDown(Buttons.DPadRight))
                    flag8 = true;
                else if (!state3.IsButtonDown(Buttons.DPadRight) && Game1.oldPadState.IsButtonDown(Buttons.DPadRight))
                    flag12 = true;
                if (state3.IsButtonDown(Buttons.DPadDown) && !Game1.oldPadState.IsButtonDown(Buttons.DPadDown))
                    flag10 = true;
                else if (!state3.IsButtonDown(Buttons.DPadDown) && Game1.oldPadState.IsButtonDown(Buttons.DPadDown))
                    flag13 = true;
                if (state3.IsButtonDown(Buttons.DPadLeft) && !Game1.oldPadState.IsButtonDown(Buttons.DPadLeft))
                    flag9 = true;
                else if (!state3.IsButtonDown(Buttons.DPadLeft) && Game1.oldPadState.IsButtonDown(Buttons.DPadLeft))
                    flag14 = true;
                if (state3.IsButtonDown(Buttons.DPadUp))
                    flag15 = true;
                if (state3.IsButtonDown(Buttons.DPadRight))
                    flag16 = true;
                if (state3.IsButtonDown(Buttons.DPadDown))
                    flag17 = true;
                if (state3.IsButtonDown(Buttons.DPadLeft))
                    flag18 = true;
                thumbSticks = state3.ThumbSticks;
                if ((double)thumbSticks.Left.X < -0.2)
                {
                    flag9 = true;
                    flag18 = true;
                }
                else
                {
                    thumbSticks = state3.ThumbSticks;
                    if ((double)thumbSticks.Left.X > 0.2)
                    {
                        flag8 = true;
                        flag16 = true;
                    }
                }
                thumbSticks = state3.ThumbSticks;
                if ((double)thumbSticks.Left.Y < -0.2)
                {
                    flag10 = true;
                    flag17 = true;
                }
                else
                {
                    thumbSticks = state3.ThumbSticks;
                    if ((double)thumbSticks.Left.Y > 0.2)
                    {
                        flag7 = true;
                        flag15 = true;
                    }
                }
                thumbSticks = Game1.oldPadState.ThumbSticks;
                if ((double)thumbSticks.Left.X < -0.2 && !flag18)
                    flag14 = true;
                thumbSticks = Game1.oldPadState.ThumbSticks;
                if ((double)thumbSticks.Left.X > 0.2 && !flag16)
                    flag12 = true;
                thumbSticks = Game1.oldPadState.ThumbSticks;
                if ((double)thumbSticks.Left.Y < -0.2 && !flag17)
                    flag13 = true;
                thumbSticks = Game1.oldPadState.ThumbSticks;
                if ((double)thumbSticks.Left.Y > 0.2 && !flag15)
                    flag11 = true;
            }
            Game1.ResetFreeCursorDrag();
            if (flag19)
            {
                int mouseClickPolling = Game1.mouseClickPolling;
                elapsedGameTime1 = time.ElapsedGameTime;
                int milliseconds = elapsedGameTime1.Milliseconds;
                Game1.mouseClickPolling = mouseClickPolling + milliseconds;
            }
            else
                Game1.mouseClickPolling = 0;
            if (Game1.mouseClickPolling > 250 && (Game1.player.CurrentTool == null || !(Game1.player.CurrentTool is MeleeWeapon)) && (Game1.player.CurrentTool == null || Game1.player.CurrentTool.GetType() != typeof(FishingRod) || Game1.player.CurrentTool.upgradeLevel <= 0))
            {
                flag3 = true;
                Game1.mouseClickPolling = 100;
            }
            if (Game1.displayHUD)
            {
                foreach (IClickableMenu onScreenMenu in Game1.onScreenMenus)
                {
                    if (Game1.wasMouseVisibleThisFrame && onScreenMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                        onScreenMenu.performHoverAction(Game1.getMouseX(), Game1.getMouseY());
                }
                if (flag20)
                {
                    foreach (IClickableMenu onScreenMenu in Game1.onScreenMenus)
                    {
                        if (onScreenMenu is ChatBox)
                        {
                            ((ChatBox)onScreenMenu).chatBox.Selected = true;
                            Game1.isChatting = true;
                            if (!state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemQuestion))
                                return;
                            ((ChatBox)onScreenMenu).chatBox.Text = "/";
                            return;
                        }
                    }
                }
                else if (Game1.isChatting && state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                {
                    Game1.ChatBox.chatBox.Selected = false;
                    Game1.isChatting = false;
                    Game1.oldKBState = state1;
                    return;
                }
            }
            if (Game1.panMode)
                Reflect.callMethod(game, "updatePanModeControls", new object[] { state2, state1 } );
            else if (flag4 && Game1.player.CurrentTool != null && (Game1.CurrentEvent == null && (double)Game1.pauseTime <= 0.0) && Game1.player.CurrentTool.onRelease(Game1.currentLocation, Game1.getMouseX(), Game1.getMouseY(), Game1.player))
            {
                Game1.oldMouseState = state2;
                Game1.oldKBState = state1;
                Game1.oldPadState = state3;
                Game1.player.usingSlingshot = false;
                Game1.player.canReleaseTool = true;
                Game1.player.usingTool = false;
                Game1.player.CanMove = true;
            }
            else
            {
                if ((flag3 && !Game1.isAnyGamePadButtonBeingPressed() || flag1 && Game1.isAnyGamePadButtonBeingPressed()) && ((double)Game1.pauseTime <= 0.0 && Game1.displayHUD && Game1.wasMouseVisibleThisFrame))
                {
                    foreach (IClickableMenu onScreenMenu in Game1.onScreenMenus)
                    {
                        if (!(onScreenMenu is ChatBox) && (!(onScreenMenu is LevelUpMenu) || (onScreenMenu as LevelUpMenu).informationUp) && onScreenMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                        {
                            onScreenMenu.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY(), true);
                            Game1.oldMouseState = state2;
                            if (!Game1.isAnyGamePadButtonBeingPressed())
                                return;
                        }
                        onScreenMenu.clickAway();
                    }
                }
                if (Game1.isChatting || Game1.player.freezePause > 0)
                {
                    Game1.oldMouseState = state2;
                    Game1.oldKBState = state1;
                    Game1.oldPadState = state3;
                }
                else
                {
                    if (Game1.eventUp && flag1 | flag3)
                        Game1.currentLocation.currentEvent.receiveMouseClick(Game1.getMouseX(), Game1.getMouseY());
                    if (flag1 || Game1.dialogueUp & flag3)
                    {
                        foreach (IClickableMenu onScreenMenu in Game1.onScreenMenus)
                        {
                            if (Game1.wasMouseVisibleThisFrame && Game1.displayHUD && onScreenMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) && (!(onScreenMenu is LevelUpMenu) || (onScreenMenu as LevelUpMenu).informationUp))
                            {
                                onScreenMenu.receiveRightClick(Game1.getMouseX(), Game1.getMouseY(), true);
                                Game1.oldMouseState = state2;
                                if (!Game1.isAnyGamePadButtonBeingPressed())
                                    return;
                            }
                        }
                        if (!Game1.pressActionButton(state1, state2, state3))
                            return;
                    }
                    else
                    {
                        if (flag3 && (!Game1.player.UsingTool || Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon) && (!Game1.pickingTool && !Game1.dialogueUp && !Game1.menuUp) && (Game1.player.CanMove || Game1.player.CurrentTool != null && (Game1.player.CurrentTool.Name.Equals("Fishing Rod") || Game1.player.CurrentTool is MeleeWeapon)))
                        {
                            if (Game1.player.CurrentTool != null)
                                Game1.player.CurrentTool.leftClick(Game1.player);
                            if (!Game1.pressUseToolButton() && Game1.player.canReleaseTool && Game1.player.usingTool)
                            {
                                Tool currentTool = Game1.player.CurrentTool;
                            }
                            Game1.oldMouseState = state2;
                            if (Game1.mouseClickPolling < 100)
                                Game1.oldKBState = state1;
                            Game1.oldPadState = state3;
                            return;
                        }
                        if (flag4 && Game1.player.canReleaseTool && (Game1.player.usingTool && Game1.player.CurrentTool != null))
                                Game1.releaseUseToolButton();
                        else if (flag2 && !Game1.player.UsingTool && !Game1.dialogueUp && ((Game1.pickingTool || Game1.player.CanMove) && (!Game1.player.areAllItemsNull() && !Game1.eventUp)))
                            Game1.pressSwitchToolButton();
                        else if ((!flag2 || Game1.player.ActiveObject == null || (Game1.dialogueUp || Game1.eventUp)) && (flag5 && !Game1.pickingTool && (!Game1.eventUp && !Game1.player.UsingTool)) && Game1.player.CanMove)
                            Game1.pressAddItemToInventoryButton();
                    }
                    if (flag6)
                    {
                        if (Game1.numberOfSelectedItems != -1)
                        {
                            Game1.numberOfSelectedItems = -1;
                            Game1.dialogueUp = false;
                            Game1.player.CanMove = true;
                        }
                        else if (Game1.nameSelectUp && NameSelect.cancel())
                        {
                            Game1.nameSelectUp = false;
                            Game1.playSound("bigDeSelect");
                        }
                    }
                    TimeSpan elapsedGameTime2;
                    if (Game1.player.CurrentTool != null & flag19 && Game1.player.canReleaseTool && (!Game1.eventUp && !Game1.dialogueUp) && (!Game1.menuUp && (double)Game1.player.Stamina >= 1.0 && !(Game1.player.CurrentTool is FishingRod)))
                    {
                        if (Game1.player.toolHold <= 0 && Game1.player.CurrentTool.upgradeLevel > Game1.player.toolPower)
                            Game1.player.toolHold = 600;
                        else if (Game1.player.CurrentTool.upgradeLevel > Game1.player.toolPower)
                        {
                            SFarmer player = Game1.player;
                            int toolHold = player.toolHold;
                            elapsedGameTime2 = time.ElapsedGameTime;
                            int milliseconds = elapsedGameTime2.Milliseconds;
                            int num = toolHold - milliseconds;
                            player.toolHold = num;
                            if (Game1.player.toolHold <= 0)
                                Game1.player.toolPowerIncrease();
                        }
                    }
                    if ((double)Game1.upPolling >= 650.0)
                        Game1.upPolling -= 100f;
                    else if ((double)Game1.downPolling >= 650.0)
                        Game1.downPolling -= 100f;
                    else if ((double)Game1.rightPolling >= 650.0)
                        Game1.rightPolling -= 100f;
                    else if ((double)Game1.leftPolling >= 650.0)
                        Game1.leftPolling -= 100f;
                    else if (!Game1.nameSelectUp && (double)Game1.pauseTime <= 0.0 && (!Game1.eventUp || Game1.CurrentEvent != null && Game1.CurrentEvent.playerControlSequence && !Game1.player.usingTool))
                    {
                        if (Game1.player.movementDirections.Count < 2)
                        {
                            int count = Game1.player.movementDirections.Count;
                            if (flag15)
                            {
                                Game1.player.setMoving((byte)1);
                                if (Game1.IsClient)
                                    Game1.client.sendMessage((byte)0, new object[1]
                                    {
                    (object) (byte) 1
                                    });
                            }
                            if (flag16)
                            {
                                Game1.player.setMoving((byte)2);
                                if (Game1.IsClient)
                                    Game1.client.sendMessage((byte)0, new object[1]
                                    {
                    (object) (byte) 2
                                    });
                            }
                            if (flag17)
                            {
                                Game1.player.setMoving((byte)4);
                                if (Game1.IsClient)
                                    Game1.client.sendMessage((byte)0, new object[1]
                                    {
                    (object) (byte) 4
                                    });
                            }
                            if (flag18)
                            {
                                Game1.player.setMoving((byte)8);
                                if (Game1.IsClient)
                                    Game1.client.sendMessage((byte)0, new object[1]
                                    {
                    (object) (byte) 8
                                    });
                            }
                            if (count == 0 && Game1.player.movementDirections.Count > 0 && Game1.player.running)
                                Game1.player.FarmerSprite.nextOffset = 1;
                        }
                        if (flag11 || Game1.player.movementDirections.Contains(0) && !flag15)
                        {
                            Game1.player.setMoving((byte)33);
                            if (Game1.IsClient)
                                Game1.client.sendMessage((byte)0, new object[1]
                                {
                  (object) 33
                                });
                            else if (Game1.IsServer && Game1.player.movementDirections.Count == 0)
                                Game1.player.setMoving((byte)64);
                        }
                        if (flag12 || Game1.player.movementDirections.Contains(1) && !flag16)
                        {
                            Game1.player.setMoving((byte)34);
                            if (Game1.IsClient)
                                Game1.client.sendMessage((byte)0, new object[1]
                                {
                  (object) 34
                                });
                            else if (Game1.IsServer && Game1.player.movementDirections.Count == 0)
                                Game1.player.setMoving((byte)64);
                        }
                        if (flag13 || Game1.player.movementDirections.Contains(2) && !flag17)
                        {
                            Game1.player.setMoving((byte)36);
                            if (Game1.IsClient)
                                Game1.client.sendMessage((byte)0, new object[1]
                                {
                  (object) 36
                                });
                            else if (Game1.IsServer && Game1.player.movementDirections.Count == 0)
                                Game1.player.setMoving((byte)64);
                        }
                        if (flag14 || Game1.player.movementDirections.Contains(3) && !flag18)
                        {
                            Game1.player.setMoving((byte)40);
                            if (Game1.IsClient)
                                Game1.client.sendMessage((byte)0, new object[1]
                                {
                  (object) 40
                                });
                            else if (Game1.IsServer && Game1.player.movementDirections.Count == 0)
                                Game1.player.setMoving((byte)64);
                        }
                        if (!flag15 && !flag16 && (!flag17 && !flag18) && !Game1.player.UsingTool)
                            Game1.player.Halt();
                    }
                    else if (Game1.isQuestion)
                    {
                        if (flag7)
                        {
                            Game1.currentQuestionChoice = Math.Max(Game1.currentQuestionChoice - 1, 0);
                            Game1.playSound("toolSwap");
                        }
                        else if (flag10)
                        {
                            Game1.currentQuestionChoice = Math.Min(Game1.currentQuestionChoice + 1, Game1.questionChoices.Count - 1);
                            Game1.playSound("toolSwap");
                        }
                    }
                    else if (Game1.numberOfSelectedItems != -1 && !Game1.dialogueTyping)
                    {
                        int val2 = 99;
                        if (Game1.selectedItemsType.Equals("Animal Food"))
                            val2 = 999 - Game1.player.Feed;
                        else if (Game1.selectedItemsType.Equals("calicoJackBet"))
                            val2 = Math.Min(Game1.player.clubCoins, 999);
                        else if (Game1.selectedItemsType.Equals("flutePitch"))
                            val2 = 26;
                        else if (Game1.selectedItemsType.Equals("drumTone"))
                            val2 = 6;
                        else if (Game1.selectedItemsType.Equals("jukebox"))
                            val2 = Game1.player.songsHeard.Count - 1;
                        else if (Game1.selectedItemsType.Equals("Fuel"))
                            val2 = 100 - ((Lantern)Game1.player.getToolFromName("Lantern")).fuelLeft;
                        if (flag8)
                        {
                            Game1.numberOfSelectedItems = Math.Min(Game1.numberOfSelectedItems + 1, val2);
                            Game1.playItemNumberSelectSound();
                        }
                        else if (flag9)
                        {
                            Game1.numberOfSelectedItems = Math.Max(Game1.numberOfSelectedItems - 1, 0);
                            Game1.playItemNumberSelectSound();
                        }
                        else if (flag7)
                        {
                            Game1.numberOfSelectedItems = Math.Min(Game1.numberOfSelectedItems + 10, val2);
                            Game1.playItemNumberSelectSound();
                        }
                        else if (flag10)
                        {
                            Game1.numberOfSelectedItems = Math.Max(Game1.numberOfSelectedItems - 10, 0);
                            Game1.playItemNumberSelectSound();
                        }
                    }
                    if (flag15 && !Game1.player.CanMove)
                    {
                        double upPolling = (double)Game1.upPolling;
                        elapsedGameTime2 = time.ElapsedGameTime;
                        double milliseconds = (double)elapsedGameTime2.Milliseconds;
                        Game1.upPolling = (float)(upPolling + milliseconds);
                    }
                    else if (flag17 && !Game1.player.CanMove)
                    {
                        double downPolling = (double)Game1.downPolling;
                        elapsedGameTime2 = time.ElapsedGameTime;
                        double milliseconds = (double)elapsedGameTime2.Milliseconds;
                        Game1.downPolling = (float)(downPolling + milliseconds);
                    }
                    else if (flag16 && !Game1.player.CanMove)
                    {
                        double rightPolling = (double)Game1.rightPolling;
                        elapsedGameTime2 = time.ElapsedGameTime;
                        double milliseconds = (double)elapsedGameTime2.Milliseconds;
                        Game1.rightPolling = (float)(rightPolling + milliseconds);
                    }
                    else if (flag18 && !Game1.player.CanMove)
                    {
                        double leftPolling = (double)Game1.leftPolling;
                        elapsedGameTime2 = time.ElapsedGameTime;
                        double milliseconds = (double)elapsedGameTime2.Milliseconds;
                        Game1.leftPolling = (float)(leftPolling + milliseconds);
                    }
                    else if (flag11)
                        Game1.upPolling = 0.0f;
                    else if (flag13)
                        Game1.downPolling = 0.0f;
                    else if (flag12)
                        Game1.rightPolling = 0.0f;
                    else if (flag14)
                        Game1.leftPolling = 0.0f;
                    if (Game1.debugMode)
                    {
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Q))
                            Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Q);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P))
                            Game1.NewDay(0.0f);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.M) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.M))
                        {
                            Game1.dayOfMonth = 28;
                            Game1.NewDay(0.0f);
                        }
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.T) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.T))
                        {
                            Game1.timeOfDay += 100;
                            foreach (GameLocation location in Game1.locations)
                            {
                                for (int index = 0; index < location.getCharacters().Count; ++index)
                                {
                                    location.getCharacters()[index].checkSchedule(Game1.timeOfDay);
                                    location.getCharacters()[index].checkSchedule(Game1.timeOfDay - 50);
                                    location.getCharacters()[index].checkSchedule(Game1.timeOfDay - 60);
                                    location.getCharacters()[index].checkSchedule(Game1.timeOfDay - 70);
                                    location.getCharacters()[index].checkSchedule(Game1.timeOfDay - 80);
                                    location.getCharacters()[index].checkSchedule(Game1.timeOfDay - 90);
                                }
                            }
                            switch (Game1.timeOfDay)
                            {
                                case 2100:
                                    Game1.globalOutdoorLighting = 0.9f;
                                    break;
                                case 2200:
                                    Game1.globalOutdoorLighting = 1f;
                                    break;
                                case 1900:
                                    Game1.globalOutdoorLighting = 0.5f;
                                    Game1.currentLocation.switchOutNightTiles();
                                    break;
                                case 2000:
                                    Game1.globalOutdoorLighting = 0.7f;
                                    if (!Game1.isRaining)
                                    {
                                        Game1.changeMusicTrack("none");
                                        break;
                                    }
                                    break;
                            }
                        }
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Y) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Y))
                        {
                            if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
                                Game1.timeOfDay -= 10;
                            else
                                Game1.timeOfDay += 10;
                            if (Game1.timeOfDay % 100 == 60)
                                Game1.timeOfDay += 40;
                            if (Game1.timeOfDay % 100 == 90)
                                Game1.timeOfDay -= 40;
                            Game1.currentLocation.performTenMinuteUpdate(Game1.timeOfDay);
                            foreach (GameLocation location in Game1.locations)
                            {
                                for (int index = 0; index < location.getCharacters().Count; ++index)
                                    location.getCharacters()[index].checkSchedule(Game1.timeOfDay);
                            }
                            if (Game1.isLightning)
                                Utility.performLightningUpdate();
                            switch (Game1.timeOfDay)
                            {
                                case 2000:
                                    Game1.globalOutdoorLighting = 0.7f;
                                    if (!Game1.isRaining)
                                    {
                                        Game1.changeMusicTrack("none");
                                        break;
                                    }
                                    break;
                                case 2100:
                                    Game1.globalOutdoorLighting = 0.9f;
                                    break;
                                case 2200:
                                    Game1.globalOutdoorLighting = 1f;
                                    break;
                                case 1750:
                                    Game1.globalOutdoorLighting = 0.0f;
                                    Game1.outdoorLight = Color.White;
                                    break;
                                case 1900:
                                    Game1.globalOutdoorLighting = 0.5f;
                                    Game1.currentLocation.switchOutNightTiles();
                                    break;
                            }
                        }
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D1) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D1))
                            Game1.warpFarmer("Mountain", 15, 35, false);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D2) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D2))
                            Game1.warpFarmer("Town", 35, 35, false);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D3) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D3))
                            Game1.warpFarmer("Farm", 64, 15, false);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D4) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D4))
                            Game1.warpFarmer("Forest", 34, 13, false);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D5) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D4))
                            Game1.warpFarmer("Beach", 34, 10, false);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D6) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D6))
                            Game1.warpFarmer("Mine", 18, 12, false);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D7) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D7))
                            Game1.warpFarmer("SandyHouse", 16, 3, false);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.K) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.K))
                        {
                            if (Game1.mine == null)
                                Game1.mine = new MineShaft();
                            Game1.enterMine(false, Game1.mine.mineLevel + 1, (string)null);
                        }
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.H) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.H))
                            Game1.player.changeHat(Game1.random.Next(FarmerRenderer.hatsTexture.Height / 80 * 12));
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.I) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.I))
                            Game1.player.changeHairStyle(Game1.random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.J) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.J))
                        {
                            Game1.player.changeShirt(Game1.random.Next(40));
                            Game1.player.changePants(new Color(Game1.random.Next((int)byte.MaxValue), Game1.random.Next((int)byte.MaxValue), Game1.random.Next((int)byte.MaxValue)));
                        }
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.L) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.L))
                        {
                            Game1.player.changeShirt(Game1.random.Next(40));
                            Game1.player.changePants(new Color(Game1.random.Next((int)byte.MaxValue), Game1.random.Next((int)byte.MaxValue), Game1.random.Next((int)byte.MaxValue)));
                            Game1.player.changeHairStyle(Game1.random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
                            if (Game1.random.NextDouble() < 0.5)
                                Game1.player.changeHat(Game1.random.Next(-1, FarmerRenderer.hatsTexture.Height / 80 * 12));
                            else
                                Game1.player.changeHat(-1);
                            Game1.player.changeHairColor(new Color(Game1.random.Next((int)byte.MaxValue), Game1.random.Next((int)byte.MaxValue), Game1.random.Next((int)byte.MaxValue)));
                            Game1.player.changeSkinColor(Game1.random.Next(16));
                        }
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.U) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.U))
                        {
                            (Game1.getLocationFromName("FarmHouse") as FarmHouse).setWallpaper(Game1.random.Next(112), -1, true);
                            (Game1.getLocationFromName("FarmHouse") as FarmHouse).setFloor(Game1.random.Next(40), -1, true);
                        }
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F2))
                            Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F2);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F5) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F5))
                            Game1.displayFarmer = !Game1.displayFarmer;
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F6))
                            Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F6);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F7) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F7))
                            Game1.drawGrid = !Game1.drawGrid;
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B))
                            Game1.player.shiftToolbar(false);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.N) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.N))
                            Game1.player.shiftToolbar(true);
                        if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F10) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F10))
                        {
                            if (Game1.server == null)
                            {
                                Game1.multiplayerMode = (byte)2;
                                Game1.server = (Server)new LidgrenServer("server");
                                Game1.server.initializeConnection();
                            }
                            if (Game1.ChatBox == null)
                                Game1.onScreenMenus.Add((IClickableMenu)new ChatBox());
                        }
                    }
                    else if (!Game1.player.UsingTool)
                    {
                        if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot1) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot1))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(0))
                                Game1.player.CurrentToolIndex = 0;
                        }
                        else if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot2) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot2))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(1))
                                Game1.player.CurrentToolIndex = 1;
                        }
                        else if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot3) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot3))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(2))
                                Game1.player.CurrentToolIndex = 2;
                        }
                        else if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot4) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot4))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(3))
                                Game1.player.CurrentToolIndex = 3;
                        }
                        else if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot5) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot5))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(4))
                                Game1.player.CurrentToolIndex = 4;
                        }
                        else if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot6) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot6))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(5))
                                Game1.player.CurrentToolIndex = 5;
                        }
                        else if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot7) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot7))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(6))
                                Game1.player.CurrentToolIndex = 6;
                        }
                        else if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot8) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot8))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(7))
                                Game1.player.CurrentToolIndex = 7;
                        }
                        else if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot9) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot9))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(8))
                                Game1.player.CurrentToolIndex = 8;
                        }
                        else if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot10) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot10))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(9))
                                Game1.player.CurrentToolIndex = 9;
                        }
                        else if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot11) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot11))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(10))
                                Game1.player.CurrentToolIndex = 10;
                        }
                        else if (Game1.isOneOfTheseKeysDown(state1, Game1.options.inventorySlot12) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot12))
                        {
                            if (SpaceEvents.InvokeSelectHotbarSlot(11))
                                Game1.player.CurrentToolIndex = 11;
                        }
                    }
                    if (!Program.releaseBuild)
                    {
                        if (Game1.IsPressEvent(ref state1, Microsoft.Xna.Framework.Input.Keys.F3) || Game1.IsPressEvent(ref state3, Buttons.LeftStick))
                        {
                            Game1.debugMode = !Game1.debugMode;
                            if ((int)Game1.gameMode == 11)
                                Game1.gameMode = (byte)3;
                        }
                        if (Game1.IsPressEvent(ref state1, Microsoft.Xna.Framework.Input.Keys.F8) || Game1.IsPressEvent(ref state3, Buttons.RightStick))
                            game.requestDebugInput();
                    }
                    if (state1.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F4) && !Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F4))
                    {
                        Game1.displayHUD = !Game1.displayHUD;
                        Game1.playSound("smallSelect");
                        if (!Game1.displayHUD)
                            Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3666"));
                    }
                    bool flag21 = Game1.isOneOfTheseKeysDown(state1, Game1.options.menuButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.menuButton);
                    bool flag22 = Game1.isOneOfTheseKeysDown(state1, Game1.options.journalButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.journalButton);
                    bool flag23 = Game1.isOneOfTheseKeysDown(state1, Game1.options.mapButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.mapButton);
                    if (Game1.options.gamepadControls && !flag21)
                        flag21 = state3.IsButtonDown(Buttons.Start) && !Game1.oldPadState.IsButtonDown(Buttons.Start) || state3.IsButtonDown(Buttons.B) && !Game1.oldPadState.IsButtonDown(Buttons.B);
                    if (Game1.options.gamepadControls && !flag22)
                        flag22 = state3.IsButtonDown(Buttons.Back) && !Game1.oldPadState.IsButtonDown(Buttons.Back);
                    if (Game1.options.gamepadControls && !flag23)
                        flag23 = state3.IsButtonDown(Buttons.Y) && !Game1.oldPadState.IsButtonDown(Buttons.Y);
                    if (((Game1.dayOfMonth <= 0 ? 0 : (Game1.player.CanMove ? 1 : 0)) & (flag21 ? 1 : 0)) != 0 && !Game1.dialogueUp && (!Game1.eventUp || Game1.isFestival() && Game1.CurrentEvent.festivalTimer <= 0) && Game1.currentMinigame == null)
                    {
                        if (Game1.activeClickableMenu == null)
                            Game1.activeClickableMenu = (IClickableMenu)new GameMenu();
                        else if (Game1.activeClickableMenu.readyToClose())
                            Game1.exitActiveMenu();
                    }
                    if (((Game1.dayOfMonth <= 0 ? 0 : (Game1.player.CanMove ? 1 : 0)) & (flag22 ? 1 : 0)) != 0 && !Game1.dialogueUp && !Game1.eventUp)
                    {
                        if (Game1.activeClickableMenu == null)
                            Game1.activeClickableMenu = (IClickableMenu)new QuestLog();
                    }
                    else if (((!Game1.eventUp ? 0 : (Game1.CurrentEvent != null ? 1 : 0)) & (flag22 ? 1 : 0)) != 0 && !Game1.CurrentEvent.skipped && Game1.CurrentEvent.skippable)
                    {
                        Game1.CurrentEvent.skipped = true;
                        Game1.CurrentEvent.skipEvent();
                        Game1.freezeControls = false;
                    }
                    if (((!Game1.options.gamepadControls || Game1.dayOfMonth <= 0 || !Game1.player.CanMove ? 0 : (Game1.isAnyGamePadButtonBeingPressed() ? 1 : 0)) & (flag23 ? 1 : 0)) != 0 && !Game1.dialogueUp && !Game1.eventUp)
                    {
                        if (Game1.activeClickableMenu == null)
                            Game1.activeClickableMenu = (IClickableMenu)new GameMenu(4, -1);
                    }
                    else if (((Game1.dayOfMonth <= 0 ? 0 : (Game1.player.CanMove ? 1 : 0)) & (flag23 ? 1 : 0)) != 0 && !Game1.dialogueUp && (!Game1.eventUp && Game1.activeClickableMenu == null))
                        Game1.activeClickableMenu = (IClickableMenu)new GameMenu(3, -1);
                    Game1.checkForRunButton(state1, false);
                    Game1.oldKBState = state1;
                    Game1.oldMouseState = state2;
                    Game1.oldPadState = state3;
                }
            }
        }
    }
}
