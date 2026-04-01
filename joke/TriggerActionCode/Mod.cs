using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SpaceShared;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;

namespace TriggerActionCode;

public class Mod : BaseMod<Mod>
{
    protected override void ModEntry()
    {
        TriggerActionManager.RegisterAction("cs", OnActionTriggered);
    }

    private bool OnActionTriggered(string[] args, TriggerActionContext context, out string error)
    {
        string line = ArgUtility.UnsplitQuoteAware(args.Skip(1).ToArray(), ' ').Replace('`', '"');
        string[] scriptArgs = null;
        if (args[1] == "--script")
        {
            line = File.ReadAllText(Util.FetchFullPath(Helper.ModRegistry, args[2]));
            scriptArgs = args.Skip(3).ToArray();
        }
        Log.Trace($"Action Code: {line}");
        try
        {
            var func = this.MakeFunc(line);
            if (func != null)
            {
                error = null;
                return (bool)func?.Invoke(null, new object[] { Monitor, Helper, context, scriptArgs ?? [] });
            }
            else
            {
                error = "Failed to make function";
                return false;
            }
        }
        catch (Exception e)
        {
            Log.Error("Exception: " + e);
            error = "exception thrown when running trigger action code";
            return false;
        }
    }

    private static int iter = 0;

    private MethodInfo MakeFunc(string userCode)
    {
        List<string> asms = new();
        List<MetadataReference> refs = new();
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var mr = MetadataReference.CreateFromFile(asm.Location);
                if (asm.GetName().Name == "MonoMod.Common")
                {
                    mr = mr.WithAliases(new string[] { "MMC" });
                }
                refs.Add(mr);
                asms.Add(asm.GetName().Name);
            }
            catch (Exception e)
            {
                //Log.Trace("Couldn't add assembly " + asm + ": " + e);
            }
        }

        int i_ = 0;
        string attrs = "";
        foreach (var r in refs)
        {
            string str = "\"" + asms[i_] + "\"";
            attrs += $"[assembly: MMC::System.Runtime.CompilerServices.IgnoresAccessChecksTo({str})]\n";
            ++i_;
        }
        string code = $@"
extern alias MMC;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using xTile;
using System.Runtime.CompilerServices;

{attrs}
namespace TriggerActionCode
{{
    public class UserCode{iter}
    {{
        public static bool Main(IMonitor Monitor, IModHelper Helper, StardewValley.Delegates.TriggerActionContext context, string[] args)
        {{
            {userCode}
            return true;
        }}
    }}
}}
";
        var opts = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithMetadataImportOptions(MetadataImportOptions.All);

        // https://stackoverflow.com/a/72653299
        var topLevelBinderFlagsProperty = opts.GetType().GetProperty("TopLevelBinderFlags", BindingFlags.Instance | BindingFlags.NonPublic);
        object obj = Enum.ToObject(opts.GetType().Assembly.GetType("Microsoft.CodeAnalysis.CSharp.BinderFlags"), (uint)(1 << 22));
        topLevelBinderFlagsProperty.SetValue(opts, obj);

        CSharpCompilation compilation = CSharpCompilation.Create(Path.GetRandomFileName(), new[] { CSharpSyntaxTree.ParseText(code) }, refs, opts);

        using MemoryStream ms = new();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var fails = result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);

            StringBuilder sb = new();
            sb.AppendLine("Errors: ");
            foreach (var fail in fails)
            {
                sb.AppendLine($"{fail.Id}: {fail.GetMessage()}");
            }
            Log.Error(sb.ToString());

            return null;
        }
        else
        {
            int i = iter++;

            ms.Seek(0, SeekOrigin.Begin);
            Assembly asm = AssemblyLoadContext.Default.LoadFromStream(ms);
            Type type = asm.GetType($"TriggerActionCode.UserCode{i}");
            MethodInfo meth = type.GetMethod("Main");
            return meth;
        }
    }

}
