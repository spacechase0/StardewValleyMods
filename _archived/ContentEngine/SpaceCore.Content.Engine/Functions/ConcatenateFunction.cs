using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Content.Functions;
internal class ConcatenateFunction : BaseFunction
{
    public ConcatenateFunction()
    :   base("Concatenate")
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        string str = "";
        foreach (var param in fcall.Parameters)
        {
            str += param.SimplifyToToken(ce).Value;
        }

        return new Token()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Value = str,
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
