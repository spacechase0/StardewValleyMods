using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;
using StardewModdingAPI;

namespace ConsoleCode
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            helper.ConsoleCommands.Add("cs", "Execute C# code.", onCommandReceived);
        }

        private void onCommandReceived( string cmd, string[] args )
        {
            string line = string.Join(" ", args);
            if ( args[0] == "--script" )
            {
                line = File.ReadAllText(Path.Combine(Helper.DirectoryPath, args[1]));
            }
            Log.trace($"Input: {line}");
            try
            {
                var func = makeFunc(line);
                object result = func.Invoke(null, new object[] { });
                if (result == null)
                    Log.info("Output: <null>");
                else if (result is string)
                    Log.info($"Output: \"{result}\"");
                else
                    Log.info($"Output: {result}");
            }
            catch (CompilationException e)
            {
                Log.error("Error(s) when compiling: ");
                foreach ( CompilerError error in e.Results.Errors )
                {
                    Log.error($"{error}");
                }
            }
            catch (Exception e)
            {
                Log.error("Exception: " + e);
            }
        }

        int num = 0;
        private MethodInfo makeFunc(string userCode)
        {
            var libs = new List<string>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    libs.Add(asm.Location);
                }
                catch (Exception e)
                {
                    //Log.trace("Couldn't add assembly " + asm + ": " + e);
                }
            }

            string code = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using xTile;
namespace ConsoleCode
{
    public class UserCode<ITER>
    {
        public static IModHelper Helper { get { return ConsoleCode.Mod.instance.Helper; } }
        public static object func()
        {
            <USER_CODE>
            return null;
        }
    }
}
";

            code = code.Replace("<ITER>", num.ToString());
            code = code.Replace("<USER_CODE>", userCode);

            //Log.trace(code);

            var provider = new CSharpCodeProvider();
            var @params = new CompilerParameters();
            @params.GenerateInMemory = true;
            @params.GenerateExecutable = false;
            @params.IncludeDebugInformation = true;
            foreach (var lib in libs)
                @params.ReferencedAssemblies.Add(lib);
            CompilerResults results = provider.CompileAssemblyFromSource(@params, code);
            if (results.Errors.Count > 0)
                throw new CompilationException(results);
            return results.CompiledAssembly.GetType($"ConsoleCode.UserCode{num++}").GetMethod("func");
        }

        private class CompilationException : Exception
        {
            public CompilerResults Results { get; }

            public CompilationException( CompilerResults results )
            {
                Results = results;
            }
        }
    }
}
