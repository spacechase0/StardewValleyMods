using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;

namespace Stardew3D.Handlers.Game;

public interface IGameHandler
{
    public const string CategoryFlatscreen = "Flatscreen";
    public const string CategoryVR = "VR";
    public const string CategoryFirstPerson = "FirstPerson";
    public const string CategoryThirdPerson = "ThirdPerson";

    public string Id { get; }
    public string[] Tags { get; }

    public ICamera Camera { get; }
    public IReadOnlyList<IGameCursor> Cursors { get; }

    public RenderTarget2D CurrentTargetScreen { get; }
    public PBREnvironment GetCurrentEnvironmentFor(GameLocation location);
    public Matrix GetCurrentTransformFor(GameLocation location);

    public void SwitchOn( IGameHandler previousHandler );
    public void SwitchOff( IGameHandler nextHandler );

    delegate void DefaultInputHandling(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState);
    public void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, DefaultInputHandling defaultInputHandling);

    public void BeforeUpdate();
    public void AfterUpdate();

    public bool HandleRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Func<RenderSteps, SpriteBatch, GameTime, RenderTarget2D, bool> defaultRender);
    public bool AfterRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen);
}
