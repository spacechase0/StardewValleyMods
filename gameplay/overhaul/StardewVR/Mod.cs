using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.Attributes;
using Stardew3D.Handlers.Game;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewVR.Handlers.Game;
using StardewVR.Handlers.Game.FirstPerson;
using StardewVR.Handlers.Menu;
using Valve.VR;


// This might be incorrect since I don't understand matrices super well, but:
//
// MonoGame docs and code say (or seem to say) the following: Row major, pre-multiplication, right handed, forward = -Z
//      Row major: operator[] docs
//      Pre-multiplication: Code in EffectHelpers.SetWorldViewProjAndFog does world * view * projection, not projection * view * world
//      Right handed: Matrix docs
//      Forward: Vector3.Forward = (0, 0, -1)
//
// OpenVR docs and code say (or seem to say) the following: Column major, post-multiplication, right handed, forward = -Z
//      Column major (logically): From https://github.com/ValveSoftware/openvr/wiki/Matrix-Usage-Example
//      Post-multiplication: Same link as previous, the example does projection * view * world
//      Right handed: openvr.h, comment right above the matrix struct definitions
//      Forward: openvr.h, same spot as previous
//
// OpenVR.Net docs and code say (or seem to say) the following: Column major, post-multiplication, left handed, forward = ?
//      Column major: It doesn't change how the values are used compared to base OpenVR
//      Post-multiplication: README shows it, the example has projection on the first of the multiplied terms
//      Left handed: README mentions it
//      Forward: ? (not sure if it even has concept of this)
//
// You might have noticed that OpenVR.Net said that their coordinates are left handed, unlike OpenVR and MonoGame.
// I don't see any conversion in the projection matrix code (not that I know if there would be any for projection matrices).
// But I *do* see conversion in their function to extract the position from an OpenVR matrix.
// (It seems like for rotations too, but it's harder to be sure because I understand quaternions even less than matrices.)
//
// So... basically we're gonna pretend OpenVR.Net's wrapper level doesn't exist for anything but initialization, cleanup, and the Update* methods. :P
// (I was using a different wrapper library previously but was having weird problems that went away when I switched...
// though that likely was just me doing things wrong)
//
// So we just gotta transpose OpenVR matrices before using them.
// 
// We try to stick with MonoGame conventions here, though that's hard when I don't know what I'm doing.

namespace StardewVR
{
    [HasConfig<Configuration>]
    //[HasState<State>]
    [HasHarmony]
    public partial class Mod : BaseMod< Mod >
    {
        public string DefaultVrHandler => $"{Mod.Instance.ModManifest.UniqueID}/FirstPerson";

        protected override void ModEntry()
        {
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            Stardew3D.State.AddingGameHandlers += (sender, _) =>
            {
                var state = sender as Stardew3D.State;
                state.AddGameHandler(new FirstPersonVRGameHandler());
            };
            Stardew3D.State.GameHandlersFinalized += (sender, _) =>
            {
                var state = sender as Stardew3D.State;
                state.SetJointHandlerForGameHandlerTags<IClickableMenu, GenericMenuHandler<IClickableMenu>>([IGameHandler.CategoryVR], (handler) => (menu) => new GenericMenuHandler<IClickableMenu>(handler as VRGameHandler, menu as IClickableMenu));
                state.SetJointHandlerForGameHandlerTags<TitleMenu, TitleMenuHandler>([IGameHandler.CategoryVR], (handler) => (menu) => new TitleMenuHandler(handler as VRGameHandler, menu as TitleMenu));
            };
        }

        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if (Config.ToggleVirtualReality.JustPressed())
            {
                var currHandler = Stardew3D.Mod.State.ActiveHandler;
                var targetHandler = Stardew3D.Mod.State.GetGameHandler(DefaultVrHandler);
                if (currHandler != null)
                {
                    if (!currHandler.Tags.Contains(IGameHandler.CategoryVR))
                    {
                        string[] tags = currHandler.Tags.Select(t => t == IGameHandler.CategoryFlatscreen ? IGameHandler.CategoryVR : t).ToArray();
                        targetHandler = Stardew3D.Mod.State.FindGameHandlersMatching(tags).FirstOrDefault() ?? targetHandler;
                    }
                    else
                    {
                        targetHandler = null;
                    }
                }

                Stardew3D.Mod.State.ActiveHandler = targetHandler;
            }
        }

        internal static Texture_t GetTextureFrom(RenderTarget2D target)
        {
            // TODO: Use SMAPI reflection since it caches
            var fieldInfo = typeof(Texture2D).GetField("glTexture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var handle = new IntPtr((int)fieldInfo.GetValue(target));

            var tex = new Texture_t();
            tex.handle = handle;
            tex.eType = ETextureType.OpenGL;
            tex.eColorSpace = EColorSpace.Auto;
            return tex;
        }

        // Based on https://eecs.qmul.ac.uk/~gslabaugh/publications/euler.pdf
        public static Vector3 GetRotationFrom(Matrix mat)
        {
            // roll yaw pitch
            // o u o_?

            if (MathF.Abs(mat.M31) != 1)
            {
                var o1 = -MathF.Asin(mat.M31);
                //var o2 = MathF.PI - o1;
                var u1 = MathF.Atan2(mat.M32 / MathF.Cos(o1), mat.M33 / MathF.Cos(o1));
                //var u2 = MathF.Atan2(mat.M32 / MathF.Cos(o2), mat.M33 / MathF.Cos(o2));
                var o_1 = MathF.Atan2(mat.M21 / MathF.Cos(o1), mat.M11 / MathF.Cos(o1));
                //var o_2 = MathF.Atan2(mat.M21 / MathF.Cos(o2), mat.M11 / MathF.Cos(o2));
                return new(o1, u1, o_1);
            }
            else
            {
                var o_ = 0f;
                if (mat.M31 == -1)
                {
                    var o = MathF.PI / 2;
                    var u = o_ + MathF.Atan2(mat.M12, mat.M13);
                    return new(o, u, o_);
                }
                else
                {
                    var o = -MathF.PI / 2;
                    var u = -o_ + MathF.Atan2(-mat.M12, -mat.M13);
                    return new(o, u, o_);
                }
            }
        }
    }
}
