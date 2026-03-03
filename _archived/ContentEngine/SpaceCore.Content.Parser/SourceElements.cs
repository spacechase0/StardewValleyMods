using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Content;

internal class SourceElements
{
}
public class SourceElement
{
    public string FilePath { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }

    public Block Context { get; set; }
    public string Uid { get; set; }
    public object UserData { get; set; }
}

public class Token : SourceElement
{
    public string Value { get; set; }
    public string ExtraWhitespace { get; set; }
    public bool IsString { get; set; } = false;

    public bool IsStartArray() { return !IsString && Value == "["; }
    public bool IsEndArray() { return !IsString && Value == "]"; }
    public bool IsStartBlock() { return !IsString && Value == "{"; }
    public bool IsEndBlock() { return !IsString && Value == "}"; }
    public bool IsStartParenthesis() { return !IsString && Value == "("; }
    public bool IsEndParenthesis() { return !IsString && Value == ")"; }
    public bool IsEndStatement() { return !IsString && Value == ";"; }
    public bool IsNameSeparator() { return !IsString && Value == ":"; }

    public bool IsNull() { return !IsString && Value == "~"; }

    public bool IsEnder()
    {
        return IsEndArray() || IsEndBlock() || IsEndParenthesis() || IsEndStatement();
    }

    public override string ToString()
    {
        return $"{Value} @ {FilePath}:{Line}:{Column}";
    }

    public override bool Equals(object obj)
    {
        if (obj is Token other)
            return Value == other.Value && IsString == other.IsString;
        return false;
    }

    public override int GetHashCode()
    {
        return ((IsString ? "T" : "F") + Value).GetHashCode();
    }
}

public class FuncCall : SourceElement
{
    public string Function { get; set; }
    public List<SourceElement> Parameters { get; set; } = new();
}

public class Block : SourceElement
{
    public Dictionary<Token, SourceElement> Contents { get; set; } = new();
}

public class Array : SourceElement
{
    public List<SourceElement> Contents { get; set; } = new();
}

public class Statement : SourceElement
{
    public FuncCall FuncCall { get; set; }
    public SourceElement Data { get; set; }
}
