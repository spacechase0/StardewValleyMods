using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Content.Functions;
internal class JoinFunction : BaseFunction
{
    public override bool IsLateResolver => true;

    public JoinFunction()
    :   base( "Join" )
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count < 2 || fcall.Parameters[0] is not Token sep || fcall.Parameters[1] is not Array toJoin)
            return LogErrorAndGetToken($"Join must have a separator parameter (token) then an array parameter (things to join)", fcall, ce);

        bool bail = false;

        StringBuilder contents = new();
        bool first = true;
        void JoinArray(Array arr)
        {
            foreach (var entryRaw in arr.Contents)
            {
                var entry = entryRaw.DoSimplify(ce, true);
                if (entry == null)
                {
                    bail = true;
                    return;
                }

                if (entry is Token tok)
                {
                    if (!first)
                        contents.Append(sep.Value);
                    contents.Append(tok.Value);
                    first = false;
                }
                else if (entry is Array arr2)
                {
                    JoinArray(arr2);
                }
            }
        }
        JoinArray(toJoin);

        if (bail)
            return null;

        return new Token()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Value = contents.ToString(),
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
