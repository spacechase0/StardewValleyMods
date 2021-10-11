using System;
using System.Collections.Generic;
using System.Linq;
using GenericModConfigMenu.Framework.UI;
using GenericModConfigMenu.ModOption;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        private readonly bool InGame;
        private readonly Action<string> OpenPage;
        private readonly Action ReturnToList;

        private readonly ModConfig ModConfig;
        private readonly int ScrollSpeed;
        private bool IsSubPage => !string.IsNullOrEmpty(this.CurrPage);

        private RootElement Ui = new();
        private readonly Table Table;
        private readonly List<Label> OptHovers = new();

        private readonly Dictionary<string, Image[]> FieldTextures = new();

        /// <summary>Whether the user hit escape.</summary>
        private bool ExitOnNextUpdate;

        private SimpleModOption<SButton> KeybindingOpt;
        private SimpleModOption<KeybindList> Keybinding2Opt;
        private Label KeybindingLabel;

        /// <summary>Whether a keybinding UI is open.</summary>
        private bool IsBindingKey => this.KeybindingOpt != null || this.Keybinding2Opt != null;


        /*********
        ** Accessors
        *********/
        public IManifest Manifest => this.ModConfig.ModManifest;
        public readonly string CurrPage;
        public static IClickableMenu ActiveConfigMenu;


        /*********
        ** Public methods
        *********/
        public SpecificModConfigMenu(ModConfig config, bool inGame, int scrollSpeed, string page, Action<string> openPage, Action returnToList)
        {
            this.ModConfig = config;
            this.InGame = inGame;
            this.ScrollSpeed = scrollSpeed;
            this.OpenPage = openPage;
            this.ReturnToList = returnToList;

            this.CurrPage = page ?? "";

            this.ModConfig.ActiveDisplayPage = this.ModConfig.Options[this.CurrPage];

            this.Table = new Table
            {
                RowHeight = 50,
                Size = new Vector2(Math.Min(1200, Game1.uiViewport.Width - 200), Game1.uiViewport.Height - 128 - 116)
            };
            this.Table.LocalPosition = new Vector2((Game1.uiViewport.Width - this.Table.Size.X) / 2, (Game1.uiViewport.Height - this.Table.Size.Y) / 2);
            foreach (var opt in this.ModConfig.Options[this.CurrPage].Options)
            {
                string name = opt.Name();
                string tooltip = opt.Tooltip();

                opt.GetLatest();
                if (this.InGame && !opt.EditableInGame)
                    continue;

                Label label = new Label
                {
                    String = name,
                    UserData = tooltip
                };
                if (!string.IsNullOrEmpty(tooltip))
                    this.OptHovers.Add(label);

                Element other = new Label
                {
                    String = "TODO",
                    LocalPosition = new Vector2(500, 0)
                };
                Element other2 = null;
                switch (opt)
                {
                    case ComplexModOption option:
                        other = new ComplexModOptionWidget(option)
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2, 0)
                        };
                        break;

                    case SimpleModOption<bool> option:
                        other = new Checkbox
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2, 0),
                            Checked = option.Value,
                            Callback = (Element e) => option.Value = (e as Checkbox).Checked
                        };
                        break;

                    case SimpleModOption<SButton> option:
                        if (Constants.TargetPlatform == GamePlatform.Android)
                            continue; // TODO: Support virtual keyboard input.

                        other = new Label
                        {
                            String = option.Value != SButton.None ? option.Value.ToString() : I18n.Config_RebindKey_NoKey(),
                            LocalPosition = new Vector2(this.Table.Size.X / 2, 0),
                            Callback = (Element e) => this.DoKeybindingFor(option, e as Label)
                        };
                        break;

                    case SimpleModOption<KeybindList> option:
                        if (Constants.TargetPlatform == GamePlatform.Android)
                            continue; // TODO: Support virtual keyboard input.

                        other = new Label
                        {
                            String = option.Value.IsBound ? option.Value.ToString() : I18n.Config_RebindKey_NoKey(),
                            LocalPosition = new Vector2(this.Table.Size.X / 2, 0),
                            Callback = (Element e) => this.DoKeybinding2For(option, e as Label)
                        };
                        break;

                    case NumericModOption<int> option when (option.Minimum.HasValue && option.Maximum.HasValue):
                        {
                            var label2 = new Label
                            {
                                String = option.Value.ToString(),
                                LocalPosition = new Vector2(this.Table.Size.X / 2 + this.Table.Size.X / 3 + 50, 0)
                            };
                            other2 = label2;

                            other = new Slider<int>
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
                                    label2.String = option.Value.ToString();
                                }
                            };
                            break;
                        }

                    case NumericModOption<float> option when (option.Minimum.HasValue && option.Maximum.HasValue):
                        {
                            var label2 = new Label
                            {
                                String = option.Value.ToString(),
                                LocalPosition = new Vector2(this.Table.Size.X / 2 + this.Table.Size.X / 3 + 50, 0)
                            };
                            other2 = label2;

                            other = new Slider<float>
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
                                    label2.String = option.Value.ToString();
                                }
                            };
                            break;
                        }

                    // The following need to come after the Clamped/ChoiceModOption's since those subclass these
                    case ChoiceModOption<string> option:
                        other = new Dropdown
                        {
                            Choices = option.Choices,
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

                        other = new Intbox
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2 - 8, 0),
                            Value = option.Value,
                            Callback = (Element e) => option.Value = (e as Intbox).Value
                        };
                        break;

                    case SimpleModOption<float> option:
                        if (Constants.TargetPlatform == GamePlatform.Android)
                            continue; // TODO: Support virtual keyboard input.

                        other = new Floatbox
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2 - 8, 0),
                            Value = option.Value,
                            Callback = (Element e) => option.Value = (e as Floatbox).Value
                        };
                        break;

                    case SimpleModOption<string> option:
                        if (Constants.TargetPlatform == GamePlatform.Android)
                            continue; // TODO: Support virtual keyboard input.

                        other = new Textbox
                        {
                            LocalPosition = new Vector2(this.Table.Size.X / 2 - 8, 0),
                            String = option.Value,
                            Callback = (Element e) => option.Value = (e as Textbox).String
                        };
                        break;

                    case SectionTitleModOption option:
                        label.LocalPosition = new Vector2(-8, 0);
                        label.Bold = true;
                        if (name == "")
                            label = null;
                        other = null;
                        break;

                    case PageLinkModOption option:
                        label.Bold = true;
                        label.Callback = _ => this.OpenPage(option.PageId);
                        other = null;
                        break;

                    case ParagraphModOption option:
                        {
                            label = null;
                            other = null;

                            List<string> lines = new();
                            {
                                string nextLine = "";
                                foreach (string word in name.Split(' '))
                                {
                                    // always add at least one word
                                    if (nextLine == "")
                                    {
                                        nextLine = word;
                                        continue;
                                    }

                                    // else append if it fits
                                    string possibleLine = $"{nextLine} {word}".Trim();
                                    if (Label.MeasureString(possibleLine).X <= this.Table.Size.X)
                                    {
                                        nextLine = possibleLine;
                                        continue;
                                    }

                                    // else start new line
                                    lines.Add(nextLine);
                                    nextLine = word;
                                }

                                if (nextLine != "")
                                    lines.Add(nextLine);
                            }

                            this.Table.AddRow(new Element[]
                            {
                                new Label
                                {
                                    UserData = tooltip,
                                    NonBoldScale = 0.75f,
                                    NonBoldShadow = false,
                                    String = string.Join("\n", lines)
                                }
                            });
                            break;
                        }

                    case ImageModOption option:
                        {
                            var tex = option.Texture();
                            var imgSize = new Vector2(tex.Width, tex.Height);
                            if (option.TexturePixelArea.HasValue)
                                imgSize = new Vector2(option.TexturePixelArea.Value.Width, option.TexturePixelArea.Value.Height);
                            imgSize *= option.Scale;


                            var localPos = new Vector2(this.Table.Size.X / 2 - imgSize.X / 2, 0);
                            var baseRectPos = new Vector2(
                                option.TexturePixelArea?.X ?? 0,
                                option.TexturePixelArea?.Y ?? 0
                            );

                            var images = new List<Image>();

                            for (int ir = 0; ir < imgSize.Y / this.Table.RowHeight; ++ir)
                            {
                                int section = Math.Min((int)(imgSize.Y / option.Scale), this.Table.RowHeight);
                                int baseY = (int)(baseRectPos.Y + section * ir);
                                if (baseY + section > baseRectPos.Y + imgSize.Y / option.Scale)
                                {
                                    section = (int)(baseRectPos.Y + imgSize.Y / option.Scale) - baseY;
                                }
                                var img = new Image
                                {
                                    Texture = tex,
                                    TextureRect = new Rectangle((int)baseRectPos.X, baseY, (int)imgSize.X / option.Scale, section),
                                    Scale = option.Scale,
                                    LocalPosition = localPos
                                };
                                images.Add(img);
                                this.Table.AddRow(new Element[] { img });
                            }

                            this.FieldTextures[opt.FieldId] = images.ToArray();

                            continue;
                        }
                }

                this.Table.AddRow(new[] { label, other, other2 }.Where(p => p != null).ToArray());
            }
            this.Ui.AddChild(this.Table);
            this.AddDefaultLabels(this.Manifest);

            // We need to update widgets at least once so ComplexModOptionWidget's get initialized
            this.Table.ForceUpdateEvenHidden();

            SpecificModConfigMenu.ActiveConfigMenu = this;
        }

        /// <inheritdoc />
        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape && !this.IsBindingKey)
                this.ExitOnNextUpdate = true;
        }

        public void ReceiveScrollWheelActionSmapi(int direction)
        {
            if (TitleMenu.subMenu == this || Game1.activeClickableMenu == this)
            {
                if (Dropdown.ActiveDropdown == null)
                    this.Table.Scrollbar.ScrollBy(direction / -this.ScrollSpeed);
            }
            else
                SpecificModConfigMenu.ActiveConfigMenu = null;
        }

        /// <inheritdoc />
        public override bool readyToClose()
        {
            return false;
        }

        /// <inheritdoc />
        public override void update(GameTime time)
        {
            base.update(time);
            this.Ui.Update();

            if (this.ExitOnNextUpdate)
                this.Cancel();
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Color(0, 0, 0, 192));
            IClickableMenu.drawTextureBox(b, (Game1.uiViewport.Width - 800) / 2 - 32, 32, 800 + 64, 50 + 20, Color.White);
            IClickableMenu.drawTextureBox(b, (Game1.uiViewport.Width - 800) / 2 - 32, Game1.uiViewport.Height - 50 - 20 - 32, 800 + 64, 50 + 20, Color.White);

            this.Ui.Draw(b);

            if (this.KeybindingOpt != null)
            {
                b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Color(0, 0, 0, 192));

                int boxX = (Game1.uiViewport.Width - 650) / 2, boxY = (Game1.uiViewport.Height - 200) / 2;
                IClickableMenu.drawTextureBox(b, boxX, boxY, 650, 200, Color.White);

                string s = I18n.Config_RebindKey_Title(this.KeybindingOpt.Name());
                int sw = (int)Game1.dialogueFont.MeasureString(s).X;
                b.DrawString(Game1.dialogueFont, s, new Vector2((Game1.uiViewport.Width - sw) / 2, boxY + 20), Game1.textColor);

                s = I18n.Config_RebindKey_SimpleInstructions();
                sw = (int)Game1.dialogueFont.MeasureString(s).X;
                b.DrawString(Game1.dialogueFont, s, new Vector2((Game1.uiViewport.Width - sw) / 2, boxY + 100), Game1.textColor);
            }

            if (this.Keybinding2Opt != null)
            {
                b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Color(0, 0, 0, 192));

                int boxX = (Game1.uiViewport.Width - 650) / 2, boxY = (Game1.uiViewport.Height - 200) / 2;
                IClickableMenu.drawTextureBox(b, boxX, boxY, 650, 200, Color.White);

                string s = I18n.Config_RebindKey_Title(this.Keybinding2Opt.Name());
                int sw = (int)Game1.dialogueFont.MeasureString(s).X;
                b.DrawString(Game1.dialogueFont, s, new Vector2((Game1.uiViewport.Width - sw) / 2, boxY + 20), Game1.textColor);

                s = I18n.Config_RebindKey_ComboInstructions();
                sw = (int)Game1.dialogueFont.MeasureString(s).X;
                b.DrawString(Game1.dialogueFont, s, new Vector2((Game1.uiViewport.Width - sw) / 2, boxY + 100), Game1.textColor);
            }

            this.drawMouse(b);

            if (Constants.TargetPlatform != GamePlatform.Android)
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
        }


        /*********
        ** Private methods
        *********/
        private void AddDefaultLabels(IManifest modManifest)
        {
            // add page title
            {
                string pageTitle = this.ModConfig.Options[this.CurrPage].PageTitle();
                var titleLabel = new Label
                {
                    String = modManifest.Name + (pageTitle == "" ? "" : " > " + pageTitle),
                    Bold = true
                };
                titleLabel.LocalPosition = new Vector2((Game1.uiViewport.Width - titleLabel.Measure().X) / 2, 12 + 32);
                titleLabel.HoverTextColor = titleLabel.IdleTextColor;
                this.Ui.AddChild(titleLabel);
            }

            // add buttons
            this.Ui.AddChild(new Label
            {
                String = I18n.Config_Buttons_Cancel(),
                Bold = true,
                LocalPosition = new Vector2(Game1.uiViewport.Width / 2 - 400, Game1.uiViewport.Height - 50 - 36),
                Callback = _ => this.Cancel()
            });
            this.Ui.AddChild(new Label
            {
                String = I18n.Config_Buttons_ResetToDefault(),
                Bold = true,
                LocalPosition = new Vector2(Game1.uiViewport.Width / 2 - 200, Game1.uiViewport.Height - 50 - 36),
                Callback = _ => this.ResetConfig()
            });
            this.Ui.AddChild(new Label
            {
                String = I18n.Config_Buttons_Save(),
                Bold = true,
                LocalPosition = new Vector2(Game1.uiViewport.Width / 2 + 50, Game1.uiViewport.Height - 50 - 36),
                Callback = _ => this.SaveConfig()
            });
            this.Ui.AddChild(new Label
            {
                String = I18n.Config_Buttons_SaveAndClose(),
                Bold = true,
                LocalPosition = new Vector2(Game1.uiViewport.Width / 2 + 200, Game1.uiViewport.Height - 50 - 36),
                Callback = _ =>
                {
                    this.SaveConfig();
                    this.Close();
                }
            });
        }

        private void ResetConfig()
        {
            Game1.playSound("backpackIN");
            this.ModConfig.Reset();
            foreach (var page in this.ModConfig.Options)
                foreach (var opt in page.Value.Options)
                    opt.GetLatest();
            this.ModConfig.Save();

            this.OpenPage(this.CurrPage);
        }

        private void SaveConfig()
        {
            Game1.playSound("money");
            foreach (var page in this.ModConfig.Options)
                foreach (var opt in page.Value.Options)
                    opt.Save();
            this.ModConfig.Save();
        }

        private void Close()
        {
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

        private void DoKeybindingFor(SimpleModOption<SButton> opt, Label label)
        {
            Game1.playSound("breathin");
            this.KeybindingOpt = opt;
            this.KeybindingLabel = label;
            this.Ui.Obscured = true;
            Mod.Instance.Helper.Events.Input.ButtonPressed += this.AssignKeybinding;
        }

        private void DoKeybinding2For(SimpleModOption<KeybindList> opt, Label label)
        {
            Game1.playSound("breathin");
            this.Keybinding2Opt = opt;
            this.KeybindingLabel = label;
            this.Ui.Obscured = true;
            Mod.Instance.Helper.Events.Input.ButtonsChanged += this.AssignKeybinding2;
        }

        private void AssignKeybinding(object sender, ButtonPressedEventArgs e)
        {
            if (this.KeybindingOpt == null)
                return;
            if (!e.Button.TryGetKeyboard(out _) && !e.Button.TryGetController(out _))
                return;

            if (e.Button == SButton.Escape)
                Game1.playSound("bigDeSelect");
            else
            {
                Game1.playSound("coin");
                this.KeybindingOpt.Value = e.Button;
                this.KeybindingLabel.String = e.Button.ToString();
            }
            Mod.Instance.Helper.Events.Input.ButtonPressed -= this.AssignKeybinding;
            this.KeybindingOpt = null;
            this.KeybindingLabel = null;
            this.Ui.Obscured = false;
        }

        private void AssignKeybinding2(object sender, ButtonsChangedEventArgs e)
        {
            if (this.Keybinding2Opt == null)
                return;

            List<SButton> all = new List<SButton>();
            foreach (var button in e.Held)
            {
                if (button.TryGetKeyboard(out _) || button.TryGetController(out _))
                    all.Add(button);
            }

            foreach (var button in e.Released)
            {
                bool stop = false;
                if (button == SButton.Escape)
                {
                    stop = true;
                    Game1.playSound("bigDeSelect");
                }
                if (!stop && (button.TryGetKeyboard(out _) || button.TryGetController(out _)))
                {
                    stop = true;
                    all.Add(button);

                    Game1.playSound("coin");
                    this.Keybinding2Opt.Value = new KeybindList(new Keybind(all.ToArray()));
                    this.KeybindingLabel.String = this.Keybinding2Opt.Value.ToString();
                }

                if (stop)
                {
                    Mod.Instance.Helper.Events.Input.ButtonsChanged -= this.AssignKeybinding2;
                    this.Keybinding2Opt = null;
                    this.KeybindingLabel = null;
                    this.Ui.Obscured = false;
                }

                return;
            }

        }
    }
}
