using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenericModConfigMenu.Framework.ModOption;
using GenericModConfigMenu.Framework.Overlays;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu.Framework
{
    /// <summary>The config UI for a specific mod.</summary>
    internal class SpecificModConfigMenu : IClickableMenu
    {
        /*********
        ** Fields
        *********/
        /// <summary>The minimum number of pixels between each main button.</summary>
        private const int MinimumButtonGap = 32;

        private readonly Action<string> OpenPage;
        private readonly Action ReturnToList;

        private readonly ModConfig ModConfig;
        private readonly int ScrollSpeed;
        private bool IsSubPage => !string.IsNullOrEmpty(this.CurrPage);

        private RootElement Ui = new();
        private readonly Table Table;
        private readonly List<Label> OptHovers = new();

        /// <summary>Whether the user hit escape.</summary>
        private bool ExitOnNextUpdate;

        /// <summary>Whether a save is currently loaded.</summary>
        private bool InGame => Context.IsWorldReady;

        /// <summary>The active keybind overlay, if any.</summary>
        private IKeybindOverlay ActiveKeybindOverlay;

        /// <summary>Whether a keybinding UI is open.</summary>
        private bool IsBindingKey => this.ActiveKeybindOverlay != null;

        /// <summary>The current width of the title label.</summary>
        private int TitleLabelWidth = 0;

        private ModConfigManager ConfigsForKeybinds;

        private List<Label> keybindOpts = new();


        /*********
        ** Accessors
        *********/
        public IManifest Manifest => this.ModConfig?.ModManifest;
        public readonly string CurrPage;
        public bool IsKeybindingsPage => Manifest == null;


        /*********
        ** Public methods
        *********/
        // This is the keybindings menu constructor
        public SpecificModConfigMenu(ModConfigManager mods, int scrollSpeed, Action returnToList)
        {
            ConfigsForKeybinds = mods;
            ScrollSpeed = scrollSpeed;
            ReturnToList = returnToList;

            this.Table = new Table(fixedRowHeight: false)
            {
                RowHeight = 50,
                Size = new Vector2(Math.Min(1200, Game1.uiViewport.Width - 200), Game1.uiViewport.Height - 128 - 116)
            };
            this.Table.LocalPosition = new Vector2((Game1.uiViewport.Width - this.Table.Size.X) / 2, (Game1.uiViewport.Height - this.Table.Size.Y) / 2);
            foreach (var config in mods.GetAll())
            {
                List<Element[]> rows = new();
                foreach (var opt in config.GetAllOptions())
                {
                    if (!(opt is SimpleModOption<SButton> || opt is SimpleModOption<KeybindList>))
                        continue;

                    string name = opt.Name();
                    string tooltip = opt.Tooltip();

                    if (this.InGame && opt.IsTitleScreenOnly)
                        continue;

                    opt.BeforeMenuOpened();

                    Label label = new Label
                    {
                        String = name,
                        UserData = tooltip
                    };
                    if (!string.IsNullOrEmpty(tooltip))
                        this.OptHovers.Add(label);

                    Element optionElement = new Label
                    {
                        String = "TODO",
                        LocalPosition = new Vector2(500, 0)
                    };
                    Label rightLabel = null;
                    switch (opt)
                    {
                        case SimpleModOption<SButton> option:
                            if (Constants.TargetPlatform == GamePlatform.Android)
                                continue; // TODO: Support virtual keyboard input.

                            optionElement = new Label
                            {
                                String = option.FormatValue(),
                                LocalPosition = new Vector2(this.Table.Size.X / 5 * 4, 0),
                                Callback = (Element e) => this.ShowKeybindOverlay(option, e as Label),
                                UserData = opt,
                            };
                            break;

                        case SimpleModOption<KeybindList> option:
                            if (Constants.TargetPlatform == GamePlatform.Android)
                                continue; // TODO: Support virtual keyboard input.

                            optionElement = new Label
                            {
                                String = option.FormatValue(),
                                LocalPosition = new Vector2(this.Table.Size.X / 5 * 4, 0),
                                Callback = (Element e) => this.ShowKeybindOverlay(option, e as Label),
                                UserData = opt,
                            };
                            break;
                    }

                    keybindOpts.Add(optionElement as Label);
                    rows.Add(new[] { label, optionElement, rightLabel }.Where(p => p != null).ToArray());
                }

                if (rows.Count > 0)
                {
                    Label header = new()
                    {
                        String = config.ModName,
                        UserData = config.ModManifest.Description,
                        Bold = true,
                    };
                    if (!string.IsNullOrEmpty(config.ModManifest.Description))
                        OptHovers.Add(header);
                    Table.AddRow([header]);

                    foreach (var row in rows)
                        Table.AddRow(row);

                    Table.AddRow([]);
                }
            }
            this.Ui.AddChild(this.Table);
            this.AddDefaultLabels(null);

            // We need to update widgets at least once so ComplexModOptionWidget's get initialized
            this.Table.ForceUpdateEvenHidden();

            RefreshKeybindingColor();
        }

        public SpecificModConfigMenu(ModConfig config, int scrollSpeed, string page, Action<string> openPage, Action returnToList)
        {
            this.ModConfig = config;
            this.ScrollSpeed = scrollSpeed;
            this.OpenPage = openPage;
            this.ReturnToList = returnToList;

            this.CurrPage = page ?? "";

            this.ModConfig.ActiveDisplayPage = this.ModConfig.Pages[this.CurrPage];

            this.Table = new Table(fixedRowHeight: false)
            {
                RowHeight = 50,
                Size = new Vector2(Math.Min(1200, Game1.uiViewport.Width - 200), Game1.uiViewport.Height - 128 - 116)
            };
            this.Table.LocalPosition = new Vector2((Game1.uiViewport.Width - this.Table.Size.X) / 2, (Game1.uiViewport.Height - this.Table.Size.Y) / 2);
            foreach (var opt in this.ModConfig.Pages[this.CurrPage].Options)
            {
                string name = opt.Name();
                string tooltip = opt.Tooltip();

                if (this.InGame && opt.IsTitleScreenOnly)
                    continue;

                opt.BeforeMenuOpened();

                Label label = new Label
                {
                    String = name,
                    UserData = tooltip
                };
                if (!string.IsNullOrEmpty(tooltip))
                    this.OptHovers.Add(label);

                Element optionElement = new Label
                {
                    String = "TODO",
                    LocalPosition = new Vector2(500, 0)
                };
                Label rightLabel = null;
                switch (opt)
                {
                    case ComplexModOption option:
                        optionElement = new ComplexModOptionWidget(option)
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2, 0)
                        };
                        break;

                    case SimpleModOption<bool> option:
                        optionElement = new Checkbox
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2, 0),
                            Checked = option.Value,
                            Callback = (Element e) => option.Value = (e as Checkbox).Checked
                        };
                        break;

                    case SimpleModOption<SButton> option:
                        if (Constants.TargetPlatform == GamePlatform.Android)
                            continue; // TODO: Support virtual keyboard input.

                        optionElement = new Label
                        {
                            String = option.FormatValue(),
                            LocalPosition = new Vector2(this.Table.Size.X / 2, 0),
                            Callback = (Element e) => this.ShowKeybindOverlay(option, e as Label)
                        };
                        break;

                    case SimpleModOption<KeybindList> option:
                        if (Constants.TargetPlatform == GamePlatform.Android)
                            continue; // TODO: Support virtual keyboard input.

                        optionElement = new Label
                        {
                            String = option.FormatValue(),
                            LocalPosition = new Vector2(this.Table.Size.X / 2, 0),
                            Callback = (Element e) => this.ShowKeybindOverlay(option, e as Label)
                        };
                        break;

                    case NumericModOption<int> option when (option.Minimum.HasValue && option.Maximum.HasValue):
                        rightLabel = new Label
                        {
                            String = option.FormatValue()
                        };

                        optionElement = new Slider<int>
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2, 0),
                            RequestWidth = (int)this.Table.Size.X / 3,
                            Value = option.Value,
                            Minimum = option.Minimum.Value,
                            Maximum = option.Maximum.Value,
                            Interval = option.Interval ?? 1,
                            Callback = e =>
                            {
                                option.Value = (e as Slider<int>).Value;
                                rightLabel.String = option.FormatValue();
                            }
                        };

                        rightLabel.LocalPosition = optionElement.LocalPosition + new Vector2(x: optionElement.Width + 15, y: 0);
                        break;

                    case NumericModOption<float> option when (option.Minimum.HasValue && option.Maximum.HasValue):
                        rightLabel = new Label
                        {
                            String = option.FormatValue()
                        };

                        optionElement = new Slider<float>
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2, 0),
                            RequestWidth = (int)this.Table.Size.X / 3,
                            Value = option.Value,
                            Minimum = option.Minimum.Value,
                            Maximum = option.Maximum.Value,
                            Interval = option.Interval ?? 0.01f,
                            Callback = (Element e) =>
                            {
                                option.Value = (e as Slider<float>).Value;
                                rightLabel.String = option.FormatValue();
                            }
                        };

                        rightLabel.LocalPosition = optionElement.LocalPosition + new Vector2(x: optionElement.Width + 15, y: 0);
                        break;

                    // The following need to come after the Clamped/ChoiceModOption's since those subclass these
                    case ChoiceModOption<string> option:
                        optionElement = new Dropdown
                        {
                            Choices = option.Choices,
                            Labels = option.Choices.Select(value => option.FormatChoice?.Invoke(value) ?? value).ToArray(),
                            LocalPosition = new Vector2(this.Table.Size.X / 2, 0),
                            RequestWidth = (int)this.Table.Size.X / 2,
                            Value = option.Value,
                            MaxValuesAtOnce = Math.Min(option.Choices.Length, 5),
                            Callback = (Element e) => option.Value = (e as Dropdown).Value
                        };
                        break;

                    case SimpleModOption<int> option:
                        if (Constants.TargetPlatform == GamePlatform.Android)
                            continue; // TODO: Support virtual keyboard input.

                        optionElement = new Intbox
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2 - 8, 0),
                            Value = option.Value,
                            Callback = (Element e) => option.Value = (e as Intbox).Value
                        };
                        break;

                    case SimpleModOption<float> option:
                        if (Constants.TargetPlatform == GamePlatform.Android)
                            continue; // TODO: Support virtual keyboard input.

                        optionElement = new Floatbox
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2 - 8, 0),
                            Value = option.Value,
                            Callback = (Element e) => option.Value = (e as Floatbox).Value
                        };
                        break;

                    case SimpleModOption<string> option:
                        if (Constants.TargetPlatform == GamePlatform.Android)
                            continue; // TODO: Support virtual keyboard input.

                        optionElement = new Textbox
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2 - 8, 0),
                            String = option.Value,
                            Callback = (Element e) => option.Value = (e as Textbox).String
                        };
                        if (option.Width.HasValue)
                            (optionElement as Textbox).SetWidth = option.Width.Value;
                        if (option.Height.HasValue)
                            (optionElement as Textbox).SetHeight = option.Height.Value;
                        break;

                    case SectionTitleModOption _:
                        label.LocalPosition = new Vector2(-8, 0);
                        label.Bold = true;
                        if (name == "")
                            label = null;
                        optionElement = null;
                        break;

                    case PageLinkModOption option:
                        label.Bold = true;
                        label.Callback = _ => this.OpenPage(option.PageId);
                        optionElement = null;
                        break;

                    case ParagraphModOption _:
                        {
                            label = null;
                            optionElement = null;

                            StringBuilder text = new StringBuilder(name.Length + 50);
                            {
                                string nextLine = "";
                                foreach (string word in name.Split(' '))
                                {
                                    // respect newline characters
                                    if (word == "\n") {
                                        text.AppendLine(nextLine);
                                        nextLine = "";
                                        continue;
                                    }

                                    // always add at least one word
                                    if (nextLine == "")
                                    {
                                        nextLine = word;
                                        continue;
                                    }

                                    // else append if it fits
                                    string possibleLine = $"{nextLine} {word}".Trim();
                                    if (Label.MeasureString(possibleLine, font: Game1.smallFont).X <= this.Table.Size.X)
                                    {
                                        nextLine = possibleLine;
                                        continue;
                                    }

                                    // else start new line
                                    text.AppendLine(nextLine);
                                    nextLine = word;
                                }

                                if (nextLine != "")
                                    text.AppendLine(nextLine);
                            }

                            label = null;
                            optionElement = new Label
                            {
                                UserData = tooltip,
                                NonBoldScale = 1f,
                                NonBoldShadow = false,
                                Font = Game1.smallFont,
                                String = text.ToString()
                            };
                            break;
                        }

                    case ImageModOption option:
                        {
                            var texture = option.Texture();

                            var size = new Vector2(texture.Width, texture.Height);
                            if (option.TexturePixelArea.HasValue)
                                size = new Vector2(option.TexturePixelArea.Value.Width, option.TexturePixelArea.Value.Height);
                            size *= option.Scale;

                            var localPos = new Vector2(this.Table.Size.X / 2 - size.X / 2, 0);
                            optionElement = new Image
                            {
                                Texture = texture,
                                TexturePixelArea = option.TexturePixelArea ?? new Rectangle(0, 0, (int)size.X, (int)size.Y),
                                Scale = option.Scale,
                                LocalPosition = localPos
                            };

                            break;
                        }
                }

                this.Table.AddRow(new[] { label, optionElement, rightLabel }.Where(p => p != null).ToArray());

            }
            this.Ui.AddChild(this.Table);
            this.AddDefaultLabels(this.Manifest);

            // We need to update widgets at least once so ComplexModOptionWidget's get initialized
            this.Table.ForceUpdateEvenHidden();
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.IsBindingKey)
            {
                this.ActiveKeybindOverlay.OnLeftClick(x, y);
                if (this.ActiveKeybindOverlay.IsFinished)
                    this.CloseKeybindOverlay();
            }
        }

        /// <inheritdoc />
        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape && !this.IsBindingKey)
                this.ExitOnNextUpdate = true;
        }

        /// <inheritdoc />
        public override void receiveScrollWheelAction(int direction)
        {
            if (Dropdown.ActiveDropdown == null)
                this.Table.Scrollbar.ScrollBy(direction / -this.ScrollSpeed);
        }

        /// <inheritdoc />
        public override bool readyToClose()
        {
            return false;
        }

        private int scrollCounter = 0;
        /// <inheritdoc />
        public override void update(GameTime time)
        {
            base.update(time);
            this.Ui.Update();

            // TODO: This will be different if a dropdown is open
            if (Game1.input.GetGamePadState().ThumbSticks.Right.Y != 0)
            {
                if (++scrollCounter == 5)
                {
                    scrollCounter = 0;
                    this.Table.Scrollbar.ScrollBy(Math.Sign(Game1.input.GetGamePadState().ThumbSticks.Right.Y) * 120 / -this.ScrollSpeed);
                }
            }
            else scrollCounter = 0;

            if (this.ExitOnNextUpdate)
                this.Cancel();
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            // main background
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Color(0, 0, 0, 192));

            // title background
            int titleBoxWidth = Math.Clamp(this.TitleLabelWidth + 64, 800 + 64, Game1.uiViewport.Width);
            IClickableMenu.drawTextureBox(b, (Game1.uiViewport.Width - titleBoxWidth) / 2, 32, titleBoxWidth, 50 + 20, Color.White);

            // button background
            IClickableMenu.drawTextureBox(b, (Game1.uiViewport.Width - 800) / 2 - 32 - 64, Game1.uiViewport.Height - 50 - 20 - 32, 800 + 64 + 128, 50 + 20, Color.White);

            // UI elements
            this.Ui.Draw(b);

            // keybind UI
            this.ActiveKeybindOverlay?.Draw(b);

            // mouse
            this.drawMouse(b);

            // hover tooltips
            if (Constants.TargetPlatform != GamePlatform.Android && GetChildMenu() == null)
            {
                foreach (var label in this.OptHovers)
                {
                    if (!label.Hover)
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

            Vector2 newSize = new Vector2(Math.Min(1200, Game1.uiViewport.Width - 200), Game1.uiViewport.Height - 128 - 116);

            foreach (Element opt in this.Table.Children)
            {
                opt.LocalPosition = new Vector2(newSize.X / (this.Table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);
                if (opt is Slider slider)
                    slider.RequestWidth = (int)(newSize.X / (this.Table.Size.X / slider.Width));
            }

            this.Table.Size = newSize;
            this.Table.LocalPosition = new Vector2((Game1.uiViewport.Width - this.Table.Size.X) / 2, (Game1.uiViewport.Height - this.Table.Size.Y) / 2);
            this.Table.Scrollbar.Update();
            this.Ui.AddChild(this.Table);
            this.AddDefaultLabels(this.Manifest);

            this.ActiveKeybindOverlay?.OnWindowResized();
        }

        /// <inheritdoc/>
        public override bool overrideSnappyMenuCursorMovementBan()
        {
            return true;
        }


        /// <summary>Raised when any buttons are pressed or released.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnButtonsChanged(ButtonsChangedEventArgs e)
        {
            if (this.IsBindingKey)
            {
                this.ActiveKeybindOverlay.OnButtonsChanged(e);
                if (this.ActiveKeybindOverlay.IsFinished)
                    this.CloseKeybindOverlay();
            }
        }


        /*********
        ** Private methods
        *********/
        private void AddDefaultLabels(IManifest modManifest)
        {
            // add page title
            {
                string pageTitle = modManifest == null ? "" : this.ModConfig.Pages[this.CurrPage].PageTitle();
                var titleLabel = new Label
                {
                    String = modManifest == null ? I18n.List_Keybindings() : (modManifest.Name + (pageTitle == "" ? "" : " > " + pageTitle)),
                    Bold = true
                };
                titleLabel.LocalPosition = new Vector2((Game1.uiViewport.Width - titleLabel.Measure().X) / 2, 12 + 32);
                titleLabel.HoverTextColor = titleLabel.IdleTextColor;
                this.Ui.AddChild(titleLabel);
                this.TitleLabelWidth = (int) titleLabel.Measure().X;
            }

            // add buttons
            {
                // create buttons
                Vector2 leftPosition = new Vector2(Game1.uiViewport.Width / 2 - 450, Game1.uiViewport.Height - 50 - 36);
                var cancelButton = new Label
                {
                    String = I18n.Config_Buttons_Cancel(),
                    Bold = true,
                    LocalPosition = leftPosition,
                    Callback = _ => this.Cancel()
                };
                var resetButton = new Label
                {
                    String = I18n.Config_Buttons_ResetToDefault(),
                    Bold = true,
                    LocalPosition = leftPosition,
                    Callback = _ => this.ResetConfig(),
                    ForceHide = () => this.IsSubPage || modManifest == null
                };
                var saveButton = new Label
                {
                    String = I18n.Config_Buttons_Save(),
                    Bold = true,
                    LocalPosition = leftPosition,
                    Callback = _ => this.SaveConfig()
                };
                var saveAndCloseButton = new Label
                {
                    String = I18n.Config_Buttons_SaveAndClose(),
                    Bold = true,
                    LocalPosition = leftPosition,
                    Callback = _ =>
                    {
                        this.SaveConfig();
                        this.Close();
                    }
                };
                Label[] buttons = new[] { cancelButton, resetButton, saveButton, saveAndCloseButton };
                int[] widths = buttons.Select(p => p.Width).ToArray();

                // calculate positions to spread evenly across available space
                // (if the buttons are too big to fit, overflow the button area instead of overlapping buttons)
                int totalButtonWidths = widths.Sum();
                int leftOffset = 0;
                int gap = ((800 + 64 + 50) - totalButtonWidths) / (buttons.Length - 1);
                if (gap < MinimumButtonGap)
                {
                    leftOffset = -((MinimumButtonGap - gap) / 2) * (buttons.Length - 1);
                    gap = MinimumButtonGap;
                }

                // set button positions
                for (int i = 0; i < buttons.Length; i++)
                    buttons[i].LocalPosition += new Vector2(leftOffset + widths.Take(i).Sum() + (gap * i), 0);

                // add to UI
                foreach (var button in buttons)
                    this.Ui.AddChild(button);
            }
        }

        private void ResetConfig()
        {
            Game1.playSound("backpackIN");

            // reset
            foreach (var option in this.ModConfig.GetAllOptions())
                option.BeforeReset();
            this.ModConfig.Reset();
            foreach (var option in this.ModConfig.GetAllOptions())
                option.AfterReset();

            // save & fetch new values
            this.SaveConfig(playSound: false);

            // reopen page
            this.OpenPage(this.CurrPage);
        }

        private void SaveConfig(bool playSound = true)
        {
            if (playSound)
                Game1.playSound("money");

            if (ModConfig != null)
            {
                foreach (var option in this.ModConfig.GetAllOptions())
                    option.BeforeSave();
                this.ModConfig.Save();
                foreach (var option in this.ModConfig.GetAllOptions())
                    option.AfterSave();
            }
            else
            {
                foreach (var config in ConfigsForKeybinds.GetAll().ToArray())
                {
                    bool foundKey = false;
                    foreach (var option in config.GetAllOptions())
                    {
                        if (option is SimpleModOption<SButton> || option is SimpleModOption<KeybindList>)
                        {
                            foundKey = true;
                            break;
                        }
                    }

                    if (!foundKey)
                        continue;

                    foreach (var option in config.GetAllOptions())
                        option.BeforeSave();
                    config.Save();
                    foreach (var option in config.GetAllOptions())
                        option.AfterSave();
                }
            }
        }

        private void Close()
        {
            if (ModConfig != null)
            {
                foreach (var option in this.ModConfig.ActiveDisplayPage.Options)
                    option.BeforeMenuClosed();
            }
            else
            {
                foreach (var config in ConfigsForKeybinds.GetAll())
                {
                    foreach (var option in config.GetAllOptions())
                    {
                        if (option is SimpleModOption<SButton> || option is SimpleModOption<KeybindList>)
                        {
                            option.BeforeMenuClosed();
                        }
                    }
                }
            }

            if (this.IsSubPage)
                this.OpenPage(null);
            else
                this.ReturnToList();
        }

        private void Cancel()
        {
            Game1.playSound("bigDeSelect");
            this.Close();
        }

        /// <summary>Show the keybind overlay for an option.</summary>
        /// <typeparam name="TKeybind">The keybind type.</typeparam>
        /// <param name="option">The option being bound.</param>
        /// <param name="label">The label to update when the key is reassigned.</param>
        private void ShowKeybindOverlay<TKeybind>(SimpleModOption<TKeybind> option, Label label)
        {
            Game1.playSound("breathin");
            this.ActiveKeybindOverlay = new KeybindOverlay<TKeybind>(option, label);
            this.Ui.Obscured = true;
        }

        /// <summary>Close the current keybind overlay.</summary>
        private void CloseKeybindOverlay()
        {
            this.ActiveKeybindOverlay = null;
            this.Ui.Obscured = false;

            RefreshKeybindingColor();
        }

        private void RefreshKeybindingColor()
        {
            if (IsKeybindingsPage)
            {
                Dictionary<string, int> keybinds = new();
                foreach (var opt in keybindOpts)
                {
                    if (opt.UserData is SimpleModOption<Keybind> kopt)
                    {
                        string entry = kopt.FormatValue();
                        if (!keybinds.ContainsKey(entry))
                            keybinds.Add(entry, 0);
                        keybinds[entry]++;
                    }
                    else if (opt.UserData is SimpleModOption<KeybindList> klopt)
                    {
                        string entry = klopt.FormatValue();
                        if (!keybinds.ContainsKey(entry))
                            keybinds.Add(entry, 0);
                        keybinds[entry]++;
                    }
                }

                foreach (var opt in keybindOpts)
                {
                    string entry = "";
                    if (opt.UserData is SimpleModOption<Keybind> kopt)
                        entry = kopt.FormatValue();
                    else if (opt.UserData is SimpleModOption<KeybindList> klopt)
                        entry = klopt.FormatValue();

                    if (!keybinds.ContainsKey(entry))
                        continue; // I have no clue how this happened.

                    if (keybinds[entry] > 1 && entry != "(None)")
                    {
                        opt.IdleTextColor = Color.Red;
                        opt.HoverTextColor = Color.PaleVioletRed;
                    }
                    else
                    {
                        opt.IdleTextColor = Game1.textColor;
                        opt.HoverTextColor = Game1.unselectedOptionColor;
                    }
                }
            }
        }
    }
}
