using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceCore.Content.Functions;
using StardewValley;

namespace SpaceCore.Content.StardewFunctions;
internal class QuickQuestionFunction : BaseFunction
{
    private static readonly Token ResponseKey = new Token() { Value = "Response", IsString = true };
    private static readonly Token CommandsKey = new Token() { Value = "Commands", IsString = true };

    public override bool IsLateResolver => true;

    public QuickQuestionFunction()
    : base("QuickQuestion")
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count != 2)
            return LogErrorAndGetToken($"QuickQuestion function must have exactly two parameters (a quqestion, and a array of answer objects (Response and Commands))", fcall, ce);
        Token questionTok = fcall.Parameters[0].SimplifyToToken(ce, true);
        var responsesArrayRaw = fcall.Parameters[1].DoSimplify(ce, true);
        if (questionTok == null || responsesArrayRaw == null)
            return null;

        Array responsesArray = responsesArrayRaw as Array;
        if (responsesArray == null)
            return LogErrorAndGetToken($"QuickQuestion function must have exactly two parameters (a quqestion, and a array of answer objects (Response and Commands))", fcall, ce);

        string question = questionTok.Value;
        List<(string response, string script)> responses = new();
        foreach (var respEntry in responsesArray.Contents)
        {
            if ( respEntry is not Block block)
                return LogErrorAndGetToken($"QuickQuestion function must have exactly two parameters (a quqestion, and a array of answer objects (Response and Commands))", fcall, ce);
            if (!block.Contents.TryGetValue(ResponseKey, out SourceElement responseRaw) ||
                !block.Contents.TryGetValue(CommandsKey, out SourceElement scriptRaw))
                return LogErrorAndGetToken($"QuickQuestion function must have exactly two parameters (a quqestion, and a array of answer objects (Response and Commands))", fcall, ce);

            var responseTok = responseRaw.SimplifyToToken(ce, true);
            var scriptObj = scriptRaw.DoSimplify(ce, true);
            if (responseTok == null || scriptObj == null)
                return null;

            string script = "";
            if (scriptObj is Token scriptTok)
            {
                script = scriptTok.Value.Replace('/', '\\');
            }
            else if (scriptObj is Array scriptArr)
            {
                StringBuilder contents = new();
                bool first = true;
                void JoinArray(Array arr)
                {
                    foreach (var entry in arr.Contents)
                    {
                        if (entry is Token tok)
                        {
                            if (!first)
                                contents.Append("\\");
                            contents.Append(tok.Value);
                            first = false;
                        }
                        else if (entry is Array arr2)
                        {
                            JoinArray(arr2);
                        }
                    }
                }
                JoinArray(scriptArr);
                script = contents.ToString();
            }
            else
            {
                return LogErrorAndGetToken($"QuickQuestion response Commands invalid type (must be array or string)", fcall, ce);
            }

            responses.Add(new(responseTok.Value, script));
        }

        return new Token()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Value = $"quickQuestion {question}#{string.Join("#", responses.Select(r => r.response).ToArray())}(break){string.Join("(break)", responses.Select(r => r.script).ToArray())}",
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
