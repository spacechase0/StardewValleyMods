using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using SpaceShared.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu.Framework
{
    /// <summary>Textbox with customizable width for search functionality.</summary>
    internal class WideTextbox : Textbox
    {
        private readonly int CustomWidth;

        public WideTextbox(int width)
        {
            this.CustomWidth = width;
        }

        public override int Width => this.CustomWidth;

        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);

            // Handle click to select/deselect
            if (this.ClickGestured)
            {
                this.Selected = this.Hover;
                if (this.Callback != null)
                    this.Callback(this);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            // Draw textbox background using drawTextureBox so it scales properly
            IClickableMenu.drawTextureBox(b, 
                Game1.menuTexture, 
                new Rectangle(0, 256, 60, 60), 
                (int)this.Position.X, 
                (int)this.Position.Y, 
                this.CustomWidth, 
                48, 
                Color.White);

            // Draw the text
            string text = this.String;
            Vector2 textSize;
            int maxWidth = this.CustomWidth - 32; // Subtract margins
            for (textSize = Game1.smallFont.MeasureString(text); textSize.X > maxWidth; textSize = Game1.smallFont.MeasureString(text))
                text = text.Substring(1);

            // Draw blinking cursor if selected
            if (DateTime.UtcNow.Millisecond % 1000 >= 500 && this.Selected)
                b.Draw(Game1.staminaRect, new Rectangle((int)this.Position.X + 16 + (int)textSize.X + 2, (int)this.Position.Y + 8, 4, 32), Game1.textColor);

            b.DrawString(Game1.smallFont, text, this.Position + new Vector2(16, 12), Game1.textColor);
        }
    }

    internal class ModConfigMenu : IClickableMenu
    {
        /*********
        ** Fields
        *********/
        private RootElement Ui;
        private readonly Table Table;

        /*********
        ** Accessors
        *********/
        /// <summary>Whether the search textbox is currently active (typing).</summary>
        public bool IsTypingInSearchBox => this.SearchBox != null && this.SearchBox.Selected;

        /// <summary>The number of field rows to offset when scrolling a config menu.</summary>
        private readonly int ScrollSpeed;

        /// <summary>Open the config UI for a specific mod.</summary>
        private readonly Action<IManifest, int> OpenModMenu;
        private bool InGame => Context.IsWorldReady;

        private List<Label> LabelsWithTooltips = new();

        /// <summary>The search textbox for filtering mods.</summary>
        private Textbox SearchBox;

        /// <summary>The current search query.</summary>
        private string CurrentSearchQuery = "";

        /// <summary>The last search query that was processed.</summary>
        private string LastProcessedSearchQuery = "";

        /// <summary>The placeholder label for the search box.</summary>
        private Label SearchPlaceholder;

        /// <summary>All mod configs available for display.</summary>
        private readonly ModConfigManager AllConfigs;

        private Button KeybindsButton;


        /*********
        ** Accessors
        *********/
        /// <summary>The scroll position, represented by the row index at the top of the visible area.</summary>
        public int ScrollRow
        {
            get => this.Table.Scrollbar.TopRow;
            set => this.Table.Scrollbar.ScrollTo(value);
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
            this.AllConfigs = configs;

            // init UI
            this.Ui = new RootElement(() => currentlySnappedComponent, dir => moveCursorInDirection(dir));

            // Table width (standard size)
            int tableWidth = 800;
            // Search bar width = full UI width (table + 64px borders on each side)
            int searchWidth = tableWidth + 128;

            // Create search box (at the top, same width as full UI with margin)
            this.SearchBox = new WideTextbox(searchWidth)
            {
                LocalPosition = new Vector2((Game1.uiViewport.Width - searchWidth) / 2, 16),
                String = "",
                Callback = _ => this.OnSearchChanged(),
                ScreenReaderIgnore = true,
            };
            this.Ui.AddChild(this.SearchBox);

            // Automatically activate textbox so user can type immediately
            if (!Game1.options.gamepadControls || Game1.lastCursorMotionWasMouse)
                this.SearchBox.Selected = true;

            // Create search placeholder (will be hidden when typing) - black text
            this.SearchPlaceholder = new Label
            {
                String = I18n.List_SearchLabel(),
                LocalPosition = new Vector2((Game1.uiViewport.Width - searchWidth) / 2 + 20, 20),
                NonBoldScale = 0.8f,
                IdleTextColor = Color.Black * 0.6f,
                HoverTextColor = Color.Black * 0.6f,
            };
            this.Ui.AddChild(this.SearchPlaceholder);

            this.Table = new Table
            {
                RowHeight = 50,
                LocalPosition = new Vector2((Game1.uiViewport.Width - tableWidth) / 2, 64 + 50),
                Size = new Vector2(tableWidth, Game1.uiViewport.Height - 128 - 50)
            };

            // Populate initial list
            this.RebuildModList();

            this.Ui.AddChild(this.Table);

            KeybindsButton = new Button(keybindsTexture)
            {
                LocalPosition = this.Table.LocalPosition - new Vector2( keybindsTexture.Width / 2 + 32, 0 ),
                Callback = _ => openKeybindsMenu( this.ScrollRow),
                ScreenReaderText = I18n.List_Keybinds(),
            };
            this.Ui.AddChild(KeybindsButton);

            if (Constants.TargetPlatform == GamePlatform.Android)
                this.initializeUpperRightCloseButton();
            else
                this.upperRightCloseButton = null;

            if (scrollTo != null)
                this.ScrollRow = scrollTo.Value;

            populateClickableComponentList();
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.upperRightCloseButton?.containsPoint(x, y) == true && this.readyToClose())
            {
                if (playSound)
                    Game1.playSound("bigDeSelect");

                Mod.ActiveConfigMenu = null;
                return;
            }

            // If clicked outside the textbox, deselect it
            if (this.SearchBox != null && this.SearchBox.Selected)
            {
                Rectangle searchBoxBounds = new Rectangle(
                    (int)this.SearchBox.Position.X,
                    (int)this.SearchBox.Position.Y,
                    this.SearchBox.Width,
                    this.SearchBox.Height
                );

                if (!searchBoxBounds.Contains(x, y))
                {
                    this.SearchBox.Selected = false;
                }
            }
        }

        /// <inheritdoc />
        public override void receiveKeyPress(Keys key)
        {
            // If textbox is active, don't process escape key here
            // The textbox will handle its own input through Game1.keyboardDispatcher
            if (this.SearchBox != null && this.SearchBox.Selected)
                return;

            // Only process Escape when not typing
            if (key == Keys.Escape)
            {
                Mod.ActiveConfigMenu = null;
                return;
            }

            if (Game1.options.snappyMenus && Game1.options.gamepadControls && !overrideSnappyMenuCursorMovementBan())
            {
                applyMovementKey(key);
            }
        }

        /// <inheritdoc />
        public override void receiveScrollWheelAction(int direction)
        {
            this.Table.Scrollbar.ScrollBy(direction / -this.ScrollSpeed);
            snapCursorToCurrentSnappedComponent();
        }

        private int scrollCounter = 0;
        /// <inheritdoc />
        public override void update(GameTime time)
        {
            base.update(time);
            this.Ui.Update();

            // Hide placeholder when typing
            if (this.SearchPlaceholder != null)
            {
                this.SearchPlaceholder.ForceHide = () => !string.IsNullOrEmpty(this.SearchBox.String);
            }

            if (Game1.input.GetGamePadState().ThumbSticks.Right.Y != 0)
            {
                if (++scrollCounter == 5)
                {
                    scrollCounter = 0;
                    this.Table.Scrollbar.ScrollBy(Math.Sign(Game1.input.GetGamePadState().ThumbSticks.Right.Y) * 120 / -this.ScrollSpeed);
                }
            }
            else scrollCounter = 0;

            if (Ui.GamepadMovementRegionsDirty)
                populateClickableComponentList();
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

            this.Ui = new RootElement(() => currentlySnappedComponent, dir => moveCursorInDirection(dir));

            // Table width (standard size)
            int tableWidth = 800;
            // Search bar width = full UI width (table + 64px borders on each side)
            int searchWidth = tableWidth + 128;

            // Reposition search box (at the top, same width as full UI with margin)
            this.SearchBox.LocalPosition = new Vector2((Game1.uiViewport.Width - searchWidth) / 2, 16);
            this.Ui.AddChild(this.SearchBox);

            // Re-add search placeholder
            this.SearchPlaceholder.LocalPosition = new Vector2((Game1.uiViewport.Width - searchWidth) / 2 + 20, 20);
            this.Ui.AddChild(this.SearchPlaceholder);

            Vector2 newSize = new Vector2(tableWidth, Game1.uiViewport.Height - 128 - 50);
            this.Table.LocalPosition = new Vector2((Game1.uiViewport.Width - tableWidth) / 2, 64 + 50);
            foreach (Element opt in this.Table.Children)
                opt.LocalPosition = new Vector2(newSize.X / (this.Table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);

            this.Table.Size = newSize;
            this.Table.Scrollbar.Update();
            this.Ui.AddChild(this.Table);

            var b = oldUi.Children.First(e => e is Button);
            oldUi.RemoveChild(b);
            b.LocalPosition = this.Table.LocalPosition - new Vector2(b.Width / 2 + 32, 0);
            this.Ui.AddChild(b);

            populateClickableComponentList();
        }

        public override void populateClickableComponentList()
        {
            base.populateClickableComponentList();

            foreach (var entry in Ui.GetGamepadMovementRegions().ToArray())
            {
                if (entry.leftNeighborID == -1)
                    entry.leftNeighborID = KeybindsButton.GetGamepadMovementRegions().First().myID;
                allClickableComponents.Add(entry);
            }
            Ui.GamepadMovementRegionsDirty = false;

            if (allClickableComponents.Contains(currentlySnappedComponent))
                snapToDefaultClickableComponent();
        }

        /// <inheritdoc/>
        public override bool overrideSnappyMenuCursorMovementBan()
        {
            return (Ui.CurrentSnappedElement?.CurrentlyUsingGamepadMovement(out bool snappy) ?? false) ? !snappy : false;
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = SearchBox.GetGamepadMovementRegions().FirstOrDefault();
            snapCursorToCurrentSnappedComponent();
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

        /// <summary>Called when the search text changes.</summary>
        private void OnSearchChanged()
        {
            this.CurrentSearchQuery = this.SearchBox.String;
            
            // Only rebuild if the text actually changed
            if (this.CurrentSearchQuery != this.LastProcessedSearchQuery)
            {
                this.LastProcessedSearchQuery = this.CurrentSearchQuery;
                this.RebuildModList();
                populateClickableComponentList();
            }
        }

        /// <summary>Clears all rows from the table.</summary>
        private void ClearTable()
        {
            // Use reflection to access the private rows list
            var rowsField = typeof(Table).GetField("Rows", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rowsField != null)
            {
                var rows = rowsField.GetValue(this.Table) as System.Collections.Generic.List<Element[]>;
                if (rows != null)
                {
                    // Remove all children except the scrollbar
                    foreach (var row in rows.ToArray())
                    {
                        foreach (var element in row)
                        {
                            this.Table.RemoveChild(element);
                        }
                    }
                    rows.Clear();
                }
            }
        }

        /// <summary>Rebuilds the mod list based on the current search query.</summary>
        private void RebuildModList()
        {
            this.LabelsWithTooltips.Clear();
            this.ClearTable();

            string searchQuery = this.CurrentSearchQuery.ToLower().Trim();

            // Get filtered mods
            ModConfig[] allMods = this.AllConfigs.GetAll().ToArray();
            ModConfig[] filteredMods = allMods;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                filteredMods = allMods
                    .Where(entry =>
                        entry.ModName.ToLower().Contains(searchQuery) ||
                        (entry.ModManifest.Description?.ToLower().Contains(searchQuery) ?? false)
                    )
                    .ToArray();
            }

            // Editable mods section
            {
                ModConfig[] editable = filteredMods
                    .Where(entry => entry.AnyEditableInGame || !this.InGame)
                    .OrderBy(entry => entry.ModName)
                    .ToArray();

                if (editable.Any())
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
                    foreach (ModConfig entry in editable)
                    {
                        Label label = new Label
                        {
                            String = entry.ModName,
                            UserData = entry.ModManifest.Description,
                            Callback = _ => this.ChangeToModPage(entry.ModManifest)
                        };
                        this.Table.AddRow(new Element[] { label });
                        this.LabelsWithTooltips.Add(label);
                    }
                }
            }

            // Non-editable mods section
            {
                ModConfig[] notEditable = filteredMods
                    .Where(entry => !entry.AnyEditableInGame && this.InGame)
                    .OrderBy(entry => entry.ModName)
                    .ToArray();

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
                            HoverTextColor = Color.Black * 0.4f
                        };

                        this.Table.AddRow(new Element[] { label });
                        this.LabelsWithTooltips.Add(label);
                    }
                }
            }

            // Show "no results" message if search returned nothing
            if (!filteredMods.Any() && !string.IsNullOrWhiteSpace(searchQuery))
            {
                var noResultsLabel = new Label
                {
                    String = I18n.List_NoResults(),
                    IdleTextColor = Color.Gray,
                    HoverTextColor = Color.Gray
                };
                noResultsLabel.LocalPosition = new Vector2((800 - noResultsLabel.Measure().X) / 2, noResultsLabel.LocalPosition.Y);
                this.Table.AddRow(new Element[] { noResultsLabel });
            }

            // Reset scroll to top only when there's an active search
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                this.ScrollRow = 0;
            }
        }
    }
}
