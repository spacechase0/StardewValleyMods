using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Tools;
using StardewValley.Tools;
using StardewValley.ItemTypeDefinitions;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="ToolDataDefinition"/></summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ToolDataDefinitionPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<ToolDataDefinition>("CreateToolInstance"),
                postfix: this.GetHarmonyMethod(nameof(After_CreateToolInstance))
            );
        }

        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="ToolDataDefinition.CreateToolInstance"/>.</summary>
        /// <param name="tool">The original result.</param>
        /// <param name="toolData">The data used to create the tool.</param>
        /// <returns>The original tool if it is valid, a custom tool if it is registered with <see cref="Api.RegisterSerializerType"/>,
        /// or an error tool if neither is true</returns>
        private static Tool After_CreateToolInstance(Tool tool, ToolData toolData)
        {
            // valid tool, do not interfere
            if (tool is not ErrorTool)
                return tool;

            // try to find a registered type matching provided type name
            Type type = SpaceCore.ModTypes.Where(t => t.AssemblyQualifiedName == toolData.ClassName).FirstOrDefault();

            // not found
            if (type is null)
            {
                Log.Debug($"Could not instantiate tool '{toolData.Name}'. Type '{toolData.ClassName}' does not exist in StardewValley.Tools, and was not registered with SpaceCore's serializer.");
                return tool;
            }

            // not a tool
            if (!typeof(Tool).IsAssignableFrom(type))
            {
                Log.Error($"Could not instantiate tool '{toolData.Name}'. Type '{toolData.ClassName}' does not inherit StardewValley.Tools.Tool.");
                return tool;
            }

            try
            {
                Tool ret = (Tool)Activator.CreateInstance(type);
                Log.Trace($"Successfully instantiated tool '{toolData.Name}' with type '{toolData.ClassName}'.");
                return ret;
            }
            catch (Exception ex)
            {
                Log.Error($"Could not instantiate tool '{toolData.Name}' with type '{toolData.ClassName}'. {ex.ToString}");
                return tool;
            }
        }
    }
}
