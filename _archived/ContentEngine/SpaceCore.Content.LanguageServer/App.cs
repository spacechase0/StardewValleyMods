using System.Runtime.InteropServices;
using LanguageServer;
using LanguageServer.Parameters.General;
using LanguageServer.Parameters.TextDocument;
using LanguageServer.Parameters.Workspace;

namespace SpaceCore.Content.LanguageServer;

// Based on https://github.com/matarillo/vscode-languageserver-csharp-example/
internal class App : ServiceConnection
{
    internal Dictionary<Uri, (TextDocumentItem doc, int version)> docs = new();

    private ContentEngine engine;
    private Dictionary<Uri, ContentEngine> workspaceEngines = new();

    public App(Stream input, Stream output)
        : base(input, output)
    {
    }

    protected override Result<InitializeResult, ResponseError<InitializeErrorData>> Initialize(InitializeParams @params)
    {
        // TODO: Read unique ID from manifest.json
        engine = new AppContentEngine("meow", "", "content.spacecore", this);
        if ((@params.rootUri?.AbsolutePath ?? "") != "")
        {
            workspaceEngines.Add(@params.rootUri, new AppContentEngine("meow", @params.rootUri?.AbsolutePath, "content.spacecore", this));
        }
        InitializeResult result = new()
        {
            capabilities = new ServerCapabilities()
            {
                textDocumentSync = TextDocumentSyncKind.Full,
                completionProvider = new CompletionOptions()
                {
                    resolveProvider = true
                }
            }
        };
        return Result<InitializeResult, ResponseError<InitializeErrorData>>.Success(result);
    }

    protected override void DidChangeWorkspaceFolders(DidChangeWorkspaceFoldersParams @params)
    {
        foreach (var entry in @params.@event.added)
            workspaceEngines.TryAdd(entry.uri, new AppContentEngine("meow", entry.uri?.AbsolutePath ?? "", "content.spacecore", this));
        foreach (var entry in @params.@event.removed)
            workspaceEngines.Remove(entry.uri);
    }

    private SourceElement lastParse = null;

    private void Validate(TextDocumentItem doc)
    {
        List<Diagnostic> errors = new();
        try
        {
            var engine = this.engine;
            foreach (var entry in workspaceEngines.Keys)
            {
                if (entry.IsBaseOf(doc.uri))
                    engine = workspaceEngines[entry];
            }

            string absPath = doc.uri.AbsoluteUri.Replace("file://", "");
            string baseFolder = engine.ContentRootFolderActual;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                absPath = AppContentEngine.regexBecauseLazy.Replace(absPath, "$2:");
                baseFolder = AppContentEngine.regexBecauseLazy.Replace(baseFolder, "$2:");
            }
            Console.Error.WriteLine("validating " + doc.uri.AbsoluteUri + " " + absPath + " " + baseFolder);
            string path = engine.ContentRootFolderActual != "" ? Path.GetRelativePath(baseFolder, absPath) : absPath;
            //parser.LoadText(doc.text, path);
            engine.LastErrors.Clear();
            var lastParse = (Array) engine.RecursiveLoad(path, flatten: false);
            foreach (var entry in lastParse.Contents)
            {
                entry.DoSimplify(engine, true);
            }

            foreach (var e in engine.LastErrors)
            {
                Console.Error.WriteLine("Error: " + e);
                if (e.File.Replace('/', '\\') != path.Replace('/', '\\') && e.File.Replace('/', '\\') != absPath.Replace('/', '\\'))
                    continue;
                if (e.Line == 0 && e.Column == 0)
                {
                    // Generally only happens for the starting and ending added [] tokens
                    // The user doesn't need to see that
                    /*
                    errors.Add(new()
                    {
                        severity = DiagnosticSeverity.Error,
                        range = new()
                        {
                            start = new() { line = 0, character = 0 },
                            end = new() { line = 0, character = 1 },
                        },
                        message = "Error not noticed until end of file (do you have an open function call somewhere?)? " + e.Message,
                        source = "ex", // ?
                    });
                    */
                }
                else
                {
                    errors.Add(new()
                    {
                        severity = DiagnosticSeverity.Error,
                        range = new()
                        {
                            start = new() { line = e.Line - 1, character = e.Column - 1 },
                            end = new() { line = e.Line - 1, character = e.Column - 1 + e.Length },
                        },
                        message = e.Message,
                        source = "ex", // ?
                    });
                }
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("meow? " + e.Message);
            Console.Error.WriteLine(e);
        }

        Proxy.TextDocument.PublishDiagnostics(new()
        {
            uri = doc.uri,
            diagnostics = errors.ToArray(),
        });
    }

    private void ApplyChanges(TextDocumentItem doc, TextDocumentContentChangeEvent[] changes)
    {
        foreach (var change in changes)
        {
            if (change == null)
                Console.Error.WriteLine("null change?");

            if (change.range == null)
            {
                // what
                doc.text = change.text;
                continue;
            }

            Console.Error.WriteLine($"Change at {change.text} {change.rangeLength ?? -1} {change.range?.start?.line ?? -1} {change.range?.start?.character ?? -1} {change.range?.end?.line ?? -1} {change.range?.end?.character ?? -1}");
            string str = doc.text;
            int begin = GetPosition(str, (int)change.range.start.line, (int)change.range.start.character);
            int end = GetPosition(str, (int)change.range.end.line, (int)change.range.end.character);
            doc.text = str.Substring(0, begin) + change.text + str.Substring(end);
        }
    }

    
    private static int GetPosition(string text, int line, int character)
    {
        int pos = 0;
        for (; 0 <= line; line--)
        {
            int lf = text.IndexOf('\n', pos);
            if (lf < 0)
            {
                return text.Length;
            }
            pos = lf + 1;
        }
        int linefeed = text.IndexOf('\n', pos);
        int max = 0;
        if (linefeed < 0)
        {
            max = text.Length;
        }
        else if (linefeed > 0 && text[linefeed - 1] == '\r')
        {
            max = linefeed - 1;
        }
        else
        {
            max = linefeed;
        }
        pos += character;
        return (pos < max) ? pos : max;
    }

    protected override void DidOpenTextDocument(DidOpenTextDocumentParams @params)
    {
        docs.TryAdd(@params.textDocument.uri, new(@params.textDocument, 0));
        Validate(@params.textDocument);
    }

    protected override void DidCloseTextDocument(DidCloseTextDocumentParams @params)
    {
        docs.Remove( @params.textDocument.uri);
    }

    protected override void DidChangeTextDocument(DidChangeTextDocumentParams @params)
    {
        if (!docs.TryGetValue(@params.textDocument.uri, out var doc) || doc.version >= @params.textDocument.version)
            return;

        ApplyChanges(doc.doc, @params.contentChanges);
        Validate(doc.doc);
    }

    protected override Result<CompletionResult, ResponseError> Completion(CompletionParams @params)
    {
        return base.Completion(@params);
    }

    protected override Result<CompletionItem, ResponseError> ResolveCompletionItem(CompletionItem @params)
    {
        return base.ResolveCompletionItem(@params);
    }
}
