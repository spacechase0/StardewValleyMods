using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceShared;


namespace SpaceCore.Content.Functions;
internal class ChooseWeightedFunction : BaseFunction, IRefreshingFunction
{
    private static Token WeightKey = new() { Value = "Weight", IsString = true };
    private static Token DefaultWeight = new() { Value = "1.0", IsString = true };

    public override bool IsLateResolver => true;

    public ChooseWeightedFunction()
    :   base( "ChooseWeighted" )
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        var firstParam = fcall.Parameters.ElementAtOrDefault(0)?.DoSimplify(ce, true);
        if (firstParam is not Array arr)
            return LogErrorAndGetToken($"ChooseWeighted function must have an array parameter first", fcall, ce);

        if (fcall.Parameters.Count > 2)
            return LogErrorAndGetToken($"Too many parameters", fcall, ce);

        bool flatten = false;
        if (fcall.Parameters.Count == 2 && fcall.Parameters[1] is not Token { Value: "Flatten", IsString: true })
            return LogErrorAndGetToken($"Second argument to ChooseWeighted can only be \"Flatten\"", fcall, ce);
        else if (fcall.Parameters.Count == 2)
            flatten = true;

        List<Weighted<SourceElement>> choices = new();
        foreach (var entry in arr.Contents)
        {
            double weight = 1.0;
            if (entry is Block block)
            {
                SourceElement weightElem = block.Contents.GetOrDefault(WeightKey, DefaultWeight);
                Token weightTok = weightElem.SimplifyToToken(ce);
                if (weightTok != null)
                {
                    if (!double.TryParse(weightTok.Value, out weight))
                        ;// Log.Warn($"Failed to parse weight value as number, at {weightTok.FilePath}:{weightTok.Line}:{weightTok.Column}");
                }

                if (flatten && block != null &&
                    ((weightTok == DefaultWeight && block.Contents.Count == 1) || (weightTok != DefaultWeight && block.Contents.Count == 2)))
                {
                    SourceElement toAdd = block.Contents.First().Value;
                    if (toAdd == weightTok)
                        toAdd = block.Contents.Skip(1).First().Value;
                    choices.Add(new(weight, toAdd));
                }
                else
                {
                    Block newBlock = new()
                    {
                        FilePath = block.FilePath,
                        Line = block.Line,
                        Column = block.Column,
                        Context = block.Context,
                        Uid = block.Uid,
                        UserData = block.UserData,
                    };
                    foreach (var entryValEntry in block.Contents)
                    {
                        if (entryValEntry.Value != weightElem)
                            newBlock.Contents.Add(entryValEntry.Key, entryValEntry.Value);
                    }
                    choices.Add(new(weight, newBlock));
                }
            }
            else
            {
                choices.Add(new(weight, entry));
            }
        }

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

        return choices.Choose(r);
    }

    public bool WouldChangeFromRefresh(FuncCall fcall, ContentEngine pce)
    {
        return true;
    }
}
