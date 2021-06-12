using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcherAnimations
{
    public class PatchData
    {
        public object patchObj;
        public Func<bool> IsActive;
        public Func<Texture2D> TargetFunc;
        public Texture2D Target;
        public Func<Texture2D> SourceFunc;
        public Texture2D Source;
        public Func<Rectangle> FromAreaFunc;
        public Func<Rectangle> ToAreaFunc;
        public int CurrentFrame;
    }

    public class ScreenState
    {
        public IEnumerable cpPatches;

        public Dictionary<Patch, PatchData> animatedPatches = new();

        public uint frameCounter;
        public int findTargetsCounter;
        public Queue<Patch> findTargetsQueue = new();
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public const BindingFlags PublicI = BindingFlags.Public | BindingFlags.Instance;
        public const BindingFlags PublicS = BindingFlags.Public | BindingFlags.Static;
        public const BindingFlags PrivateI = BindingFlags.NonPublic | BindingFlags.Instance;
        public const BindingFlags PrivateS = BindingFlags.NonPublic | BindingFlags.Static;

        private StardewModdingAPI.Mod contentPatcher;
        private PerScreen<ScreenState> screenState = new();
        internal ScreenState ScreenState => this.screenState.Value;

        private WatchForUpdatesAssetEditor watcher;

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            this.Helper.Events.GameLoop.UpdateTicked += this.UpdateAnimations;

            Action updateTargets = () =>
            {
                foreach (var screen in this.screenState.GetActiveValues())
                    screen.Value.findTargetsCounter = 1;
            };

            this.Helper.Events.GameLoop.SaveCreated += (s, e) => updateTargets();
            this.Helper.Events.GameLoop.SaveLoaded += (s, e) => updateTargets();
            this.Helper.Events.GameLoop.DayStarted += (s, e) => updateTargets();

            helper.Content.AssetEditors.Add(this.watcher = new WatchForUpdatesAssetEditor());

            helper.ConsoleCommands.Add("cpa", "...", this.OnCommand);
        }

        private void OnCommand(string cmd, string[] args)
        {
            if (args[0] == "reload")
            {
                this.CollectPatches();
            }
        }

        private void UpdateAnimations(object sender, UpdateTickedEventArgs e)
        {
            if (this.contentPatcher == null)
            {
                var modData = this.Helper.ModRegistry.Get("Pathoschild.ContentPatcher");
                this.contentPatcher = (StardewModdingAPI.Mod)modData.GetType().GetProperty("Mod", Mod.PrivateI | Mod.PublicI).GetValue(modData);
            }

            if (this.screenState.Value == null)
            {
                this.screenState.Value = new ScreenState();
            }

            if (this.ScreenState.cpPatches == null)
            {
                var screenManagerPerScreen = this.contentPatcher.GetType().GetField("ScreenManager", Mod.PrivateI).GetValue(this.contentPatcher);
                var screenManager = screenManagerPerScreen.GetType().GetProperty("Value").GetValue(screenManagerPerScreen);
                var patchManager = screenManager.GetType().GetProperty("PatchManager").GetValue(screenManager);
                this.screenState.Value.cpPatches = (IEnumerable)patchManager.GetType().GetField("Patches", Mod.PrivateI).GetValue(patchManager);

                this.CollectPatches();
            }


            if (this.ScreenState.findTargetsCounter > 0 && --this.ScreenState.findTargetsCounter == 0)
                this.UpdateTargetTextures();
            while (this.ScreenState.findTargetsQueue.Count > 0)
            {
                var patch = this.ScreenState.findTargetsQueue.Dequeue();
                this.UpdateTargetTextures(patch);
            }

            ++this.ScreenState.frameCounter;
            Game1.graphics.GraphicsDevice.Textures[0] = null;
            foreach (var patch in this.ScreenState.animatedPatches)
            {
                if (!patch.Value.IsActive.Invoke() || patch.Value.Source == null || patch.Value.Target == null)
                    continue;

                try
                {
                    if (this.ScreenState.frameCounter % patch.Key.AnimationFrameTime == 0)
                    {
                        if (++patch.Value.CurrentFrame >= patch.Key.AnimationFrameCount)
                            patch.Value.CurrentFrame = 0;

                        var sourceRect = patch.Value.FromAreaFunc.Invoke();
                        sourceRect.X += patch.Value.CurrentFrame * sourceRect.Width;
                        var targetRect = patch.Value.ToAreaFunc.Invoke();
                        if (targetRect == Rectangle.Empty)
                            targetRect = new Rectangle(0, 0, sourceRect.Width, sourceRect.Height);
                        var cols = new Color[sourceRect.Width * sourceRect.Height];
                        patch.Value.Source.GetData(0, sourceRect, cols, 0, cols.Length);
                        patch.Value.Target.SetData(0, targetRect, cols, 0, cols.Length);
                    }
                }
                catch (ObjectDisposedException e_)
                {
                    // No idea why this happens, hack fix
                    patch.Value.Target = null;
                    this.ScreenState.findTargetsQueue.Enqueue(patch.Key);
                }
            }
        }

        private void UpdateTargetTextures()
        {
            foreach (var patch in this.ScreenState.animatedPatches)
            {
                try
                {
                    if (!patch.Value.IsActive.Invoke())
                        continue;

                    patch.Value.Source = patch.Value.SourceFunc();
                    patch.Value.Target = patch.Value.TargetFunc();
                }
                catch (Exception e)
                {
                    Log.Trace("Exception loading " + patch.Key.LogName + " textures, delaying to try again next frame: " + e);
                    this.ScreenState.findTargetsQueue.Enqueue(patch.Key);
                }
            }
        }

        private void UpdateTargetTextures(Patch key)
        {
            try
            {
                var patch = this.ScreenState.animatedPatches[key];
                if (!patch.IsActive())
                    return;

                patch.Source = patch.SourceFunc();
                patch.Target = patch.TargetFunc();
            }
            catch (Exception e)
            {
                Log.Error("Exception loading " + key.LogName + " textures: " + e);
            }
        }

        private void CollectPatches()
        {
            this.ScreenState.animatedPatches.Clear();
            this.ScreenState.findTargetsQueue.Clear();
            foreach (var pack in this.contentPatcher.Helper.ContentPacks.GetOwned())
            {
                var patches = pack.ReadJsonFile<PatchList>("content.json");
                foreach (var patch in patches.Changes)
                {
                    if (patch.AnimationFrameTime > 0 && patch.AnimationFrameCount > 0)
                    {
                        Log.Trace("Loading animated patch from content pack " + pack.Manifest.UniqueID);
                        if (patch.LogName == null || patch.LogName == "")
                        {
                            Log.Error("Animated patches must specify a LogName!");
                            continue;
                        }

                        PatchData data = new PatchData();

                        object targetPatch = null;
                        foreach (var cpPatch in this.ScreenState.cpPatches)
                        {
                            var path = cpPatch.GetType().GetProperty("Path", Mod.PublicI).GetValue(cpPatch);
                            if (path.ToString() == pack.Manifest.Name + " > " + patch.LogName)
                            {
                                targetPatch = cpPatch;
                                break;
                            }
                        }
                        if (targetPatch == null)
                        {
                            Log.Error("Failed to find patch with name \"" + patch.LogName + "\"!?!?");
                            continue;
                        }
                        var appliedProp = targetPatch.GetType().GetProperty("IsApplied", Mod.PublicI);
                        var sourceProp = targetPatch.GetType().GetProperty("FromAsset", Mod.PublicI);
                        var targetProp = targetPatch.GetType().GetProperty("TargetAsset", Mod.PublicI);

                        data.patchObj = targetPatch;
                        data.IsActive = () => (bool)appliedProp.GetValue(targetPatch);
                        data.SourceFunc = () => pack.LoadAsset<Texture2D>((string)sourceProp.GetValue(targetPatch));
                        data.TargetFunc = () => this.FindTargetTexture((string)targetProp.GetValue(targetPatch));
                        data.FromAreaFunc = () => this.GetRectangleFromPatch(targetPatch, "FromArea");
                        data.ToAreaFunc = () => this.GetRectangleFromPatch(targetPatch, "ToArea", new Rectangle(0, 0, data.FromAreaFunc().Width, data.FromAreaFunc().Height));

                        this.ScreenState.animatedPatches.Add(patch, data);
                    }
                }
            }
        }

        private Texture2D FindTargetTexture(string target)
        {
            if (this.Helper.Content.NormalizeAssetName(target) == this.Helper.Content.NormalizeAssetName("TileSheets\\tools"))
            {
                return this.Helper.Reflection.GetField<Texture2D>(typeof(Game1), "_toolSpriteSheet").GetValue();
            }
            var tex = Game1.content.Load<Texture2D>(target);
            if (tex.GetType().Name == "ScaledTexture2D")
            {
                Log.Trace("Found ScaledTexture2D from PyTK: " + target);
                tex = this.Helper.Reflection.GetProperty<Texture2D>(tex, "STexture").GetValue();
            }
            return tex;
        }

        private Rectangle GetRectangleFromPatch(object targetPatch, string rectName, Rectangle defaultTo = default(Rectangle))
        {
            var rect = targetPatch.GetType().GetField(rectName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(targetPatch);
            if (rect == null)
            {
                return defaultTo;
            }
            var tryGetRectValue = rect.GetType().GetMethod("TryGetRectangle");

            object[] args = new object[] { null, null };
            if (!((bool)tryGetRectValue.Invoke(rect, args)))
            {
                return Rectangle.Empty;
            }

            return (Rectangle)args[0];
        }
    }
}
