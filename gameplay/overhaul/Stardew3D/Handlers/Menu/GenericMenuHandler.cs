using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.DataModels;
using Stardew3D.GameModes;
using Stardew3D.GameModes.FirstPersonVR;
using Stardew3D.GameModes.VR;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace Stardew3D.Handlers.Menu;
internal class GenericMenuHandler : RendererFor<MenuModelData, IClickableMenu>, IUpdateHandler
{
    public VRGameMode GameMode;

    public Matrix BaseOrientation;
    public Vector3 DisplayPosition;
    public Vector2 DisplaySize;

    public Dictionary<ClickableComponent, BoundingBox> Clickables { get; } = new();

    public Dictionary<IGameCursor, Matrix?> cursorTargetMapping = new();

    public virtual bool ShouldDraw2DMenu => true;

    public GenericMenuHandler(VRGameMode mode, IClickableMenu menu)
        : base(menu)
    {
        GameMode = mode;

        var basePosition = mode.Camera.Position;
        BaseOrientation = mode.Camera.ViewMatrix.NoTranslation().Inverted();

        // TODO: Configurable distance for these menus
        DisplayPosition = basePosition + BaseOrientation.Forward * 5;
        DisplaySize = new Vector2(Game1.game1.uiScreen.Width / (float)Game1.game1.uiScreen.Height, 1) * 3;
    }

    public virtual void Update(IUpdateHandler.UpdateContext ctx)
    {
        ctx.ForceUpdateIfNotAlreadyRun(ctx);

        foreach (var cursor in GameMode.Cursors.Reverse())
            HandleCursor(ctx, cursor);
    }

    protected virtual void HandleCursor(IUpdateHandler.UpdateContext ctx, IGameCursor cursor)
    {
        if (!ShouldDraw2DMenu)
            return;

        BoundingBox display = new(new(-DisplaySize.X / 2, -DisplaySize.Y / 2, 0), new(DisplaySize.X / 2, DisplaySize.Y / 2, 0.05f));

        Matrix cursorTransform = Matrix.CreateTranslation(-DisplayPosition) * BaseOrientation.Inverted();
        Vector3 cursorPos = Vector3.Transform(cursor.PointerPosition, cursorTransform);
        Vector3 cursorDir = Vector3.TransformNormal(cursor.PointerFacing, cursorTransform);
        Ray cursorRay = new(cursorPos, cursorDir);
        var spot = cursorRay.Intersects(display);
        if (spot.HasValue)
        {
            var intersectionPoint = cursorRay.Position + cursorRay.Direction * spot.Value;
            //cursorTargetMapping[cursor] = Matrix.CreateTranslation(intersectionPoint) * Matrix.CreateLookAt(intersectionPoint, cursor.PointerPosition, cursor.PointerUp);
            cursorTargetMapping[cursor] = Matrix.CreateTranslation(intersectionPoint) * cursorTransform.Inverted();

            Vector2 clickableLocal = new((intersectionPoint.X - display.Min.X) / (display.Max.X - display.Min.X) * Game1.game1.uiScreen.Bounds.Width,
                                         Game1.game1.uiScreen.Bounds.Height - (intersectionPoint.Y - display.Min.Y) / (display.Max.Y - display.Min.Y) * Game1.game1.uiScreen.Bounds.Height);
            //Object.performHoverAction((int)clickableLocal.X, (int)clickableLocal.Y);
            this.GameMode.EmulatedCursor = clickableLocal.ToPoint();
        }
        else
        {
            cursorTargetMapping[cursor] = null;
        }
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new RenderData(ctx, this);
    }

    protected class RenderData : RenderData<GenericMenuHandler>
    {
        public RenderData(IRenderHandler.RenderContext ctx, GenericMenuHandler parent)
            : base(ctx, parent)
        {
            if (ctx.TargetScreen == Game1.game1.uiScreen)
                return;
        }

        public override void Update(IRenderHandler.RenderContext ctx)
        {
            base.Update(ctx);

            if (Parent.ShouldDraw2DMenu)
            {
                Batch.AddSprite(Vector2.Zero, Parent.DisplayPosition, Parent.BaseOrientation, 0, new SpriteBatchItem()
                {
                    Texture = Game1.game1.uiScreen,
                    vertexTL = new(new(-Parent.DisplaySize.X / 2, -Parent.DisplaySize.Y / 2, 0), Color.White, new(1, 0)),
                    vertexTR = new(new(Parent.DisplaySize.X / 2, -Parent.DisplaySize.Y / 2, 0), Color.White, new(0, 0)),
                    vertexBL = new(new(-Parent.DisplaySize.X / 2, Parent.DisplaySize.Y / 2, 0), Color.White, new(1, 1)),
                    vertexBR = new(new(Parent.DisplaySize.X / 2, Parent.DisplaySize.Y / 2, 0), Color.White, new(0, 1)),
                }, Game1.tileSize);
            }

            // This isn't needed when we only have the normal menu showing
            if (false)
            {
                var cursorSize = new Vector2(16f / Game1.game1.uiScreen.Width, 16f / Game1.game1.uiScreen.Height) * Game1.pixelZoom * Parent.DisplaySize;
                int i = 0;
                foreach (var cursor in Parent.GameMode.Cursors.Reverse())
                {
                    ++i;
                    if (!Parent.cursorTargetMapping.TryGetValue(cursor, out var cursorTransform) || !cursorTransform.HasValue)
                        continue;

                    Rectangle texRect = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, 16, 16);
                    Vector2 texRectTL = new Vector2(texRect.X, texRect.Y) / new Vector2(Game1.mouseCursors.ActualWidth, Game1.mouseCursors.ActualHeight);
                    Vector2 texRectTR = new Vector2(texRect.Right, texRect.Y) / new Vector2(Game1.mouseCursors.ActualWidth, Game1.mouseCursors.ActualHeight);
                    Vector2 texRectBL = new Vector2(texRect.X, texRect.Bottom) / new Vector2(Game1.mouseCursors.ActualWidth, Game1.mouseCursors.ActualHeight);
                    Vector2 texRectBR = new Vector2(texRect.Right, texRect.Bottom) / new Vector2(Game1.mouseCursors.ActualWidth, Game1.mouseCursors.ActualHeight);

                    if (!(cursor as FirstPersonVRCursor)?.FlipMenuSprite ?? true)
                    {
                        Util.Swap(ref texRectTL, ref texRectTR);
                        Util.Swap(ref texRectBL, ref texRectBR);
                    }

                    Batch.AddSprite(Vector2.Zero, Vector3.Zero, cursorTransform.Value, 0, new SpriteBatchItem()
                    {
                        Texture = Game1.mouseCursors,
                        vertexTL = new(new(-cursorSize.X / 2, -cursorSize.Y / 2, 0), Color.White, texRectTL),
                        vertexTR = new(new(cursorSize.X / 2, -cursorSize.Y / 2, 0), Color.White, texRectTR),
                        vertexBL = new(new(-cursorSize.X / 2, cursorSize.Y / 2, 0), Color.White, texRectBL),
                        vertexBR = new(new(cursorSize.X / 2, cursorSize.Y / 2, 0), Color.White, texRectBR),
                    }, Game1.tileSize);
                }
            }
        }
    }
}
