using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SpaceCore.Content.Functions;

namespace SpaceCore.Content.StardewFunctions;
internal class ContentPatcherTokenFunction : BaseFunction, IRefreshingFunction
{
    public override bool IsLateResolver => true;

    public ContentPatcherTokenFunction()
    : base("CP")
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count != 1)
            return LogErrorAndGetToken($"CP function must have only 1 parameter", fcall, ce);

        return new Token()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Value = "<CP token value>",
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }

    public bool WouldChangeFromRefresh(FuncCall fcall, ContentEngine ce)
    {
        return true;
    }
}
