using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;

#if IS_SPACECORE
namespace SpaceCore.Content
{
#else
namespace SpaceShared.Content
{
#endif

    public static class ContentExtensions
    {
        public static Token SimplifyToToken(this SourceElement se, ContentEngine ce)
        {
            Token tok = se as Token;
            if (se is Statement statement)
                tok = statement.FuncCall.Simplify(ce) as Token;
            if (tok == null)
                throw new ArgumentException($"Source element must simplify to string at {se.FilePath}:{se.Line}:{se.Column}");
            return tok;
        }

        public static SourceElement Simplify(this FuncCall fcall, ContentEngine ce)
        {
            if (fcall.Function == "^")
            {
                if (fcall.Parameters.Count < 1)
                    throw new ArgumentException($"Dynamic value function ^ must have one string parameter, at {fcall.FilePath}:{fcall.Line}:{fcall.Column}");

                var tok = fcall.Parameters[0].SimplifyToToken(ce);
                if (!fcall.Context.Contents.TryGetValue(tok, out SourceElement se))
                {
                    if (fcall.Parameters.Count == 1)
                    {
                        se = new Token()
                        {
                            FilePath = fcall.FilePath,
                            Line = fcall.Line,
                            Column = fcall.Column,
                            Value = $"invalid dynamic value {tok.Value} @ {fcall.FilePath}:{fcall.Line}:{fcall.Column}",
                            Context = fcall.Context,
                        };
                    }
                    else return fcall.Parameters[1];
                }

                return se;
            }
            else if (fcall.Function == "@")
            {
                if (fcall.Parameters.Count != 1)
                    throw new ArgumentException($"Asset path function @ must have exactly one string parameter, at {fcall.FilePath}:{fcall.Line}:{fcall.Column}");

                string path = Path.Combine(ce.ContentRootFolder, Path.GetDirectoryName(fcall.Parameters[0].FilePath), fcall.Parameters[0].SimplifyToToken(ce).Value).Replace('\\', '/');
                List<string> pathParts = new(path.Split('/'));
                for (int i = 1; i < pathParts.Count; ++i)
                {
                    if (pathParts[i] == "..")
                    {
                        pathParts.RemoveAt(i);
                        pathParts.RemoveAt(i - 1);
                    }
                }
                path = string.Join('/', pathParts);

                return new Token()
                {
                    FilePath = fcall.FilePath,
                    Line = fcall.Line,
                    Column = fcall.Column,
                    Value = ce.Helper.ModContent.GetInternalAssetName(path).Name,
                    Context = fcall.Context,
                };
            }
            else if (fcall.Function == "%")
            {
                if (fcall.Parameters.Count != 1)
                    throw new ArgumentException($"I18n function $ must have exactly one string parameter, at {fcall.FilePath}:{fcall.Line}:{fcall.Column}");

                return new Token()
                {
                    FilePath = fcall.FilePath,
                    Line = fcall.Line,
                    Column = fcall.Column,
                    Value = ce.Helper.Translation.Get(fcall.Parameters[0].SimplifyToToken(ce).Value),
                    Context = fcall.Context,
                };
            }
            else if (fcall.Function == "Vector2")
            {
                if (fcall.Parameters.Count != 2)
                    throw new ArgumentException($"Vector2 function must have exactly two float parameters, at {fcall.FilePath}:{fcall.Line}:{fcall.Column}");
                Token tokX = fcall.Parameters[0].SimplifyToToken(ce);
                Token tokY = fcall.Parameters[1].SimplifyToToken(ce);
                if (!float.TryParse(tokX.Value, out float x) || !float.TryParse(tokY.Value, out float y))
                    throw new ArgumentException($"Vector2 function must have exactly two float parameters, at {fcall.FilePath}:{fcall.Line}:{fcall.Column}");

                return new Block()
                {
                    FilePath = fcall.FilePath,
                    Line = fcall.Line,
                    Column = fcall.Column,
                    Contents =
                    {
                        { new Token() { Value = "X" }, tokX },
                        { new Token() { Value = "Y" }, tokY },
                    },
                    Context = fcall.Context,
                };
            }
            else if (fcall.Function == "Rectangle")
            {
                if (fcall.Parameters.Count != 4)
                    throw new ArgumentException($"Vector2 function must have exactly two integer parameters, at {fcall.FilePath}:{fcall.Line}:{fcall.Column}");
                Token tokX = fcall.Parameters[0].SimplifyToToken(ce);
                Token tokY = fcall.Parameters[1].SimplifyToToken(ce);
                Token tokW = fcall.Parameters[2].SimplifyToToken(ce);
                Token tokH = fcall.Parameters[3].SimplifyToToken(ce);
                if (!int.TryParse(tokX.Value, out int x) || !int.TryParse(tokY.Value, out int y) || !int.TryParse(tokX.Value, out int w) || !int.TryParse(tokY.Value, out int h))
                    throw new ArgumentException($"Vector2 function must have exactly two float parameters, at {fcall.FilePath}:{fcall.Line}:{fcall.Column}");

                return new Block()
                {
                    FilePath = fcall.FilePath,
                    Line = fcall.Line,
                    Column = fcall.Column,
                    Contents =
                    {
                        { new Token() { Value = "X" }, tokX },
                        { new Token() { Value = "Y" }, tokY },
                        { new Token() { Value = "Width" }, tokW },
                        { new Token() { Value = "Height" }, tokH },
                    },
                    Context = fcall.Context,
                };
            }
            else if (fcall.Function == "Concatenate")
            {
                string str = "";
                foreach (var param in fcall.Parameters)
                {
                    str += param.SimplifyToToken(ce).Value;
                }

                return new Token()
                {
                    FilePath = fcall.FilePath,
                    Line = fcall.Line,
                    Column = fcall.Column,
                    Value = str,
                    Context = fcall.Context,
                };
            }

            return fcall;
        }
    }

    public class ContentEngine
    {
        protected IManifest Manifest { get; }
        protected internal IModHelper Helper { get; }

        public string ContentRootFolder { get; private set; }
        public string ContentRootFolderActual { get; private set; }
        public string ContentRootFile { get; }

        private ContentParser Parser { get; }

        protected Array Contents { get; set; }

        public ContentEngine(IManifest manifest, IModHelper helper, string contentRootFile)
        {
            Manifest = manifest;
            Helper = helper;
            ContentRootFolder = Path.GetDirectoryName(contentRootFile);
            ContentRootFolderActual = Path.Combine(Helper.DirectoryPath, ContentRootFolder);
            ContentRootFile = contentRootFile;
            Parser = new(Manifest, Helper, ContentRootFolder);

            Reload();
        }

        public void OnReloadMonitorInstead(string pathModifier, [CallerFilePath] string path = "" )
        {
            if (path == "")
                throw new ArgumentException("No caller file path?");

            Parser.ContentRootFolderActual = ContentRootFolderActual = Path.Combine(Path.GetDirectoryName(path), pathModifier);
        }

        public void Reload()
        {
            Contents = (Array)RecursiveLoad(Path.GetFileName(ContentRootFile), flatten: false);
            PostReload();
        }

        protected virtual void PostReload()
        {
        }

        private SourceElement RecursiveLoad(string file, bool flatten = true, Block ctx = null )
        {
            ctx ??= new();

            Array contents = Parser.Load(file);
            RecursiveLoadImpl( contents, Path.GetDirectoryName( file ), flatten: false, ctx, out SourceElement se );
            contents = (Array)se;

            if (flatten && contents.Contents.Count == 1)
                return contents.Contents[0];

            return contents;
        }

        private bool RecursiveLoadImpl(SourceElement elem, string subfolder, bool flatten, Block ctx, out SourceElement replacement)
        {
            elem.Context = ctx;

            if (elem is Statement statement)
            {
                statement.FuncCall.Context = ctx;
                for (int i = 0; i < statement.FuncCall.Parameters.Count; ++i)
                {
                    statement.FuncCall.Parameters[i].Context = ctx;
                    RecursiveLoadImpl(statement.FuncCall.Parameters[i], subfolder, flatten: true, ctx, out var param);
                    statement.FuncCall.Parameters[i] = param;
                }
                if ( statement.Data != null )
                    statement.Data.Context = ctx;

                if (statement.FuncCall.Function == "Include")
                {
                    if (statement.FuncCall.Parameters.Count < 1 || statement.FuncCall.Parameters[0] is not Token token || !token.IsString)
                    {
                        throw new ArgumentException($"Include at {statement.FuncCall.FilePath}:{statement.FuncCall.Line}:{statement.FuncCall.Column} needs a string for the include path");
                    }
                    if (statement.FuncCall.Parameters.Count >= 2 && statement.FuncCall.Parameters[1] is not Block)
                    {
                        throw new ArgumentException($"Include context at {statement.FuncCall.FilePath}:{statement.FuncCall.Line}:{statement.FuncCall.Column} must be a block");
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

                    replacement = RecursiveLoad(Path.Combine(subfolder, token.Value), flatten, newCtx);
                    return true;
                }
                else
                {
                    SourceElement se = statement.FuncCall.Simplify(this);
                    if ( se as FuncCall == statement.FuncCall )
                    {
                        if (statement.Data != null)
                            RecursiveLoadImpl(statement.Data, subfolder, flatten: true, ctx, out se);
                        statement.Data = se;
                    }
                    else
                    {
                        replacement = se;
                        return true;
                    }
                }
            }
            else if ( elem is FuncCall fcall && fcall.Function == "Include" )
            {
                foreach (var param in fcall.Parameters)
                    param.Context = ctx;

                if ( fcall.Parameters.Count < 1 || fcall.Parameters[0] is not Token token || !token.IsString )
                {
                    throw new ArgumentException($"Include at {fcall.FilePath}:{fcall.Line}:{fcall.Column} needs a string for the include path");
                }
                if (fcall.Parameters.Count >= 2 && fcall.Parameters[1] is not Block)
                {
                    throw new ArgumentException($"Include context at {fcall.FilePath}:{fcall.Line}:{fcall.Column} must be a block");
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

                replacement = RecursiveLoad(Path.Combine(subfolder, token.Value), flatten: true, newCtx);
                return true;
            }
            else if ( elem is Block block )
            {
                foreach ( var key in block.Contents.Keys )
                {
                    block.Contents[key].Context = ctx;

                    RecursiveLoadImpl(block.Contents[key], subfolder, flatten: true, ctx, out SourceElement se);
                    block.Contents[key] = se;
                }
            }
            else if (elem is Array array)
            {
                for ( int i = 0; i < array.Contents.Count; ++i )
                {
                    array.Contents[i].Context = ctx;

                    if (RecursiveLoadImpl(array.Contents[i], subfolder, flatten: false, ctx, out SourceElement se ))
                    {
                        array.Contents.RemoveAt(i);
                        array.Contents.InsertRange(i, (se as Array).Contents);
                        i += (se as Array).Contents.Count - 1;
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
}
