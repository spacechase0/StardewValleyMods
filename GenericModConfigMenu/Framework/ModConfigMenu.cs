using System;
using System.Collections.Generic;
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
        private readonly Table Table;

        /// <summary>The number of field rows to offset when scrolling a config menu.</summary>
        private readonly int ScrollSpeed;

        /// <summary>Open the config UI for a specific mod.</summary>
        private readonly Action<IManifest, int> OpenModMenu;
        private bool InGame => Context.IsWorldReady;

        private List<Label> LabelsWithTooltips = new();


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
        /// <param name="configs">The mod configurations to display.</param>
        /// <param name="scrollTo">The initial scroll position, represented by the row index at the top of the visible area.</param>
        public ModConfigMenu(int scrollSpeed, Action<IManifest, int> openModMenu, Action<int> openKeybindingsMenu, ModConfigManager configs, Texture2D keybindingsTex, int? scrollTo = null)
        {
            this.ScrollSpeed = scrollSpeed;
            this.OpenModMenu = openModMenu;

            // init UI
            this.Ui = new RootElement();
            this.Table = new Table
            {
                RowHeight = 50,
                LocalPosition = new Vector2((Game1.uiViewport.Width - 800) / 2, 64),
                Size = new Vector2(800, Game1.uiViewport.Height - 128)
            };

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
                    ModConfig[] editable = configs
                        .GetAll()
                        .Where(entry => entry.AnyEditableInGame || !this.InGame)
                        .OrderBy(entry => entry.ModName)
                        .ToArray();

                    foreach (ModConfig entry in editable)
                    {
                        Label label = new Label
                        {
                            String = entry.ModName,
                            UserData = entry.ModManifest.Description,
                            Callback = _ => this.ChangeToModPage(entry.ModManifest)
                        };
                        this.Table.AddRow(new Element[] { label });
                        LabelsWithTooltips.Add(label);
                    }
                }
            }

            // non-editable mods heading
            {
                ModConfig[] notEditable = configs
                    .GetAll()
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
                        LabelsWithTooltips.Add(label);
                    }
                }
            }

            this.Ui.AddChild(this.Table);

            var button = new Button(keybindingsTex)
            {
                LocalPosition = this.Table.LocalPosition - new Vector2( keybindingsTex.Width / 2 + 32, 0 ),
                Callback = _ => openKeybindingsMenu( this.ScrollRow),
            };
            this.Ui.AddChild(button);

            if (Constants.TargetPlatform == GamePlatform.Android)
                this.initializeUpperRightCloseButton();
            else
                this.upperRightCloseButton = null;

            if (scrollTo != null)
                this.ScrollRow = scrollTo.Value;

            if (!InGame)
            {
                // This hack lets gamepad cursor movement work without a harmony patch
                Mod.instance.Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "titleInPosition").SetValue(false);
            }
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

            if (Constants.TargetPlatform != GamePlatform.Android)
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
            this.Ui = new RootElement();

            Vector2 newSize = new Vector2(800, Game1.uiViewport.Height - 128);
            this.Table.LocalPosition = new Vector2((Game1.uiViewport.Width - 800) / 2, 64);
            foreach (Element opt in this.Table.Children)
                opt.LocalPosition = new Vector2(newSize.X / (this.Table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);

            this.Table.Size = newSize;
            this.Table.Scrollbar.Update();
            this.Ui.AddChild(this.Table);
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
    }
}
