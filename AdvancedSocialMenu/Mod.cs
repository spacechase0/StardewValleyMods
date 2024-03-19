using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;

namespace AdvancedSocialInteractions
{
    public interface IApi
    {
        public event EventHandler<Action<string, Action>> AdvancedInteractionStarted;
    }

    public class Api : IApi
    {
        public event EventHandler<Action<string, Action>> AdvancedInteractionStarted;

        internal void Invoke(NPC npc, Action<string, Action> addCallback )
        {
            AdvancedInteractionStarted?.Invoke( npc, addCallback );
        }
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        internal Configuration Config { get; set; }
        private Api ApiInstance { get; set; }

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Log.Monitor = Monitor;
            Config = Helper.ReadConfig<Configuration>();
            ApiInstance = new();
            I18n.Init(Helper.Translation);

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
        }

        public override object GetApi()
        {
            return ApiInstance;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm != null)
            {
                gmcm.Register(ModManifest, () => Config = new(), () => Helper.WriteConfig(Config));
                gmcm.AddBoolOption(ModManifest, () => Config.AlwaysTrigger, (val) => Config.AlwaysTrigger = val, () => I18n.Config_AlwaysTrigger_Name(), () => I18n.Config_AlwaysTrigger_Description());
                gmcm.AddKeybindList(ModManifest, () => Config.TriggerModifier, (val) => Config.TriggerModifier = val, () => I18n.Config_TriggerModifier_Name(), () => I18n.Config_TriggerModifier_Description());
            }
        }

        internal NPC lastInteraction = null;
        internal Dictionary<string, Action> lastChoices = null;
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (e.Button.IsActionButton() && (Config.AlwaysTrigger || Config.TriggerModifier.IsDown()))
            {
                Rectangle tileRect = new Rectangle((int)e.Cursor.GrabTile.X * 64, (int)e.Cursor.GrabTile.Y * 64, 64, 64);
                NPC npc = null;
                foreach (var character in Game1.currentLocation.characters)
                {
                    if (!character.IsMonster && character.GetBoundingBox().Intersects(tileRect))
                    {
                        npc = character;
                        break;
                    }
                }
                if (npc == null)
                    npc = Game1.currentLocation.isCharacterAtTile(e.Cursor.Tile + new Vector2(0f, 1f));
                if (npc == null)
                    npc = Game1.currentLocation.isCharacterAtTile(e.Cursor.GrabTile + new Vector2(0f, 1f));

                if (npc == null || //!Utility.withinRadiusOfPlayer( npc.getStandingX(), npc.getStandingY(), 1, Game1.player ) ||
                    !Game1.NPCGiftTastes.ContainsKey( npc.Name ) || !Game1.player.friendshipData.ContainsKey( npc.Name ) )
                    return;
                Helper.Input.Suppress(e.Button);

                lastChoices = new();
                lastChoices.Add(I18n.Interaction_Chat(), () =>
                {
                    Log.Debug("wat?");
                    bool stowed = Game1.player.netItemStowed.Value;
                    Game1.player.netItemStowed.Value = true;
                    Game1.player.UpdateItemStow();
                    npc.checkAction(Game1.player, Game1.player.currentLocation);
                    Game1.player.netItemStowed.Value = stowed;
                    Game1.player.UpdateItemStow();
                });
                if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.canBeGivenAsGift())
                {
                    lastChoices.Add(I18n.Interaction_GiftHeld(), () => npc.tryToReceiveActiveObject(Game1.player));
                }
                ApiInstance.Invoke(npc, (s, a) => lastChoices.Add(s, a));

                List<Response> responses = new();
                foreach (var entry in lastChoices)
                {
                    responses.Add(new(entry.Key, entry.Key));
                }
                responses.Add(new("Cancel", I18n.Interaction_Cancel()));

                Game1.currentLocation.afterQuestion = (farmer, answer) =>
                {
                    //Log.Debug("hi");
                    Game1.activeClickableMenu = null;
                    Game1.player.CanMove = true;
                    if (lastChoices.ContainsKey(answer))
                        lastChoices[answer]();
                    else
                        ;// Log.Debug("wat");
                };
                Game1.currentLocation.createQuestionDialogue(I18n.InteractionWith(npc.displayName), responses.ToArray(), "advanced-social-interaction");
            }
        }
    }
}
