using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Harmony;
using JsonAssets.Data;
using JsonAssets.Other.ContentPatcher;
using JsonAssets.Overrides;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using SpaceCore;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

// TODO: Refactor recipes

namespace JsonAssets
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        private ExpandedPreconditionsUtilityAPI epu;
        private HarmonyInstance harmony;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.GameLoop.GameLaunched += onGameLaunched;

            harmony = HarmonyInstance.Create( "spacechase0.JsonAssets" );
            harmony.PatchAll();
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            epu = Helper.ModRegistry.GetApi<ExpandedPreconditionsUtilityAPI>( "Cherry.ExpandedPreconditionsUtility" );
            epu.Initialize( false, ModManifest.UniqueID );
        }
    }
}
