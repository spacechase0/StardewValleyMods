using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Content.Functions;
internal class RemoveFunction : BaseFunction
{
    public RemoveFunction()
    :   base( "Remove" )
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count < 2)
            return LogErrorAndGetToken($"Remove function must have at least two parameters (and array/block, and what to remove)", fcall, ce);

        var param = fcall.Parameters[0].DoSimplify(ce, true);

        if (param is Array arr)
        {
            var ret = new Array()
            {
                FilePath = fcall.FilePath,
                Line = fcall.Line,
                Column = fcall.Column,
                Context = fcall.Context,
                Uid = fcall.Uid,
                UserData = fcall.UserData,
            };

            ret.Contents.AddRange(arr.Contents);

            for (int i = 1; i < fcall.Parameters.Count; ++i)
            {
                var tok = fcall.Parameters[i].SimplifyToToken(ce, true);
                if (tok == null)
                    return null;

                foreach (var se in ret.Contents.ToList())
                {
                    var tok2 = se.SimplifyToToken(ce, true);
                    if (tok2 == null)
                        return null;

                    if (tok == tok2)
                        ret.Contents.Remove(se);
                }
            }

            return ret;
        }
        else if (param is Block block)
        {
            var ret = new Block()
            {
                FilePath = fcall.FilePath,
                Line = fcall.Line,
                Column = fcall.Column,
                Context = fcall.Context,
                Uid = fcall.Uid,
                UserData = fcall.UserData,
            };

            foreach ( var entry in block.Contents )
                ret.Contents.Add( entry.Key, entry.Value );

            for (int i = 1; i < fcall.Parameters.Count; ++i)
            {
                var tok = fcall.Parameters[i].SimplifyToToken(ce, true);
                if (tok == null)
                    return null;

                if (ret.Contents.ContainsKey(tok))
                    ret.Contents.Remove(tok);
            }

            return ret;
        }
        else if (param == null)
        {
            return null;
        }
        else
        {
            return LogErrorAndGetToken($"Remove function must have its first parameter be either an array or a block", fcall, ce);
        }
    }
}
