using StardewModdingAPI;

namespace SpaceShared.ConsoleCommands
{
    /// <summary>The base implementation for a console command.</summary>
    internal abstract class ConsoleCommand : IConsoleCommand
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The command name the user must type.</summary>
        public string Name { get; }

        /// <summary>The command description.</summary>
        public string Description { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public abstract void Handle(IMonitor monitor, string command, string[] args);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The command name the user must type.</param>
        /// <param name="description">The command description.</param>
        protected ConsoleCommand(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>Log an error indicating incorrect usage.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="error">A sentence explaining the problem.</param>
        protected void LogUsageError(IMonitor monitor, string error)
        {
            monitor.Log($"{error} Type 'help {this.Name}' for usage.", LogLevel.Error);
        }
    }
}
