using System;
using System.Collections.Generic;
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
    internal class SpecificModConfigMenu : IClickableMenu, IAssetEditor
    {
        private readonly IManifest Manifest;
        private readonly bool InGame;

        private readonly ModConfig ModConfig;
        private readonly string CurrPage;
        private string PrevPage;

        private RootElement Ui = new();
        private readonly Table Table;
        private readonly List<Label> OptHovers = new();
        public static IClickableMenu ActiveConfigMenu;

        private readonly Dictionary<string, List<Image>> Textures = new();
        private readonly Queue<string> PendingTexChanges = new();

        public bool CanEdit<T>(IAssetInfo asset)
        {
            foreach (string key in this.Textures.Keys)
            {
                if (asset.AssetNameEquals(key))
                    return true;
            }
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            foreach (string key in this.Textures.Keys)
            {
                if (asset.AssetNameEquals(key))
                {
                    this.PendingTexChanges.Enqueue(key);
                }
            }
        }

        public SpecificModConfigMenu(IManifest modManifest, bool inGame, string page = "", string prevPage = null)
        {
            this.Manifest = modManifest;
            this.InGame = inGame;

            this.ModConfig = Mod.Instance.Configs[this.Manifest];
            this.CurrPage = page;

            Mod.Instance.Configs[this.Manifest].ActiveDisplayPage = this.ModConfig.Options[this.CurrPage];

            this.Table = new Table();
            this.Table.RowHeight = 50;
            this.Table.Size = new Vector2(Math.Min(1200, Game1.viewport.Width - 200), Game1.viewport.Height - 128 - 116);
            this.Table.LocalPosition = new Vector2((Game1.viewport.Width - this.Table.Size.X) / 2, (Game1.viewport.Height - this.Table.Size.Y) / 2);
            foreach (var opt in this.ModConfig.Options[page].Options)
            {
                opt.SyncToMod();
                if (this.InGame && !opt.AvailableInGame)
                    continue;

                var label = new Label() { String = opt.Name };
                label.UserData = opt.Description;
                if (opt.Description != null && opt.Description != "")
                    this.OptHovers.Add(label);

                Element other = new Label() { String = "TODO", LocalPosition = new Vector2(500, 0) };
                Element other2 = null;
                if (opt is ComplexModOption c)
                {
                    var custom = new ComplexModOptionWidget(c);
                    custom.LocalPosition = new Vector2(this.Table.Size.X / 2, 0);
                    other = custom;
                }
                else if (opt is SimpleModOption<bool> b)
                {
                    var checkbox = new Checkbox();
                    checkbox.LocalPosition = new Vector2(this.Table.Size.X / 2, 0);
                    checkbox.Checked = b.Value;
                    checkbox.Callback = (Element e) => b.Value = (e as Checkbox).Checked;
                    other = checkbox;
                }
                else if (opt is SimpleModOption<SButton> k)
                {
                    if (Constants.TargetPlatform == GamePlatform.Android)
                        continue; // TODO: Support virtual keyboard input.
                    var label2 = new Label() { String = k.Value.ToString() };
                    label2.LocalPosition = new Vector2(this.Table.Size.X / 2, 0);
                    label2.Callback = (Element e) => this.DoKeybindingFor(k, e as Label);
                    other = label2;
                }
                else if (opt is SimpleModOption<KeybindList> k2)
                {
                    if (Constants.TargetPlatform == GamePlatform.Android)
                        continue; // TODO: Support virtual keyboard input.
                    var label2 = new Label() { String = k2.Value.IsBound ? k2.Value.Keybinds[0].ToString() : "(None)" };
                    label2.LocalPosition = new Vector2(this.Table.Size.X / 2, 0);
                    label2.Callback = (Element e) => this.DoKeybinding2For(k2, e as Label);
                    other = label2;
                }
                else if (opt is ClampedModOption<int> ci)
                {
                    var label2 = new Label() { String = ci.Value.ToString() };
                    label2.LocalPosition = new Vector2(this.Table.Size.X / 2 + this.Table.Size.X / 3 + 50, 0);
                    other2 = label2;
                    var slider = new Slider<int>();
                    slider.LocalPosition = new Vector2(this.Table.Size.X / 2, 0);
                    slider.RequestWidth = (int)this.Table.Size.X / 3;
                    slider.Value = ci.Value;
                    slider.Minimum = ci.Minimum;
                    slider.Maximum = ci.Maximum;
                    slider.Interval = ci.Interval;
                    slider.Callback = (Element e) =>
                    {
                        ci.Value = (e as Slider<int>).Value;
                        label2.String = ci.Value.ToString();
                    };
                    other = slider;
                }
                else if (opt is ClampedModOption<float> cf)
                {
                    var label2 = new Label() { String = cf.Value.ToString() };
                    label2.LocalPosition = new Vector2(this.Table.Size.X / 2 + this.Table.Size.X / 3 + 50, 0);
                    other2 = label2;
                    var slider = new Slider<float>();
                    slider.LocalPosition = new Vector2(this.Table.Size.X / 2, 0);
                    slider.RequestWidth = (int)this.Table.Size.X / 3;
                    slider.Value = cf.Value;
                    slider.Minimum = cf.Minimum;
                    slider.Maximum = cf.Maximum;
                    slider.Interval = cf.Interval;
                    slider.Callback = (Element e) =>
                    {
                        cf.Value = (e as Slider<float>).Value;
                        label2.String = cf.Value.ToString();
                    };
                    other = slider;
                }
                else if (opt is ChoiceModOption<string> cs)
                {
                    var dropdown = new Dropdown() { Choices = cs.Choices };
                    dropdown.LocalPosition = new Vector2(this.Table.Size.X / 2, 0);
                    dropdown.RequestWidth = (int)this.Table.Size.X / 2;
                    dropdown.Value = cs.Value;
                    dropdown.MaxValuesAtOnce = Math.Min(dropdown.Choices.Length, 5);
                    dropdown.Callback = (Element e) => cs.Value = (e as Dropdown).Value;
                    other = dropdown;
                }
                // The following need to come after the Clamped/ChoiceModOption's since those subclass these
                else if (opt is SimpleModOption<int> i)
                {
                    if (Constants.TargetPlatform == GamePlatform.Android)
                        continue; // TODO: Support virtual keyboard input.
                    var intbox = new Intbox();
                    intbox.LocalPosition = new Vector2(this.Table.Size.X / 2 - 8, 0);
                    intbox.Value = i.Value;
                    intbox.Callback = (Element e) => i.Value = (e as Intbox).Value;
                    other = intbox;
                }
                else if (opt is SimpleModOption<float> f)
                {
                    if (Constants.TargetPlatform == GamePlatform.Android)
                        continue; // TODO: Support virtual keyboard input.
                    var floatbox = new Floatbox();
                    floatbox.LocalPosition = new Vector2(this.Table.Size.X / 2 - 8, 0);
                    floatbox.Value = f.Value;
                    floatbox.Callback = (Element e) => f.Value = (e as Floatbox).Value;
                    other = floatbox;
                }
                else if (opt is SimpleModOption<string> s)
                {
                    if (Constants.TargetPlatform == GamePlatform.Android)
                        continue; // TODO: Support virtual keyboard input.
                    var textbox = new Textbox();
                    textbox.LocalPosition = new Vector2(this.Table.Size.X / 2 - 8, 0);
                    textbox.String = s.Value;
                    textbox.Callback = (Element e) => s.Value = (e as Textbox).String;
                    other = textbox;
                }
                else if (opt is LabelModOption l)
                {
                    label.LocalPosition = new Vector2(-8, 0);
                    label.Bold = true;
                    if (l.Name == "")
                        label = null;
                    other = null;
                }
                else if (opt is PageLabelModOption pl)
                {
                    label.Bold = true;
                    label.Callback = (Element e) =>
                    {
                        if (TitleMenu.subMenu == this)
                            TitleMenu.subMenu = new SpecificModConfigMenu(this.Manifest, this.InGame, pl.NewPage, this.CurrPage);
                        else if (Game1.activeClickableMenu == this)
                            Game1.activeClickableMenu = new SpecificModConfigMenu(this.Manifest, this.InGame, pl.NewPage, this.CurrPage);
                    };
                    other = null;
                }
                else if (opt is ParagraphModOption p)
                {
                    label.NonBoldScale = 0.75f;
                    label.NonBoldShadow = false;
                    other = null;

                    string[] text = p.Name.Split(' ');
                    label.String = text[0] + " ";
                    for (int it = 1; it < text.Length; ++it)
                    {
                        string oldStr = label.String;
                        label.String += text[it];
                        if (label.Measure().X >= this.Table.Size.X)
                        {
                            label.String = oldStr + "\n" + text[it];
                        }
                        if (it < text.Length - 1)
                            label.String += " ";
                    }

                    string[] lines = label.String.Split('\n');
                    for (int il = 0; il < lines.Length; il += 2)
                    {
                        this.Table.AddRow(new Element[] { new Label()
                        {
                            UserData = opt.Description,
                            NonBoldScale = 0.75f,
                            NonBoldShadow = false,
                            String = lines[ il + 0 ] + "\n" + (il + 1 >= lines.Length ? "" : lines[ il + 1 ])
                        } });
                        continue;
                    }
                    continue;
                }
                else if (opt is ImageModOption t)
                {
                    var tex = Game1.content.Load<Texture2D>(t.TexturePath);
                    var imgSize = new Vector2(tex.Width, tex.Height);
                    if (t.TextureRect.HasValue)
                        imgSize = new Vector2(t.TextureRect.Value.Width, t.TextureRect.Value.Height);
                    imgSize *= t.Scale;


                    var localPos = new Vector2(this.Table.Size.X / 2 - imgSize.X / 2, 0);
                    var baseRectPos = new Vector2(
                        t.TextureRect.HasValue ? t.TextureRect.Value.X : 0,
                        t.TextureRect.HasValue ? t.TextureRect.Value.Y : 0
                    );

                    var texs = new List<Image>();
                    if (this.Textures.ContainsKey(t.TexturePath))
                        texs = this.Textures[t.TexturePath];
                    else
                        this.Textures.Add(t.TexturePath, texs);

                    for (int ir = 0; ir < imgSize.Y / this.Table.RowHeight; ++ir)
                    {
                        int section = Math.Min((int)(imgSize.Y / t.Scale), this.Table.RowHeight);
                        int baseY = (int)(baseRectPos.Y + section * ir);
                        if (baseY + section > baseRectPos.Y + imgSize.Y / t.Scale)
                        {
                            section = (int)(baseRectPos.Y + imgSize.Y / t.Scale) - baseY;
                        }
                        var img = new Image
                        {
                            Texture = tex,
                            TextureRect = new Rectangle((int)baseRectPos.X, baseY, (int)imgSize.X / t.Scale, section),
                            Scale = t.Scale,
                            LocalPosition = localPos
                        };
                        texs.Add(img);
                        this.Table.AddRow(new Element[] { img });
                    }

                    continue;
                }

                if (label == null)
                    this.Table.AddRow(new Element[] { });
                else if (other == null)
                    this.Table.AddRow(new Element[] { label });
                else if (other2 == null)
                    this.Table.AddRow(new[] { label, other });
                else
                    this.Table.AddRow(new[] { label, other, other2 });
            }
            this.Ui.AddChild(this.Table);

            this.AddDefaultLabels(modManifest);

            // We need to update widgets at least once so ComplexModOptionWidget's get initialized
            this.Table.ForceUpdateEvenHidden();

            SpecificModConfigMenu.ActiveConfigMenu = this;

            Mod.Instance.Helper.Content.AssetEditors.Add(this);
        }

        private void AddDefaultLabels(IManifest modManifest)
        {
            string page = this.ModConfig.Options[this.CurrPage].DisplayName;
            var titleLabel = new Label() { String = modManifest.Name + (page == "" ? "" : " > " + page), Bold = true };
            titleLabel.LocalPosition = new Vector2((Game1.viewport.Width - titleLabel.Measure().X) / 2, 12 + 32);
            titleLabel.HoverTextColor = titleLabel.IdleTextColor;
            this.Ui.AddChild(titleLabel);

            var cancelLabel = new Label() { String = "Cancel", Bold = true };
            cancelLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 - 400, Game1.viewport.Height - 50 - 36);
            cancelLabel.Callback = (Element e) => this.Cancel();
            this.Ui.AddChild(cancelLabel);

            var defaultLabel = new Label() { String = "Default", Bold = true };
            defaultLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 - 200, Game1.viewport.Height - 50 - 36);
            defaultLabel.Callback = (Element e) => this.RevertToDefault();
            this.Ui.AddChild(defaultLabel);

            var saveLabel = new Label() { String = "Save", Bold = true };
            saveLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 + 50, Game1.viewport.Height - 50 - 36);
            saveLabel.Callback = (Element e) => this.Save();
            this.Ui.AddChild(saveLabel);

            var saveCloseLabel = new Label() { String = "Save&Close", Bold = true };
            saveCloseLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 + 200, Game1.viewport.Height - 50 - 36);
            saveCloseLabel.Callback = (Element e) => { this.Save(); this.Close(); };
            this.Ui.AddChild(saveCloseLabel);
        }

        public void ReceiveScrollWheelActionSmapi(int direction)
        {
            if (TitleMenu.subMenu == this || Game1.activeClickableMenu == this)
            {
                if (Dropdown.ActiveDropdown == null)
                    this.Table.Scrollbar.ScrollBy(direction / -120);
            }
            else
                SpecificModConfigMenu.ActiveConfigMenu = null;
        }

        public override bool readyToClose()
        {
            return false;
        }

        public override void update(GameTime time)
        {
            base.update(time);
            this.Ui.Update();

            while (this.PendingTexChanges.Count > 0)
            {
                string texPath = this.PendingTexChanges.Dequeue();
                var tex = Game1.content.Load<Texture2D>(texPath);

                foreach (var images in this.Textures[texPath])
                {
                    images.Texture = tex;
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Color(0, 0, 0, 192));
            IClickableMenu.drawTextureBox(b, (Game1.viewport.Width - 800) / 2 - 32, 32, 800 + 64, 50 + 20, Color.White);
            IClickableMenu.drawTextureBox(b, (Game1.viewport.Width - 800) / 2 - 32, Game1.viewport.Height - 50 - 20 - 32, 800 + 64, 50 + 20, Color.White);

            this.Ui.Draw(b);

            if (this.KeybindingOpt != null)
            {
                b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Color(0, 0, 0, 192));

                int boxX = (Game1.viewport.Width - 650) / 2, boxY = (Game1.viewport.Height - 200) / 2;
                IClickableMenu.drawTextureBox(b, boxX, boxY, 650, 200, Color.White);

                string s = "Rebinding key: " + this.KeybindingOpt.Name;
                int sw = (int)Game1.dialogueFont.MeasureString(s).X;
                b.DrawString(Game1.dialogueFont, s, new Vector2((Game1.viewport.Width - sw) / 2, boxY + 20), Game1.textColor);

                s = "Press a key to rebind";
                sw = (int)Game1.dialogueFont.MeasureString(s).X;
                b.DrawString(Game1.dialogueFont, s, new Vector2((Game1.viewport.Width - sw) / 2, boxY + 100), Game1.textColor);
            }

            if (this.Keybinding2Opt != null)
            {
                b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Color(0, 0, 0, 192));

                int boxX = (Game1.viewport.Width - 650) / 2, boxY = (Game1.viewport.Height - 200) / 2;
                IClickableMenu.drawTextureBox(b, boxX, boxY, 650, 200, Color.White);

                string s = "Rebinding key: " + this.Keybinding2Opt.Name;
                int sw = (int)Game1.dialogueFont.MeasureString(s).X;
                b.DrawString(Game1.dialogueFont, s, new Vector2((Game1.viewport.Width - sw) / 2, boxY + 20), Game1.textColor);

                s = "Press a key combination to rebind";
                sw = (int)Game1.dialogueFont.MeasureString(s).X;
                b.DrawString(Game1.dialogueFont, s, new Vector2((Game1.viewport.Width - sw) / 2, boxY + 100), Game1.textColor);
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

        private void RevertToDefault()
        {
            Game1.playSound("backpackIN");
            this.ModConfig.RevertToDefault.Invoke();
            foreach (var page in this.ModConfig.Options)
                foreach (var opt in page.Value.Options)
                    opt.SyncToMod();
            this.ModConfig.SaveToFile.Invoke();

            if (TitleMenu.subMenu == this)
                TitleMenu.subMenu = new SpecificModConfigMenu(this.Manifest, this.InGame, this.CurrPage, this.PrevPage);
            else if (Game1.activeClickableMenu == this)
                Game1.activeClickableMenu = new SpecificModConfigMenu(this.Manifest, this.InGame, this.CurrPage, this.PrevPage);
        }

        private void Save()
        {
            Game1.playSound("money");
            foreach (var page in this.ModConfig.Options)
                foreach (var opt in page.Value.Options)
                    opt.Save();
            this.ModConfig.SaveToFile.Invoke();
        }

        private void Close()
        {
            if (TitleMenu.subMenu == this)
                TitleMenu.subMenu = new ModConfigMenu(this.InGame);
            else if (!this.InGame && Game1.activeClickableMenu == this)
                Game1.activeClickableMenu = null;
            else
                Game1.activeClickableMenu = new ModConfigMenu(this.InGame);

            Mod.Instance.Helper.Content.AssetEditors.Remove(this);
        }

        private void Cancel()
        {
            Game1.playSound("bigDeSelect");
            this.Close();
        }

        private SimpleModOption<SButton> KeybindingOpt;
        private SimpleModOption<KeybindList> Keybinding2Opt;
        private Label KeybindingLabel;
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
            if (!e.Button.TryGetKeyboard(out Keys keys) && !e.Button.TryGetController(out _))
                return;
            if (e.Button.ToString() == "Escape")
            {
                Game1.playSound("bigDeSelect");
            }
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
                if (button.TryGetKeyboard(out Keys keys) || button.TryGetController(out _))
                {
                    all.Add(button);
                }
            }

            foreach (var button in e.Released)
            {
                bool stop = false;
                if (button.ToString() == "Escape")
                {
                    stop = true;
                    Game1.playSound("bigDeSelect");
                }
                if (!stop && (button.TryGetKeyboard(out Keys keys) || button.TryGetController(out _)))
                {
                    stop = true;
                    all.Add(button);

                    Game1.playSound("coin");
                    this.Keybinding2Opt.Value.Keybinds[0] = new Keybind(all.ToArray());
                    this.KeybindingLabel.String = this.Keybinding2Opt.Value.Keybinds[0].ToString();
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

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.Ui = new RootElement();

            Vector2 newSize = new Vector2(Math.Min(1200, Game1.viewport.Width - 200), Game1.viewport.Height - 128 - 116);

            foreach (Element opt in this.Table.Children)
            {
                opt.LocalPosition = new Vector2(newSize.X / (this.Table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);
                if (opt is Slider slider)
                    slider.RequestWidth = (int)(newSize.X / (this.Table.Size.X / slider.Width));
            }

            this.Table.Size = newSize;
            this.Table.LocalPosition = new Vector2((Game1.viewport.Width - this.Table.Size.X) / 2, (Game1.viewport.Height - this.Table.Size.Y) / 2);
            this.Table.Scrollbar.Update();
            this.Ui.AddChild(this.Table);
            this.AddDefaultLabels(this.Manifest);
        }
    }
}
