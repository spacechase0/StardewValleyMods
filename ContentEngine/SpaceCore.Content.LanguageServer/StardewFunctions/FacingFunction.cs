using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceCore.Content.Functions;

namespace SpaceCore.Content.StardewFunctions;
internal class FacingFunction : BaseFunction
{
    public FacingFunction()
    : base("Facing")
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count != 1)
            return LogErrorAndGetToken($"Facing function must have exactly one string parameters", fcall, ce);
        Token tok = fcall.Parameters[0].SimplifyToToken(ce);

        return new Token()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Value = tok.Value.ToLower() switch
            {
                "up" => 0.ToString(),
                "right" => 1.ToString(),
                "down" => 2.ToString(),
                "left" => 3.ToString(),
                _ => 2.ToString(),
            },
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
