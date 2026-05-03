using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Mods;

namespace Stardew3D.GameModes;

public interface IGameMode
{
    public const string CategoryFlatscreen = "Category/Flatscreen";
    public const string CategoryVR = "Category/VR";
    public const string CategoryFirstPerson = "Category/FirstPerson";
    public const string CategoryThirdPerson = "Category/ThirdPerson";
    public const string CategoryEditor = "Category/Editor"; // Causes the base game update functions to stop

    public const string FeatureMotionControls = "Feature/MotionControls";
    public const string FeaturePointAndClick = "Feature/PointAndClick";

    public string Id { get; }
    public string[] Tags { get; }

    public ICamera Camera { get; }
    public IReadOnlyList<IGameCursor> Cursors { get; }

    public RenderTarget2D CurrentTargetScreen { get; }
    public PBREnvironment GetCurrentEnvironmentFor(GameLocation location);
    public Matrix GetCurrentTransformFor(GameLocation location);

    public void SwitchOn( IGameMode previousMode );
    public void SwitchOff( IGameMode nextMode );

    delegate void DefaultInputHandling(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState);
    public void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, DefaultInputHandling defaultInputHandling);

    public void BeforeUpdate();
    public void AfterUpdate();

    public bool HandleRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Func<RenderSteps, SpriteBatch, GameTime, RenderTarget2D, bool> defaultRender);
    public bool AfterRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen);
}
