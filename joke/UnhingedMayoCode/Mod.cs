using System;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace UnhingedMayoJar
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            SpaceEvents.ActionActivated += this.SpaceEvents_ActionActivated;
        }

        private void SpaceEvents_ActionActivated(object sender, EventArgsAction e)
        {
            if (e.Action == "ClimbMayo")
            {
                Game1.currentLocation.createQuestionDialogue(I18n.ClimbQuestion(), new[] { new Response("Climb", I18n.Climb()), new Response("Leave", I18n.WalkAway()) }, "ClimbMayo");
                Game1.currentLocation.afterQuestion = (Farmer who, string whichAnswer) =>
                {
                    if (whichAnswer == "Climb")
                    {
                        Game1.warpFarmer("Forest", 71, 36, false);
                        Game1.locationRequest.OnWarp += () =>
                        {
                            Game1.player.changeIntoSwimsuit();
                            Game1.player.swimming.Value = true;
                            Game1.delayedActions.Add(new DelayedAction(1000)
                            {
                                behavior = () =>
                                {
                                    Game1.drawObjectDialogue(I18n.MayoCovering());
                                }
                            });
                            Game1.delayedActions.Add(new DelayedAction(10000)
                            {
                                behavior = () =>
                                {
                                    Game1.warpFarmer("Forest", 71, 47, false);
                                    Game1.locationRequest.OnWarp += () =>
                                    {
                                        Game1.player.changeOutOfSwimSuit();
                                        Game1.player.swimming.Value = false;
                                    };
                                }
                            });
                        };
                    }
                };
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Forest"))
            {
                e.Edit((asset) => asset.AsMap().PatchMap(Helper.ModContent.Load<xTile.Map>("assets/Forest-mayo.tmx"), new(67, 28, 12, 18), new(67, 28, 12, 18), PatchMapMode.Replace)); ;
            }
        }
    }
}
