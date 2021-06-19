using System;
using System.IO;
using Mono.CSharp;
using SpaceShared;
using StardewModdingAPI;

namespace ConsoleCode
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.ConsoleCommands.Add("cs", "Execute C# code.", this.OnCommandReceived);
        }

        private void OnCommandReceived(string cmd, string[] args)
        {
            string line = string.Join(" ", args).Replace('`', '"');
            if (args[0] == "--script")
            {
                line = File.ReadAllText(Path.Combine(this.Helper.DirectoryPath, args[1]));
            }
            Log.Trace($"Input: {line}");
            try
            {
                var func = this.MakeFunc(line);
                object result = null;
                func.Invoke(ref result);

                switch (result)
                {
                    case null:
                        Log.Info("Output: <null>");
                        break;

                    case string:
                        Log.Info($"Output: \"{result}\"");
                        break;

                    default:
                        Log.Info($"Output: {result}");
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e);
            }
        }

        private CompiledMethod MakeFunc(string userCode)
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
            eval.Compile(@"
                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using StardewModdingAPI;
                using Microsoft.Xna.Framework;
                using Microsoft.Xna.Framework.Graphics;
                using StardewValley;
                using xTile;
            ");
            return eval.Compile(userCode);
        }
    }
}
