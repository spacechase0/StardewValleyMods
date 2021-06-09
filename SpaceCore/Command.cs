using System;
using System.Collections.Generic;
using SpaceShared;

namespace SpaceCore
{
    public static class Command
    {
        private static readonly Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();

        internal static void register(string name, Action<string[]> callback)
        {
            // TODO: Load documentation from a file.
            SpaceCore.instance.Helper.ConsoleCommands.Add(name, "TO BE IMPLEMENTED", Command.doCommand);
            Command.commands.Add(name, callback);
        }

        private static void doCommand(string cmd, string[] args)
        {
            try
            {
                Command.commands[cmd].Invoke(args);
            }
            catch (Exception e)
            {
                Log.error("Error running command.");
                Log.debug("Exception: " + e);
            }
        }
    }
}
