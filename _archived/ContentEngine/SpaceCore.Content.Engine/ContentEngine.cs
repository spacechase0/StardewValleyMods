using SpaceCore.Content.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("SpaceCore.Content.LanguageServer")]

namespace SpaceCore.Content;

public static class ContentExtensions
{
    public static Token SimplifyToToken(this SourceElement se, ContentEngine ce, bool allowLateResolve = false, bool require = true)
    {
        Token tok = se as Token;
        if (se is Statement statement)
        {
            var simplified = statement.FuncCall.Simplify(ce, allowLateResolve);
            if (allowLateResolve && simplified == null)
                return null;
            tok = simplified as Token;
        }
        if (tok == null && require)
        {
            ce.LastErrors.Add(new($"Source element must simplify to string")
            {
                File = se.FilePath,
                Line = se.Line,
                Column = se.Column,
                Length = 1
            });
            return new Token()
            {
                FilePath = se.FilePath,
                Line = se.Line,
                Column = se.Column,
                IsString = true,
                Value = "error",
                Context = se.Context,
                Uid = se.Uid,
                UserData = se.UserData,
            };
        }
        return tok;
    }

    public static SourceElement DoSimplify(this SourceElement se, ContentEngine ce, bool allowLateResolve = false)
    {
        if (se is Statement statement)
        {
            var simplified = statement.FuncCall.Simplify(ce, allowLateResolve);
            if (allowLateResolve && simplified == null)
                return null;
            return simplified;
        }
        return se;
    }

    public static SourceElement Simplify(this FuncCall fcall, ContentEngine ce, bool allowLateResolve = false)
    {
        if (ce.SimplifyFunctions.TryGetValue(fcall.Function, out var func))
        {
            if (!allowLateResolve && func.IsLateResolver)
                return fcall;

            return func.Simplify(fcall, ce);
        }

        return fcall;
    }
}

public class ContentEngine
{
    public string ContentRootFolder { get; private set; }
    public string ContentRootFolderActual { get; private set; }
    public string ContentRootFile { get; }

    protected ContentParser Parser { get; }

    protected Array Contents { get; set; }

    public Dictionary<string, BaseFunction> SimplifyFunctions { get; } = new();
    internal Func<string, string> AssetNameSimplfier { get; set; }

    public void AddSimplifyFunction(BaseFunction func)
    {
        SimplifyFunctions.Add(func.Name, func);
    }

    public Random Random { get; } = new();

    public delegate Random RandGenDelegate(int seed, bool isStatic);
    public RandGenDelegate RandomGenerator;

    public ContentEngine(string uid, string dirPath, string contentRootFile, Func<string, string> assetNameFunc, RandGenDelegate randGen)
    {
        ContentRootFolder = Path.GetDirectoryName(contentRootFile);
        ContentRootFolderActual = Path.Combine(dirPath, ContentRootFolder);
        ContentRootFile = contentRootFile;
        Parser = new(uid, dirPath, ContentRootFolder);
        AssetNameSimplfier = assetNameFunc;
        RandomGenerator = randGen;

        AddSimplifyFunction(new AssetPathFunction(absolute: false));
        AddSimplifyFunction(new AssetPathFunction(absolute: true));
        AddSimplifyFunction(new ChooseFunction());
        AddSimplifyFunction(new ChooseWeightedFunction());
        AddSimplifyFunction(new ColorFunction());
        AddSimplifyFunction(new CombineFunction());
        AddSimplifyFunction(new ConcatenateFunction());
        AddSimplifyFunction(new ContextValueFunction());
        AddSimplifyFunction(new FilterByConditionFunction());
        AddSimplifyFunction(new JoinFunction());
        AddSimplifyFunction(new RectangleFunction());
        AddSimplifyFunction(new Vector2Function());
        AddSimplifyFunction(new RemoveFunction());
    }

    public virtual bool CheckCondition(Token condition)
    {
        if (condition.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            return true;
        else
            return false;
    }

    public void OnReloadMonitorInstead(string pathModifier, [CallerFilePath] string path = "")
    {
        if (path == "")
            throw new ArgumentException("No caller file path?");

        Parser.ContentRootFolderActual = ContentRootFolderActual = Path.Combine(Path.GetDirectoryName(path), pathModifier);
    }

    public List<ParseError> LastErrors { get; } = new();
    public void Reload()
    {
        LastErrors.Clear();
        Contents = (Array)RecursiveLoad(Path.GetFileName(ContentRootFile), flatten: false);
        if (Contents == null)
        {
            LastErrors.Add( new($"Failed to find root content file {Path.GetFileName(ContentRootFile)}") );
        }
        PostReload();
    }

    protected virtual void PostReload()
    {
    }

    protected virtual Array ParserLoad(string file, string uidContext)
    {
        return Parser.Load(file, uidContext);
    }

    internal SourceElement RecursiveLoad(string file, bool flatten = true, Block ctx = null, string uidContext = "")
    {
        ctx ??= new();

        Array contents = ParserLoad(file, uidContext);
        if (contents == null) // file not found
            return null;
        LastErrors.AddRange(Parser.LastErrors);
        RecursiveLoadImpl(contents, Path.GetDirectoryName(file), flatten: false, ref ctx, out SourceElement se);
        contents = (Array)se;

        if (flatten && contents.Contents.Count == 1)
            return contents.Contents[0];

        return contents;
    }

    private bool RecursiveLoadImpl(SourceElement elem, string subfolder, bool flatten, ref Block ctx, out SourceElement replacement)
    {
        elem.Context = ctx;

        if (elem is Statement statement)
        {
            statement.FuncCall.Context = ctx;
            for (int i = 0; i < statement.FuncCall.Parameters.Count; ++i)
            {
                statement.FuncCall.Parameters[i].Context = ctx;
                RecursiveLoadImpl(statement.FuncCall.Parameters[i], subfolder, flatten: true, ref ctx, out var param);
                statement.FuncCall.Parameters[i] = param;
            }
            if (statement.Data != null)
                statement.Data.Context = ctx;

            if (statement.FuncCall.Function == "Include")
            {
                if (statement.FuncCall.Parameters.Count < 1 || statement.FuncCall.Parameters[0] is not Token token || !token.IsString)
                {
                    LastErrors.Add(new($"Include needs a string for the include path")
                    {
                        File = statement.FuncCall.FilePath,
                        Line = statement.FuncCall.Line,
                        Column = statement.FuncCall.Column,
                        Length = 1,
                    });
                    replacement = elem;
                    return false;
                }
                if (statement.FuncCall.Parameters.Count >= 2 && statement.FuncCall.Parameters[1] is not Block)
                {
                    LastErrors.Add(new($"Include context at must be a block; will be ignored since it's not")
                    {
                        File = statement.FuncCall.Parameters[1].FilePath,
                        Line = statement.FuncCall.Parameters[1].Line,
                        Column = statement.FuncCall.Parameters[1].Column,
                        Length = 1,
                    });
                }

                Block block = statement.FuncCall.Parameters.Count >= 2 ? statement.FuncCall.Parameters[1] as Block : null;

                Block newCtx = new();
                newCtx.Contents = new(ctx.Contents);
                if (block != null)
                {
                    foreach (var entry in block.Contents)
                    {
                        newCtx.Contents[entry.Key] = entry.Value;
                    }
                }

                if (token.Value.StartsWith('/') && ContentRootFolderActual == "")
                {
                    LastErrors.Add(new("Include with absolute paths only supported with a selected folder for your workspace")
                    {
                        File = token.FilePath,
                        Line = token.Line,
                        Column = token.Column,
                        Length = 1,
                    });
                    replacement = statement;
                    return false;
                }

                replacement = RecursiveLoad(token.Value.StartsWith('/') ? token.Value.Substring(1) : Path.Combine(subfolder, token.Value), flatten, newCtx, statement.Uid);
                if (replacement == null)
                {
                    LastErrors.Add(new($"Failed to find Include file \"{token.Value}\"")
                    {
                        File = token.FilePath,
                        Line = token.Line,
                        Column = token.Column,
                        Length = 1,
                    });
                    replacement = new Token()
                    {
                        FilePath = token.FilePath,
                        Line = token.Line,
                        Column = token.Column,
                        Value = "~",
                        Context = token.Context,
                        Uid = token.Uid,
                        UserData = token.UserData,
                    };
                }
                return true;
            }
            else if (statement.FuncCall.Function == "If")
            {
                if (statement.FuncCall.Parameters.Count != 1)
                {
                    LastErrors.Add(new($"If needs a condition; this block will be ignored since there wasn't one")
                    {
                        File = statement.FuncCall.FilePath,
                        Line = statement.FuncCall.Line,
                        Column = statement.FuncCall.Column,
                        Length = 2,
                    });

                    replacement = new Token()
                    {
                        FilePath = statement.FilePath,
                        Line = statement.Line,
                        Column = statement.Column,
                        Value = "~",
                        IsString = false,
                        Context = ctx, // Can't recall if this is right or not... probably doesn't matter in this case though.
                        Uid = statement.Uid,
                        UserData = statement.UserData,
                    };
                    return true;
                }

                var tok = statement.FuncCall.Parameters[0].SimplifyToToken(this); // TODO: Figure out how to handle errors from here
                if (tok.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    RecursiveLoadImpl(statement.Data, subfolder, flatten, ref ctx, out replacement);
                }
                else
                {
                    replacement = new Token()
                    {
                        FilePath = statement.FilePath,
                        Line = statement.Line,
                        Column = statement.Column,
                        Value = "~",
                        IsString = false,
                        Context = ctx, // Can't recall if this is right or not... probably doesn't matter in this case though.
                        Uid = statement.Uid,
                        UserData = statement.UserData,
                    };
                }
                return true;
            }
            else if (statement.FuncCall.Function == "Set^")
            {
                if (statement.FuncCall.Parameters.Count != 2)
                {
                    LastErrors.Add(new($"Set^ needs a value name to set, and an actual value. This block will be ignored since those were not specified.")
                    {
                        File = statement.FuncCall.FilePath,
                        Line = statement.FuncCall.Line,
                        Column = statement.FuncCall.Column,
                        Length = 4,
                    });

                    replacement = new Token()
                    {
                        FilePath = statement.FilePath,
                        Line = statement.Line,
                        Column = statement.Column,
                        Value = "~",
                        IsString = false,
                        Context = ctx, // Can't recall if this is right or not... probably doesn't matter in this case though.
                        Uid = statement.Uid,
                        UserData = statement.UserData,
                    };
                    return true;
                }

                Token param = statement.FuncCall.Parameters[0].SimplifyToToken(this, false);
                SourceElement val = statement.FuncCall.Parameters[1];

                Block newCtx = new();
                newCtx.Contents = new(ctx.Contents);
                newCtx.Contents[param] = val;
                ctx = newCtx;

                replacement = new Token()
                {
                    FilePath = statement.FilePath,
                    Line = statement.Line,
                    Column = statement.Column,
                    Value = "~",
                    IsString = false,
                    Context = ctx, // Can't recall if this is right or not... probably doesn't matter in this case though.
                    Uid = statement.Uid,
                    UserData = statement.UserData,
                };
                return true;
            }
            else
            {
                SourceElement se = statement.FuncCall.Simplify(this);
                if (se as FuncCall == statement.FuncCall)
                {
                    if (statement.Data != null)
                        RecursiveLoadImpl(statement.Data, subfolder, flatten: true, ref ctx, out se);
                    statement.Data = se;
                }
                else
                {
                    replacement = se;
                    return true;
                }
            }
        }
        else if (elem is FuncCall fcall && fcall.Function == "Include")
        {
            foreach (var param in fcall.Parameters)
                param.Context = ctx;

            if (fcall.Parameters.Count < 1 || fcall.Parameters[0] is not Token token || !token.IsString)
            {
                LastErrors.Add(new($"Include needs a string for the include path")
                {
                    File = fcall.FilePath,
                    Line = fcall.Line,
                    Column = fcall.Column,
                    Length = 1,
                });
                replacement = elem;
                return false;
            }
            if (fcall.Parameters.Count >= 2 && fcall.Parameters[1] is not Block)
            {
                LastErrors.Add(new($"Include context must be a block; will be ignored since it isn't")
                {
                    File = fcall.Parameters[1].FilePath,
                    Line = fcall.Parameters[1].Line,
                    Column = fcall.Parameters[1].Column,
                    Length = 1,
                });
            }

            Block block = fcall.Parameters.Count >= 2 ? fcall.Parameters[1] as Block : null;

            Block newCtx = new();
            newCtx.Contents = new(ctx.Contents);
            if (block != null)
            {
                foreach (var entry in block.Contents)
                {
                    newCtx.Contents[entry.Key] = entry.Value;
                }
            }

            replacement = RecursiveLoad(Path.Combine(subfolder, token.Value), flatten: true, newCtx, elem.Uid);
            if (replacement == null)
            {
                LastErrors.Add(new($"Failed to find Include file \"{token.Value}\"")
                {
                    File = token.FilePath,
                    Line = token.Line,
                    Column = token.Column,
                    Length = 1,
                });
                replacement = new Token()
                {
                    FilePath = token.FilePath,
                    Line = token.Line,
                    Column = token.Column,
                    Value = "~",
                    Context = token.Context,
                    Uid = token.Uid,
                    UserData = token.UserData,
                };
            }
            return true;
        }
        else if (elem is Block block)
        {
            foreach (var key in block.Contents.Keys)
            {
                block.Contents[key].Context = ctx;

                RecursiveLoadImpl(block.Contents[key], subfolder, flatten: true, ref ctx, out SourceElement se);
                block.Contents[key] = se;
            }
        }
        else if (elem is Array array)
        {
            for (int i = 0; i < array.Contents.Count; ++i)
            {
                array.Contents[i].Context = ctx;

                if (RecursiveLoadImpl(array.Contents[i], subfolder, flatten: false, ref ctx, out SourceElement se))
                {
                    array.Contents.RemoveAt(i);
                    // array.Contents.InsertRange(i, (se as Array).Contents);
                    if (se is Array)
                    {
                        array.Contents.InsertRange(i, (se as Array).Contents);
                        i += (se as Array).Contents.Count - 1;
                    }
                    else
                    {
                        array.Contents.Insert(i, se);
                    }
                }
                else
                {
                    array.Contents[i] = se;
                }
            }
        }

        replacement = elem;
        return false;
    }
}
