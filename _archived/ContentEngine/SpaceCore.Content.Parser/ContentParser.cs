using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SpaceCore.Content;

public class ParseError
{
    public ParseError(string msg)
    {
        Message = msg;
    }

    public string Message { get; init; }
    public string File { get; init; }
    public int Line { get; init; }
    public int Column { get; init; }
    public int Length { get; init; } = 1;

    public override string ToString()
    {
        return $"{Message} @ {File}:{Line}:{Column}+{Length}";
    }
}

public class ContentParser
{
    private string UniqueId { get; }

    public string ContentRootFolder { get; set; }
    public string ContentRootFolderActual { get; set; }

    public List<ParseError> LastErrors = new();

    public ContentParser(string uid, string dir, string contentRootFolder)
    {
        UniqueId = uid;
        ContentRootFolder = contentRootFolder;
        ContentRootFolderActual = Path.Combine(dir, contentRootFolder);
    }
    public Array Load(string filePath, string uidContext = "")
    {
        string fullPath = Path.Combine(ContentRootFolderActual, filePath);
        if (!File.Exists(fullPath))
            return null;

        string contents = File.ReadAllText(fullPath);

        return LoadText(contents, filePath, uidContext);
    }

    public Array LoadText(string contents, string filePath, string uidContext = "")
    {
        LastErrors.Clear();

        List<Token> tokens = Tokenize(contents);
        tokens.ForEach(t =>
        {
            t.FilePath = filePath;
            t.Uid = $"{uidContext}|{t.FilePath}:{t.Line}:{t.Column}";
        });

        tokens.Insert(0, new Token() { FilePath = filePath, Value = "[" });
        tokens.Add(new Token() { FilePath = filePath, Value = "]" });
        (Array statements, _) = BuildArray(tokens, 0);

        return statements;
    }

    private List<Token> Tokenize(string contents)
    {
        StringBuilder buffer = new();
        List<Token> tokens = new();

        int line = 1;
        int column = 0;

        void FlushBuffer()
        {
            if (buffer.Length > 0)
            {
                tokens.Add(new Token()
                {
                    Line = line,
                    Column = column - buffer.Length + 1,
                    Value = buffer.ToString(),
                    IsString = true
                });
                buffer.Clear();
            }
        }

        bool escaped = false;
        for (int ic = 0; ic < contents.Length; ++ic)
        {
            char c = contents[ic];
            if (!char.IsLetterOrDigit(c))
            {
                // Comments
                if (ic > 0 && c == '/' && contents[ic - 1] == '/')
                {
                    buffer.Remove(buffer.Length - 1, 1);
                    for (; ic < contents.Length && contents[ic] != '\n'; ++ic, ++column) ;
                    c = contents[ic];
                }
                else if (ic > 0 && c == '*' && contents[ic - 1] == '/')
                {
                    buffer.Remove(buffer.Length - 1, 1);
                    for (++ic; ic < contents.Length && !(contents[ic - 1] == '*' && contents[ic] == '/'); ++ic)
                    {
                        if (contents[ic] == '\n') { ++line; column = 0; }
                        else ++column;
                    }
                    c = contents[ic];
                }

                // Escaping
                else if (c == '\\')
                {
                    if (escaped)
                    {
                        buffer.Append(c);
                        escaped = false;
                    }
                    else escaped = true;
                }

                // Strings
                else if (c == '\'' || c == '\"')
                {
                    if (escaped)
                    {
                        buffer.Append(c);
                        escaped = false;
                    }
                    else
                    {
                        for (++ic; ic < contents.Length && (contents[ic] != c || escaped); ++ic)
                        {
                            if (contents[ic] == '\\')
                                escaped = true;
                            else
                            {
                                buffer.Append(contents[ic]);
                                escaped = false;
                            }
                        }
                    }
                }

                // Symbols
                else if (!escaped && (c == '(' || c == ')' || c == '{' || c == '}' || c == '[' || c == ']' || c == ';' || c == ':' || c == '~'))
                {
                    FlushBuffer();
                    tokens.Add(new Token()
                    {
                        Line = line,
                        Column = column + 1,
                        Value = c.ToString()
                    });
                }

                // Whitespace
                else if (!escaped && char.IsWhiteSpace(c))
                {
                    FlushBuffer();
                    if (tokens.Count > 0) // IF there's whitespace at the beginning of a file this won't be true
                        tokens[tokens.Count - 1].ExtraWhitespace += c;
                }

                // Mod ID Shortcut
                else if (!escaped && c == '&')
                {
                    buffer.Append(UniqueId);
                }

                // Other
                else
                {
                    buffer.Append(c);
                    escaped = false;
                }

                if (c == '\n') { ++line; column = 0; }
                else ++column;
            }
            else
            {
                buffer.Append(c);
                escaped = false;
                ++column;
            }
        }
        FlushBuffer();

        // Condense strings to single token
        for (int i = 0; i < tokens.Count - 1; ++i)
        {
            if (tokens[i].IsString && tokens[i + 1].IsString)
            {
                tokens[i].Value += tokens[i].ExtraWhitespace + tokens[i + 1].Value;
                tokens[i].ExtraWhitespace = tokens[i + 1].ExtraWhitespace;
                tokens.RemoveAt(i + 1);
                --i;
            }
        }

        return tokens;
    }

    private Tuple<SourceElement, int> BuildSourceElement(List<Token> tokens, int start)
    {
        Token PeekNext(int offset = 1)
        {
            if (start + offset >= tokens.Count)
                return null;
            return tokens[start + offset];
        }
        bool CheckNext(Func<Token, bool> func, int offset = 1)
        {
            Token peek = PeekNext(offset);
            return peek == null ? false : func(peek);
        }

        Token tok = tokens[start];

        if (tok.IsString)
        {
            if (CheckNext(t => t.IsEnder()))
            {
                return new(tok, start + (PeekNext().IsEndStatement() ? 2 : 1));
            }
            else if (CheckNext(t => t.IsStartParenthesis()))
            {
                (SourceElement se, start) = BuildStatement(tokens, start);
                return new(se, start);
            }
            /*
            else if (CheckNext(t => t.IsStartBlock()))
            {
                (SourceElement se, start) = BuildBlock(tokens, start);
                return new(se, start);
            }
            */
        }
        else if (tok.IsStartBlock())
        {
            (SourceElement se, start) = BuildBlock(tokens, start);
            return new(se, start + (tokens[start].IsEndStatement() ? 1 : 0));
        }
        else if (tok.IsStartArray())
        {
            (SourceElement se, start) = BuildArray(tokens, start);
            return new(se, start + (tokens[start].IsEndStatement() ? 1 : 0));
        }
        else if (tok.IsNull() || tok.IsEndStatement())
        {
            return new(tok, start + 1);
        }

        if (tok == tokens.Last() && tok.IsEndArray())
        {
            var realLast = tokens.SkipLast(1).Last();
            LastErrors.Add(new ParseError($"Finished parsing in incomplete state")
            {
                File = realLast.FilePath,
                Line = realLast.Line,
                Column = realLast.Column,
                Length = realLast.Value.Length,
            });
        }
        else
        {
            LastErrors.Add(new ParseError($"Invalid token \"{tok.Value}\"")
            {
                File = tok.FilePath,
                Line = tok.Line,
                Column = tok.Column,
                Length = tok.Value.Length,
            });
        }
        return new(tok, start + 1);
    }

    private Tuple<Array, int> BuildArray(List<Token> tokens, int start)
    {
        if (!tokens[start].IsStartArray())
            throw new ArgumentException($"Expected start array, got {tokens[0].Value}");

        Array ret = new()
        {
            FilePath = tokens[start].FilePath,
            Line = tokens[start].Line,
            Column = tokens[start].Column,
            Uid = tokens[start].Uid,
            UserData = tokens[start].UserData,
        };

        int i = start + 1;
        while (i < tokens.Count)
        {
            Token PeekNext(int offset = 1)
            {
                if (i + offset >= tokens.Count)
                    return null;
                return tokens[i + offset];
            }
            bool CheckNext(Func<Token, bool> func, int offset = 1)
            {
                Token peek = PeekNext(offset);
                return peek == null ? false : func(peek);
            }

            if (tokens[i].IsEndArray())
                break;

            (SourceElement obj, i) = BuildSourceElement(tokens, i);
            ret.Contents.Add(obj);
        }

        if (i >= tokens.Count)
            LastErrors.Add(new("Reached end of file while building array")
            {
                File = tokens[start].FilePath,
                Line = tokens[start].Line,
                Column = tokens[start].Column,
                Length = 1,
            } );

        return new(ret, i + 1);
    }
    private Tuple<Block, int> BuildBlock(List<Token> tokens, int start)
    {
        if (!tokens[start].IsStartBlock())
            throw new ArgumentException($"Expected start block, got {tokens[0].Value}");

        Block ret = new()
        {
            FilePath = tokens[start].FilePath,
            Line = tokens[start].Line,
            Column = tokens[start].Column,
            Uid = tokens[start].Uid,
            UserData = tokens[start].UserData,
        };

        int i = start + 1;
        while (i < tokens.Count)
        {
            Token PeekNext(int offset = 1)
            {
                if (i + offset >= tokens.Count)
                    return null;
                return tokens[i + offset];
            }
            bool CheckNext(Func<Token, bool> func, int offset = 1)
            {
                Token peek = PeekNext(offset);
                return peek == null ? false : func(peek);
            }

            var tok = tokens[i];

            if (tok.IsEndBlock())
                break;

            if (!tok.IsString)
                LastErrors.Add(new($"Expected string instead of \"{tok.Value}\"")
                {
                    File = tok.FilePath,
                    Line = tok.Line,
                    Column = tok.Column,
                    Length = tok.Value.Length,
                });
            if (!CheckNext(t => t.IsNameSeparator()))
                LastErrors.Add(new($"Expected : instead of \"{tok.Value}\"")
                {
                    File = tok.FilePath,
                    Line = tok.Line,
                    Column = tok.Column,
                    Length = tok.Value.Length,
                });

            (SourceElement obj, i) = BuildSourceElement(tokens, i + 2);
            ret.Contents.Add(tok, obj);
        }

        if (i >= tokens.Count)
            LastErrors.Add(new("Reached end of file while building block")
            {
                File = tokens[start].FilePath,
                Line = tokens[start].Line,
                Column = tokens[start].Column,
                Length = 1,
            });

        return new(ret, i + 1);
    }

    private Tuple<Statement, int> BuildStatement(List<Token> tokens, int start)
    {
        if (!tokens[start].IsString || !tokens[start + 1].IsStartParenthesis())
            throw new ArgumentException($"Expected string and start parenthesis, got \"{tokens[start].Value}\" and \"{tokens[start + 1].Value}\"");

        Statement ret = new()
        {
            FilePath = tokens[start].FilePath,
            Line = tokens[start].Line,
            Column = tokens[start].Column,
            Uid = tokens[start].Uid,
            UserData = tokens[start].UserData,
        };

        (ret.FuncCall, start) = BuildFuncCall(tokens, start);

        if (start >= tokens.Count || tokens[start].IsEnder())
            return new(ret, start + (start < tokens.Count && tokens[start].IsEndStatement() ? 1 : 0));

        (ret.Data, start) = BuildSourceElement(tokens, start);

        return new(ret, start);
    }

    private Tuple<FuncCall, int> BuildFuncCall(List<Token> tokens, int start)
    {
        if (!tokens[start].IsString || !tokens[start + 1].IsStartParenthesis())
            throw new ArgumentException($"Expected string and start parenthesis, got \"{tokens[start].Value}\" and \"{tokens[start + 1].Value}\"");

        FuncCall ret = new()
        {
            FilePath = tokens[start].FilePath,
            Line = tokens[start].Line,
            Column = tokens[start].Column,
            Uid = tokens[start].Uid,
            UserData = tokens[start].UserData,
        };

        ret.Function = tokens[start].Value;

        int i = start + 2;
        while (i < tokens.Count)
        {
            Token PeekNext(int offset = 1)
            {
                if (i + offset >= tokens.Count)
                    return null;
                return tokens[i + offset];
            }
            bool CheckNext(Func<Token, bool> func, int offset = 1)
            {
                Token peek = PeekNext(offset);
                return peek == null ? false : func(peek);
            }

            Token tok = tokens[i];

            if (tok.IsEndParenthesis())
                break;

            (SourceElement se, i) = BuildSourceElement(tokens, i);
            ret.Parameters.Add(se);
        }

        if (i >= tokens.Count)
            LastErrors.Add(new("Reached end of file while building function call")
            {
                File = tokens[start].FilePath,
                Line = tokens[start].Line,
                Column = tokens[start].Column,
                Length = tokens[start].Value.Length,
            });

        return new(ret, i + 1);
    }
}
