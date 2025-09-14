using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceCore.Content.Functions;

namespace SpaceCore.Content.StardewFunctions;
internal class HasModFunction : BaseFunction
{
    public HasModFunction()
    :   base( "HasMod" )
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count != 1)
            return LogErrorAndGetToken($"HasMod function must have exactly one string parameters", fcall, ce);
        Token modTok = fcall.Parameters[0].SimplifyToToken(ce);

        return new Token()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Value = (ce as PatchContentEngine).Helper.ModRegistry.IsLoaded(modTok.Value) ? "true" : "false",
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
