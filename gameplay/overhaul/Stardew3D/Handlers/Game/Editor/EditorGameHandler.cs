using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Font;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using Stardew3D.Handlers.Game.Editor.Editables;
using Stardew3D.Handlers.Game.Editor.Editables.Map;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Mods;

namespace Stardew3D.Handlers.Game.Editor;

public class EditorGameHandler : CommonGameHandler
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/Editor";
    public override string[] Tags => [ IGameHandler.CategoryEditor ];

    public override ICamera Camera { get; } = new Camera();
    public override Matrix ProjectionMatrix { get; protected set; }
    public override IReadOnlyList<IGameCursor> Cursors => [];

    public SpriteBatch SpriteBatch { get; private set; }
    public UiSystem Ui { get; private set; }

    public List<IEditableType> EditableTypes =
    [
        new MapEditableType(),
    ];

    private float oldUiScale;

    public override void SwitchOn(IGameHandler previousHandler)
    {
        base.SwitchOn(previousHandler);
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Mod.Config.FieldOfViewDegrees), Game1.graphics.GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 10000);

        oldUiScale = Game1.options.baseUIScale;
        Game1.options.baseUIScale = 1;

        SpriteBatch = new(Game1.graphics.GraphicsDevice);
        Ui = new(GameRunner.instance, new UntexturedStyle(SpriteBatch)
        {
            Font = new GenericSpriteFont( Game1.smallFont ),
            AdditionalFonts = new()
            {
                ["Default 0.5"] = new ScaledGenericSpriteFont( 0.5f, Game1.smallFont ),
                ["Default"] = new GenericSpriteFont( Game1.smallFont ),
                ["Default 1"] = new GenericSpriteFont( Game1.smallFont ),
                ["Default 2"] = new ScaledGenericSpriteFont( 2, Game1.smallFont ),
                ["Dialogue 0.5"] = new ScaledGenericSpriteFont( 0.5f, Game1.dialogueFont ),
                ["Dialogue"] = new GenericSpriteFont( Game1.dialogueFont ),
                ["Dialogue 1"] = new GenericSpriteFont( Game1.dialogueFont ),
                ["Dialogue 2"] = new ScaledGenericSpriteFont( 2, Game1.dialogueFont ),
            },
            TooltipTextWidth = 640,
            TooltipOffset = new( 32, 32 ),
        }, automaticViewport: false);

        var root = new Group(Anchor.Center, new Vector2(1, 1));
        Ui.Add("Root", root);

        Panel editableTypesPanel = new Panel(Anchor.CenterLeft, new Vector2(0.2f, 1));
        Panel editableTypesTabs = new Panel(Anchor.TopCenter, new Vector2(1, 48), setHeightBasedOnChildren: true, scrollOverflow: true);
        editableTypesPanel.AddChild(editableTypesTabs);
        Panel editableTypesListing = new Panel(Anchor.AutoCenter, new Vector2(1, 1), scrollOverflow: true);
        editableTypesPanel.AddChild(editableTypesListing);
        {
            foreach (var editableType_ in EditableTypes)
            {
                var editableType = editableType_;
                editableTypesTabs.AddChild(new Button(Anchor.AutoInline, new Vector2(200, 32), editableType.TypeListName)
                {
                    SetWidthBasedOnChildren = true,
                    OnPressed = e =>
                    {
                        foreach (var child in editableTypesTabs.Children)
                        {
                            if (child is not Button other)
                                continue;
                            other.IsDisabled = false;
                        }

                        List<KeyValuePair<string, IEditable>> entries = new();
                        void AddListing(EditableTree tree, string baseStr )
                        {
                            if (!string.IsNullOrEmpty(baseStr))
                                entries.Add(new(baseStr, null));
                            entries.AddRange(tree.Entries.Select(kvp => new KeyValuePair<string, IEditable>($"{baseStr}{kvp.Key}", kvp.Value)));
                            foreach (var entry in tree.SubTrees)
                            {
                                AddListing(entry.Value, $"{baseStr}{entry.Key}/");
                            }
                        }
                        AddListing(editableType.GetListing(), "");
                        entries.Sort((a, b) => a.Key.CompareTo(b.Key, StringComparison.OrdinalIgnoreCase));

                        editableTypesListing.RemoveChildren(_ => true);
                        foreach (var entry_ in entries)
                        {
                            var entry = entry_;

                            string str = entry.Key;
                            int levels = str.Count(c => c == '/');
                            if (str.EndsWith('/'))
                                levels -= 1;
                            str = levels > 0 ? str.Substring(str.LastIndexOf('/', str.Length - 2) + 1) : str;

                            // TODO: Make these buttons with no background for highlight and click and such
                            editableTypesListing.AddChild(new Paragraph(Anchor.AutoLeft, 1, $"<f Default 0.5>{str}")
                            {
                                PositionOffset = new Vector2(levels * 16, 0),
                                OnPressed = _ => { }
                            });
                        }

                        var b = e as Button;
                        b.IsDisabled = true;
                    },
                });
            }

            var firstTab = (editableTypesTabs.Children.Where(e => e is Button).FirstOrDefault() as Button);
            firstTab?.OnPressed(firstTab);
        }
        Ui.Add("Editable Types", editableTypesPanel);

        root.AddChild(new Paragraph(Anchor.AutoCenter, 1, "<f Default 0.5><c Green>meow</c> <c Red>kitty</c>", autoAdjustWidth: true));
        root.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.25f, 50), "MEOW", "<a wobbly>kitties</a> go <i><b>meow</b></i>")
        {
            OnPressed = _ => root.AddChild( new Paragraph( Anchor.AutoCenter, 1, "<s><f Dialogue>MORE</f></s> <o>kitties</o>", autoAdjustWidth: true ) ),
        });

    }

    public override void SwitchOff(IGameHandler nextHandler)
    {
        base.SwitchOff(nextHandler);
        Game1.options.baseUIScale = oldUiScale;
        Ui?.Dispose();
        Ui = null;
    }

    public override void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, IGameHandler.DefaultInputHandling defaultInputHandling)
    {
        keyboardState = default;
        mouseState = default;
        gamePadState = default;
    }

    public override void AfterUpdate()
    {
        base.AfterUpdate();

        Ui.Viewport = new(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height);
        Ui.Update(Game1.currentGameTime);
    }

    public override bool AfterRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen)
    {
        if (step < RenderSteps.MenuBackground)
            return base.AfterRender(step, sb, time, targetScreen);
        else if (step > RenderSteps.Menu)
            return false;

        Ui.Draw(Game1.currentGameTime, SpriteBatch);

        return false;
    }

    protected override void UpdateCamera()
    {
        RenderHelper.GenericEffect.View = Camera.ViewMatrix;
    }
}
