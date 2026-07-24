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
        public readonly int CustomWidth;

        public WideTextbox(int width)
        {
            this.CustomWidth = width;
        }

        public override int Width => this.CustomWidth;

        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            // Draw textbox background using drawTextureBox so it scales properly
            IClickableMenu.drawTextureBox(b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                (int)this.Position.X,
                (int)this.Position.Y - 8,
                this.CustomWidth,
                64,
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

    internal class ModConfigMenu : ElementMenu
    {
        /*********
        ** Fields
        *********/
        private readonly Table Table;

        /*********
        ** Accessors
        *********/
        /// <summary>Whether the search textbox is currently active (typing).</summary>
        public bool IsTypingInSearchBox => this.SearchBox != null && this.SearchBox.Selected;

        /// <summary>Open the config UI for a specific mod.</summary>
        private readonly Action<IManifest, int> OpenModMenu;
        private bool InGame => Context.IsWorldReady;

        private List<Label> LabelsWithTooltips = new();

        /// <summary>The search textbox for filtering mods.</summary>
        private WideTextbox? SearchBox;

        /// <summary>The current search query.</summary>
        private string CurrentSearchQuery = "";

        /// <summary>The last search query that was processed.</summary>
        private string LastProcessedSearchQuery = "";

        /// <summary>The placeholder label for the search box.</summary>
        private Label? SearchPlaceholder;

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
            : base(scrollSpeed)
        {
            this.OpenModMenu = openModMenu;
            this.AllConfigs = configs;

            // Table width (standard size)
            int tableWidth = 800;
            this.Table = new Table
            {
                RowHeight = 50,
                LocalPosition = new Vector2((Game1.uiViewport.Width - tableWidth) / 2, 64 + 50),
                Size = new Vector2(tableWidth, Game1.uiViewport.Height - 128 - 50)
            };

            // Create search box (at the top, same width as full UI with margin)
            // Search bar width = full UI width (table + 64px borders on each side)
            int searchWidth = tableWidth + 64;
            this.SearchBox = new WideTextbox(searchWidth)
            {
                LocalPosition = new Vector2((Game1.uiViewport.Width - searchWidth) / 2, 16),
                String = "",
                Callback = _ => this.OnSearchChanged(),
            };

            KeybindsButton = new Button(keybindsTexture)
            {
                LocalPosition = this.Table.LocalPosition - new Vector2(keybindsTexture.Width / 2 + 32, 0),
                Callback = _ => openKeybindsMenu(this.ScrollRow),
                ScreenReaderText = I18n.List_Keybinds(),
            };

            // Create search placeholder (will be hidden when typing) - black text
            this.SearchPlaceholder = new Label
            {
                String = I18n.List_SearchLabel(),
                LocalPosition = new Vector2((Game1.uiViewport.Width - SearchBox.CustomWidth) / 2 + 18, 24),
                NonBoldScale = 0.8f,
                IdleTextColor = Color.Black * 0.6f,
                HoverTextColor = Color.Black * 0.6f,
                ForceHide = () => !string.IsNullOrEmpty(this.SearchBox.String),
                ScreenReaderIgnore = true,
            };

            MakeUi();
            if (scrollTo != null)
                this.ScrollRow = scrollTo.Value;
        }

        protected override void AddUiContents()
        {
            Ui.AddChild(Table);
            Ui.AddChild(SearchBox);
            Ui.AddChild(KeybindsButton);
            //Ui.AddChild(SearchPlaceholder);

            // Populate initial list
            this.RebuildModList();
        }

        protected override void UnhandledKeyPress(Keys key)
        {
            if (key == Keys.Escape)
            {
                Mod.ActiveConfigMenu = null;
                return;
            }

            base.UnhandledKeyPress(key);
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Color(0, 0, 0, 192));
            base.draw(b);

            if (GetChildMenu() == null)
            {
                foreach (var label in this.LabelsWithTooltips)
                {
                    if (!label.Hover || label.UserData == null)
                        continue;
                    string text = (string)label.UserData;
                    if (text != null && !text.Contains("\n"))
                        text = Game1.parseText(text, Game1.smallFont, 800);
                    string? title = label.String;
                    if (title != null && !title.Contains("\n"))
                        title = Game1.parseText(title, Game1.dialogueFont, 800);
                    IClickableMenu.drawToolTip(b, text, title, null);
                }
            }
        }

        /// <inheritdoc />
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            // Table width (standard size)
            int tableWidth = 800;
            this.Table.LocalPosition = new Vector2((Game1.uiViewport.Width - tableWidth) / 2, 64 + 50);

            Vector2 newSize = new Vector2(tableWidth, Game1.uiViewport.Height - 128 - 50);
            this.Table.Size = newSize;
            this.Table.Scrollbar.Update();

            // Reposition search box (at the top, same width as full UI with margin)
            this.SearchBox.LocalPosition = new Vector2((Game1.uiViewport.Width - SearchBox.CustomWidth) / 2, 16);
            this.SearchPlaceholder.LocalPosition = new Vector2((Game1.uiViewport.Width - SearchBox.CustomWidth) / 2 + 20, 20);

            KeybindsButton.LocalPosition = this.Table.LocalPosition - new Vector2(KeybindsButton.Width / 2 + 32, 0);

            base.gameWindowSizeChanged(oldBounds, newBounds);
        }

        public override void populateClickableComponentList()
        {
            foreach (var entry in Ui.GetGamepadMovementRegions().ToArray())
            {
                if (entry.leftNeighborID == -1)
                    entry.leftNeighborID = KeybindsButton.GetGamepadMovementRegions().First().myID;
            }

            base.populateClickableComponentList();
        }

        public override void snapToDefaultClickableComponent()
        {
            if (Game1.options.gamepadControls)
            {
                var allTable = Table.Children.SelectMany(c => c.GetGamepadMovementRegions()).ToArray();
                currentlySnappedComponent = allClickableComponents.FirstOrDefault(c => c.visible && allTable.Contains(c));
                currentlySnappedComponent ??= allClickableComponents.FirstOrDefault(c => c.visible);
                snapCursorToCurrentSnappedComponent();
            }
            else
            {
                currentlySnappedComponent = SearchBox.GetGamepadMovementRegions().FirstOrDefault();
                snapCursorToCurrentSnappedComponent();
                SearchBox.Selected = true;
            }
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
            this.CurrentSearchQuery = this.SearchBox?.String ?? "";

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
