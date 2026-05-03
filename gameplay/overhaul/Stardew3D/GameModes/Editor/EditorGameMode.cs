using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Font;
using MLEM.Input;
using MLEM.Maths;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using SpaceShared;
using Stardew3D.GameModes;
using Stardew3D.GameModes.Editor.Editables;
using Stardew3D.GameModes.Editor.Editables.Map;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.GameModes.Editor;

public class EditorGameMode : BaseGameMode
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/Editor";
    public override string[] Tags => [ IGameMode.CategoryEditor ];

    private Camera camera = new();
    public override ICamera Camera => camera;
    public override Matrix ProjectionMatrix { get; protected set; }
    public override IReadOnlyList<IGameCursor> Cursors => [];

    public SpriteBatch SpriteBatch { get; private set; }
    private InputHandler _input { get; set; }
    public UiSystem Ui { get; private set; }
    private Panel EditableDataPanel { get; set; }

    public PBREnvironment EditorEnvironment { get; set; }
    public RenderBatcher EditorWorldBatch { get; private set; }

    public List<IEditableType> EditableTypes =
    [
        new MapEditableType(),
    ];

    private IEditable ActiveEditable
    {
        get => field;
        set
        {
            field?.BeforeHidePanelContents();
            EditableDataPanel.RemoveChildren();
            Mod.State.ClearHandlerState();
            EditorWorldBatch.ClearData();

            field = value;

            if (field == null)
                return;

            var newChildren = field.PopulatePanelContents();
            foreach ( var child in newChildren)
                EditableDataPanel.AddChild( child );
        }
    }

    private float oldUiScale;
    private bool oldHardwareCursor;

    public override void SwitchOn(IGameMode previousMode)
    {
        base.SwitchOn(previousMode);
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Mod.Config.FieldOfViewDegrees), Game1.graphics.GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 10000);

        oldUiScale = Game1.options.baseUIScale;
        oldHardwareCursor = Game1.options.hardwareCursor;
        Game1.options.baseUIScale = 1;
        Game1.options.hardwareCursor = true;

        SpriteBatch = new(Game1.graphics.GraphicsDevice);
        _input = new InputHandler(GameRunner.instance);
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
        }, _input, automaticViewport: false);

        var root = new Group(Anchor.Center, new Vector2(1, 1));
        Ui.Add("Root", root);

        Panel editableTypesPanel = new Panel(Anchor.CenterLeft, new Vector2(0.2f, 1));
        Panel editableTypesTabs = new Panel(Anchor.TopCenter, new Vector2(1, 48), setHeightBasedOnChildren: true, scrollOverflow: true);
        editableTypesPanel.AddChild(editableTypesTabs);
        Panel editableTypesListing = new Panel(Anchor.AutoCenter, new Vector2(1, 1), scrollOverflow: true)
        {
            PreventParentSpill = true,
        };
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
                                AddListing(entry.Value, $"{baseStr}{entry.Key}\\");
                            }
                        }
                        AddListing(editableType.GetListing(), "");
                        entries.Sort((a, b) => a.Key.CompareTo(b.Key, StringComparison.OrdinalIgnoreCase));

                        editableTypesListing.RemoveChildren(_ => true);
                        foreach (var entry_ in entries)
                        {
                            var entry = entry_;

                            string str = entry.Key;
                            int levels = str.Count(c => c == '\\');
                            if (str.EndsWith('\\'))
                                levels -= 1;
                            str = levels > 0 ? str.Substring(str.LastIndexOf('\\', str.Length - 2) + 1) : str;

                            Button button = new(Anchor.AutoLeft, new Vector2(1, 24), $"<f Default 0.5>{str}")
                            {
                                AutoSizeAddedAbsolute = new Vector2(-levels * 16, 0),
                                PositionOffset = new Vector2(levels * 16, 0),
                                OnPressed = _ => ActiveEditable = entry.Value,
                            };
                            editableTypesListing.AddChild(button);
                        }

                        var b = e as Button;
                        b.IsDisabled = true;
                    },
                });
            }

            var firstTab = editableTypesTabs.Children.Where(e => e is Button).FirstOrDefault() as Button;
            firstTab?.OnPressed(firstTab);
        }
        Ui.Add("Editable Types", editableTypesPanel);

        EditableDataPanel = new Panel(Anchor.CenterRight, new Vector2(0.2f, 1));
        Ui.Add("Editable Data", EditableDataPanel);

        EditorEnvironment = PBREnvironment.CreateDefault();
        EditorWorldBatch = new RenderBatcher(Game1.graphics.GraphicsDevice);
    }

    public override void SwitchOff(IGameMode nextMode)
    {
        base.SwitchOff(nextMode);

        Game1.options.baseUIScale = oldUiScale;
        Game1.options.hardwareCursor = oldHardwareCursor;

        Ui?.Dispose();
        Ui = null;

        EditorEnvironment = null;
        EditorWorldBatch?.Dispose();
        EditorWorldBatch = null;
    }

    public override void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, IGameMode.DefaultInputHandling defaultInputHandling)
    {
        keyboardState = default;
        mouseState = default;
        gamePadState = default;
    }

    public void SetCamera(Vector3? position = null, Vector3? dir = null)
    {
        if (position.HasValue)
            camera.Position = position.Value;

        if (dir.HasValue)
            camera.Forward = dir.Value;
    }

    public void DoAfterConfirm(Action action)
    {
        // TODO
        action();
    }

    private bool Rotating = false;
    private Point rotateOrigin;
    public override void AfterUpdate()
    {
        base.AfterUpdate();

        _input.Update();
        if (Rotating)
        {
            Point mousePos = Ui.Controls.Input.MousePosition;
            Game1.setMousePositionRaw(rotateOrigin.X, rotateOrigin.Y);
            Point mouseDiff = rotateOrigin - mousePos;

            Vector3 facing = Camera.Forward;
            facing = Vector3.Transform(facing, Matrix.CreateRotationY(mouseDiff.X / 250f));
            Vector3 moreVert = Vector3.Transform(facing, Matrix.CreateFromAxisAngle(Vector3.Cross(facing, Camera.Up), mouseDiff.Y / 250f));
            moreVert.Y = Utility.Clamp(moreVert.Y, -0.9f, 0.9f);
            facing = moreVert.Normalized();

            camera.Forward = facing;

            Vector3 movement = Vector3.Zero;
            if (_input.IsDown(Keys.W)) movement += Vector3.UnitZ;
            if (_input.IsDown(Keys.S)) movement -= Vector3.UnitZ;
            if (_input.IsDown(Keys.D)) movement += Vector3.UnitX;
            if (_input.IsDown(Keys.A)) movement -= Vector3.UnitX;
            if (_input.IsDown(Keys.Space)) movement += Vector3.UnitY;
            if (_input.IsDown(Keys.LeftShift)) movement -= Vector3.UnitY;
            _input.TryConsumePressed(Keys.W);
            _input.TryConsumePressed(Keys.S);
            _input.TryConsumePressed(Keys.D);
            _input.TryConsumePressed(Keys.A);
            _input.TryConsumePressed(Keys.Space);
            _input.TryConsumePressed(Keys.LeftShift);

            float movementSpeed = 0.2f;
            if (movement.X != 0)
                camera.Position += Vector3.Cross(camera.Forward, camera.Up) * movement.X * movementSpeed;
            if (movement.Y != 0)
                camera.Position += Vector3.Up * movement.Y * movementSpeed;
            if (movement.Z != 0)
                camera.Position += camera.Forward * movement.Z * movementSpeed;
        }

        Ui.Viewport = new(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height);
        Ui.Update(Game1.currentGameTime);
        ActiveEditable?.Update();

        if (Ui.Controls.Input.TryConsumePressed(MouseButton.Middle))
        {
            Rotating = true;
            rotateOrigin = Ui.Controls.Input.MousePosition;
        }
        else if (Rotating && !Ui.Controls.Input.IsDown(MouseButton.Middle))
            Rotating = false;

        if (Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Control) && Ui.Controls.Input.TryConsumePressed(Keys.S) && ActiveEditable != null)
        {
            Log.Info($"Saving {ActiveEditable.Id}...");
            var formats = ActiveEditable.Save();

            string path = Path.Combine(Mod.Instance.Helper.DirectoryPath, "EditorOutput");
#if DEBUG
            path = Mod.GetDevAssetsFolder();
#endif
            path = Path.Combine(path, ActiveEditable.Id);

            Log.Info($"Saving to {path}...");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            foreach (var format in formats)
                File.WriteAllText($"{path}.{format.Key}", format.Value);
        }
    }

    public override void RenderWorld()
    {
        ActiveEditable?.RenderWorld(EditorWorldBatch);
        EditorWorldBatch.PrepareSprites(Matrix.Identity, Camera);
        EditorWorldBatch.DrawBatched(EditorEnvironment, Matrix.Identity, Camera.ViewMatrix, ProjectionMatrix);
        EditorWorldBatch.HideInstancesAfterFrame();
        ActiveEditable?.AfterRenderWorld();
    }

    public override bool HandleRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Func<RenderSteps, SpriteBatch, GameTime, RenderTarget2D, bool> defaultRender)
    {
        if (step < RenderSteps.MenuBackground)
            return base.HandleRender(step, sb, time, targetScreen, defaultRender);

        // Don't want the vanilla UI to show
        return false;
    }

    public override bool AfterRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen)
    {
        if (step < RenderSteps.MenuBackground)
            return base.AfterRender(step, sb, time, targetScreen);
        else if (step > RenderSteps.Menu)
            return false;

        ActiveEditable?.RenderMenu(sb);
        Ui.Draw(Game1.currentGameTime, SpriteBatch);

        return false;
    }

    protected override void UpdateCamera()
    {
        RenderHelper.GenericEffect.View = Camera.ViewMatrix;
    }
}
