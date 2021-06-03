using GenericModConfigMenu.ModOption;
using GenericModConfigMenu.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace GenericModConfigMenu
{
    internal class SpecificModConfigMenu : IClickableMenu, IAssetEditor
    {
        private IManifest mod;
        private bool ingame;

        private ModConfig modConfig;
        private string currPage;
        private string prevPage;

        private RootElement ui = new RootElement();
        private Table table;
        private List<Label> optHovers = new List<Label>();
        public static IClickableMenu ActiveConfigMenu;

        private Dictionary<string, List<Image>> textures = new Dictionary<string, List<Image>>();
        private Queue<string> pendingTexChanges = new Queue<string>();

        public bool CanEdit<T>( IAssetInfo asset )
        {
            foreach ( var key in textures.Keys )
            {
                if ( asset.AssetNameEquals( key ) )
                    return true;
            }
            return false;
        }

        public void Edit<T>( IAssetData asset )
        {
            foreach ( var key in textures.Keys )
            {
                if ( asset.AssetNameEquals( key ) )
                {
                    pendingTexChanges.Enqueue( key );
                }
            }
        }

        public SpecificModConfigMenu(IManifest modManifest, bool inGame, string page = "", string prevPage = null)
        {
            mod = modManifest;
            ingame = inGame;

            modConfig = Mod.instance.configs[mod];
            currPage = page;

            Mod.instance.configs[ mod ].ActiveDisplayPage = modConfig.Options[ currPage ];

            table = new Table();
            table.RowHeight = 50;
            table.Size = new Vector2(Math.Min(1200, Game1.viewport.Width - 200), Game1.viewport.Height - 128 - 116);
            table.LocalPosition = new Vector2((Game1.viewport.Width - table.Size.X) / 2, (Game1.viewport.Height - table.Size.Y) / 2);
            foreach (var opt in modConfig.Options[ page ].Options)
            {
                opt.SyncToMod();
                if ( ingame && !opt.AvailableInGame )
                    continue;

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
                else if ( opt is SimpleModOption<KeybindList> k2 )
                {
                    if ( Constants.TargetPlatform == GamePlatform.Android )
                        continue; // TODO: Support virtual keyboard input.
                    var label2 = new Label() { String = k2.Value.IsBound ? k2.Value.Keybinds[0].ToString() : "(None)" };
                    label2.LocalPosition = new Vector2( table.Size.X / 2, 0 );
                    label2.Callback = ( Element e ) => doKeybinding2For( k2, e as Label );
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
                else if ( opt is PageLabelModOption pl )
                {
                    label.Bold = true;
                    label.Callback = ( Element e ) =>
                    {
                        if ( TitleMenu.subMenu == this )
                            TitleMenu.subMenu = new SpecificModConfigMenu( mod, ingame, pl.NewPage, currPage );
                        else if ( Game1.activeClickableMenu == this )
                            Game1.activeClickableMenu = new SpecificModConfigMenu( mod, ingame, pl.NewPage, currPage );
                    };
                    other = null;
                }
                else if ( opt is ParagraphModOption p )
                {
                    label.NonBoldScale = 0.75f;
                    label.NonBoldShadow = false;
                    other = null;

                    string[] text = p.Name.Split( ' ' );
                    label.String = text[ 0 ] + " ";
                    for ( int it = 1; it < text.Length; ++it )
                    {
                        string oldStr = label.String;
                        label.String += text[ it ];
                        if ( label.Measure().X >= table.Size.X )
                        {
                            label.String = oldStr + "\n" + text[ it ];
                        }
                        if ( it < text.Length - 1 )
                            label.String += " ";
                    }

                    string[] lines = label.String.Split( '\n' );
                    for ( int il = 0; il < lines.Length; il += 2 )
                    {
                        table.AddRow( new Element[] { new Label()
                        {
                            UserData = opt.Description,
                            NonBoldScale = 0.75f,
                            NonBoldShadow = false,
                            String = lines[ il + 0 ] + "\n" + (il + 1 >= lines.Length ? "" : lines[ il + 1 ])
                        } } );
                        continue;
                    }
                    continue;
                }
                else if ( opt is ImageModOption t )
                {
                    var tex = Game1.content.Load<Texture2D>( t.TexturePath );
                    var imgSize = new Vector2( tex.Width, tex.Height );
                    if ( t.TextureRect.HasValue )
                        imgSize = new Vector2( t.TextureRect.Value.Width, t.TextureRect.Value.Height );
                    imgSize *= t.Scale;
                    
                    
                    var localPos = new Vector2( table.Size.X / 2 - imgSize.X / 2, 0 );
                    var baseRectPos = new Vector2( t.TextureRect.HasValue ? t.TextureRect.Value.X : 0,
                                                   t.TextureRect.HasValue ? t.TextureRect.Value.Y : 0 );

                    var texs = new List<Image>();
                    if ( textures.ContainsKey( t.TexturePath ) )
                        texs = textures[ t.TexturePath ];
                    else
                        textures.Add( t.TexturePath, texs );
                    
                    for ( int ir = 0; ir < imgSize.Y / table.RowHeight; ++ir )
                    {
                        int section = Math.Min( (int)( imgSize.Y / t.Scale ), table.RowHeight );
                        int baseY = ( int )( baseRectPos.Y + section * ir );
                        if ( baseY + section > baseRectPos.Y + imgSize.Y / t.Scale )
                        {
                            section = ( int ) ( baseRectPos.Y + imgSize.Y / t.Scale ) - baseY;
                        }
                        var img = new Image()
                        {
                            Texture = tex,
                            TextureRect = new Rectangle( (int)baseRectPos.X, baseY, (int)imgSize.X / t.Scale, section ),
                            Scale = t.Scale,
                        };
                        img.LocalPosition = localPos;
                        texs.Add( img );
                        table.AddRow( new Element[] { img } );
                    }

                    continue;
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

            Mod.instance.Helper.Content.AssetEditors.Add( this );
        }

        private void addDefaultLabels(IManifest modManifest)
        {
            string page = modConfig.Options[ currPage ].DisplayName;
            var titleLabel = new Label() { String = modManifest.Name + ( page == "" ? "" : " > " + page ), Bold = true };
            titleLabel.LocalPosition = new Vector2((Game1.viewport.Width - titleLabel.Measure().X) / 2, 12 + 32);
            titleLabel.HoverTextColor = titleLabel.IdleTextColor;
            ui.AddChild(titleLabel);

            var cancelLabel = new Label() { String = "Cancel", Bold = true };
            cancelLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 - 400, Game1.viewport.Height - 50 - 36);
            cancelLabel.Callback = (Element e) => cancel();
            ui.AddChild(cancelLabel);

            var defaultLabel = new Label() { String = "Default", Bold = true };
            defaultLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 - 200, Game1.viewport.Height - 50 - 36);
            defaultLabel.Callback = (Element e) => revertToDefault();
            ui.AddChild(defaultLabel);

            var saveLabel = new Label() { String = "Save", Bold = true };
            saveLabel.LocalPosition = new Vector2(Game1.viewport.Width / 2 + 50, Game1.viewport.Height - 50 - 36);
            saveLabel.Callback = (Element e) => save();
            ui.AddChild(saveLabel);

            var saveCloseLabel = new Label() { String = "Save&Close", Bold = true };
            saveCloseLabel.LocalPosition = new Vector2( Game1.viewport.Width / 2 + 200, Game1.viewport.Height - 50 - 36 );
            saveCloseLabel.Callback = ( Element e ) => { save(); close(); };
            ui.AddChild( saveCloseLabel );
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

            while ( pendingTexChanges.Count > 0 )
            {
                var texPath = pendingTexChanges.Dequeue();
                var tex = Game1.content.Load<Texture2D>( texPath );

                foreach ( var images in textures[ texPath ] )
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

            if ( keybinding2Opt != null )
            {
                b.Draw( Game1.staminaRect, new Rectangle( 0, 0, Game1.viewport.Width, Game1.viewport.Height ), new Color( 0, 0, 0, 192 ) );

                int boxX = (Game1.viewport.Width - 650) / 2, boxY = (Game1.viewport.Height - 200) / 2;
                IClickableMenu.drawTextureBox( b, boxX, boxY, 650, 200, Color.White );

                string s = "Rebinding key: " + keybinding2Opt.Name;
                int sw = (int)Game1.dialogueFont.MeasureString(s).X;
                b.DrawString( Game1.dialogueFont, s, new Vector2( ( Game1.viewport.Width - sw ) / 2, boxY + 20 ), Game1.textColor );

                s = "Press a key combination to rebind";
                sw = ( int ) Game1.dialogueFont.MeasureString( s ).X;
                b.DrawString( Game1.dialogueFont, s, new Vector2( ( Game1.viewport.Width - sw ) / 2, boxY + 100 ), Game1.textColor );
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
            foreach (var page in modConfig.Options)
                foreach ( var opt in page.Value.Options )
                    opt.SyncToMod();
            modConfig.SaveToFile.Invoke();

            if (TitleMenu.subMenu == this)
                TitleMenu.subMenu = new SpecificModConfigMenu(mod, ingame, currPage, prevPage);
            else if (Game1.activeClickableMenu == this)
                Game1.activeClickableMenu = new SpecificModConfigMenu(mod, ingame, currPage, prevPage);
        }

        private void save()
        {
            Game1.playSound("money");
            foreach ( var page in modConfig.Options )
                foreach ( var opt in page.Value.Options )
                    opt.Save();
            modConfig.SaveToFile.Invoke();
        }

        private void close()
        {
            if ( TitleMenu.subMenu == this )
                TitleMenu.subMenu = new ModConfigMenu( ingame );
            else if ( !ingame && Game1.activeClickableMenu == this )
                Game1.activeClickableMenu = null;
            else
                Game1.activeClickableMenu = new ModConfigMenu( ingame );
            
            Mod.instance.Helper.Content.AssetEditors.Remove( this );
        }

        private void cancel()
        {
            Game1.playSound("bigDeSelect");
            close();
        }

        private SimpleModOption<SButton> keybindingOpt;
        private SimpleModOption<KeybindList> keybinding2Opt;
        private Label keybindingLabel;
        private void doKeybindingFor( SimpleModOption<SButton> opt, Label label )
        {
            Game1.playSound("breathin");
            keybindingOpt = opt;
            keybindingLabel = label;
            ui.Obscured = true;
            Mod.instance.Helper.Events.Input.ButtonPressed += assignKeybinding;
        }
        private void doKeybinding2For( SimpleModOption<KeybindList> opt, Label label )
        {
            Game1.playSound( "breathin" );
            keybinding2Opt = opt;
            keybindingLabel = label;
            ui.Obscured = true;
            Mod.instance.Helper.Events.Input.ButtonsChanged += assignKeybinding2;
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
        private void assignKeybinding2( object sender, ButtonsChangedEventArgs e )
        {
            if ( keybinding2Opt == null )
                return;

            List<SButton> all = new List<SButton>();
            foreach ( var button in e.Held )
            {
                if ( button.TryGetKeyboard( out Keys keys ) || button.TryGetController( out _ ) )
                {
                    all.Add( button );
                }
            }

            foreach ( var button in e.Released )
            {
                bool stop = false;
                if ( button.ToString() == "Escape" )
                {
                    stop = true;
                    Game1.playSound( "bigDeSelect" );
                }
                if ( !stop && ( button.TryGetKeyboard( out Keys keys ) || button.TryGetController( out _ ) ) )
                {
                    stop = true;
                    all.Add( button );

                    Game1.playSound( "coin" );
                    keybinding2Opt.Value.Keybinds[ 0 ] = new Keybind( all.ToArray() );
                    keybindingLabel.String = keybinding2Opt.Value.Keybinds[ 0 ].ToString();
                }

                if ( stop )
                {
                    Mod.instance.Helper.Events.Input.ButtonsChanged -= assignKeybinding2;
                    keybinding2Opt = null;
                    keybindingLabel = null;
                    ui.Obscured = false;
                }

                return;
            }

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