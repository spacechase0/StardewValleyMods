using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using SpaceCore.Interface;
using SpaceCore.Patches;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Triggers;
using static SpaceCore.SpaceCore;

namespace SpaceCore.VanillaAssetExpansion
{
    internal class VanillaAssetExpansion
    {
        private static Dictionary<string, TextureOverridePackData> texs = new();
        private static Dictionary<string, CustomCraftingRecipe> craftingRecipes = new();
        private static Dictionary<string, CustomCraftingRecipe> cookingRecipes = new();

        public static void Init()
        {
            SpaceCore.Instance.Helper.Events.Content.AssetRequested += Content_AssetRequested;
            SpaceCore.Instance.Helper.Events.Content.AssetsInvalidated += Content_AssetInvalidated;
            SpaceCore.Instance.Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            SpaceCore.Instance.Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            SpaceEvents.BeforeGiftGiven += SpaceEvents_BeforeGiftGiven;
            SpaceEvents.AfterGiftGiven += SpaceEvents_AfterGiftGiven;
            SpaceEvents.OnItemEaten += SpaceEvents_OnItemEaten;

            TriggerActionManager.RegisterTrigger("spacechase0.SpaceCore_OnItemUsed");
            TriggerActionManager.RegisterTrigger("spacechase0.SpaceCore_OnItemEaten");
        }

        private static void SpaceEvents_BeforeGiftGiven(object sender, EventArgsBeforeReceiveObject e)
        {
            string npc = e.Npc.Name;
            string item = e.Gift.ItemId;

            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (!dict.TryGetValue(item, out var data))
                return;

            if (data.GiftableToNpcDisallowList != null && data.GiftableToNpcDisallowList.TryGetValue(npc, out string disallowed) && disallowed != null)
            {
                if (!e.Probe)
                {
                    e.Cancel = true;
                    Game1.activeClickableMenu = new DialogueBox(new Dialogue(e.Npc, "spacecore:objectextensiondata:gift_disallowed", disallowed));
                }
            }
            else if (data.GiftableToNpcAllowList != null)
            {
                if (!data.GiftableToNpcAllowList.TryGetValue(npc, out bool allowed) && !allowed)
                {
                    if (!e.Probe)
                    {
                        e.Cancel = true;
                        Game1.activeClickableMenu = new DialogueBox(new Dialogue(e.Npc, "spacecore:objectextensiondata:gift_not_disallowed", data.GiftedToNotOnAllowListMessage));
                    }
                }
            }
        }

        private static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            SetupTextureOverrides();
            SetupRecipes();
        }

        private static void Content_AssetInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            //Console.WriteLine("meow:" + string.Concat(e.NamesWithoutLocale.Select(an => an.ToString())));
            if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo("spacechase0.SpaceCore/TextureOverrides")))
            {
                //Console.WriteLine("meow! 1");
                SetupTextureOverrides();
            }
            if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo("spacechase0.SpaceCore/CraftingRecipeOverrides")) ||
                e.NamesWithoutLocale.Any(an => an.IsEquivalentTo("spacechase0.SpaceCore/CookingRecipeOverrides")))
            {
                //Console.WriteLine("meow! 2");
                SetupRecipes();
            }
        }

        private static void SetupRecipes()
        {
            Dictionary<string, VAECraftingRecipe> newCrafting = Game1.content.Load<Dictionary<string, VAECraftingRecipe>>("spacechase0.SpaceCore/CraftingRecipeOverrides");
            Dictionary<string, VAECraftingRecipe> newCooking = Game1.content.Load<Dictionary<string, VAECraftingRecipe>>("spacechase0.SpaceCore/CookingRecipeOverrides");

            CustomCraftingRecipe.CraftingRecipes.RemoveWhere( r => craftingRecipes.Values.Contains( r.Value ) );
            CustomCraftingRecipe.CookingRecipes.RemoveWhere( r => cookingRecipes.Values.Contains( r.Value ) );
            craftingRecipes.Clear();
            cookingRecipes.Clear();

            foreach ( var recipe in newCrafting )
            {
                var ccr = new VAECustomCraftingRecipe(false, recipe.Key, recipe.Value);
                craftingRecipes.Add(recipe.Key, ccr);
                CustomCraftingRecipe.CraftingRecipes.Add(recipe.Key, ccr);
            }
            foreach ( var recipe in newCooking )
            {
                var ccr = new VAECustomCraftingRecipe(true, recipe.Key, recipe.Value);
                cookingRecipes.Add(recipe.Key, ccr);
                CustomCraftingRecipe.CookingRecipes.Add(recipe.Key, ccr);
            }
        }

        private static void SetupTextureOverrides()
        {
            if (texs == null)
                texs = new();
            Dictionary<string, TextureOverridePackData> newTexs = Game1.content.Load<Dictionary<string, TextureOverridePackData>>("spacechase0.SpaceCore/TextureOverrides");

            {
                var existingOverrides = newTexs.Where(kvp => texs.ContainsKey(kvp.Key));

                foreach (var newTex in newTexs)
                {
                    if (existingOverrides.Contains(newTex))
                    {
                        if (texs[newTex.Key].GetHashCode() == newTex.Value.GetHashCode())
                        {
                            newTex.Value.currFrame = texs[newTex.Key].currFrame;
                            newTex.Value.currFrameTick = texs[newTex.Key].currFrameTick;
                            newTex.Value.sourceTex = texs[newTex.Key].sourceTex;
                            newTex.Value.sourceRectCache = texs[newTex.Key].sourceRectCache;
                        }
                    }
                    else
                    {
                        //Texture2D targetTex = Game1.content.Load<Texture2D>(newTex.Value.TargetTexture);
                        Texture2D sourceTex = Game1.content.Load<Texture2D>(newTex.Value.animation.Frames[0].FilePath);
                        int ind = newTex.Value.animation.Frames[0].SpriteIndex;
                        int x = (ind * newTex.Value.TargetRect.Width) % sourceTex.Width;
                        int y = (ind * newTex.Value.TargetRect.Width) / sourceTex.Width * newTex.Value.TargetRect.Height;

                        newTex.Value.sourceTex = sourceTex;
                        newTex.Value.sourceRectCache = new Rectangle(x, y, newTex.Value.TargetRect.Width, newTex.Value.TargetRect.Height);
                    }
                }

                texs = newTexs;

                SpriteBatchPatcher.packOverrides.Clear();
                foreach (var tex in texs)
                {
                    if (!SpriteBatchPatcher.packOverrides.ContainsKey(tex.Value.TargetTexture))
                        SpriteBatchPatcher.packOverrides.Add(tex.Value.TargetTexture, new());
                    SpriteBatchPatcher.packOverrides[tex.Value.TargetTexture].Add(tex.Value.TargetRect, tex.Value);
                }
            }
        }

        private static void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            foreach (var kvp in texs)
            {
                var texOverride = kvp.Value;
                if (++texOverride.currFrameTick >= texOverride.animation.Frames[texOverride.currFrame].Duration)
                {
                    texOverride.currFrameTick = 0;
                    if (++texOverride.currFrame >= texOverride.animation.Frames.Length)
                    {
                        texOverride.currFrame = 0;
                    }

                    //Texture2D targetTex = Game1.content.Load<Texture2D>(kvp.Value.TargetTexture);
                    Texture2D sourceTex = Game1.content.Load<Texture2D>(kvp.Value.animation.Frames[texOverride.currFrame].FilePath);
                    int ind = kvp.Value.animation.Frames[texOverride.currFrame].SpriteIndex;
                    int x = (ind * kvp.Value.TargetRect.Width) % sourceTex.Width;
                    int y = (ind * kvp.Value.TargetRect.Width) / sourceTex.Width * kvp.Value.TargetRect.Height;

                    kvp.Value.sourceTex = sourceTex;
                    kvp.Value.sourceRectCache = new Rectangle(x, y, kvp.Value.TargetRect.Width, kvp.Value.TargetRect.Height);
                }
            }
        }

        private static void SpaceEvents_OnItemEaten(object sender, EventArgs e)
        {
            var farmer = sender as Farmer;
            if (farmer != Game1.player)
                return;

            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            TriggerActionManager.Raise("spacechase0.SpaceCore_OnItemEaten", location: Game1.player.currentLocation, player: Game1.player, inputItem: Game1.player.itemToEat);
        }

        private static void SpaceEvents_AfterGiftGiven(object sender, EventArgsGiftGiven e)
        {
            var farmer = sender as Farmer;
            if (farmer != Game1.player) return;

            var dict = Game1.content.Load<Dictionary<string, NpcExtensionData>>("spacechase0.SpaceCore/NpcExtensionData");
            if (!dict.TryGetValue(e.Npc.Name, out var npcEntry))
                return;

            if (!npcEntry.GiftEventTriggers.TryGetValue(e.Gift.ItemId, out string eventStr))
                return;

            string[] data = eventStr.Split('/');
            string eid = data[0];

            Game1.PlayEvent(eid, checkPreconditions: false);
        }

        private static void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/ObjectExtensionData"))
                e.LoadFrom(() => new Dictionary<string, ObjectExtensionData>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/CropExtensionData"))
                e.LoadFrom(() => new Dictionary<string, CropExtensionData>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/FurnitureExtensionData"))
                e.LoadFrom(() => new Dictionary<string, FurnitureExtensionData>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/NpcExtensionData"))
                e.LoadFrom(() => new Dictionary<string, NpcExtensionData>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/ShopExtensionData"))
                e.LoadFrom(() => new Dictionary<string, ShopExtensionData>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/FarmExtensionData"))
                e.LoadFrom(() => new Dictionary<string, FarmExtensionData>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/TextureOverrides"))
                e.LoadFrom(() => new Dictionary<string, TextureOverridePackData>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/CraftingRecipeOverrides"))
                e.LoadFrom(() => new Dictionary<string, VAECraftingRecipe>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/CookingRecipeOverrides"))
                e.LoadFrom(() => new Dictionary<string, VAECraftingRecipe>(), AssetLoadPriority.Low);
        }
    }
}
