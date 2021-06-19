using System;
using System.Collections.Generic;
using SpaceShared;

namespace SpaceCore.Framework
{
    internal static class Command
    {
        private static readonly Dictionary<string, Action<string[]>> Commands = new();

        internal static void Register(string name, Action<string[]> callback)
        {
            // TODO: Load documentation from a file.
            SpaceCore.Instance.Helper.ConsoleCommands.Add(name, "TO BE IMPLEMENTED", Command.DoCommand);
            Command.Commands.Add(name, callback);
        }

        private static void DoCommand(string cmd, string[] args)
        {
            try
            {
                Command.Commands[cmd].Invoke(args);
            }
            catch (Exception e)
            {
                Log.Error("Error running command.");
                Log.Debug("Exception: " + e);
            }
        }
    }
}
