using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceCore.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewValley.Menus;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="LoadGameMenu"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class LoadGameMenuPatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages the custom save serialization.</summary>
        private static SerializerManager SerializerManager;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="serializerManager">Manages the custom save serialization.</param>
        public LoadGameMenuPatcher(SerializerManager serializerManager)
        {
            LoadGameMenuPatcher.SerializerManager = serializerManager;
        }

        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<LoadGameMenu>("FindSaveGames"),
                prefix: this.GetHarmonyMethod(nameof(Before_FindSaveGames))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="LoadGameMenu.FindSaveGames"/>.</summary>
        public static void Before_FindSaveGames()
        {
            LoadGameMenuPatcher.SerializerManager.InitializeSerializers();
        }
    }
}
