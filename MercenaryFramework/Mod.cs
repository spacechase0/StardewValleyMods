using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;
using xTile;

namespace MercenaryFramework
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public ConditionalWeakTable<Farmer, List<Vector2>> trails = new();

        public static int TrailingDistance = 10;

        private bool timeJustChanged = false;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Helper.Events.Specialized.LoadStageChanged += this.Specialized_LoadStageChanged;
            Helper.Events.Player.Warped += this.Player_Warped;
            Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            Helper.Events.Multiplayer.PeerDisconnected += this.Multiplayer_PeerDisconnected;
            Helper.Events.GameLoop.DayEnding += this.GameLoop_DayEnding;
            Helper.Events.GameLoop.TimeChanged += this.GameLoop_TimeChanged;

            Harmony harmony = new(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.MercenaryFramework/Mercenaries"))
            {
                e.LoadFrom(() => new Dictionary<string, MercenaryData>(), AssetLoadPriority.Low);
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType( typeof( Mercenary ) );

            var asi = Helper.ModRegistry.GetApi<IAdvancedSocialInteractionsApi>("spacechase0.AdvancedSocialInteractions");
            asi.AdvancedInteractionStarted += this.Asi_AdvancedInteractionStarted;
        }

        private void Asi_AdvancedInteractionStarted(object sender, Action<string, Action> e)
        {
            var npc = sender as NPC;
            var mercData = Game1.content.Load<Dictionary<string, MercenaryData>>("spacechase0.MercenaryFramework/Mercenaries");

            if (npc.IsAlreadyMercenary() || !mercData.ContainsKey( npc.Name ))
                return;

            var data = mercData[npc.Name];

            if (!GameStateQuery.CheckConditions(data.CanRecruit, player: Game1.player))
                return;

            e(data.RecruitCost == 0 ? I18n.RecruitFree() : I18n.RecruitCost( data.RecruitCost ), () =>
            {
                npc.ClearSchedule();
                npc.currentLocation.characters.Remove(npc);
                Game1.getLocationFromName("Custom_MercenaryWaitingArea").characters.Add(npc);

                Game1.player.GetCurrentMercenaries().Add( new Mercenary( npc.Name, npc.Position ) );

                npc.Position = Vector2.Zero;
            });
        }

        private void Specialized_LoadStageChanged(object sender, StardewModdingAPI.Events.LoadStageChangedEventArgs e)
        {
            if (e.NewStage == LoadStage.CreatedInitialLocations || e.NewStage == LoadStage.SaveAddedLocations)
            {
                var map = Helper.ModContent.Load<Map>("assets/waitingarea.tmx");
                Game1.locations.Add(new GameLocation(Helper.ModContent.GetInternalAssetName("assets/waitingarea.tmx").BaseName, "Custom_MercenaryWaitingArea")); 
            }
        }
        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (e.Player != Game1.player)
                return;

            foreach (var merc in e.Player.GetCurrentMercenaries())
            {
                merc.Position = e.Player.Position;
                trails.GetOrCreateValue(e.Player).Clear();
            }
        }

        private void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            timeJustChanged = true;
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            var mercData = Game1.content.Load<Dictionary<string, MercenaryData>>("spacechase0.MercenaryFramework/Mercenaries");

            foreach (var player in Game1.getOnlineFarmers())
            {
                int i = 0;
                List<int> toRemove = new();
                foreach (var merc in player.GetCurrentMercenaries())
                {
                    if (timeJustChanged)
                    {
                        if (player == Game1.player &&
                            !GameStateQuery.CheckConditions(mercData[merc.CorrespondingNpc].CanRecruit, player: player))
                        {
                            merc.OnLeave();
                            toRemove.Add(i);
                        }
                    }

                    merc.UpdateForFarmer(player, i, Game1.currentGameTime);
                    ++i;
                }

                foreach ( int index in toRemove )
                    player.GetCurrentMercenaries().RemoveAt(index);

                var trail = trails.GetOrCreateValue(player);
                if (trail.Count == 0 || trail[0] != player.Position)
                {
                    trail.Insert(0, player.Position);
                    if (trail.Count > player.GetCurrentMercenaries().Count * TrailingDistance)
                        trail.RemoveAt(trail.Count - 1);
                }
            }

            timeJustChanged = false;
        }

        private void Multiplayer_PeerDisconnected(object sender, PeerDisconnectedEventArgs e)
        {
            if (Game1.IsMasterGame)
            {
                var farmer = Game1.getFarmerMaybeOffline(e.Peer.PlayerID); // I don't know if they count as offline or not at this point
                foreach (var merc in farmer.GetCurrentMercenaries())
                {
                    merc.OnLeave();
                }
                farmer.GetCurrentMercenaries().Clear();
            }
        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;

            foreach (var player in Game1.getOnlineFarmers())
            {
                foreach (var merc in player.GetCurrentMercenaries())
                {
                    merc.OnLeave();
                }
                player.GetCurrentMercenaries().Clear();
            }
        }
    }

    [HarmonyPatch(typeof(GameLocation), "drawCharacters")]
    public static class GameLocationDrawMercsPatch
    {
        public static void Postfix(GameLocation __instance, SpriteBatch b)
        {
            if (__instance.shouldHideCharacters() || __instance.currentEvent != null)
                return;

            foreach (var farmer in __instance.farmers)
            {
                foreach (var merc in farmer.GetCurrentMercenaries())
                {
                    merc.draw(b);
                }
            }
        }
    }
}
