using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Yarn;
using Yarn.Compiler;

namespace YarnEvents;
internal class YarnCustomEventScript : ICustomEventScript
{
    private Event evt;
    private EventContext ctx;
    private List<string> after = new();

    private ITranslationHelper translations;
    private Yarn.Dialogue Dialogue;
    private IDictionary<string, StringInfo> DefaultStrings;

    private string lastLine;
    private bool shouldContinue = false;
    private bool done = false;

    public YarnCustomEventScript(Event @event, EventContext context, string modId, string localPath)
    {
        after = @event.eventCommands.Skip(@event.CurrentCommand + 1).ToList();
        evt = @event;
        ctx = context;

        var modInfo = Mod.instance.Helper.ModRegistry.Get(modId);
        if (modInfo is null)
        {
            Log.Error($"Failed to find mod {modId}?");
            done = true;
            return;
        }

        string path = null;
        ISemanticVersion cpVer = Mod.instance.Helper.ModRegistry.Get("Pathoschild.ContentPatcher").Manifest.Version;
        if (modInfo.GetType().GetProperty("Mod")?.GetValue(modInfo) is IMod mod)
        {
            path = Path.Combine(mod.Helper.DirectoryPath, localPath);
            translations = mod.Helper.Translation;
        }
        else if (modInfo.GetType().GetProperty("ContentPack")?.GetValue(modInfo) is IContentPack pack)
        {
            path = Path.Combine(pack.DirectoryPath, localPath);
            translations = pack.Translation;
        }
        if (modInfo.Manifest.ContentPackFor?.UniqueID == "Pathoschild.ContentPatcher")
        {
            if (modInfo.Manifest.ContentPackFor.MinimumVersion != null)
                cpVer = modInfo.Manifest.ContentPackFor.MinimumVersion;
        }
        else if (modInfo.Manifest.Dependencies != null)
        {
            var dep = modInfo.Manifest.Dependencies.FirstOrDefault(d => d.UniqueID == "Pathoschild.ContentPatcher");
            if (dep.MinimumVersion != null)
                cpVer = dep.MinimumVersion;
        }

        Util.FetchFullPath(Mod.instance.Helper.ModRegistry, $"{modId}:{localPath}");
        if (!File.Exists(path))
        {
            Log.Error($"Failed to find '{localPath}' in {modId}?");
            done = true;
            return;
        }
        string text = File.ReadAllText(path);
        var result = Mod.CP.ParseTokenString(Mod.instance.ModManifest, text, cpVer);
        if (!result.IsValid)
        {
            Log.Error($"Failed substituting tokens into {modId}:{localPath}: {result.ValidationError}");
            done = true;
            return;
        }
        if (!result.IsReady)
        {
            Log.Error($"Failed substituting tokens into {modId}:{localPath}: Tokens not ready {result.ValidationError}");
            done = true;
            return;
        }
        text = result.Value;

        var job = CompilationJob.CreateFromString(localPath, text, Mod.YarnLibrary);
        var results = Compiler.Compile(job);
        foreach (var diag in results.Diagnostics)
        {
            switch (diag.Severity)
            {
                case Diagnostic.DiagnosticSeverity.Error:
                    Log.Error($"Error in yarn script {modId}:{localPath}: {diag}");
                    break;
                case Diagnostic.DiagnosticSeverity.Warning:
                    Log.Warn($"Error in yarn script {modId}:{localPath}: {diag}");
                    break;
                case Diagnostic.DiagnosticSeverity.Info:
                    Log.Info($"Error in yarn script {modId}:{localPath}: {diag}");
                    break;
            }
        }
        if (results.Program == null)
        {
            Log.Error($"Failed to compile yarn script {modId}:{localPath}");
            done = true;
            return;
        }

        DefaultStrings = results.StringTable;
        Dialogue = new(new MemoryVariableStore());
        Dialogue.LogDebugMessage = msg => Log.Debug($"{modId}:{localPath}: {msg}");
        Dialogue.LogErrorMessage = msg => Log.Error($"{modId}:{localPath}: {msg}");
        Dialogue.Library.ImportLibrary(Mod.YarnLibrary);
        Dialogue.SetProgram(results.Program);
        Dialogue.SetNode("main");
        Dialogue.NodeStartHandler = node => { };
        Dialogue.NodeCompleteHandler = node => { };
        Dialogue.LineHandler = HandleLine;
        Dialogue.OptionsHandler = HandleOptions;
        Dialogue.CommandHandler = HandleCommand;
        Dialogue.DialogueCompleteHandler = () => done = true;
        Dialogue.Continue();
    }

    private void HandleLine(Line line)
    {
        var t = translations.Get(line.ID);
        string str = t.HasValue() ? t.ToString() : DefaultStrings[line.ID].text;
        lastLine = Yarn.Dialogue.ExpandSubstitutions(str, line.Substitutions);

        if (DefaultStrings[line.ID].metadata.Contains("lastline"))
        {
            Continue();
        }
        else
        {
            evt.forked = true;
            evt.specialEventVariable1 = true;
            var cmds = new List<string>(["null", "yarnContinue"]);
            cmds.AddRange(after);
            evt.ReplaceAllCommands(cmds.ToArray());

            int colon = lastLine.IndexOf(':');
            if (colon == -1)
            {
                Game1.drawObjectDialogue(lastLine);
            }
            else
            {
                var actor = evt.getActorByName(lastLine.Substring(0, colon));
                actor.CurrentDialogue.Push(new StardewValley.Dialogue(actor, line.ID, lastLine.Substring(colon + 1)));
                Game1.drawDialogue(actor);
            }
        }
    }
    private void HandleOptions(OptionSet options)
    {
        if (!Game1.isQuestion && Game1.activeClickableMenu == null)
        {
            List<Response> answers = new();
            for (int i = 0; i < options.Options.Length; ++i)
            {
                var opt = options.Options[i];
                if (!opt.IsAvailable)
                    continue;

                var t = translations.Get(opt.Line.ID);
                string str = t.HasValue() ? t.ToString() : DefaultStrings[opt.Line.ID].text;
                string text = Yarn.Dialogue.ExpandSubstitutions(str, opt.Line.Substitutions);
                answers.Add(new Response(opt.ID.ToString(), text));
            }

            Game1.currentLocation.createQuestionDialogue(lastLine, answers.ToArray(), "yarnQuestion");
            Game1.currentLocation.afterQuestion = (_, answer) =>
            {
                Dialogue.SetSelectedOption(int.Parse(answer));
                Continue();
            };
            evt.forked = true;
            evt.specialEventVariable1 = true;
            var cmds = new List<string>(["null", "yarnContinue"]);
            cmds.AddRange(after);
            evt.ReplaceAllCommands(cmds.ToArray());
        }
    }
    private void HandleCommand(Command command)
    {
        if (command.Text.StartsWith("@/"))
        {
            evt.forked = true;
            evt.specialEventVariable1 = true;
            var cmds = new List<string>(command.Text.Substring(2).Split('/').Append("yarnContinue"));
            cmds.AddRange(after);
            evt.ReplaceAllCommands(cmds.ToArray());
        }
        else if (command.Text.StartsWith("@"))
        {
            evt.forked = true;
            evt.specialEventVariable1 = true;
            var cmds = new List<string>([command.Text.Substring(1), "yarnContinue"]);
            cmds.AddRange(after);
            evt.ReplaceAllCommands(cmds.ToArray());
        }
    }

    public void Continue()
    {
        shouldContinue = true;
    }

    public bool update(GameTime time, Event e)
    {
        if (shouldContinue)
        {
            shouldContinue = false;
            Dialogue.Continue();
        }
        return done;
    }

    public void draw(SpriteBatch b)
    {
        evt.currentCustomEventScript = null;
        evt.draw(b);
        evt.currentCustomEventScript = this;
    }

    public void drawAboveAlwaysFront(SpriteBatch b)
    {
    }
}
