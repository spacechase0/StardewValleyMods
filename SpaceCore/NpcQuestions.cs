using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Netcode;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace SpaceCore
{
    public static class QuestionsAsked
    {
        internal class Holder { public readonly NetStringList Value = new(); }

        internal static ConditionalWeakTable<Friendship, Holder> values = new();

        public static void set_questionsAsked(this Friendship friendship, NetStringList newVal)
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetStringList get_questionsAsked(this Friendship friendship)
        {
            var holder = values.GetOrCreateValue(friendship);
            return holder.Value;
        }
    }
    public static class AskedQuestionToday
    {
        internal class Holder { public readonly NetBool Value = new(); }

        internal static ConditionalWeakTable<Friendship, Holder> values = new();

        public static void set_askedQuestionToday(this Friendship friendship, NetBool newVal)
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetBool get_askedQuestionToday(this Friendship friendship)
        {
            var holder = values.GetOrCreateValue(friendship);
            return holder.Value;
        }
    }

    [HarmonyPatch(typeof(Friendship), MethodType.Constructor)]
    public static class FriendshipConstructorNetFieldInjection
    {
        public static void Postfix(Friendship __instance)
        {
            __instance.NetFields.AddField(__instance.get_questionsAsked())
                .AddField(__instance.get_askedQuestionToday());

        }
    }

    public class QuestionContentModel
    {
        public string ID { get; set; }
        public float Weight { get; set; } = 1;
        public string QuestionText { get; set; }
        public string AnswerText { get; set; }
        public bool CanRepeatQuestion { get; set; } = false;
        public int FriendshipModifier { get; set; } = 10;
        public string Condition { get; set; }
    }
    internal class QuestionsAskedToken
    {
        private Dictionary<string, List<string>> values = new();
        private int oldCheck = 0;

        public bool AllowsInput()
        {
            return true;
        }

        public bool RequiresInput()
        {
            return true;
        }

        public bool UpdateContext()
        {
            int check = 0;
            values = new();
            foreach (string name in Game1.player.friendshipData.Keys)
            {
                values.Add(name, Game1.player.friendshipData[name].get_questionsAsked().ToList());
                values[name].ForEach((s) => check ^= (name + "." + s).GetHashCode());
            }

            int oldoldcheck = oldCheck;
            oldCheck = check;
            return check != oldoldcheck;
        }

        public bool IsReady()
        {
            return Context.IsWorldReady;
        }

        /*
        public bool TryValidateInput(string input, out string error)
        {
            Log.Debug(input + "MEOW");
            for (int i = 0; i < Game1.locations.Count; i++)
            {
                if (Game1.locations[i] is MovieTheater)
                {
                    continue;
                }
                foreach (NPC ch in Game1.locations[i].getCharacters())
                {
                    if (!ch.eventActor && ch.isVillager())
                    {
                        Log.Debug(ch.Name+"meow");
                        if (ch.Name == input.Trim())
                        {
                            error = null;
                            return true;
                        }
                    }
                }
            }

            error = "Failed to find NPC for \"" + input + "\"";
            return false;
        }
        */

        public IEnumerable<string> GetValues(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || !values.ContainsKey(input.Trim()))
                yield break;

            foreach (string entry in values[input.Trim()])
                yield return entry;
        }
    }

    public class NpcQuestions
    {
        public static NpcQuestions instance;
        private IModHelper Helper;
        private IManifest ModManifest;

        public void Entry(IManifest manifest, IModHelper helper)
        {
            instance = this;
            Helper = helper;
            ModManifest = manifest;

            Helper.ConsoleCommands.Add("npc_getquestionsasked", "List the (non-repeatable) questions asked to an NPC", OnGetQuestionsAskedCommand);
            Helper.ConsoleCommands.Add("npc_clearquestionsasked", "Clear the list of (non-repeatable) questions asked to an NPC", OnClearQuestionsAskedCommand);
            Helper.ConsoleCommands.Add("npc_resetaskedtoday", "Allow asking an NPC a question again", OnResetAskedToday);

            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
        }

        private void OnGetQuestionsAskedCommand(string cmd, string[] args)
        {
            if (args.Length == 0)
            {
                Log.Info("You need to provide an NPC name");
                return;
            }
            var npc = Game1.getCharacterFromName(args[0]);
            if (npc == null || !Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship friendship))
            {
                Log.Info("Failed to find NPC (did you meet them yet?)");
                return;
            }

            Log.Info("Questions asked: " + string.Join(", ", friendship.get_questionsAsked()));
        }

        private void OnClearQuestionsAskedCommand(string cmd, string[] args)
        {
            if (args.Length == 0)
            {
                Log.Info("You need to provide an NPC name");
                return;
            }
            var npc = Game1.getCharacterFromName(args[0]);
            if (npc == null || !Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship friendship))
            {
                Log.Info("Failed to find NPC (did you meet them yet?)");
                return;
            }

            friendship.get_questionsAsked().Clear();
        }

        private void OnResetAskedToday(string cmd, string[] args)
        {
            if (args.Length == 0)
            {
                Log.Info("You need to provide an NPC name");
                return;
            }
            var npc = Game1.getCharacterFromName(args[0]);
            if (npc == null || !Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship friendship))
            {
                Log.Info("Failed to find NPC (did you meet them yet?)");
                return;
            }

            friendship.get_askedQuestionToday().Value = false;
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/Questions"))
            {
                Dictionary<string, List<QuestionContentModel>> dict = new();
                for (int i = 0; i < Game1.locations.Count; i++)
                {
                    if (Game1.locations[i] is MovieTheater)
                    {
                        continue;
                    }
                    foreach (NPC ch in Game1.locations[i].characters)
                    {
                        if (ch.IsVillager)
                        {
                            if (!dict.ContainsKey(ch.Name))
                                dict.Add(ch.Name, new());
                        }
                    }
                }

                e.LoadFrom(() => dict, StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<IApi>("spacechase0.SpaceCore");
            sc.RegisterCustomProperty(typeof(Friendship), "questionsAsked", typeof(NetStringList), AccessTools.Method(typeof(QuestionsAsked), nameof(QuestionsAsked.get_questionsAsked)), AccessTools.Method(typeof(QuestionsAsked), nameof(QuestionsAsked.set_questionsAsked)));
            sc.AdvancedInteractionStarted += Asi_AdvancedInteractionStarted;

            var cp = Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");
            if (cp != null)
                cp.RegisterToken(ModManifest, "QuestionsAsked", new QuestionsAskedToken());
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            foreach (var friendship in Game1.player.friendshipData.Values)
            {
                friendship.get_askedQuestionToday().Value = false;
            }
        }

        private void Asi_AdvancedInteractionStarted(object sender, Action<string, Action> e)
        {
            var npc = (sender as NPC);
            var data = Game1.content.Load<Dictionary<string, List<QuestionContentModel>>>("spacechase0.SpaceCore/Questions");
            if (!data.ContainsKey(npc.Name) || !Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship friendship) || friendship.get_askedQuestionToday().Value)
                return;

            List<QuestionContentModel> qs = data[npc.Name].ToList();
            qs.RemoveAll((q) => friendship.get_questionsAsked().Contains(q.ID));
            qs.RemoveAll( q => q.Condition != null && !GameStateQuery.CheckConditions( q.Condition, location: npc.currentLocation, player: Game1.player ) );
            qs.Sort((q1, q2) => Math.Sign(q1.Weight - q2.Weight));
            float total = qs.Sum(q => q.Weight);

            Random r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + npc.Name.GetDeterministicHashCode());

            int qAmt = 4;
            List<Response> responses = new();
            Dictionary<string, QuestionContentModel> qsUsed = new();
            for (int i = 0; i < qAmt; ++i)
            {
                float num = (float)r.NextDouble() * total;
                for (int j = 0; j < qs.Count; ++j)
                {
                    num -= qs[j].Weight;
                    if (num <= 0)
                    {
                        total -= qs[j].Weight;
                        responses.Add(new(qs[j].ID, qs[j].QuestionText));
                        qsUsed.Add(qs[j].ID, qs[j]);
                        qs.RemoveAt(j);
                        break;
                    }
                }
            }
            responses.Add(new("cancel", "Cancel"));

            if (responses.Count > 1)
            {
                e(I18n.Question_Ask(), () =>
                {
                    // This needs to be delayed because the game immediately resets afterQuestion after it runs..
                    // and we're running the middle of one.
                    Game1.delayedActions.Add(new(0, () => Game1.currentLocation.afterQuestion = (farmer, answer) =>
                    {
                        Game1.activeClickableMenu = null;
                        if (answer == "cancel")
                        {
                            Game1.player.CanMove = true;
                        }
                        else
                        {
                            var q = qsUsed[answer];
                            if (!q.CanRepeatQuestion)
                                friendship.get_questionsAsked().Add(q.ID);
                            friendship.get_askedQuestionToday().Value = true;

                            Game1.player.changeFriendship(q.FriendshipModifier, npc);
                            var fakeNpc = new NPC(npc.Sprite, npc.Position, npc.DefaultMap, npc.FacingDirection,
                                npc.Name, npc.Portrait, eventActor: true);
                            Game1.activeClickableMenu = new DialogueBox(new Dialogue(fakeNpc, $"question.{q.ID}.answer", q.AnswerText));
                        };
                    }));
                    Game1.currentLocation.createQuestionDialogue(I18n.Question_Ask(), responses.ToArray(), "backstory-questions-framework");
                });
            }
        }
    }
}
