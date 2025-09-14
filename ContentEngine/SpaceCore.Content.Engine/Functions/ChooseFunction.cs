using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceShared;


namespace SpaceCore.Content.Functions;
internal class ChooseFunction : BaseFunction, IRefreshingFunction
{
    public override bool IsLateResolver => true;

    public ChooseFunction()
    :   base( "Choose" )
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        var firstParam = fcall.Parameters.ElementAtOrDefault(0)?.DoSimplify(ce, true);
        if (fcall.Parameters.Count == 0 || firstParam is not Array arr)
            return LogErrorAndGetToken($"Choose function must have an array parameter", fcall, ce);

        Random r = ce.Random;
        if (fcall.Parameters.Count >= 2)
        {
            Token tok = fcall.Parameters[1].SimplifyToToken(ce, true);
            if (tok == null)
                return null;

            bool staticRand = false;
            if (fcall.Parameters.Count >= 3 &&
                 fcall.Parameters[2].SimplifyToToken(ce).Value.ToLower() == "static")
            {
                staticRand = true;
            }

            int seed = tok.Value.GetDeterministicHashCode();
            r = ce.RandomGenerator(seed, staticRand);
        }

        return arr.Contents[r.Next(arr.Contents.Count)];
    }

    public bool WouldChangeFromRefresh(FuncCall fcall, ContentEngine pce)
    {
        return true;
    }
}
