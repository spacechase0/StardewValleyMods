using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SpaceShared;

namespace SpaceCore.Content.Functions;
internal class FilterByConditionFunction : BaseFunction, IRefreshingFunction
{
    private static Token ConditionKey = new() { Value = "Condition", IsString = true };
    private static Token DefaultCondition = new() { Value = "true", IsString = true, Uid = "~" };

    private Dictionary<string, bool> LastStates = new();

    public override bool IsLateResolver => true;


    public FilterByConditionFunction()
    :   base("FilterByCondition")
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        var firstParam = fcall.Parameters.ElementAtOrDefault(0)?.DoSimplify(ce, true);
        if (firstParam is not Array arr)
            return LogErrorAndGetToken($"Choose function must have an array parameter first", fcall, ce);
        if (fcall.Parameters.Count > 2)
            return LogErrorAndGetToken($"Too many parameters", fcall, ce);
        bool flatten = false;
        if (fcall.Parameters.Count == 2 && fcall.Parameters[1] is not Token { Value: "Flatten", IsString: true })
            return LogErrorAndGetToken($"Second argument to FilterByCondition can only be \"Flatten\"", fcall, ce);
        else if (fcall.Parameters.Count == 2)
            flatten = true;

        Array ret = new()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };

        foreach (var entry in arr.Contents)
        {
            Token cond = DefaultCondition;
            Block entryVal = entry as Block;
            SourceElement condElem = null;
            if (entryVal != null && entryVal.Contents.TryGetValue(ConditionKey, out condElem))
            {
                cond = condElem.SimplifyToToken(ce, true);
                if (cond == null && true)
                    return null;
            }

            bool condMet = ce.CheckCondition(cond);
            if (condMet)
            {
                if (flatten && entryVal != null &&
                    ((cond == DefaultCondition && entryVal.Contents.Count == 1) || (cond != DefaultCondition && entryVal.Contents.Count == 2)))
                {
                    SourceElement toAdd = entryVal.Contents.First().Value;
                    if (toAdd == cond)
                        toAdd = entryVal.Contents.Skip(1).First().Value;
                    ret.Contents.Add(toAdd);
                }
                else if (entryVal != null)
                {
                    Block newBlock = new()
                    {
                        FilePath = entryVal.FilePath,
                        Line = entryVal.Line,
                        Column = entryVal.Column,
                        Context = entryVal.Context,
                        Uid = entryVal.Uid,
                        UserData = entryVal.UserData,
                    };
                    foreach (var entryValEntry in entryVal.Contents)
                    {
                        if (entryValEntry.Value != cond)
                            newBlock.Contents.Add(entryValEntry.Key, entryValEntry.Value);
                    }
                    ret.Contents.Add(newBlock);
                }
                else
                {
                    ret.Contents.Add(entry);
                }
            }
            LastStates[cond.Uid] = condMet;
        }

        return ret;
    }

    public bool WouldChangeFromRefresh(FuncCall fcall, ContentEngine pce)
    {
        var arr = fcall.Parameters[0].DoSimplify(pce, true) as Array;
        if ( arr == null)
            return true;

        foreach (var entry in arr.Contents)
        {
            Token cond = DefaultCondition;
            Block entryVal = entry as Block;
            SourceElement condElem = null;
            if (entryVal != null && entryVal.Contents.TryGetValue(ConditionKey, out condElem))
            {
                cond = condElem.SimplifyToToken(pce, true);
                if (cond == null && true)
                    return true;
            }

            bool condMet = pce.CheckCondition(cond);
            if (!LastStates.TryGetValue(cond.Uid, out bool lastCond) || condMet != lastCond)
            {
                return true;
            }
        }

        return false;
    }
}
