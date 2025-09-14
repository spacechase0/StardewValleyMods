using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SpaceCore.Content.Functions;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace SpaceCore.Content.StardewFunctions;
internal class ContentPatcherTokenFunction : BaseFunction, IRefreshingFunction
{
    private class CPTokenHolder
    {
        public string LastString { get; set; }
        public IManagedTokenString LastTokenString { get; set; }
    }

    private Dictionary<string, CPTokenHolder> ElementTokens = new();

    public override bool IsLateResolver => true;

    public ContentPatcherTokenFunction()
    : base("CP")
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (ce is not PatchContentEngine pce)
            return null;

        if (pce.cp == null)
            return LogErrorAndGetToken("Content Patcher API missing?", fcall, ce);
        if (fcall.Parameters.Count != 1)
            return LogErrorAndGetToken($"CP function must have only 1 parameter", fcall, ce);

        var arg = fcall.Parameters[0];
        string argStr = arg.SimplifyToToken(ce).Value;
        if (!ElementTokens.TryGetValue(arg.Uid, out var managedTok))
            ElementTokens.Add(arg.Uid, managedTok = new());
        if (managedTok.LastString != argStr)
        {
            managedTok.LastString = argStr;
            managedTok.LastTokenString = pce.cp.ParseTokenString(pce.Manifest, argStr, pce.cpVersion);
        }
        managedTok.LastTokenString.UpdateContext();

        if (!managedTok.LastTokenString.IsValid)
            return LogErrorAndGetToken($"Invalid CP token string: {managedTok.LastTokenString.ValidationError}", fcall, ce);
        if (!managedTok.LastTokenString.IsReady)
        {
            return null;
        }

        return new Token()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Value = managedTok.LastTokenString.Value,
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }

    public bool WouldChangeFromRefresh(FuncCall fcall, ContentEngine ce)
    {
        var pce = ce as PatchContentEngine;

        var arg = fcall.Parameters[0];
        string argStr = arg.SimplifyToToken(pce).Value;
        if (!ElementTokens.TryGetValue(arg.Uid, out var managedTok))
            ElementTokens.Add(arg.Uid, managedTok = new());
        if (managedTok.LastString != argStr)
        {
            managedTok.LastString = argStr;
            managedTok.LastTokenString = pce.cp.ParseTokenString(pce.Manifest, argStr, pce.cpVersion);
            return true;
        }
        if (managedTok.LastTokenString.UpdateContext().Contains(Context.ScreenId))
            return true;

        return false;
    }
}
