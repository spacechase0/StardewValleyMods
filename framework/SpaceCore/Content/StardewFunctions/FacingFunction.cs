using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceCore.Content.Functions;
using StardewValley;

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
                "up" => Game1.up.ToString(),
                "right" => Game1.right.ToString(),
                "down" => Game1.down.ToString(),
                "left" => Game1.left.ToString(),
                _ => Game1.down.ToString(),
            },
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
