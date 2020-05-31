using GenericModConfigMenu.ModOption;
using GenericModConfigMenu.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace GenericModConfigMenu
{
    internal class SpecificModConfigMenu : IClickableMenu
    {
        private IManifest mod;

        private ModConfig modConfig;

        private RootElement ui = new RootElement();
        private Table table;
        private List<Label> optHovers = new List<Label>();
        public static IClickableMenu ActiveConfigMenu;

        public SpecificModConfigMenu(IManifest modManifest)
        {
            mod = modManifest;

            modConfig = Mod.instance.configs[mod];

            table = new Table();
            table.RowHeight = 50;
            table.Size = new Vector2(Math.Min(1200, Game1.viewport.Width - 200), Game1.viewport.Height - 128 - 116);
            table.LocalPosition = new Vector2((Game1.viewport.Width - table.Size.X) / 2, (Game1.viewport.Height - table.Size.Y) / 2);
            foreach (var opt in modConfig.Options)
            {
                opt.SyncToMod();

                var label = new Label() { String = opt.Name };
                label.UserData = opt.Description;
                if (opt.Description != null && opt.Description != "")
                    optHovers.Add(label);

                Element other = new Label() { String = "TODO", LocalPosition = new Vector2(500, 0) };
                Element other2 = null;
                if ( opt is ComplexModOption c )
                {
                    var custom = new ComplexModOptionWidget(c);
                    custom.LocalPosition = new Vector2( table.Size.X / 2, 0 );
                    other = custom;
                }
                else if ( opt is SimpleModOption<bool> b )
                {
                    var checkbox = new Checkbox();
                    checkbox.LocalPosition = new Vector2( table.Size.X / 2, 0 );
                    checkbox.Checked = b.Value;
                    checkbox.Callback = (Element e) => b.Value = (e as Checkbox).Checked;
                    other = checkbox;
                }
                else if ( opt is SimpleModOption<SButton> k )
                {
                    if (Constants.TargetPlatform == GamePlatform.Android)
                        continue; // TODO: Support virtual keyboard input.
                    var label2 = new Label() { String = k.Value.ToString() };
                    label2.LocalPosition = new Vector2(table.Size.X / 2, 0);
                    label2.Callback = (Element e) => doKeybindingFor(k, e as Label);
                    other = label2;
                }
                else if ( opt is ClampedModOption<int> ci )
                {
                    var label2 = new Label() { String = ci.Value.ToString() };
                    label2.LocalPosition = new Vector2(table.Size.X / 2 + table.Size.X / 3 + 50, 0);
                    other2 = label2;
                    var slider = new Slider<int>();
                    slider.LocalPosition = new Vector2(table.Size.X / 2, 0);
                    slider.RequestWidth = (int)table.Size.X / 3;
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
                else if ( opt is ClampedModOption<float> cf )
                {
                    var label2 = new Label() { String = cf.Value.ToString() };
                    label2.LocalPosition = new Vector2(table.Size.X / 2 + table.Size.X / 3 + 50, 0);
                    other2 = label2;
                    var slider = new Slider<float>();
                    slider.LocalPosition = new Vector2(table.Size.X / 2, 0);
                    slider.RequestWidth = (int)table.Size.X / 3;
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
                    dropdown.LocalPosition = new Vector2(table.Size.X / 2, 0);
                    dropdown.RequestWidth = (int)table.Size.X / 2;
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
                    intbox.LocalPosition = new Vector2(table.Size.X / 2 - 8, 0);
                    intbox.Value = i.Value;
                    intbox.Callback = (Element e) => i.Value = (e as Intbox).Value;
                    other = intbox;
                }
                else if (opt is SimpleModOption<float> f)
                {
                    if (Constants.TargetPlatform == GamePlatform.Android)
                        continue; // TODO: Support virtual keyboard input.
                    var floatbox = new Floatbox();
                    floatbox.LocalPosition = new Vector2(table.Size.X / 2 - 8, 0);
                    floatbox.Value = f.Value;
                    floatbox.Callback = (Element e) => f.Value = (e as Floatbox).Value;
                    other = floatbox;
                }
                else if (opt is SimpleModOption<string> s)
                {
                    if (Constants.TargetPlatform == GamePlatform.Android)
                        continue; // TODO: Support virtual keyboard input.
                    var textbox = new Textbox();
                    textbox.LocalPosition = new Vector2(table.Size.X / 2 - 8, 0);
                    textbox.String = s.Value;
                    textbox.Callback = (Element e) => s.Value = (e as Textbox).String;
                    other = textbox;
                }
                else if ( opt is LabelModOption l )
                {
                    label.LocalPosition = new Vector2(-8, 0);
                    label.Bold = true;
                    if (l.Name == "")
                        label = null;
                    other = null;
                }

                if (label == null)
                    table.AddRow(new Element[] { });
                else if (other == null)
                    table.AddRow(new Element[] { label });
                else if (other2 == null)
                    table.AddRow(new Element[] { label, other });
                else
                    table.AddRow(new Element[] { label, other, other2 });
            }
            ui.AddChild(table);

            addDefaultLabels(modManifest);

            // We need to update widgets at least once so ComplexModOptionWidget's get initialized
            table.ForceUpdateEvenHidden();

            ActiveConfigMenu = this;
        }

        private void addDefaultLabels(IManifest modManifest)
        {
            var titleLabel = new Label() { String = modManifest.Name, Bold = true };
            titleLabel.LocalPosition = new Vector2((Game1.viewport.Width - titleLabel.Measure().X) / 2, 12 + 32);
            titleLabel.HoverTextColor = titleLabel.IdleTextColor;
            ui.AddChild(titleLabel);

            var cancelLabel = new Label() { String = "Cancel", Bold = true };
            cancelLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 - 300, Game1.viewport.Height - 50 - 36);
            cancelLabel.Callback = (Element e) => cancel();
            ui.AddChild(cancelLabel);

            var defaultLabel = new Label() { String = "Default", Bold = true };
            defaultLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 - 50, Game1.viewport.Height - 50 - 36);
            defaultLabel.Callback = (Element e) => revertToDefault();
            ui.AddChild(defaultLabel);

            var saveLabel = new Label() { String = "Save", Bold = true };
            saveLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 + 200, Game1.viewport.Height - 50 - 36);
            saveLabel.Callback = (Element e) => save();
            ui.AddChild(saveLabel);
        }

        public void receiveScrollWheelActionSmapi(int direction)
        {
            if (TitleMenu.subMenu == this || Game1.activeClickableMenu == this)
            {
                if (Dropdown.ActiveDropdown == null)
                    table.Scrollbar.ScrollBy(direction / -120);
            }
            else
                ActiveConfigMenu = null;
        }

        public override bool readyToClose()
        {
            return false;
        }

        public override void update(GameTime time)
        {
            base.update(time);
            ui.Update();
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Color(0, 0, 0, 192));
            IClickableMenu.drawTextureBox(b, (Game1.viewport.Width - 800) / 2 - 32, 32, 800 + 64, 50 + 20, Color.White);
            IClickableMenu.drawTextureBox(b, (Game1.viewport.Width - 800) / 2 - 32, Game1.viewport.Height - 50 - 20 - 32, 800 + 64, 50 + 20, Color.White);

            ui.Draw(b);

            if ( keybindingOpt != null )
            {
                b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Color(0, 0, 0, 192));

                int boxX = (Game1.viewport.Width - 650) / 2, boxY = (Game1.viewport.Height - 200) / 2;
                IClickableMenu.drawTextureBox(b, boxX, boxY, 650, 200, Color.White);

                string s = "Rebinding key: " + keybindingOpt.Name;
                int sw = (int)Game1.dialogueFont.MeasureString(s).X;
                b.DrawString(Game1.dialogueFont, s, new Vector2((Game1.viewport.Width - sw) / 2, boxY + 20), Game1.textColor);

                s = "Press a key to rebind";
                sw = (int)Game1.dialogueFont.MeasureString(s).X;
                b.DrawString(Game1.dialogueFont, s, new Vector2((Game1.viewport.Width - sw) / 2, boxY + 100), Game1.textColor);
            }

            drawMouse(b);

            if (Constants.TargetPlatform != GamePlatform.Android)
            {
                foreach ( var label in optHovers )
                {
                    if (!label.Hover)
                        continue;
                    string text = (string) label.UserData;
                    if (text != null && !text.Contains("\n"))
                        text = Game1.parseText(text, Game1.smallFont, 800);
                    string title = label.String;
                    if (title != null && !title.Contains("\n"))
                        title = Game1.parseText(title, Game1.dialogueFont, 800);
                    drawToolTip(b, text, title, null);
                }
            }
        }

        private void revertToDefault()
        {
            Game1.playSound("backpackIN");
            modConfig.RevertToDefault.Invoke();
            foreach (var opt in modConfig.Options)
                opt.SyncToMod();
            modConfig.SaveToFile.Invoke();

            if (TitleMenu.subMenu == this)
                TitleMenu.subMenu = new SpecificModConfigMenu(mod);
            else if (Game1.activeClickableMenu == this)
                Game1.activeClickableMenu = new SpecificModConfigMenu(mod);
        }

        private void save()
        {
            Game1.playSound("money");
            foreach (var opt in modConfig.Options)
                opt.Save();
            modConfig.SaveToFile.Invoke();
            if (TitleMenu.subMenu == this)
                TitleMenu.subMenu = new ModConfigMenu();
            else if (Game1.activeClickableMenu == this)
                Game1.activeClickableMenu = null;
        }

        private void cancel()
        {
            Game1.playSound("bigDeSelect");
            if (TitleMenu.subMenu == this)
                TitleMenu.subMenu = new ModConfigMenu();
            else if (Game1.activeClickableMenu == this)
                Game1.activeClickableMenu = null;
        }

        private SimpleModOption<SButton> keybindingOpt;
        private Label keybindingLabel;
        private void doKeybindingFor( SimpleModOption<SButton> opt, Label label )
        {
            Game1.playSound("breathin");
            keybindingOpt = opt;
            keybindingLabel = label;
            ui.Obscured = true;
            Mod.instance.Helper.Events.Input.ButtonPressed += assignKeybinding;
        }

        private void assignKeybinding(object sender, ButtonPressedEventArgs e)
        {
            if ( keybindingOpt == null )
                return;
            if ( !e.Button.TryGetKeyboard(out Keys keys) && !e.Button.TryGetController(out _) )
                return;
            if ( e.Button.ToString() == "Escape" )
            {
                Game1.playSound("bigDeSelect");
            }
            else
            {
                Game1.playSound("coin");
                keybindingOpt.Value = e.Button;
                keybindingLabel.String = e.Button.ToString();
            }
            Mod.instance.Helper.Events.Input.ButtonPressed -= assignKeybinding;
            keybindingOpt = null;
            keybindingLabel = null;
            ui.Obscured = false;
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            ui = new RootElement();

            Vector2 newSize = new Vector2(Math.Min(1200, Game1.viewport.Width - 200), Game1.viewport.Height - 128 - 116);

            foreach (Element opt in table.Children)
            {
                opt.LocalPosition = new Vector2(newSize.X / (table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);
                if (opt is Slider slider)
                    slider.RequestWidth = (int)(newSize.X / (table.Size.X / slider.Width));
            }

            table.Size = newSize;
            table.LocalPosition = new Vector2((Game1.viewport.Width - table.Size.X) / 2, (Game1.viewport.Height - table.Size.Y) / 2);
            table.Scrollbar.Update();
            ui.AddChild(table);
            addDefaultLabels(mod);
        }
    }
}