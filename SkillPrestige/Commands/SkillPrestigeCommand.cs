using System;
using System.Linq;
using SkillPrestige.Logging;
using StardewModdingAPI;

namespace SkillPrestige.Commands
{
    /// <summary>
    /// Represents a command called in the SMAPI console interface.
    /// </summary>
    internal abstract class SkillPrestigeCommand
    {
        /// <summary>
        /// The name used to call the command in the console.
        /// </summary>
        private string Name { get; }

        /// <summary>
        /// The help description of the command.
        /// </summary>
        private string Description { get; }

        /// <summary>
        /// Whether or not the command is used only in test mode.
        /// </summary>
        protected abstract bool TestingCommand { get; }

        protected SkillPrestigeCommand(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Registers a command with the SMAPI console.
        /// </summary>
        private void RegisterCommand(ICommandHelper helper)
        {
            Logger.LogInformation($"Registering {Name} command...");
            helper.Add(Name, Description, (name, args) => Apply(args));
            Logger.LogInformation($"{Name} command registered.");
        }

        /// <summary>
        /// Applies the effect of a command when it is called from the console.
        /// </summary>
        protected abstract void Apply(string[] args);

        /// <summary>
        /// Registers all commands found in the system.
        /// </summary>
        /// <param name="helper">The SMAPI Command helper.</param>
        /// <param name="testCommands">Whether or not you wish to only register testing commands.</param>
        public static void RegisterCommands(ICommandHelper helper, bool testCommands)
        {
            var concreteCommands = AppDomain.CurrentDomain.GetNonSystemAssemblies().SelectMany(x => x.GetTypesSafely())
                .Where(x => x.IsSubclassOf(typeof(SkillPrestigeCommand)) && !x.IsAbstract);
            foreach (var commandType in concreteCommands)
            {
                var command = (SkillPrestigeCommand)Activator.CreateInstance(commandType);
                if (!(testCommands ^ command.TestingCommand)) command.RegisterCommand(helper);
            }
        }
    }
}
