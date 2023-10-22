using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using HarmonyLib;
using MageDelve.Mercenaries.Actions;
using MageDelve.Mercenaries.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Toolkit.Serialization;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.GameData.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using xTile;

namespace MageDelve.Mercenaries
{
    public class MercenaryEngine
    {
        public static ConditionalWeakTable<Farmer, List<Vector2>> trails = new();

        public static int TrailingDistance = 10;

        private bool timeJustChanged = false;

        public MercenaryEngine()
        {
            Mod.instance.Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            Mod.instance.Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Mod.instance.Helper.Events.Player.Warped += this.Player_Warped;
            Mod.instance.Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            Mod.instance.Helper.Events.Multiplayer.PeerDisconnected += this.Multiplayer_PeerDisconnected;
            Mod.instance.Helper.Events.GameLoop.DayEnding += this.GameLoop_DayEnding;
            Mod.instance.Helper.Events.GameLoop.TimeChanged += this.GameLoop_TimeChanged;
            Mod.instance.Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;

            // TODO: Cache this somewhere
            object score = AccessTools.Property("StardewModdingAPI.Framework.SCore:Instance").GetValue(null);
            var tk = score.GetType().GetField("Toolkit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(score) as ModToolkit;
            var js = tk.JsonHelper.GetSerializer();

            EffectData.EffectTypes.Add("TemporarySprite", (character, effectData) =>
            {
                var effectParams = effectData.Parameters.ToObject<TemporarySpriteEffectParameters>(js);

                TemporaryAnimatedSprite tas = new(effectParams.TileSheet, effectParams.SourceRect, effectParams.FrameLength * 1000, effectParams.FrameCount, effectParams.Loops, effectParams.Offset, false, false);
                if (effectParams.AttachToCharacter)
                {
                    tas.positionFollowsAttachedCharacter = true;
                    tas.attachedCharacter = character;
                    tas.layerDepth = ((character.Position.Y + 32f) / 10000f) + effectParams.LayerModifier;
                }
                else
                {
                    tas.Position += character.Position;
                    tas.layerDepth = ((tas.Position.Y + 32f) / 10000f) + effectParams.LayerModifier;
                }

                JsonConvert.PopulateObject(effectParams.TAS.ToString(), tas, tk.JsonHelper.JsonSettings);

                character.currentLocation.TemporarySprites.Add(tas);
            });
            EffectData.EffectTypes.Add("LocalSound", (character, effectData) =>
            {
                var effectParams = effectData.Parameters.ToObject<LocalSoundEffectParameters>(js);

                character.currentLocation.playSound(effectParams.SoundId, character.TilePoint.ToVector2(), effectParams.PitchOverride);
            });
            EffectData.EffectTypes.Add("Damage", (character, effectData) =>
            {
                var effectParams = effectData.Parameters.ToObject<DamageEffectParameters>(js);

                if (character is Farmer farmer)
                {
                    if (effectParams.Type == "heal")
                        farmer.health = Math.Min(farmer.maxHealth, farmer.health + effectParams.Amount);
                    else
                        farmer.takeDamage( effectParams.Amount, false, null); // TODO: Elemental damage when imported from combat mod
                }
                else
                {
                    // TODO - monsters and mercs
                }
            });
            EffectData.EffectTypes.Add("MercenaryAnimationOverride", (character, effectData) =>
            {
                var effectParams = effectData.Parameters.ToObject<MercenaryAnimationOverrideEffectParameters>(js);

                if (character is Mercenary merc)
                {
                    merc.ApplyAnimation(effectParams.TileSheet, effectParams.StartFrame, effectParams.FrameLength, effectParams.FrameCount, effectParams.Loops);
                }
            });

            MercenaryActionData.ActionTypes.Add("MeleeAttackNearestMonster", (merc, actionData) =>
            {
                var actionParams = actionData.Parameters.ToObject<MeleeAttackMercenaryActionParameters>(js);

                if (merc.targeting != null)
                {
                    if (Vector2.Distance(merc.targeting.Position, Game1.player.Position) > actionParams.MinIgnoreRadius * Game1.tileSize)
                        merc.targeting = null;
                }

                if (merc.targeting == null)
                {
                    List<Monster> targets = new List<Monster>();
                    foreach (var target in Game1.player.currentLocation.characters)
                    {
                        if (target is Monster monster)
                        {
                            if (Vector2.Distance(monster.Position, Game1.player.Position) > actionParams.MaxEngagementRadius * Game1.tileSize)
                                continue;

                            if (actionParams.MonsterAllowList != null &&
                                 !actionParams.MonsterAllowList.Contains(monster.Name) &&
                                 !actionParams.MonsterAllowList.Contains(monster.GetType().FullName))
                                continue;

                            if (actionParams.MonsterBlockList != null &&
                                 (actionParams.MonsterBlockList.Contains(monster.Name) ||
                                   actionParams.MonsterBlockList.Contains(monster.GetType().FullName)))
                                continue;

                            targets.Add(monster);
                        }
                    }

                    if (targets.Count == 0)
                        return false;

                    targets.Sort((a, b) => Math.Sign(Vector2.DistanceSquared(a.Position, merc.Position) - (int)Vector2.DistanceSquared(b.Position, merc.Position)));
                    for (int i = 0; i < targets.Count; ++i)
                    {
                        merc.targeting = targets[i];
                        merc.untilNextPathfind = 0;
                        if (merc.DoPathfind(Game1.player.currentLocation))
                            break;
                    }
                }

                if (merc.attackId != actionData.Id)
                {
                    merc.attackId = actionData.Id;
                    merc.attackWeapon = new StardewValley.Tools.MeleeWeapon(actionParams.MeleeWeaponId);
                    foreach (string enchTypeName in actionParams.WeaponEnchantments)
                    {
                        var enchType = AccessTools.TypeByName(enchTypeName);
                        var ench = (BaseEnchantment) enchType.GetConstructor(new Type[0]).Invoke( new object[ 0 ] );
                        merc.attackWeapon.AddEnchantment(ench);
                    }
                    foreach (var data in actionParams.WeaponModData)
                    {
                        merc.attackWeapon.modData.Add(data.Key, data.Value);
                    }
                    merc.showWeapon = actionParams.ShowWeapon;

                    merc.dummy.CurrentTool = merc.attackWeapon;
                    merc.dummy.enchantments.Clear();
                    merc.dummy.ReequipEnchantments();
                }

                // TODO: Redo once weapon changes are in place
                merc.swingTime = (400 - merc.attackWeapon.speed.Value * 40) /*/ (merc.attackWeapon.type.Value == 2 ? 5 : 8)*/ / 1000f * 1.75f;

                return merc.targeting != null;
            });
            MercenaryActionData.ActionTypes.Add("SingleTargetEffect", (merc, actionData) =>
            {
                var actionParams = actionData.Parameters.ToObject<SingleTargetEffectMercenaryActionParameters>(js);

                bool CheckConditions( Character target )
                {
                    int healthMissing = 0;
                    float withinTiles = Vector2.Distance(merc.Position, target.Position) / Game1.tileSize;
                    if (target is Farmer farmer)
                    {
                        healthMissing = farmer.maxHealth - farmer.health;
                    }
                    else if (target is Monster monster)
                    {
                        healthMissing = monster.MaxHealth - monster.Health;
                    }

                    if (healthMissing < actionParams.MinimumHealthMissing)
                        return false;
                    if (withinTiles > actionParams.WithinTiles)
                        return false;

                    return true;
                }

                List<Character> targets = new();
                switch ( actionParams.Target )
                {
                    case SingleTargetEffectMercenaryActionParameters.TargetType.Farmer:
                        if (CheckConditions(Game1.player))
                            targets.Add(Game1.player);
                        break;

                    case SingleTargetEffectMercenaryActionParameters.TargetType.Party:
                        foreach (var player in Game1.player.currentLocation.farmers.ToList())
                        {
                            if (CheckConditions(player))
                                targets.Add(player);

                            foreach (var targetMerc in player.GetCurrentMercenaries().ToList())
                            {
                                if ( CheckConditions( targetMerc ) )
                                    targets.Add(targetMerc);
                            }
                        }
                        break;

                    case SingleTargetEffectMercenaryActionParameters.TargetType.Monster:
                        foreach (var character in Game1.player.currentLocation.characters)
                        {
                            if (character is Monster monster && CheckConditions(monster))
                                targets.Add(monster);
                        }
                        break;
                }


                if (targets.Count <= 0)
                    return false;
                targets.Shuffle();
                Character target = targets[0];

                foreach (var effect in actionParams.CasterEffects)
                    merc.ApplyEffect(effect);
                foreach (var effect in actionParams.TargetEffects)
                    target.ApplyEffect(effect);

                return true;
            });
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.MageDelve/Mercenaries"))
            {
                e.LoadFrom(() => new Dictionary<string, MercenaryData>(), AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                e.Edit((asset) =>
                {
                    var dict = asset.AsDictionary<string, LocationData>().Data;
                    dict.Add("Custom_MageDelve_MercenaryWaitingArea", new()
                    {
                        CreateOnLoad = new()
                        {
                            MapPath = "Maps/MageDelve_WaitingArea",
                            AlwaysActive = true,
                        },
                        ExcludeFromNpcPathfinding = true,
                    });
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps/MageDelve_WaitingArea"))
            {
                e.LoadFromModFile<Map>("assets/waitingarea.tmx", AssetLoadPriority.Low);
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = Mod.instance.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType( typeof( Mercenary ) );

            var asi = Mod.instance.Helper.ModRegistry.GetApi<IAdvancedSocialInteractionsApi>("spacechase0.AdvancedSocialInteractions");
            asi.AdvancedInteractionStarted += this.Asi_AdvancedInteractionStarted;
        }

        private void Asi_AdvancedInteractionStarted(object sender, Action<string, Action> e)
        {
            var npc = sender as NPC;
            var mercData = Game1.content.Load<Dictionary<string, MercenaryData>>("spacechase0.MageDelve/Mercenaries");

            if (npc.IsAlreadyMercenary() || !mercData.ContainsKey(npc.Name))
                return;

            var data = mercData[npc.Name];

            if (!GameStateQuery.CheckConditions(data.CanRecruit, player: Game1.player))
                return;

            e(data.RecruitCost == 0 ? I18n.RecruitFree() : I18n.RecruitCost(data.RecruitCost), () =>
            {
                npc.ClearSchedule();
                npc.currentLocation.characters.Remove(npc);
                npc.IsInvisible = true;
                Game1.getLocationFromName("Custom_MageDelve_MercenaryWaitingArea").characters.Add(npc);

                Game1.player.GetCurrentMercenaries().Add(new Mercenary(npc.Name, npc.Position));

                npc.Position = Vector2.Zero;
            });
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
            if (!Context.IsWorldReady && !Game1.HostPaused)
                return;
            var mercData = Game1.content.Load<Dictionary<string, MercenaryData>>("spacechase0.MageDelve/Mercenaries");

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
        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if ( e.Button.IsActionButton() )
            {
                foreach (var player in Game1.getOnlineFarmers())
                {
                    if (player.currentLocation != Game1.currentLocation)
                        continue;

                    foreach (var merc in player.GetCurrentMercenaries().ToList())
                    {
                        if (merc.GetBoundingBox().Contains( e.Cursor.AbsolutePixels ))
                        {
                            if (player == Game1.player && Mod.Config.MercenaryInteractModifier.IsDown() )
                            {
                                List<Response> responses = new();
                                if ( merc.GetMercenaryData().CurrentDialogueString != null )
                                    responses.Add(new("Talk", I18n.Mercenary_Interact_Talk()));
                                responses.Add(new("Dismiss", I18n.Mercenary_Interact_Dismiss()));
                                responses.Add(new("Cancel", I18n.Mercenary_Interact_Cancel()));

                                Game1.currentLocation.afterQuestion = (farmer, answer) =>
                                {
                                    Game1.activeClickableMenu = null;
                                    Game1.player.CanMove = true;

                                    if (answer == "Talk")
                                    {
                                        var mercNpc = Game1.getCharacterFromName(merc.CorrespondingNpc);
                                        Game1.activeClickableMenu = new DialogueBox(new Dialogue(mercNpc, null, merc.GetMercenaryData().CurrentDialogueString));
                                        Game1.player.Halt();
                                        Game1.player.canMove = false;
                                        Game1.currentSpeaker = mercNpc;
                                    }
                                    else if ( answer == "Dismiss" )
                                    {
                                        merc.OnLeave(); // TODO: This should probably send a message in MP to do it on the host...
                                        farmer.GetCurrentMercenaries().Remove(merc);
                                    }
                                };
                                Game1.currentLocation.createQuestionDialogue(Game1.getCharacterFromName(merc.CorrespondingNpc).displayName, responses.ToArray(), "mercenary-interact");
                            }
                            else
                            {
                                Game1.getCharacterFromName(merc.CorrespondingNpc).checkAction(Game1.player, Game1.player.currentLocation);
                            }
                            return;
                        }
                    }
                }
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
