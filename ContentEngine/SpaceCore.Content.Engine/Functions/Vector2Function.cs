using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Content.Functions;
internal class Vector2Function : BaseFunction
{
    internal static readonly Token XKey = new Token() { Value = "X", IsString = true };
    internal static readonly Token YKey = new Token() { Value = "Y", IsString = true };

    public Vector2Function()
    : base("Vector2")
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count != 2)
            return LogErrorAndGetToken($"Vector2 function must have exactly two float parameters", fcall, ce);
        Token tokX = fcall.Parameters[0].SimplifyToToken(ce);
        Token tokY = fcall.Parameters[1].SimplifyToToken(ce);
        if (!float.TryParse(tokX.Value, out float x) || !float.TryParse(tokY.Value, out float y))
            return LogErrorAndGetToken($"Vector2 function must have exactly two float parameters", fcall, ce);

        return new Block()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Contents =
            {
                { XKey, tokX },
                { YKey, tokY },
            },
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
