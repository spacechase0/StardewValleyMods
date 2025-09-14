using System;
using System.IO;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Runtime;
using SpaceShared;
using Stardew3D;
using StardewModdingAPI;
using StardewValley;

namespace SuperSpenny
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private MonoGameDeviceContent<MonoGameModelTemplate> model;
        private MonoGameModelInstance modelInstance;

        // Copied from my 1.6 branch Util class in SpaceShared
        private static string FetchFullPath(IModRegistry modRegistry, string modIdAndPath)
        {
            if (modIdAndPath == null || modIdAndPath.IndexOf('/') == -1)
                return null;

            string packId = modIdAndPath.Substring(0, modIdAndPath.IndexOf('/'));
            string path = modIdAndPath.Substring(modIdAndPath.IndexOf('/') + 1);

            // This is really bad. Pathos don't kill me.
            var modInfo = modRegistry.Get(packId);
            if (modInfo is null)
                return null;

            if (modInfo.GetType().GetProperty("Mod")?.GetValue(modInfo) is IMod mod)
                return Path.Combine(mod.Helper.DirectoryPath, path);
            else if (modInfo.GetType().GetProperty("ContentPack")?.GetValue(modInfo) is IContentPack pack)
                return Path.Combine(pack.DirectoryPath, path);

            return null;
        }

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            SkinnedEffectBony.Bytecode = File.ReadAllBytes(Path.Combine(Helper.DirectoryPath, "assets", "SkinnedEffectBony.mgfx"));

            Helper.Events.Display.RenderedWorld += this.Display_RenderedWorld;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            harmony.Patch(AccessTools.Method("SharpGLTF.Runtime.BasicEffectsLoaderContext:CreateSkinnedEffect"),
                          new HarmonyMethod(AccessTools.Method(typeof(BasicEffectsLoaderContextCreateSkinOverride), nameof(BasicEffectsLoaderContextCreateSkinOverride.Prefix))));

            // Needs to come after Harmony patch
            model = MonoGameModelTemplate.LoadDeviceModel(Game1.game1.GraphicsDevice, FetchFullPath(Helper.ModRegistry, ModManifest.UniqueID + "/assets/spenny.gltf"));
            modelInstance = model.Instance.CreateInstance();
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            var penny = Game1.currentLocation.getCharacterFromName("Penny");
            if (penny == null)
                return;

            //Game1.graphics.Clear(Color.CornflowerBlue);
            Game1.graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, float.MaxValue, 0);
            Game1.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Game1.graphics.GraphicsDevice.RasterizerState = new() { CullMode = CullMode.None };

            Matrix proj = Matrix.CreateOrthographic(Game1.viewport.Width, Game1.viewport.Height, 0.01f, 200);

            Vector2 farmerPos = Game1.player.GetBoundingBox().Center.ToVector2();
            var center_ = ( Game1.viewport.Location.AboveLeft + Game1.viewport.Location.BelowRight ) / 2;
            Vector2 center = new Vector2(center_.X, center_.Y);
            float angle = 30 * MathF.PI / 180;
            Matrix angleTransform = Matrix.CreateRotationX(angle);
            Vector3 centerSpot = new(center.X, 0, center.Y);
            Vector3 cameraSpot = centerSpot + Vector3.Transform(Vector3.UnitY * 10, angleTransform);
            Matrix view = Matrix.CreateLookAt(cameraSpot, centerSpot, Vector3.Transform( Vector3.Up, Matrix.CreateRotationX( -angle ) ));
            view = Matrix.CreateLookAt(new(0, 30, 0), new(), Vector3.UnitZ);

            Vector2 pennyPos = penny.GetBoundingBox().Center.ToVector2();
            pennyPos = new( 0, 0);
            Matrix world = Matrix.CreateScale( 1 ) *
                           Matrix.CreateWorld(new Vector3(pennyPos.X, 5, pennyPos.Y), Vector3.UnitZ, Vector3.UnitY);

            proj = Matrix.CreateOrthographic(20, 10, 0.01f, 100);

            modelInstance.Draw(proj, view, world);
        }
    }

    [HarmonyPatch(typeof(MonoGameModelInstance), "UpdateTransforms")]
    public static class MonoGameModelInstanceUpdateTransformsPatch
    {
        public static void Postfix(MonoGameModelInstance __instance, Effect effect, Matrix[] skinTransforms)
        {
            if (effect is SkinnedEffectBony seb)
            {
                seb.SetBoneTransforms(skinTransforms);
            }
        }
    }

    public static class BasicEffectsLoaderContextCreateSkinOverride
    {
        public static bool Prefix(object __instance, SharpGLTF.Schema2.Material srcMaterial, ref Effect __result)
        {
            var dstMaterial = new SkinnedEffectBony(Game1.graphics.GraphicsDevice);

            dstMaterial.Name = srcMaterial.Name;

            dstMaterial.Alpha = Mod.instance.Helper.Reflection.GetMethod(__instance.GetType(), "GetAlphaLevel").Invoke<float>(srcMaterial);// GetAlphaLevel(srcMaterial);
            dstMaterial.DiffuseColor = Mod.instance.Helper.Reflection.GetMethod(__instance.GetType(), "GetDiffuseColor").Invoke<Vector3>(srcMaterial);// GetDiffuseColor(srcMaterial);
            dstMaterial.SpecularColor = Mod.instance.Helper.Reflection.GetMethod(__instance.GetType(), "GetSpecularColor").Invoke<Vector3>(srcMaterial);// GetSpecularColor(srcMaterial);
            dstMaterial.SpecularPower = Mod.instance.Helper.Reflection.GetMethod(__instance.GetType(), "GetSpecularPower").Invoke<float>(srcMaterial);// GetSpecularPower(srcMaterial);
            dstMaterial.EmissiveColor = Mod.instance.Helper.Reflection.GetMethod(__instance.GetType(), "GeEmissiveColor").Invoke<Vector3>(srcMaterial);// GeEmissiveColor(srcMaterial);
            dstMaterial.Texture = Mod.instance.Helper.Reflection.GetMethod(__instance, "UseDiffuseTexture").Invoke<Texture2D>(srcMaterial);// UseDiffuseTexture(srcMaterial);

            dstMaterial.WeightsPerVertex = 4;
            dstMaterial.PreferPerPixelLighting = true;

            // apparently, SkinnedEffect does not support disabling textures, so we set a white texture here.
            if (dstMaterial.Texture == null) dstMaterial.Texture = Mod.instance.Helper.Reflection.GetMethod(__instance, "UseTexture").Invoke<Texture2D>((SharpGLTF.Schema2.MaterialChannel?)null, (string)null);// UseTexture(null, null); // creates a dummy white texture.

            // todo - why no texture?

            __result = dstMaterial;
            return false;
        }
    }
}
