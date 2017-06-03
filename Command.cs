using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore
{
    public static class Command
    {
        private static Dictionary<string, Action<string[]>> commands = new Dictionary< string, Action< string[] > >();

        internal static void register( string name, Action< string[] > callback )
        {
            // TODO: Load documentation from a file.
            SpaceCore.instance.Helper.ConsoleCommands.Add(name, "TO BE IMPLEMENTED", doCommand);
            commands.Add(name, callback);
        }

        private static void doCommand( string cmd, string[] args )
        {
            try
            {
                commands[cmd].Invoke(args);
            }
            catch (Exception e)
            {
                Log.error("Error running command.");
                Log.debug("Exception: " + e);
            }
        }
    }
}
