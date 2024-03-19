using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D;
using StardewModdingAPI;
using StardewValley;
using Valve.VR;

namespace StardewVR
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        internal CVRSystem vrSys;
        internal uint headsetIndex = uint.MaxValue;
        internal uint leftControllerIndex = uint.MaxValue, rightControllerIndex = uint.MaxValue;

        internal RenderTarget2D leftScreen, rightScreen;

        internal EVREye activeEye;
        internal RenderTarget2D activeScreen;
        internal Matrix activeEyeTransform;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            EVRInitError ie = EVRInitError.None;
            vrSys = OpenVR.Init(ref ie);
            if (ie != EVRInitError.None)
            {
                Log.Error("Failed to init VR headset: " + ie);
                return;
            }

            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; ++i)
            {
                var c = vrSys.GetTrackedDeviceClass(i);
                if (c == ETrackedDeviceClass.Controller)
                {
                    var role = vrSys.GetControllerRoleForTrackedDeviceIndex(i);
                    if (role == ETrackedControllerRole.LeftHand)
                        leftControllerIndex = i;
                    else if (role == ETrackedControllerRole.RightHand)
                        rightControllerIndex = i;
                }
                else if (c == ETrackedDeviceClass.HMD)
                    headsetIndex = i;
            }

            uint screenWidth = 0, screenHeight = 0;
            vrSys.GetRecommendedRenderTargetSize( ref screenWidth, ref screenHeight);
            leftScreen = new(Game1.graphics.GraphicsDevice, ( int ) screenWidth, ( int ) screenHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            rightScreen = new(Game1.graphics.GraphicsDevice, ( int ) screenWidth, ( int ) screenHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        internal void Submit(EVREye which, RenderTarget2D target)
        {
            // TODO: Use SMAPI reflection since it caches
            var fieldInfo = typeof(Texture2D).GetField("glTexture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var handle = new IntPtr((int)fieldInfo.GetValue(target));

            var tex = new Texture_t();
            tex.handle = handle;
            tex.eType = ETextureType.OpenGL;
            tex.eColorSpace = EColorSpace.Auto;
            var texBounds = new VRTextureBounds_t();
            texBounds.uMin = 0;
            texBounds.uMax = 1;
            texBounds.vMin = 1;
            texBounds.vMax = 0;
            var ce = OpenVR.Compositor.Submit(which, ref tex, ref texBounds, EVRSubmitFlags.Submit_Default);
            if (ce != EVRCompositorError.None)
                Log.Error("Compositor error: " + ce);
        }

        ~Mod()
        {
            OpenVR.Shutdown();
        }
    }

    [HarmonyPatch(typeof(Stardew3D.Mod), "DoCamera")]
    public static class FixCameraPatch
    {
        public static void Postfix()
        {
            var poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            Mod.instance.vrSys.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseSeated, 0, poses);
            
            var oldTarget = Stardew3D.Mod.State.Camera.Target;

            var x = poses[Mod.instance.headsetIndex].mDeviceToAbsoluteTracking.ToMonogame();
            Stardew3D.Mod.instance.basicEffect.Projection = Mod.instance.vrSys.GetProjectionMatrix(Mod.instance.activeEye, 0.01f, 200f).ToMonogame();
            Stardew3D.Mod.instance.basicEffect.View = Matrix.CreateLookAt(x.Translation + oldTarget, x.Translation + oldTarget + x.Forward, x.Up) * Mod.instance.activeEyeTransform;

            Stardew3D.Mod.State.Camera.Target = oldTarget;
        }
    }

    [HarmonyPatch(typeof(Stardew3D.Camera), nameof(Stardew3D.Camera.GetUp))]
    public static class FixCameraUpPatch
    {
        public static void Postfix( ref Vector3 __result )
        {
            var poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            Mod.instance.vrSys.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseSeated, 0, poses);

            //__result = poses[Mod.instance.headsetIndex].mDeviceToAbsoluteTracking.ToMonogame().Up;
            //__result = new Vector3(__result.X, __result.Y, __result.Z);
        }
    }

    [HarmonyPatch(typeof(Stardew3D.Mod.DoRender), "Prefix")]
    public static class DoDrawVRPatch
    {
        public static bool Prefix(GameTime gameTime, RenderTarget2D target_screen,
                                  Task __2, IMonitor __3, Multiplayer __4)
        {
            if (Game1.game1.takingMapScreenshot)
                return true;

            var renderPoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            var gamePoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            var ce = OpenVR.Compositor.WaitGetPoses(renderPoses, gamePoses);
            if (ce != EVRCompositorError.None)
                Log.Error("Compositor Error: " + ce);

            var oldViewport = Game1.viewport;
            var oldUiViewport = Game1.uiViewport;
            Game1.viewport.Width = Mod.instance.leftScreen.Width;
            Game1.viewport.Height = Mod.instance.leftScreen.Height;
            Game1.uiViewport.Width = Mod.instance.leftScreen.Width;
            Game1.uiViewport.Height = Mod.instance.leftScreen.Height;

            Game1.isRenderingScreenBuffer = true;

            Mod.instance.activeEye = EVREye.Eye_Left;
            Mod.instance.activeScreen = Mod.instance.leftScreen;
            Mod.instance.activeEyeTransform = Matrix.Invert( Mod.instance.vrSys.GetEyeToHeadTransform( EVREye.Eye_Left ).ToMonogame() );
            Mod.instance.Helper.Reflection.GetMethod(typeof(Stardew3D.SGameDrawOverride), "Impl").Invoke(gameTime, Mod.instance.activeScreen, __2, __3, __4);
            Mod.instance.Submit(Mod.instance.activeEye, Mod.instance.activeScreen);

            Mod.instance.activeEye = EVREye.Eye_Right;
            Mod.instance.activeScreen = Mod.instance.rightScreen;
            Mod.instance.activeEyeTransform = Matrix.Invert( Mod.instance.vrSys.GetEyeToHeadTransform(EVREye.Eye_Right).ToMonogame() );
            Mod.instance.Helper.Reflection.GetMethod(typeof(Stardew3D.SGameDrawOverride), "Impl").Invoke(gameTime, Mod.instance.activeScreen, __2, __3, __4);
            Mod.instance.Submit(Mod.instance.activeEye, Mod.instance.activeScreen);

            Game1.viewport = oldViewport;
            Game1.uiViewport = oldUiViewport;

            return true;
        }
    }
}
