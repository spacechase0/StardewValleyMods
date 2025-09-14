using System.Data.Common;

namespace SpaceCore.Content.Functions;

public abstract class BaseFunction
{
    public string Name { get; }
    public virtual bool IsLateResolver => false;

    public BaseFunction(string name)
    {
        this.Name = name;
    }

    public abstract SourceElement Simplify(FuncCall fcall, ContentEngine ce);

    protected Token LogErrorAndGetToken(string msg, SourceElement se, ContentEngine ce)
    {
        ce.LastErrors.Add(new(msg)
        {
            File = se.FilePath,
            Line = se.Line,
            Column = se.Column,
            Length = 1,
        });
        return new Token()
        {
            FilePath = se.FilePath,
            Line = se.Line,
            Column = se.Line,
            IsString = true,
            Value = "error",
            Context = se.Context,
            Uid = se.Uid,
            UserData = se.UserData,
        };
    }
}
