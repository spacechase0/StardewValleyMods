using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace DGAAutomate
{
    public class MyMachine : IMachine
    {
        private readonly CustomBigCraftable big;

        public string MachineTypeID => "DGA/" + big.FullId;

        public GameLocation Location { get; }

        public Rectangle TileArea { get; }

        public MyMachine( CustomBigCraftable cbig, GameLocation location, Vector2 tile )
        {
            this.big = cbig;
            this.Location = location;
            this.TileArea = new Rectangle( ( int ) tile.X, ( int ) tile.Y, 1, 1 );
        }

        public ITrackedStack GetOutput()
        {
            return new TrackedItem( big.heldObject.Value, onEmpty: item =>
            {
                this.big.heldObject.Value = null;
                this.big.readyForHarvest.Value = false;
            } );
        }

        public MachineState GetState()
        {
            if ( this.big.heldObject.Value == null )
                return MachineState.Empty;
            if ( this.big.readyForHarvest.Value )
                return MachineState.Done;
            return MachineState.Processing;
        }

        public bool SetInput( IStorage input )
        {
            if ( !DynamicGameAssets.Mod.customMachineRecipes.ContainsKey( big.FullId ) )
                return false;

            foreach ( var recipe in DynamicGameAssets.Mod.customMachineRecipes[ big.FullId ] )
            {
                if ( !input.TryGetIngredient( ( item ) => recipe.Ingredients[ 0 ].Matches( item.Sample ), recipe.Ingredients[ 0 ].Quantity, out IConsumable firstConsume ) )
                    continue;

                bool foundAll = true;
                IConsumable[] consumes = new IConsumable[ recipe.Ingredients.Count ];
                consumes[ 0 ] = firstConsume;
                for ( int i = 1; i < recipe.Ingredients.Count; ++i )
                {
                    if ( input.TryGetIngredient( ( item ) => recipe.Ingredients[ i ].Matches( item.Sample ), recipe.Ingredients[ i ].Quantity, out IConsumable consumed ) )
                        consumes[ i ] = consumed;
                    else
                    {
                        foundAll = false;
                        break;
                    }
                }

                if ( foundAll )
                {
                    foreach ( var consume in consumes )
                        consume.Take();

                    this.big.TextureOverride = recipe.MachineWorkingTextureOverride;
                    this.big.PendingTextureOverride = recipe.MachineFinishedTextureOverride;
                    this.big.PulseIfWorking = recipe.MachinePulseWhileWorking;
                    this.big.heldObject.Value = ( StardewValley.Object ) recipe.Result.Choose().Create();
                    this.big.MinutesUntilReady = recipe.MinutesToProcess;

                    if ( recipe.StartWorkingSound != null )
                        Location.playSound( recipe.StartWorkingSound );

                    if ( recipe.WorkingLightOverride.HasValue )
                    {
                        bool oldIsLamp = this.big.isLamp.Value;
                        this.big.isLamp.Value = recipe.WorkingLightOverride.Value;
                        if ( !oldIsLamp && this.big.isLamp.Value )
                            this.big.initializeLightSource( this.big.tileLocation.Value );
                        else if ( oldIsLamp && !this.big.isLamp.Value )
                            Location.removeLightSource( ( int ) ( this.big.tileLocation.X * 797f + this.big.tileLocation.Y * 13f + 666f ) );
                    }
                }
            }

            return false;
        }
    }

    public class MyAutomationFactory : IAutomationFactory
    {
        public IAutomatable GetFor( StardewValley.Object obj, GameLocation location, in Vector2 tile )
        {
            if ( obj is CustomBigCraftable cbig )
            {
                if ( DynamicGameAssets.Mod.customMachineRecipes.ContainsKey( cbig.FullId ) )
                    return new MyMachine( cbig, location, tile );
            }

            return null;
        }

        public IAutomatable GetFor( TerrainFeature feature, GameLocation location, in Vector2 tile )
        {
            return null;
        }

        public IAutomatable GetFor( Building building, BuildableGameLocation location, in Vector2 tile )
        {
            return null;
        }

        public IAutomatable GetForTile( GameLocation location, in Vector2 tile )
        {
            return null;
        }
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public override void Entry( IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched( object sender, GameLaunchedEventArgs e )
        {
            var automate = Helper.ModRegistry.GetApi< IAutomateAPI >( "Pathoschild.Automate" );
            automate.AddFactory( new MyAutomationFactory() );
        }
    }
}
