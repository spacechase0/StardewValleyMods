using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using StardewValley.Mods;

namespace Stardew3D;

public interface IGameHandler
{
    public const string Category3D = "3D";
    public const string CategoryVR = "VR";
    public const string CategoryFirstPerson = "FirstPerson";
    public const string CategoryThirdPerson = "ThirdPerson";

    public string Id { get; }
    public string[] Tags { get; }

    public Matrix ProjectionMatrix { get; }

    public void SwitchOn();
    public void SwitchOff();

    public void SetMenuHandler<MenuType>(Func<IClickableMenu, IMenuHandler> createHandlerFunc, bool includeMenuSubclasses = true);
    public void AddMenuHandlerAddon<MenuType>(Func<IClickableMenu, IMenuHandler> createHandlerFunc, bool includeMenuSubclasses = true);
    public IMenuHandler[] CreateApplicableMenuHandlers(IClickableMenu menu);


    delegate void DefaultInputHandling(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState);

    public void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, DefaultInputHandling defaultInputHandling);
    public void BeforeUpdate();
    public void AfterUpdate();
    public bool HandleRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Func<RenderSteps, SpriteBatch, GameTime, RenderTarget2D, bool> defaultRender);
    public bool AfterRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen);
}
