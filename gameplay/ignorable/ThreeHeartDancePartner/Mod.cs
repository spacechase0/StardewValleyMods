using System;
using System.Collections.Generic;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using ThreeHeartDancePartner.Framework;

namespace ThreeHeartDancePartner
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Log.Monitor = this.Monitor;

            this.Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => this.Config = new ModConfig(),
                    save: () => this.Helper.WriteConfig(this.Config)
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_HeartsNeeded_Name,
                    tooltip: I18n.Config_HeartsNeeded_Desc,
                    getValue: () => this.Config.RequiredHearts,
                    setValue: value => this.Config.RequiredHearts = value,
                    min: 0,
                    max: 14
                );
            }
        }

        /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // get dialog box
            if (e.NewMenu is not DialogueBox dialogBox)
                return;

            // get festival
            Event festival = Game1.currentLocation?.currentEvent;
            if (Game1.currentLocation?.Name != "Temp" || festival?.FestivalName != "Flower Dance")
                return;

            // check if rejection dialogue
            Dialogue dialog = dialogBox.characterDialogue;
            if(dialog == null || dialog.speaker == null) // I got a nullPointerException becuase of this at the end of the flower dance.
            {
                return;
            }
            NPC npc = dialog.speaker;
            if (!npc.datable.Value || npc.HasPartnerForDance)
                return;

            string rejectionText = new Dialogue(dialog.speaker, null, Game1.content.Load<Dictionary<string, string>>($"Characters\\Dialogue\\{dialog.speaker.Name}")["FlowerDance_Decline"]).getCurrentDialogue();
            if (dialog.getCurrentDialogue() != rejectionText)
                return;

            // replace with accept dialog
            // The original stuff, only the relationship point check is modified
            if (!npc.HasPartnerForDance && Game1.player.getFriendshipLevelForNPC(npc.Name) >= this.Config.RequiredHearts * NPC.friendshipPointsPerHeartLevel)
            {
                string s;
                if (npc.isMarried())
                {
                    s = Game1.content.Load<Dictionary<string, string>>($"Characters\\Dialogue\\{dialog.speaker.Name}")["FlowerDance_Accept_Spouse"];
                }else if (npc.isRoommate())
                {
                    s = Game1.content.Load<Dictionary<string, string>>($"Characters\\Dialogue\\{dialog.speaker.Name}")["FlowerDance_Accept_Roommate"];
                }
                else
                {
                    s = Game1.content.Load<Dictionary<string, string>>($"Characters\\Dialogue\\{dialog.speaker.Name}")["FlowerDance_Accept"];
                }
                

                try
                {
                    Game1.player.changeFriendship(250, Game1.getCharacterFromName(npc.Name));
                }
                catch (Exception)
                {
                }
                Game1.player.dancePartner.Value = npc;
                npc.setNewDialogue(new Dialogue(npc, null, s));

                foreach (NPC actor in festival.actors)
                {
                    if (actor.CurrentDialogue?.Count > 0 && actor.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
                        actor.CurrentDialogue.Clear();
                }

                // Okay, looks like I need to fix the current dialog box
                Game1.activeClickableMenu = new DialogueBox(new Dialogue(npc, null, s) { removeOnNextMove = false });
            }
        }
    }
}
