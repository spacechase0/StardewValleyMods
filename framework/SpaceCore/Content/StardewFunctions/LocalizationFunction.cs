using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceCore.Content.Functions;

namespace SpaceCore.Content.StardewFunctions;
internal class LocalizationFunction : BaseFunction
{
    public LocalizationFunction()
    :   base( "%" )
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count != 1)
            return LogErrorAndGetToken($"I18n function % must have exactly one string parameter", fcall, ce);

        Dictionary<string, string> passthrough = new();
        foreach (var entry in fcall.Context.Contents)
        {
            var tok = entry.Value.SimplifyToToken(ce, allowLateResolve: true, require: false);
            if (tok != null)
                passthrough.Add(entry.Key.Value, tok.Value);
        }

        return new Token()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Value = (ce as PatchContentEngine).Helper.Translation.Get(fcall.Parameters[0].SimplifyToToken(ce).Value, passthrough),
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
