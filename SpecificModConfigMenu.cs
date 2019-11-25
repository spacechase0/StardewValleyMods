using GenericModConfigMenu.ModOption;
using GenericModConfigMenu.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public SpecificModConfigMenu(IManifest modManifest)
        {
            mod = modManifest;

            modConfig = Mod.instance.configs[mod];

            table = new Table();
            table.LocalPosition = new Vector2(200 / 2, 82);
            table.Size = new Vector2(Game1.viewport.Width - 200, Game1.viewport.Height - 64 - 100);
            table.RowHeight = 50;
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
                    custom.LocalPosition = new Vector2( table.Size.X / 5 * 3, 0 );
                    other = custom;
                }
                else if ( opt is SimpleModOption<bool> b )
                {
                    var checkbox = new Checkbox();
                    checkbox.LocalPosition = new Vector2( table.Size.X / 3 * 2, 0 );
                    checkbox.Checked = b.Value;
                    checkbox.Callback = (Element e) => b.Value = (e as Checkbox).Checked;
                    other = checkbox;
                }
                else if ( opt is SimpleModOption<SButton> k )
                {
                    var label2 = new Label() { String = k.Value.ToString() };
                    label2.LocalPosition = new Vector2(table.Size.X / 3 * 2, 0);
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
                    slider.Width = (int)table.Size.X / 3;
                    slider.Value = ci.Value;
                    slider.Minimum = ci.Minimum;
                    slider.Maximum = ci.Maximum;
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
                    slider.Width = (int)table.Size.X / 3;
                    slider.Value = cf.Value;
                    slider.Minimum = cf.Minimum;
                    slider.Maximum = cf.Maximum;
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
                    dropdown.LocalPosition = new Vector2(table.Size.X / 7 * 4, 0);
                    dropdown.Value = cs.Value;
                    dropdown.Callback = (Element e) => cs.Value = (e as Dropdown).Value;
                    other = dropdown;
                }
                // The following need to come after the Clamped/ChoiceModOption's since those subclass these
                else if (opt is SimpleModOption<int> i)
                {
                    var intbox = new Intbox();
                    intbox.LocalPosition = new Vector2(table.Size.X / 5 * 3, 0);
                    intbox.Value = i.Value;
                    intbox.Callback = (Element e) => i.Value = (e as Intbox).Value;
                    other = intbox;
                }
                else if (opt is SimpleModOption<float> f)
                {
                    var floatbox = new Floatbox();
                    floatbox.LocalPosition = new Vector2(table.Size.X / 5 * 3, 0);
                    floatbox.Value = f.Value;
                    floatbox.Callback = (Element e) => f.Value = (e as Floatbox).Value;
                    other = floatbox;
                }
                else if (opt is SimpleModOption<string> s)
                {
                    var textbox = new Textbox();
                    textbox.LocalPosition = new Vector2(table.Size.X / 5 * 3, 0);
                    textbox.String = s.Value;
                    textbox.Callback = (Element e) => s.Value = (e as Textbox).String;
                    other = textbox;
                }

                if (other2 == null)
                    table.AddRow(new Element[] { label, other });
                else
                    table.AddRow(new Element[] { label, other, other2 });
            }
            ui.AddChild(table);

            var titleLabel = new Label() { String = modManifest.Name };
            titleLabel.LocalPosition = new Vector2((Game1.viewport.Width - titleLabel.Font.MeasureString(titleLabel.String).X) / 2, 12);
            titleLabel.HoverTextColor = titleLabel.IdleTextColor;
            ui.AddChild(titleLabel);

            var cancelLabel = new Label() { String = "Cancel" };
            cancelLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 - 300, Game1.viewport.Height - 50);
            cancelLabel.Callback = (Element e) => TitleMenu.subMenu = new ModConfigMenu();
            ui.AddChild(cancelLabel);

            var defaultLabel = new Label() { String = "Default" };
            defaultLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 - 50, Game1.viewport.Height - 50);
            defaultLabel.Callback = (Element e) => revertToDefault();
            ui.AddChild(defaultLabel);

            var saveLabel = new Label() { String = "Save" };
            saveLabel.LocalPosition = new Vector2(Game1.viewport.Width/ 2 + 200, Game1.viewport.Height - 50);
            saveLabel.Callback = (Element e) => save();
            ui.AddChild(saveLabel);

            // We need to update widgets at least once so ComplexModOptionWidget's get initialized
            table.ForceUpdateEvenHidden();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            table.Scrollbar.Scroll(((float)table.RowHeight / (table.RowHeight * table.RowCount)) * direction / -120);
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
            IClickableMenu.drawTextureBox(b, (Game1.viewport.Width - 800) / 2 - 12, 0, 800 + 24, 50 + 12, Color.White);
            IClickableMenu.drawTextureBox(b, (Game1.viewport.Width - 800) / 2 - 12, Game1.viewport.Height - 50 - 12, 800 + 24, 50 + 12, Color.White);

            ui.Draw(b);

            if ( keybindingOpt != null )
            {
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

            foreach ( var label in optHovers )
            {
                if (label.Hover)
                    drawToolTip(b, (string) label.UserData, label.String, null);
            }
        }

        private void revertToDefault()
        {
            modConfig.RevertToDefault.Invoke();
            foreach (var opt in modConfig.Options)
                opt.SyncToMod();
            modConfig.SaveToFile.Invoke();
            TitleMenu.subMenu = new SpecificModConfigMenu(mod);
        }

        private void save()
        {
            foreach (var opt in modConfig.Options)
                opt.Save();
            modConfig.SaveToFile.Invoke();
            TitleMenu.subMenu = new ModConfigMenu();
        }

        private SimpleModOption<SButton> keybindingOpt;
        private Label keybindingLabel;
        private void doKeybindingFor( SimpleModOption<SButton> opt, Label label )
        {
            keybindingOpt = opt;
            keybindingLabel = label;
            Mod.instance.Helper.Events.Input.ButtonPressed += assignKeybinding;
        }

        private void assignKeybinding(object sender, ButtonPressedEventArgs e)
        {
            if ( keybindingOpt == null )
                return;
            keybindingOpt.Value = e.Button;
            keybindingLabel.String = e.Button.ToString();
            Mod.instance.Helper.Events.Input.ButtonPressed -= assignKeybinding;
            keybindingOpt = null;
            keybindingLabel = null;
        }
    }
}