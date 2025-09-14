using System;
using System.Collections.Generic;
using DynamicGameAssets.PackData;
using Pathoschild.Stardew.Automate;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace DGAAutomate
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The backing field for <see cref="CustomMachineRecipes"/>.</summary>
        private static Lazy<IDictionary<string, List<MachineRecipePackData>>> CustomMachineRecipesImpl;


        /*********
        ** Accessors
        *********/
        /// <summary>The custom machine recipes registered with Dynamic Game Assets.</summary>
        public static IDictionary<string, List<MachineRecipePackData>> CustomMachineRecipes => Mod.CustomMachineRecipesImpl.Value;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;
            Mod.CustomMachineRecipesImpl = new(this.GetCustomMachineRecipes);

            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var automate = this.Helper.ModRegistry.GetApi<IAutomateAPI>("Pathoschild.Automate");
            automate.AddFactory(new MyAutomationFactory());
        }

        /// <summary>Get the custom machine recipes registered with Dynamic Game Assets.</summary>
        private IDictionary<string, List<MachineRecipePackData>> GetCustomMachineRecipes()
        {
            return this.Helper.Reflection
                .GetField<Dictionary<string, List<MachineRecipePackData>>>(typeof(DynamicGameAssets.Mod), "customMachineRecipes")
                .GetValue();
        }
    }
}
