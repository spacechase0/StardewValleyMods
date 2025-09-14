using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Content.Functions;
internal class AssetPathFunction : BaseFunction
{
    public bool AbsolutePaths { get; }

    public AssetPathFunction(bool absolute)
    :   base( absolute ? "@@" : "@" )
    {
        AbsolutePaths = absolute;
    }

    public override SourceElement Simplify(FuncCall fcall, ContentEngine ce)
    {
        if (fcall.Parameters.Count < 1)
            return LogErrorAndGetToken($"Asset path function {Name} must have exactly one string parameter", fcall, ce);

        string sep = "/";
        if (fcall.Parameters.Count >= 2)
        {
            sep = fcall.Parameters[1].SimplifyToToken(ce).Value;
        }

        string addonPath = fcall.Parameters[0].SimplifyToToken(ce).Value;
        if (!addonPath.StartsWith('/'))
        {
            string dir = Path.GetDirectoryName(fcall.Parameters[0].FilePath);
            addonPath = dir != "" ? Path.Combine(dir, addonPath) : addonPath;
        }
        else
            addonPath = addonPath.Remove(0, 1);
        string path = Path.Combine(AbsolutePaths ? ce.ContentRootFolderActual : ce.ContentRootFolder, addonPath).Replace('\\', '/');
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

        path = path.Replace("/", sep);

        return new Token()
        {
            FilePath = fcall.FilePath,
            Line = fcall.Line,
            Column = fcall.Column,
            Value = AbsolutePaths ? path : ce.AssetNameSimplfier( path ),
            IsString = true,
            Context = fcall.Context,
            Uid = fcall.Uid,
            UserData = fcall.UserData,
        };
    }
}
