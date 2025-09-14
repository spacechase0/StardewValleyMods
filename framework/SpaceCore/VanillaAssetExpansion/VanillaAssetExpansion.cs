using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceCore.Events;
using SpaceCore.Interface;
using SpaceCore.Patches;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using StardewValley.Triggers;
using static System.Collections.Specialized.BitVector32;
using static SpaceCore.SpaceCore;

namespace SpaceCore.VanillaAssetExpansion
{
    public class WearableData
    {
        public string BuffIdToApply { get; set; }
    }

    public class TriggerActionExtensionData
    {
        public List<int> Times { get; set; }
    }

    internal class VanillaAssetExpansion
    {
        private static Dictionary<string, TextureOverridePackData> texs = new();
        private static Dictionary<string, CustomCraftingRecipe> craftingRecipes = new();
        private static Dictionary<string, CustomCraftingRecipe> cookingRecipes = new();
        internal static Dictionary<string, VirtualCurrencyData> virtualCurrencies = new();
        private static Dictionary<string, WearableData> wearables = new();

        private static bool manualTriggerActionsDirty = true;
        internal static Dictionary<string, CachedTriggerAction> manualTriggerActionsById = new();
        private static Dictionary<int, List<string>> timeTriggerActions = new();

        public static void Init()
        {
            SpaceCore.Instance.Helper.Events.Content.AssetRequested += Content_AssetRequested;
            SpaceCore.Instance.Helper.Events.Content.AssetsInvalidated += Content_AssetInvalidated;
            SpaceCore.Instance.Helper.Events.Content.LocaleChanged += GameLoop_LocaleChanged;
            SpaceCore.Instance.Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            SpaceCore.Instance.Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            SpaceCore.Instance.Helper.Events.GameLoop.Saving += GameLoop_Saving;
            SpaceCore.Instance.Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            SpaceCore.Instance.Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;

            SpaceEvents.BeforeGiftGiven += SpaceEvents_BeforeGiftGiven;
            SpaceEvents.AfterGiftGiven += SpaceEvents_AfterGiftGiven;
            SpaceEvents.OnItemEaten += SpaceEvents_OnItemEaten;

            TriggerActionManager.RegisterTrigger("spacechase0.SpaceCore_OnItemUsed");
            TriggerActionManager.RegisterTrigger("spacechase0.SpaceCore_OnItemEaten");

            SpaceCore.Instance.Helper.ConsoleCommands.Add("player_addvirtualcurrency", "...", (cmd, args) =>
            {
                var data = Game1.content.Load<Dictionary<string, VirtualCurrencyData>>("spacechase0.SpaceCore/VirtualCurrencyData");
                if (args.Length != 2 || !data.ContainsKey(args[0]) || !int.TryParse(args[1], out int amt))
                {
                    Log.Info("Invalid arguments");
                    return;
                }

                SpaceCore.api.AddToVirtualCurrency(Game1.player, args[0], amt);
            });

            SpaceCore.Instance.Helper.ConsoleCommands.Add("spacecore_getcurrencyid", "...", (cmd, args) =>
            {
                var data = Game1.content.Load<Dictionary<string, VirtualCurrencyData>>("spacechase0.SpaceCore/VirtualCurrencyData");
                if (args.Length != 1 || !data.ContainsKey(args[0]))
                {
                    Log.Info("Invalid argument");
                    return;
                }

                // We OR 0xFF to make sure vanilla always has room for future expansion, and we never accidentally use those
                Log.Info($"Use this for your shop currency ID: {args[0].GetDeterministicHashCode()}");
            });
        }

        private static void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (timeTriggerActions.TryGetValue(e.NewTime, out var triggerActions))
            {
                var manualContext = new TriggerActionContext("Manual", Array.Empty<object>(), null);
                foreach (string triggerId in triggerActions)
                {
                    if (!manualTriggerActionsById.TryGetValue(triggerId, out var trigger))
                    {
                        Log.Error($"Failed to find trigger action \"{triggerId}\" with \"Manual\" trigger type");
                        continue;
                    }

                    if (GameStateQuery.CheckConditions(trigger.Data.Condition) && (!trigger.Data.HostOnly || Game1.IsMasterGame))
                    {
                        foreach (var action in trigger.Actions)
                        {
                            if (!TriggerActionManager.TryRunAction(action, manualContext, out string error, out Exception e2))
                            {
                                Log.Error($"Trigger action {trigger.Data.Id} failed: {error} {e2}");
                            }
                        }

                        if (trigger.Data.MarkActionApplied)
                            Game1.player.triggerActionsRun.Add(trigger.Data.Id);
                    }
                }
            }
        }

        private static void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            if (!Game1.IsMasterGame) return;

            foreach (var currency in virtualCurrencies)
            {
                if (currency.Value.TeamWide)
                    SpaceCore.Instance.Helper.Data.WriteSaveData($"spacechase0.SpaceCore.VirtualCurrency.{currency.Key}", new Holder<int>() { Value = Game1.player.team.GetVirtualCurrencyAmount(currency.Key) } );
            }
        }

        private static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Game1.IsMasterGame) return;

            foreach (var currency in virtualCurrencies)
            {
                if (currency.Value.TeamWide)
                {
                    var data = SpaceCore.Instance.Helper.Data.ReadSaveData<Holder<int>>($"spacechase0.SpaceCore.VirtualCurrency.{currency.Key}");
                    if (data != null)
                        VirtualCurrencyExtensions.TeamCurrencies.GetOrCreateValue(Game1.player.team).Add(currency.Key, data.Value);
                }
            }
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
            SetupTriggerActionCache();
            SetupTextureOverrides();
            SetupRecipes();
            virtualCurrencies = Game1.content.Load<Dictionary<string, VirtualCurrencyData>>("spacechase0.SpaceCore/VirtualCurrencyData");
            wearables = Game1.content.Load<Dictionary<string, WearableData>>("spacechase0.SpaceCore/WearableData");
            SetupTimedTriggerActions();

            var sc = SpaceCore.Instance.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterCustomProperty(typeof(Farmer), "SpaceCore_PersonalCurrencies", typeof(NetStringDictionary<int, NetIntDelta>), AccessTools.Method(typeof(VirtualCurrencyExtensions), nameof(VirtualCurrencyExtensions.get_PersonalCurrencies)), AccessTools.Method(typeof(VirtualCurrencyExtensions), nameof(VirtualCurrencyExtensions.set_PersonalCurrencies)));
        }

        private static void GameLoop_LocaleChanged(object sender, LocaleChangedEventArgs e)
        {
            SetupTextureOverrides();
        }

        private static void Content_AssetInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            //Console.WriteLine("meow:" + string.Concat(e.NamesWithoutLocale.Select(an => an.ToString())));
            if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo("Data/TriggerActions")))
            {
                manualTriggerActionsDirty = true;
            }

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
            if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo("spacechase0.SpaceCore/VirtualCurrencyData")))
            {
                virtualCurrencies = Game1.content.Load<Dictionary<string, VirtualCurrencyData>>("spacechase0.SpaceCore/VirtualCurrencyData" );
            }
            if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo("spacechase0.SpaceCore/WearableData")))
            {
                wearables = Game1.content.Load<Dictionary<string, WearableData>>("spacechase0.SpaceCore/WearableData");
            }
            if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo("spacechase0.SpaceCore/TriggerActionExtensionData")))
            {
                SetupTimedTriggerActions();
            }
        }

        private static void SetupTriggerActionCache()
        {
            manualTriggerActionsDirty = false;
            manualTriggerActionsById.Clear();

            var manual = TriggerActionManager.GetActionsForTrigger("Manual");
            foreach (var entry in manual)
            {
                manualTriggerActionsById.Add(entry.Data.Id, entry);
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
                        int x = (ind * (newTex.Value.SourceSizeOverride?.X ?? newTex.Value.TargetRect.Width)) % sourceTex.Width;
                        int y = (ind * (newTex.Value.SourceSizeOverride?.X ?? newTex.Value.TargetRect.Width)) / sourceTex.Width * (newTex.Value.SourceSizeOverride?.Y ?? newTex.Value.TargetRect.Height);

                        newTex.Value.sourceTex = sourceTex;
                        newTex.Value.sourceRectCache = new Rectangle(x, y, newTex.Value.SourceSizeOverride?.X ?? newTex.Value.TargetRect.Width, newTex.Value.SourceSizeOverride?.Y ?? newTex.Value.TargetRect.Height);
                    }
                }

                texs = newTexs;

                SpriteBatchPatcher.packOverrides.Clear();
                string localeStr = !string.IsNullOrEmpty(Instance.Helper.GameContent.CurrentLocale) ? "." + Instance.Helper.GameContent.CurrentLocale : "";

                foreach (var tex in texs)
                {
                    string localeTex = tex.Value.TargetTexture + localeStr;
                    if (Instance.Helper.GameContent.DoesAssetExist<Texture2D>(Instance.Helper.GameContent.ParseAssetName(localeTex)))
                        tex.Value.TargetTexture = localeTex;
                    if (!SpriteBatchPatcher.packOverrides.ContainsKey(tex.Value.TargetTexture))
                        SpriteBatchPatcher.packOverrides.Add(tex.Value.TargetTexture, new());
                    SpriteBatchPatcher.packOverrides[tex.Value.TargetTexture].Add(tex.Value.TargetRect, tex.Value);
                }
            }
        }

        private static void SetupTimedTriggerActions()
        {
            var data = Game1.content.Load<Dictionary<string, TriggerActionExtensionData>>("spacechase0.SpaceCore/TriggerActionExtensionData");

            timeTriggerActions.Clear();

            foreach (var entry in data)
            {
                foreach (int time in entry.Value.Times)
                {
                    if (!timeTriggerActions.TryGetValue(time, out var triggerActions))
                        timeTriggerActions.Add(time, triggerActions = new());
                    triggerActions.Add(entry.Key);
                }
            }
        }

        private static void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (manualTriggerActionsDirty)
            {
                SetupTriggerActionCache();
            }

            if (Context.ScreenId == 0)
            {
                foreach (var kvp in texs)
                {
                    var texOverride = kvp.Value;
                    if (++texOverride.currFrameTick >= texOverride.animation.Frames[texOverride.currFrame].Duration)
                    {
                        texOverride.currFrameTick = 0;

                        double defaultAnimationChance = 1.0; // could make this a config option, but it may be clearer to always keep it 1.0
                        if (texOverride.currFrame == 0 && (texOverride.ChancePerTick ?? defaultAnimationChance) <= Game1.random.NextDouble())
                        {
                            continue;
                        }

                        if (++texOverride.currFrame >= texOverride.animation.Frames.Length)
                        {
                            texOverride.currFrame = 0;
                        }

                        //Texture2D targetTex = Game1.content.Load<Texture2D>(kvp.Value.TargetTexture);
                        Texture2D sourceTex = Game1.content.Load<Texture2D>(kvp.Value.animation.Frames[texOverride.currFrame].FilePath);
                        int ind = kvp.Value.animation.Frames[texOverride.currFrame].SpriteIndex;
                        int x = (ind * (kvp.Value.SourceSizeOverride?.X ?? kvp.Value.TargetRect.Width)) % sourceTex.Width;
                        int y = (ind * (kvp.Value.SourceSizeOverride?.X ?? kvp.Value.TargetRect.Width)) / sourceTex.Width * (kvp.Value.SourceSizeOverride?.Y ?? kvp.Value.TargetRect.Height);

                        kvp.Value.sourceTex = sourceTex;
                        kvp.Value.sourceRectCache = new Rectangle(x, y, kvp.Value.SourceSizeOverride?.X ?? kvp.Value.TargetRect.Width, kvp.Value.SourceSizeOverride?.Y ?? kvp.Value.TargetRect.Height);
                    }
                }
            }

            if (Game1.shouldTimePass())
            {
                List<Item> toCheck = Game1.player.GetEquippedItems().ToList();
                if (Game1.player.ActiveItem != null &&
                    !toCheck.Contains(Game1.player.CurrentItem)) // GetEquippedItems adds CurrentTool, which means it will already be there if it's a tool but not if it's a normal item
                    toCheck.Add(Game1.player.CurrentItem);
                foreach (var trinket in Game1.player.trinketItems)
                {
                    if (trinket != null)
                        toCheck.Add(trinket);
                }

                foreach (var item in toCheck.ToList())
                {
                    if (item is not Tool t)
                        continue;

                    foreach (var attached in t.attachments)
                    {
                        if (attached != null)
                            toCheck.Add(attached);
                    }
                }

                Queue<Ring> rings = new();
                foreach (var ring in toCheck.Where(i => i is Ring ring).Cast<Ring>().ToList())
                {
                    toCheck.Remove(ring);
                    rings.Enqueue(ring);
                }

                List<string> worn = new();
                foreach (var item in toCheck)
                    worn.Add(item.QualifiedItemId);

                while (rings.Count > 0)
                {
                    Ring r = rings.Dequeue();
                    if (r == null)
                        continue;

                    if (r is CombinedRing cr)
                    {
                        cr.combinedRings.ToList().ForEach(r2 => rings.Enqueue(r2));
                    }
                    else
                    {
                        worn.Add(r.QualifiedItemId);
                    }
                }

                Dictionary<string, List<Buff>> buffsBySource = new();
                foreach (var buff in Game1.player.buffs.AppliedBuffs)
                {
                    if (buff.Value.source == null)
                        continue;

                    if (!buffsBySource.ContainsKey(buff.Value.source))
                        buffsBySource.Add(buff.Value.source, new());
                    buffsBySource[buff.Value.source].Add(buff.Value);
                }

                List<Buff> buffsToUpdate = new();
                foreach (string wornId in worn)
                {
                    if (!wearables.ContainsKey(wornId))
                        continue;

                    string sourceId = $"wearable_{wornId}";
                    if (!buffsBySource.ContainsKey(sourceId))
                    {
                        Buff b = new Buff(wearables[wornId].BuffIdToApply, source: sourceId, displaySource: ItemRegistry.GetDataOrErrorItem(wornId).DisplayName, duration: 100);
                        Game1.player.applyBuff(b);
                        buffsBySource.Add(sourceId, [b]);
                    }
                    else
                        buffsToUpdate.AddRange(buffsBySource[sourceId]);
                }

                foreach (var buff in buffsToUpdate)
                {
                    buff.millisecondsDuration = 100;
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
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/ScheduleAnimationExtensionData"))
                e.LoadFrom(() => new Dictionary<string, ScheduleAnimationExtensionData>(), AssetLoadPriority.Low);
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
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/VirtualCurrencyData"))
                e.LoadFrom(() => new Dictionary<string, VirtualCurrencyData>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/WearableData"))
                e.LoadFrom(() => new Dictionary<string, WearableData>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/TriggerActionExtensionData"))
                e.LoadFrom(() => new Dictionary<string, TriggerActionExtensionData>(), AssetLoadPriority.Low);
        }
    }
}
