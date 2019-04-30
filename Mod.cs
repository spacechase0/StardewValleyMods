using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using ContentPatcher;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Projectiles;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;

namespace ContentPatcherAnimations
{
    public class PatchData
    {
        public Func<bool> IsActive;
        public Texture2D Target;
        public Texture2D Source;
        public int CurrentFrame = 0;
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public const BindingFlags PublicI = BindingFlags.Public | BindingFlags.Instance;
        public const BindingFlags PublicS = BindingFlags.Public | BindingFlags.Static;
        public const BindingFlags PrivateI = BindingFlags.NonPublic | BindingFlags.Instance;
        public const BindingFlags PrivateS = BindingFlags.NonPublic | BindingFlags.Static;

        private StardewModdingAPI.Mod contentPatcher;
        private IEnumerable cpPatches;

        private Dictionary<Patch, PatchData> animatedPatches = new Dictionary<Patch, PatchData>();

        public static uint frameCounter = 0;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            var modData = helper.ModRegistry.Get("Pathoschild.ContentPatcher");
            contentPatcher = (StardewModdingAPI.Mod) modData.GetType().GetProperty("Mod", PrivateI | PublicI).GetValue(modData);
            var patchManager = contentPatcher.GetType().GetField("PatchManager", PrivateI).GetValue(contentPatcher);
            cpPatches = (IEnumerable) patchManager.GetType().GetField("Patches", PrivateI).GetValue(patchManager);

            CollectPatches();

            Helper.Events.GameLoop.UpdateTicked += UpdateAnimations;
            Helper.Events.GameLoop.SaveCreated += UpdateTargetTextures;
            Helper.Events.GameLoop.SaveLoaded += UpdateTargetTextures;
            Helper.Events.GameLoop.DayStarted += UpdateTargetTextures;
        }

        private void UpdateAnimations(object sender, UpdateTickedEventArgs e)
        {
            ++frameCounter;
            foreach ( var patch in animatedPatches )
            {
                if (!patch.Value.IsActive.Invoke())
                    continue;

                if ( frameCounter % patch.Key.AnimationFrameTime == 0 )
                {
                    if (++patch.Value.CurrentFrame >= patch.Key.AnimationFrameCount)
                        patch.Value.CurrentFrame = 0;

                    var sourceRect = patch.Key.FromArea;
                    sourceRect.X += patch.Value.CurrentFrame * sourceRect.Width;
                    var cols = new Color[sourceRect.Width * sourceRect.Height];
                    patch.Value.Source.GetData(0, sourceRect, cols, 0, cols.Length);
                    patch.Value.Target.SetData(0, patch.Key.ToArea, cols, 0, cols.Length);
                }
            }
        }

        private void UpdateTargetTextures(object sender, EventArgs args)
        {
            foreach ( var patch in animatedPatches )
            {
                patch.Value.Target = FindTargetTexture(patch.Key.Target);
            }
        }

        private void CollectPatches()
        {
            foreach (var pack in contentPatcher.Helper.ContentPacks.GetOwned())
            {
                var patches = pack.ReadJsonFile<PatchList>("content.json");
                foreach (var patch in patches.Changes)
                {
                    if (patch.AnimationFrameTime > 0 && patch.AnimationFrameCount > 0)
                    {
                        if (patch.LogName == "")
                        {
                            Log.error("Animated patches must specify a LogName!");
                            continue;
                        }
                        if (patch.Target.Contains("{{") || patch.FromFile.Contains("{{"))
                        {
                            Log.error("Dynamic tokens not supported for animations! Patch: " + patch.LogName);
                            continue;
                        }
                        
                        if ( patch.ToArea == new Rectangle() )
                        {
                            patch.ToArea = new Rectangle(0, 0, patch.FromArea.Width, patch.FromArea.Height);
                        }

                        PatchData data = new PatchData();

                        object targetPatch = null;
                        foreach (var cpPatch in cpPatches)
                        {
                            var name = (string)cpPatch.GetType().GetProperty("LogName", PublicI).GetValue(cpPatch);
                            if (name == pack.Manifest.Name + " > " + patch.LogName)
                            {
                                targetPatch = cpPatch;
                                break;
                            }
                        }
                        if (targetPatch == null)
                        {
                            Log.error("Failed to find patch with name " + patch.LogName + "!?!?");
                            continue;
                        }
                        var appliedProp = targetPatch.GetType().GetProperty("IsApplied", PublicI);
                        Log.trace("Applied prop:" + appliedProp);

                        data.IsActive = () => (bool)appliedProp.GetValue(targetPatch);
                        data.Source = pack.LoadAsset<Texture2D>(patch.FromFile);
                        data.Target = FindTargetTexture(patch.Target);
                        if ( data.Target == null )
                        {
                            Log.error("Failed to find target texture " + patch.Target + "! Patch: " + patch.LogName);
                            continue;
                        }

                        animatedPatches.Add(patch, data);
                    }
                }
            }
        }

        private Texture2D FindTargetTexture(string target)
        {
            return Game1.content.Load<Texture2D>(target);
            /*
            switch ( target.ToLower() )
            {
                case "buildings\\houses":
                    return Farm.houseTextures;
                case "characters\\farmer\\accessories":
                    return FarmerRenderer.accessoriesTexture;
                case "characters\\farmer\\hairstyles":
                    return FarmerRenderer.hairStylesTexture;
                case "characters\\farmer\\hats":
                    return FarmerRenderer.hatsTexture;
                case "characters\\farmer\\shirts":
                    return FarmerRenderer.shirtsTexture;
                case "loosesprites\\lighting\\greenlight":
                    return Game1.cauldronLight;
                case "loosesprites\\lighting\\indoorwindowlight":
                    return Game1.indoorWindowLight;
                case "loosesprites\\lighting\\lantern":
                    return Game1.lantern;
                case "loosesprites\\lighting\\sconcelight":
                    return Game1.sconceLight;
                case "loosesprites\\lighting\\windowlight":
                    return Game1.windowLight;
                case "loosesprites\\controllermaps":
                    return Game1.controllerMaps;
                case "loosesprites\\cursors":
                    return Game1.mouseCursors;
                case "loosesprites\\daybg":
                    return Game1.daybg;
                case "loosesprites\\font_bold":
                    return SpriteText.spriteTexture;
                case "loosesprites\\font_colored":
                    return SpriteText.coloredTexture;
                case "loosesprites\\nightbg":
                    return Game1.nightbg;
                case "loosesprites\\shadow":
                    return Game1.shadowTexture;
                case "tilesheets\\crops":
                    return Game1.cropSpriteSheet;
                case "tilesheets\\debris":
                    return Game1.debrisSpriteSheet;
                case "tilesheets\\emotes":
                    return Game1.emoteSpriteSheet;
                case "tilesheets\\furniture":
                    return Furniture.furnitureTexture;
                case "tilesheets\\projectiles":
                    return Projectile.projectileSheet;
                case "tilesheets\\rain":
                    return Game1.rainTexture;
                case "tilesheets\\tools":
                    return (Texture2D) typeof(Game1).GetField("_toolSpriteSheet", PrivateS).GetValue(null);
                case "tilesheets\\weapons":
                    return Tool.weaponsTexture;
                case "maps\\menutiles":
                    return Game1.menuTexture;
                case "maps\\springobjects":
                    return Game1.objectSpriteSheet;
                case "maps\\walls_and_floors":
                    return Wallpaper.wallpaperTexture;
                case "tilesheets\\animations":
                    return Game1.animations;
                case "tilesheets\\buffsicons":
                    return Game1.buffsIcons;
                case "tilesheets\\craftables":
                    return Game1.bigCraftableSpriteSheet;
                case "tilesheets\\fruittrees":
                    return FruitTree.texture;
                case "terrainfeatures\\flooring":
                    return Flooring.floorsTexture;
                case "terrainfeatures\\hoedirt":
                    return HoeDirt.lightTexture;
                case "terrainfeatures\\hoedirtdark":
                    return HoeDirt.darkTexture;
                case "terrainfeatures\\hoedirtsnow":
                    return HoeDirt.snowTexture;
            }

            // bushes
            // trees
            // animals
            // buildings
            // characters
            // fences
            // portraits

            return null;
            */
        }
    }
}
