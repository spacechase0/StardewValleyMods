using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using StardewModdingAPI;
using StardewValley;

#if IS_SPACECORE
namespace SpaceCore.Content
{
#else
namespace SpaceShared.Content
{
#endif
    public class SourceElement
    {
        public string FilePath { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public Block Context { get; set; }
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
            return ((IsString ? "T" : "F" ) + Value).GetHashCode();
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

    internal class ContentParser
    {
        private IManifest Manifest { get; set; }
        private IModHelper Helper { get; }

        public string ContentRootFolder { get; internal set; }
        public string ContentRootFolderActual { get; internal set; }

        public ContentParser(IManifest manifest, IModHelper helper, string contentRootFolder)
        {
            Manifest = manifest;
            Helper = helper;
            ContentRootFolder = contentRootFolder;
            ContentRootFolderActual = Path.Combine(Helper.DirectoryPath, contentRootFolder);
        }

        public Array Load(string filePath)
        {
            string fullPath = Path.Combine(ContentRootFolderActual, filePath);

            string contents = File.ReadAllText(fullPath);

            List<Token> tokens = Tokenize(contents);
            tokens.ForEach(t => t.FilePath = filePath);

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
                    tokens.Add(new Token() { Line = line, Column = column - buffer.Length + 1, Value = buffer.ToString(), IsString = true });
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
                        for (; ic < contents.Length && contents[ic] != '\n'; ++ic, ++column);
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
                        tokens.Add(new Token() { Line = line, Column = column + 1, Value = c.ToString() });
                    }

                    // Whitespace
                    else if (!escaped && char.IsWhiteSpace(c))
                    {
                        FlushBuffer();
                        tokens[tokens.Count - 1].ExtraWhitespace += c;
                    }

                    // Mod ID Shortcut
                    else if ( !escaped && c == '&' )
                    {
                        buffer.Append(Manifest.UniqueID);
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
            for ( int i = 0; i < tokens.Count - 1; ++i )
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
            else if (tok.IsNull())
            {
                return new(tok, start + 1);
            }

            throw new ArgumentException($"Invalid token \"{tok.Value}\" at {tok.FilePath}:{tok.Line}:{tok.Column}");
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

            if (!tokens[i].IsEndArray())
                throw new ArgumentException("Reached end of file while building array");

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
                    throw new ArgumentException($"Expected string instead of \"{tok.Value}\" at {tok.FilePath}:{tok.Line}:{tok.Column}");
                if (!CheckNext(t => t.IsNameSeparator()))
                    throw new ArgumentException($"Expected : instead of \"{tok.Value}\" at {tok.FilePath}:{tok.Line}:{tok.Column}");

                (SourceElement obj, i) = BuildSourceElement(tokens, i + 2);
                ret.Contents.Add(tok, obj);
            }

            if (!tokens[i].IsEndBlock())
                throw new ArgumentException("Reached end of file while building block");

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
            };

            (ret.FuncCall, start) = BuildFuncCall(tokens, start);

            if (tokens[start].IsEnder())
                return new(ret, start + (tokens[start].IsEndStatement() ? 1 : 0));

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

            if (!tokens[i].IsEndParenthesis())
                throw new ArgumentException("Reached end of file while building function call");

            return new(ret, i + 1);
        }
    }
}
