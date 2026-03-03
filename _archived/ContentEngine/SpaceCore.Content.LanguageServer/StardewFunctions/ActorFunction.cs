using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceCore.Content.Functions;

namespace SpaceCore.Content.StardewFunctions;
internal class ActorFunction : BaseFunction
{
    internal static readonly Token XKey = new Token() { Value = "X", IsString = true };
    internal static readonly Token YKey = new Token() { Value = "Y", IsString = true };

    public ActorFunction()
    : base("Actor")
    {
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count != 3)
            return LogErrorAndGetToken($"Actor function must have exactly three parameters (an actor name, a Vector2 position, and a Facing direction)", fcall, ce);
        Token actorTok = fcall.Parameters[0].SimplifyToToken(ce);
        Token facingTok = fcall.Parameters[2].SimplifyToToken(ce);

        Block posBlock = fcall.Parameters[1].DoSimplify(ce) as Block;
        if (posBlock == null)
            return LogErrorAndGetToken($"Actor function must have exactly three parameters (an actor name, a Vector2 position, and a Facing direction)", fcall, ce);

        return new Token()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Value = $"{actorTok.Value} {(posBlock.Contents[XKey] as Token).Value} {(posBlock.Contents[YKey] as Token).Value} {facingTok.Value}",
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
