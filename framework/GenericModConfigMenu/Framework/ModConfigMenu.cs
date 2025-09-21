using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu.Framework
{
    internal class ModConfigMenu : IClickableMenu
    {
        /*********
        ** Fields
        *********/
        private RootElement Ui;
        private Table Table;
        private Textbox Filter;

        /// <summary>The number of field rows to offset when scrolling a config menu.</summary>
        private readonly int ScrollSpeed;

        /// <summary>Open the config UI for a specific mod.</summary>
        private readonly Action<IManifest, int> OpenModMenu;
        private bool InGame => Context.IsWorldReady;

        private List<Label> LabelsWithTooltips = new();

        private static Dictionary<string, string> modPaths = null;
        private static string filterText = "";
        private static int sortOrder = 0;

        /*********
        ** Accessors
        *********/
        /// <summary>The scroll position, represented by the row index at the top of the visible area.</summary>
        public int ScrollRow
        {
            get => this.Table.Scrollbar.TopRow;
            set => this.Table.Scrollbar.ScrollTo(value);
        }


        private void UpdateFilter(string filter)
        {
            filterText = filter ?? "";
            Table.Scrollbar.ScrollTo(0);
        }

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="scrollSpeed">The number of field rows to offset when scrolling a config menu.</param>
        /// <param name="openModMenu">Open the config UI for a specific mod.</param>
        /// <param name="openKeybindsMenu">Open the menu to configure mod keybinds.</param>
        /// <param name="keybindsTexture">The icon texture for the keybinds menu.</param>
        /// <param name="configs">The mod configurations to display.</param>
        /// <param name="scrollTo">The initial scroll position, represented by the row index at the top of the visible area.</param>
        public ModConfigMenu(int scrollSpeed, Action<IManifest, int> openModMenu, Action<int> openKeybindsMenu, ModConfigManager configs, Texture2D keybindsTexture, int? scrollTo = null)
        {
            this.ScrollSpeed = scrollSpeed;
            this.OpenModMenu = openModMenu;

            ModConfigMenu_InitUI(scrollSpeed, openModMenu, openKeybindsMenu, configs, keybindsTexture, scrollTo);

            if (!InGame)
            {
                // This hack lets gamepad cursor movement work without a harmony patch
                Mod.instance.Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "titleInPosition").SetValue(false);
            }
        }
        private void ModConfigMenu_InitUI(int scrollSpeed, Action<IManifest, int> openModMenu, Action<int> openKeybindsMenu, ModConfigManager configs, Texture2D keybindsTexture, int? scrollTo = null)
        {
            this.Ui = new RootElement();
            this.Table = new Table
            {
                RowHeight = 50,
                LocalPosition = new Vector2((Game1.uiViewport.Width - 800) / 2, 64),
                Size = new Vector2(800, Game1.uiViewport.Height - 128)
            };


            Filter = new() { String = filterText, Callback = (Element e) => UpdateFilter((e as Textbox).String) };
            Button btn = new(Mod.instance.Helper.GameContent.Load<Texture2D>(sortOrder == 0 ? AssetManager.SortNameButton : sortOrder == 1 ? AssetManager.SortCreatedButton : AssetManager.SortModifiedButton))
            {
                LocalPosition = new(200, -4),
                Callback = (e) =>
                {
                    if (e is Button b)
                    {
                        sortOrder++;
                        if (sortOrder == 3) sortOrder = 0;
                        ModConfigMenu_InitUI(scrollSpeed, openModMenu, openKeybindsMenu, configs, keybindsTexture, scrollTo);
                    }
                }
            };
            this.Table.AddRow([Filter, btn]);

            IEnumerable<ModConfig> allMods = configs.GetAll();

            if (modPaths == null)
            {
                modPaths = [];
                //could not find any way to locate all mod config files via API...
                string root = Path.GetDirectoryName(Mod.instance.Helper.DirectoryPath);//Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods");
                foreach (var manifest in Directory.GetFiles(root, "manifest.json", SearchOption.AllDirectories))
                {
                    string config = manifest.Replace("manifest.json", "config.json", StringComparison.Ordinal);
                    if (File.Exists(config))
                    {
                        try
                        {
                            using (StreamReader streamReader = new(manifest, System.Text.Encoding.UTF8))
                            {
                                string line;
                                while ((line = streamReader.ReadLine()) != null)
                                {
                                    if (line.Contains("\"Dependencies\"", StringComparison.Ordinal)) break;
                                    if (line.Contains("\"UniqueID\"", StringComparison.Ordinal))
                                    {
                                        foreach (var mod in allMods)
                                        {
                                            if (line.Contains("\"" + mod.ModManifest.UniqueID + "\"", StringComparison.Ordinal))
                                            {
                                                modPaths[mod.ModManifest.UniqueID] = config;
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            if (sortOrder == 0) allMods = allMods.OrderBy(entry => entry.ModName);
            else
            {
                allMods = allMods.OrderByDescending(entry =>
                {
                    if (modPaths.TryGetValue(entry.ModManifest.UniqueID, out string p))
                    {
                        if (sortOrder == 1) return File.GetCreationTimeUtc(p);
                        else return File.GetLastWriteTimeUtc(p);
                    }
                    return DateTime.MinValue;
                }).ThenBy(entry => entry.ModName);
            }

            // editable mods section
            {
                // heading
                var heading = new Label
                {
                    String = I18n.List_EditableHeading(),
                    Bold = true
                };
                heading.LocalPosition = new Vector2((800 - heading.Measure().X) / 2, heading.LocalPosition.Y);
                this.Table.AddRow(new Element[] { heading });

                // mod list
                {
                    foreach (ModConfig entry in allMods.Where(entry => entry.AnyEditableInGame || !this.InGame))
                    {
                        Label label = new Label
                        {
                            String = entry.ModName,
                            UserData = entry.ModManifest.Description,
                            Callback = _ => this.ChangeToModPage(entry.ModManifest),
                            ForceHide = () => !(string.IsNullOrEmpty(Filter.String) || entry.ModName.Contains(Filter.String, StringComparison.OrdinalIgnoreCase))
                        };
                        this.Table.AddRow(new Element[] { label });
                        LabelsWithTooltips.Add(label);
                    }
                }
            }

            // non-editable mods heading
            {
                IEnumerable<ModConfig> notEditable = allMods.Where(entry => !entry.AnyEditableInGame && this.InGame);

                if (notEditable.Any())
                {
                    // heading
                    var heading = new Label
                    {
                        String = I18n.List_NotEditableHeading(),
                        Bold = true
                    };
                    this.Table.AddRow(Array.Empty<Element>());
                    this.Table.AddRow(new Element[] { heading });

                    // mod list
                    foreach (ModConfig entry in notEditable)
                    {
                        Label label = new Label
                        {
                            String = entry.ModName,
                            UserData = entry.ModManifest.Description,
                            IdleTextColor = Color.Black * 0.4f,
                            HoverTextColor = Color.Black * 0.4f,
                            ForceHide = () => !(string.IsNullOrEmpty(Filter.String) || entry.ModName.Contains(Filter.String, StringComparison.OrdinalIgnoreCase))
                        };

                        this.Table.AddRow(new Element[] { label });
                        LabelsWithTooltips.Add(label);
                    }
                }
            }

            this.Ui.AddChild(this.Table);

            var button = new Button(keybindsTexture)
            {
                LocalPosition = this.Table.LocalPosition - new Vector2(keybindsTexture.Width / 2 + 32, 0),
                Callback = _ => openKeybindsMenu(this.ScrollRow),
            };
            this.Ui.AddChild(button);

            if (Constants.TargetPlatform == GamePlatform.Android)
                this.initializeUpperRightCloseButton();
            else
                this.upperRightCloseButton = null;

            if (scrollTo != null)
                this.ScrollRow = scrollTo.Value;
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.upperRightCloseButton?.containsPoint(x, y) == true && this.readyToClose())
            {
                if (playSound)
                    Game1.playSound("bigDeSelect");

                Mod.ActiveConfigMenu = null;
            }
        }

        /// <inheritdoc />
        public override void receiveScrollWheelAction(int direction)
        {
            this.Table.Scrollbar.ScrollBy(direction / -this.ScrollSpeed);
        }

        private int scrollCounter = 0;
        /// <inheritdoc />
        public override void update(GameTime time)
        {
            base.update(time);
            this.Ui.Update();

            if (Game1.input.GetGamePadState().ThumbSticks.Right.Y != 0)
            {
                if (++scrollCounter == 5)
                {
                    scrollCounter = 0;
                    this.Table.Scrollbar.ScrollBy(Math.Sign(Game1.input.GetGamePadState().ThumbSticks.Right.Y) * 120 / -this.ScrollSpeed);
                }
            }
            else scrollCounter = 0;
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Color(0, 0, 0, 192));
            this.Ui.Draw(b);
            this.upperRightCloseButton?.draw(b); // bring it above the backdrop
            if (this.InGame)
                this.drawMouse(b);

            if (Constants.TargetPlatform != GamePlatform.Android && GetChildMenu() == null)
            {
                foreach (var label in this.LabelsWithTooltips)
                {
                    if (!label.Hover || label.UserData == null)
                        continue;
                    string text = (string)label.UserData;
                    if (text != null && !text.Contains("\n"))
                        text = Game1.parseText(text, Game1.smallFont, 800);
                    string title = label.String;
                    if (title != null && !title.Contains("\n"))
                        title = Game1.parseText(title, Game1.dialogueFont, 800);
                    IClickableMenu.drawToolTip(b, text, title, null);
                }
            }
        }

        /// <inheritdoc />
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            var oldUi = this.Ui;

            this.Ui = new RootElement();

            Vector2 newSize = new Vector2(800, Game1.uiViewport.Height - 128);
            this.Table.LocalPosition = new Vector2((Game1.uiViewport.Width - 800) / 2, 64);
            foreach (Element opt in this.Table.Children)
                opt.LocalPosition = new Vector2(newSize.X / (this.Table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);

            this.Table.Size = newSize;
            this.Table.Scrollbar.Update();
            this.Ui.AddChild(this.Table);

            var b = oldUi.Children.First(e => e is Button);
            oldUi.RemoveChild(b);
            this.Ui.AddChild(b);
        }

        /// <inheritdoc/>
        public override bool overrideSnappyMenuCursorMovementBan()
        {
            return true;
        }

        /*********
        ** Private methods
        *********/
        private void ChangeToModPage(IManifest modManifest)
        {
            Log.Trace("Changing to mod config page for mod " + modManifest.UniqueID);
            Game1.playSound("bigSelect");

            this.OpenModMenu(modManifest, this.ScrollRow);
        }


        public void receiveKeyPress(IModHelper helper, SButton key)
        {
            if (Filter.Selected)
            {
                if (key == SButton.Escape)
                {
                    Filter.Selected = false;
                }
                if (key is not SButton.MouseLeft and not SButton.MouseRight and not SButton.MouseMiddle)
                {
                    helper.Input.Suppress(key);
                }
            }
            //if (key == SButton.MouseLeft) //test - should be in Textbox, but there's no mouse input check there
            //{
            //    ICursorPosition cursorPos = Mod.instance.Helper.Input.GetCursorPosition();
            //    if (Filter.Bounds.Contains(cursorPos.ScreenPixels))
            //    {
            //        Vector2 textWidth = Game1.smallFont.MeasureString(Filter.String);

            //    }
            //}
        }
    }
}
