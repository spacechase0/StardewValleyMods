using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GenericModConfigMenu.Framework.ModOption;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ControllerRadialKeybindings
{
    internal class ScreenState
    {
        public int? showingMenu;
        public Keybind pressing;
        public int pressLengthLeft = 0;
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private const float DEADZONE = 0.2f;

        internal Configuration Config { get; set; }

        internal ScreenState State => _screenState.Value;
        private PerScreen<ScreenState> _screenState = new(() => new ScreenState());

        private BasicEffect effect;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            // Cannot use Log since we're using GMCM's instance, since [InternalsVisibleTo]
            //Log.Monitor = Monitor;
            Config = Helper.ReadConfig<Configuration>();

            effect = new BasicEffect(Game1.graphics.GraphicsDevice);
            effect.World = Matrix.Identity;
            effect.View = Matrix.CreateLookAt(
                new Vector3(0, 0, -1),
                new Vector3(0, 0, 0),
                new Vector3(0, -1, 0));
            effect.Projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateOrthographicOffCenter(0, Game1.uiViewport.Width, Game1.uiViewport.Height, 0, 0, 1);
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Helper.Events.GameLoop.UpdateTicking += this.GameLoop_UpdateTicking;
            Helper.Events.Display.Rendered += this.Display_Rendered;
        }

        [EventPriority(EventPriority.Low - 1)] // We want ALL the keybindings
        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            gmcm.Register(ModManifest, () => Config = new(), () => Helper.WriteConfig(Config));

            gmcm.AddPageLink(ModManifest, "RadialA", () => Helper.Translation.Get( "main.configure.first" ) );
            gmcm.AddPageLink(ModManifest, "RadialB", () => Helper.Translation.Get( "main.configure.second" ));

            BuildKeybindingCache();
            List<Tuple<string, IManifest, string>> keybindOpts = new();
            foreach (var mod in keybinds)
            {
                foreach (var opt in mod.Value)
                {
                    keybindOpts.Add(new(mod.Key.Name + ": " + opt.Value.Name(), mod.Key, opt.Value.Name()));
                }
            }

            void ConfigureRadial( Configuration.RadialConfig r )
            {
                gmcm.AddKeybindList(ModManifest, () => r.Trigger, (kl) => r.Trigger = kl, () => Helper.Translation.Get( "configure.radial.trigger" ), () => Helper.Translation.Get( "configure.radial.trigger.tooltip" ));

                gmcm.AddParagraph(ModManifest, () => Helper.Translation.Get("configure.radial.tip"));
                for (int i = 0; i < r.Keybindings.Length; ++i)
                {
                    int icopy = i; // For lambda
                    gmcm.AddTextOption(ModManifest,
                        () =>
                        {
                            return keybindOpts.FirstOrDefault(
                                (ko) => ko.Item2.UniqueID == r.Keybindings[icopy]?.ModId &&
                                ko.Item3 == r.Keybindings[icopy]?.KeybindOption
                            )?.Item1 ?? " ";
                        },
                        (opt) =>
                        {
                            if (opt == " ")
                                r.Keybindings[icopy] = new() { PressDuration = r.Keybindings[icopy]?.PressDuration ?? 1 };
                            else
                            {
                                var entry = keybindOpts.First((ko) => ko.Item1 == opt);
                                r.Keybindings[icopy] = new()
                                {
                                    ModId = entry.Item2.UniqueID,
                                    KeybindOption = entry.Item3,
                                    PressDuration = r.Keybindings[icopy]?.PressDuration ?? 1,
                                };
                            }
                        },
                        () => Helper.Translation.Get("configure.radial.entry", new { num = icopy }),
                        () => Helper.Translation.Get("configure.radial.entry.tooltip"),
                        keybindOpts.Select(t => t.Item1).Append( " " ).ToArray()
                    );
                    gmcm.AddNumberOption(ModManifest,
                        () => r.Keybindings[icopy]?.PressDuration ?? 1,
                        (dur) => r.Keybindings[icopy] = new()
                        {
                            ModId = r.Keybindings[icopy]?.ModId ?? null,
                            KeybindOption = r.Keybindings[icopy]?.KeybindOption ?? null,
                            PressDuration = dur,
                        },
                        () => Helper.Translation.Get("configure.radial.entry.duration", new { num = icopy }),
                        () => Helper.Translation.Get("configure.radial.entry.duration.tooltip"),
                        1, 60 * 5, 1, (num) => num == 1 ? Helper.Translation.Get("instant") : Helper.Translation.Get("seconds", new { num = num / 60f })
                    );
                }
            }

            gmcm.AddPage(ModManifest, "RadialA", () => Helper.Translation.Get("title.configure.first"));
            ConfigureRadial(Config.A);

            gmcm.AddPage(ModManifest, "RadialB", () => Helper.Translation.Get("title.configure.second"));
            ConfigureRadial(Config.B);
        }

        [EventPriority(EventPriority.High + 1)]
        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            var radials = new[] { Config.A, Config.B };
            if (State.showingMenu != null && !radials[State.showingMenu.Value].Trigger.IsDown())
            {
                int menu = State.showingMenu.Value;
                State.showingMenu = null;

                var radial = radials[menu];

                var sticks = Game1.input.GetGamePadState().ThumbSticks;
                Vector2 cursor = new Vector2(sticks.Left.X, sticks.Left.Y);
                if (cursor.Length() < DEADZONE)
                {
                    cursor = new Vector2(sticks.Right.X, sticks.Right.Y);
                    if (cursor.Length() < DEADZONE)
                        cursor = Vector2.Zero;
                }
                float cursorAngle = cursor != Vector2.Zero ? MathF.Atan2(cursor.Y, cursor.X) : 0;
                if (cursorAngle < 0)
                    cursorAngle += MathF.PI * 2;

                // Lazy way - probably could calculate this directly
                int? index = null;
                if (cursor != Vector2.Zero)
                {
                    float seg = MathF.PI * 2 / radial.Keybindings.Length;
                    float start = 0 - (MathF.PI * 2 / radial.Keybindings.Length) / 2;
                    for (int i = 0; i < radial.Keybindings.Length; ++i)
                    {
                        float startAngle = start + seg * i;
                        float endAngle = start + seg * (i + 1);
                        float midAngle = (startAngle + endAngle) / 2;
                        float totalAngle = (endAngle - startAngle);

                        bool sel = false;
                        if (cursor != Vector2.Zero && cursorAngle >= startAngle && cursorAngle < endAngle)
                        {
                            index = i;
                            break;
                        }
                    }
                }

                if (index == null)
                    return;

                var keybindOpt = radials[menu].Keybindings[index.Value];

                var kdict = keybinds.FirstOrDefault(kvp => kvp.Key.UniqueID == keybindOpt.ModId);
                if (kdict.Key == null)
                {
                    if ( keybindOpt.ModId != null && keybindOpt.ModId != "" )
                        Game1.showRedMessage($"No mod with ID \"{keybindOpt.ModId}\".");
                    return;
                }

                if (!kdict.Value.ContainsKey(keybindOpt.KeybindOption) ||
                     kdict.Value[keybindOpt.KeybindOption] is not SimpleModOption<Keybind> and not SimpleModOption<KeybindList>)
                {
                    Game1.showRedMessage($"No such keybinding option \"{keybindOpt.KeybindOption}\" in mod \"{kdict.Key.Name}\".");
                    return;
                }

                if (kdict.Value[keybindOpt.KeybindOption] is SimpleModOption<Keybind> smok)
                {
                    smok.BeforeMenuClosed(); // updates the cached value
                    if (smok.Value == null || !smok.Value.IsBound)
                    {
                        Game1.showRedMessage($"The keybinding \"{keybindOpt.KeybindOption}\" in mod \"{kdict.Key.Name}\" is not bound.");
                        return;
                    }
                    else
                    {
                        State.pressing = smok.Value;
                    }
                }
                else if (kdict.Value[keybindOpt.KeybindOption] is SimpleModOption<KeybindList> smokl)
                {
                    smokl.BeforeMenuClosed(); // updates the cached value
                    if (smokl.Value == null || !smokl.Value.IsBound)
                    {
                        Game1.showRedMessage($"The keybinding \"{keybindOpt.KeybindOption}\" in mod \"{kdict.Key.Name}\" is not bound.");
                        return;
                    }
                    else
                    {
                        State.pressing = smokl.Value.Keybinds[0]; // only need the first one
                    }
                }

                State.pressLengthLeft = keybindOpt.PressDuration;
            }
            else if (State.showingMenu == null)
            {
                for ( int i = 0; i < radials.Length; ++i )
                {
                    var radial = radials[i];
                    if (radial.Trigger.JustPressed())
                    {
                        State.showingMenu = i;
                    }
                }
            }

            if (State.pressing != null && State.pressLengthLeft > 0)
            {
                object inputStateGetter = Helper.Input.GetType().GetField("CurrentInputState", BindingFlags.NonPublic | BindingFlags.Instance).GetValue( Helper.Input );
                object inputState = (inputStateGetter as Delegate).DynamicInvoke();
                foreach ( var button in State.pressing.Buttons )
                    inputState.GetType().GetMethod("OverrideButton").Invoke(inputState, new object[] { button, true });

                if (--State.pressLengthLeft == 0)
                {
                    State.pressing = null;
                }
            }
        }

        [EventPriority(EventPriority.Low)]
        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if (State.showingMenu == null)
                return;

            var sticks = Game1.input.GetGamePadState().ThumbSticks;
            Vector2 cursor = new Vector2(sticks.Left.X, sticks.Left.Y);
            if (cursor.Length() < DEADZONE)
            {
                cursor = new Vector2(sticks.Right.X, sticks.Right.Y);
                if (cursor.Length() < DEADZONE)
                    cursor = Vector2.Zero;
            }
            float cursorAngle = cursor != Vector2.Zero ? MathF.Atan2(cursor.Y, cursor.X) : 0;
            if (cursorAngle < 0)
                cursorAngle += MathF.PI * 2;

            var radials = new[] { Config.A, Config.B };
            var radial = radials[State.showingMenu.Value];

            effect.Projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateOrthographicOffCenter(0, Game1.uiViewport.Width, Game1.uiViewport.Height, 0, 0, 1);

            Vector2 baseOrigin = new Vector2(
                Game1.uiViewport.Width / 2,
                Game1.uiViewport.Height / 2
            );

            var r = Game1.graphics.GraphicsDevice.RasterizerState;
            Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            try
            {

                float seg = MathF.PI * 2 / radial.Keybindings.Length;
                float start = 0 - (MathF.PI * 2 / radial.Keybindings.Length) / 2;
                for (int i = 0; i < radial.Keybindings.Length; ++i)
                {
                    float startAngle = start + seg * i;
                    float endAngle = start + seg * (i + 1);
                    float midAngle = (startAngle + endAngle) / 2;
                    float totalAngle = Math.Abs(endAngle - startAngle);

                    bool sel = false;
                    if (cursor != Vector2.Zero && cursorAngle >= startAngle && cursorAngle < endAngle)
                        sel = true;

                    int Slices = 4;

                    List<Vector2> vs = new();
                    for (int iv = 0; iv < Slices; ++iv)
                    {
                        float circSpot = startAngle + totalAngle / Slices * iv;
                        float nextSpot = startAngle + totalAngle / Slices * (iv + 1);

                        float minRad = 32;
                        float maxRad = 256 + 32;

                        vs.Add(new Vector2(MathF.Cos(circSpot) * minRad, -MathF.Sin(circSpot) * minRad));
                        vs.Add(new Vector2(MathF.Cos(circSpot) * maxRad, -MathF.Sin(circSpot) * maxRad));
                        vs.Add(new Vector2(MathF.Cos(nextSpot) * maxRad, -MathF.Sin(nextSpot) * maxRad));
                        vs.Add(new Vector2(MathF.Cos(nextSpot) * maxRad, -MathF.Sin(nextSpot) * maxRad));
                        vs.Add(new Vector2(MathF.Cos(nextSpot) * minRad, -MathF.Sin(nextSpot) * minRad));
                        vs.Add(new Vector2(MathF.Cos(circSpot) * minRad, -MathF.Sin(circSpot) * minRad));
                    }

                    float offset = 48;
                    Vector2 origin = baseOrigin;
                    origin += new Vector2(MathF.Cos(midAngle) * offset, -MathF.Sin(midAngle) * offset);
                    Color col = new Color(Color.DarkGray, 0.75f);
                    if (sel)
                        col = new Color(Color.LightGray, 0.85f);

                    var vpcs = new VertexPositionColor[vs.Count];
                    for (int iv = 0; iv < vs.Count; ++iv)
                    {
                        vpcs[iv] = new(new Vector3(origin.X + vs[iv].X, origin.Y + vs[iv].Y, 0), col);
                    }

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Game1.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vpcs, 0, vpcs.Length / 3);
                    }

                    var kdict = keybinds.FirstOrDefault(kvp => kvp.Key.UniqueID == radial.Keybindings[ i ].ModId);
                    if (kdict.Key == null)
                        continue;
                    
                    offset = 192;
                    Vector2 midPoint = origin;
                    midPoint += new Vector2(MathF.Cos(midAngle) * offset, -MathF.Sin(midAngle) * offset);
                    e.SpriteBatch.DrawString(Game1.smallFont, kdict.Key.Name, midPoint - Game1.smallFont.MeasureString( kdict.Key.Name ) / 2, Color.Black);
                    e.SpriteBatch.DrawString(Game1.smallFont, radial.Keybindings[ i ].KeybindOption, midPoint - Game1.smallFont.MeasureString( radial.Keybindings[ i ].KeybindOption ) / 2 + new Vector2( 0, 24 ), Color.Black);

                }
            }
            finally
            {
                Game1.graphics.GraphicsDevice.RasterizerState = r;
            }
            }

        private Dictionary<IManifest, Dictionary<string, BaseModOption>> keybinds;
        private void BuildKeybindingCache()
        {
            keybinds = new();

            foreach (var config in GenericModConfigMenu.Mod.instance.ConfigManager.GetAll())
            {
                foreach (var opt in config.GetAllOptions())
                {
                    if (!(opt is SimpleModOption<SButton> || opt is SimpleModOption<KeybindList>))
                        continue;

                    if (Context.IsWorldReady && opt.IsTitleScreenOnly)
                        continue;

                    string name = config.ModManifest + ": " + opt.Name();
                    if ( !keybinds.ContainsKey( config.ModManifest ) )
                        keybinds.Add(config.ModManifest, new());
                    keybinds[config.ModManifest].Add(opt.Name(), opt);
                }
            }
        }
    }
}
