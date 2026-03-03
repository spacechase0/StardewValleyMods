using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Content.Functions;
internal class ContextValueFunction : BaseFunction
{
    public ContextValueFunction()
    :   base( "^" )
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count < 1)
            return LogErrorAndGetToken($"Context value function ^ must have one string parameter", fcall, ce);

        var tok = fcall.Parameters[0].SimplifyToToken(ce);
        if (!fcall.Context.Contents.TryGetValue(tok, out SourceElement se))
        {
            if (fcall.Parameters.Count == 1)
            {
                LogErrorAndGetToken($"Invalid context value {tok.Value}", fcall, ce); // TODO: Warnings
                se = new Token()
                {
                    FilePath = fcall.FilePath,
                    Line = fcall.Line,
                    Column = fcall.Column,
                    Value = $"invalid context value {tok.Value} @ {fcall.FilePath}:{fcall.Line}:{fcall.Column}",
                    IsString = true,
                    Context = fcall.Context,
                    Uid = fcall.Uid,
                    UserData = fcall.UserData,
                };
            }
            else return fcall.Parameters[1];
        }

        return se;
    }
}
