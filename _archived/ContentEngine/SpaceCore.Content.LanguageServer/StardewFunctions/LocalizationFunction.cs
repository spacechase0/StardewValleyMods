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

        return new Token()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Value = $"<localized {fcall.Parameters[0].SimplifyToToken(ce).Value}>",
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
