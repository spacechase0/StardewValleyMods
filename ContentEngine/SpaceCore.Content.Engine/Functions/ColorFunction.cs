using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Content.Functions;
internal class ColorFunction : BaseFunction
{
    public ColorFunction()
    :   base("Color")
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count < 3 || fcall.Parameters.Count > 4)
            return LogErrorAndGetToken($"Color function must have either three or four integer parameters", fcall, ce);
        Token tokR = fcall.Parameters[0].SimplifyToToken(ce);
        Token tokG = fcall.Parameters[1].SimplifyToToken(ce);
        Token tokB = fcall.Parameters[2].SimplifyToToken(ce);
        Token tokA = fcall.Parameters.Count == 4 ? fcall.Parameters[3].SimplifyToToken(ce) : new Token() { Value = "255", IsString = true };
        if (!int.TryParse(tokR.Value, out int r) || !int.TryParse(tokG.Value, out int g) || !int.TryParse(tokB.Value, out int b) || !int.TryParse(tokA.Value, out int a))
            return LogErrorAndGetToken($"Color function must have either three or four integer parameters", fcall, ce);

        return new Block()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Contents =
                    {
                        { new Token() { Value = "R", IsString = true }, tokR },
                        { new Token() { Value = "G", IsString = true }, tokG },
                        { new Token() { Value = "B", IsString = true }, tokB },
                        { new Token() { Value = "A", IsString = true }, tokA },
                    },
            Context = fcall.Context,
        };
    }
}
