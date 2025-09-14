using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Content.Functions;
internal class CombineFunction : BaseFunction
{
    public CombineFunction()
    :   base("Combine")
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count == 0)
        {
            return new Token()
            {
                FilePath = fcall.FilePath,
                Line = fcall.Line,
                Column = fcall.Column,
                Value = "~", // null
                Context = fcall.Context,
                Uid = fcall.Uid,
                UserData = fcall.UserData,
            };
        }

        SourceElement ret = null;
        if (fcall.Parameters[0] is Array)
        {
            var arr = new Array()
            {
                FilePath = fcall.FilePath,
                Line = fcall.Line,
                Column = fcall.Column,
                Context = fcall.Context,
                Uid = fcall.Uid,
                UserData = fcall.UserData,
            };

            foreach (var param in fcall.Parameters)
            {
                arr.Contents.AddRange((param as Array).Contents);
            }

            ret = arr;
        }
        else if (fcall.Parameters[0] is Block)
        {
            var block = new Block()
            {
                FilePath = fcall.FilePath,
                Line = fcall.Line,
                Column = fcall.Column,
                Context = fcall.Context,
                Uid = fcall.Uid,
                UserData = fcall.UserData,
            };

            foreach (var param in fcall.Parameters)
            {
                foreach (var pair in (param as Block).Contents)
                    block.Contents.Add(pair.Key, pair.Value);
            }

            ret = block;
        }
        else
        {
            ret = new Token()
            {
                FilePath = fcall.FilePath,
                Line = fcall.Line,
                Column = fcall.Column,
                Value = "~", // null
                Context = fcall.Context,
                Uid = fcall.Uid,
                UserData = fcall.UserData,
            };
        }

        // TODO: Do I need to re-simplify here?

        return ret;
    }
}
