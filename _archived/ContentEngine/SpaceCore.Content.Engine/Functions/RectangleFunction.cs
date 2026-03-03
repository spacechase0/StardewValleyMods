using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Content.Functions;
internal class RectangleFunction : BaseFunction
{
    public RectangleFunction()
    :   base( "Rectangle" )
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count != 4)
            return LogErrorAndGetToken($"Rectangle function must have exactly four integer parameters", fcall, ce);
        Token tokX = fcall.Parameters[0].SimplifyToToken(ce);
        Token tokY = fcall.Parameters[1].SimplifyToToken(ce);
        Token tokW = fcall.Parameters[2].SimplifyToToken(ce);
        Token tokH = fcall.Parameters[3].SimplifyToToken(ce);
        if (!int.TryParse(tokX.Value, out int x) || !int.TryParse(tokY.Value, out int y) || !int.TryParse(tokX.Value, out int w) || !int.TryParse(tokY.Value, out int h))
            return LogErrorAndGetToken($"Rectangle function must have exactly four integer parameters", fcall, ce);

        return new Block()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Contents =
            {
                // TODO: Reuuse these keys
                { new Token() { Value = "X", IsString = true }, tokX },
                { new Token() { Value = "Y", IsString = true }, tokY },
                { new Token() { Value = "Width", IsString = true }, tokW },
                { new Token() { Value = "Height", IsString = true }, tokH },
            },
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
