using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpaceCore.Content.StardewFunctions;

namespace SpaceCore.Content.LanguageServer;
internal class AppContentEngine : ContentEngine
{
    private App app;

    public AppContentEngine(string uid, string dirPath, string contentRootFile, App app)
    :   base(uid, dirPath, contentRootFile, (asset) => asset, (seed, isStatic) => new Random(seed))
    {
        this.app = app;

        AddSimplifyFunction(new ActorFunction());
        AddSimplifyFunction(new ContentPatcherTokenFunction());
        AddSimplifyFunction(new FacingFunction());
        AddSimplifyFunction(new HasModFunction());
        AddSimplifyFunction(new LocalizationFunction());
        AddSimplifyFunction(new QuickQuestionFunction());
    }

    internal static readonly Regex regexBecauseLazy = new("^(/|\\\\)(\\w+)(%3[aA]|:)");

    protected override Array ParserLoad(string file_, string uidContext)
    {
        // This is a terrible hack
        string file = file_;
        if (file.StartsWith("file:") && file.StartsWith("file:\\"))
            file = file.Replace('\\', '/').Replace("file:/", "file:///");
        else if (!file.StartsWith("file:///"))
            file = $"file:///{file}";

        string path = ContentRootFolderActual == "" ? file : Path.Combine(ContentRootFolderActual, file_);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            path = regexBecauseLazy.Replace(path, "$2:");
        Console.Error.WriteLine($"test: {uidContext} {file} {path}");
        Uri uri = new Uri(path, UriKind.Absolute);
        if (app.docs.TryGetValue(uri, out var doc))
            return Parser.LoadText(doc.doc.text, file, uidContext);

        Console.Error.WriteLine($"Falling back to file loading for path: {file} {path} {uri} {uri.AbsoluteUri} {uri.AbsolutePath}");
        Console.Error.WriteLine("Options were: " + string.Join(", ", app.docs.Keys.Select(u => u.ToString()).ToArray()));
        string path2 = path.Replace("file://", "");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            path2 = regexBecauseLazy.Replace(path2, "$2:");
        Console.Error.WriteLine($"{path2} vs {path}");
        return base.ParserLoad(path2, uidContext);
    }
}
