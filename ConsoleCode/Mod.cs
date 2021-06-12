using System;
using System.IO;
using Mono.CSharp;
using SpaceShared;
using StardewModdingAPI;

namespace ConsoleCode
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            helper.ConsoleCommands.Add("cs", "Execute C# code.", this.onCommandReceived);
        }

        private void onCommandReceived(string cmd, string[] args)
        {
            string line = string.Join(" ", args).Replace('`', '"');
            if (args[0] == "--script")
            {
                line = File.ReadAllText(Path.Combine(this.Helper.DirectoryPath, args[1]));
            }
            Log.Trace($"Input: {line}");
            try
            {
                var func = this.makeFunc(line);
                object result = null;
                func.Invoke(ref result);
                if (result == null)
                    Log.Info("Output: <null>");
                else if (result is string)
                    Log.Info($"Output: \"{result}\"");
                else
                    Log.Info($"Output: {result}");
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e);
            }
        }

        private CompiledMethod makeFunc(string userCode)
        {
            var settings = new CompilerSettings
            {
                Unsafe = true
            };

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    settings.AssemblyReferences.Add(asm.CodeBase);
                }
                catch
                {
                    //Log.trace("Couldn't add assembly " + asm + ": " + e);
                }
            }

            var eval = new Evaluator(new CompilerContext(settings, new ConsoleReportPrinter()));
            string code = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using xTile;";
            eval.Compile(code);
            return eval.Compile("IModHelper Helper = ConsoleCode.Mod.instance.Helper;\n" + userCode);
        }
    }
}
