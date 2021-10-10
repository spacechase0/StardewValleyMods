using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace GenericModConfigMenu.Framework
{
    /// <summary>The config UI for a specific mod.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The name of the mod which registered the mod configuration.</summary>
        public string ModName => this.ModManifest.Name;

        /// <summary>The manifest for the mod which registered the mod configuration.</summary>
        public IManifest ModManifest { get; }

        /// <summary>Reset the mod's config to its default values.</summary>
        public Action RevertToDefault { get; }

        /// <summary>Save the mod's current config to the <c>config.json</c> file.</summary>
        public Action SaveChanges { get; }

        /// <summary>Whether new options can be edited from the in-game options menu by default. If this is false, they can only be edited from the title screen.</summary>
        public bool DefaultEditableInGame { get; set; } = false;

        /// <summary>Whether any of the registered options can be edited in-game.</summary>
        public bool AnyEditableInGame { get; set; }

        /// <summary>The options in the form UI, indexed by page ID. Each form has a page with an empty ID for the default page.</summary>
        public Dictionary<string, ModConfigPage> Options { get; } = new();

        /// <summary>The page to which any new fields should be added.</summary>
        public ModConfigPage ActiveRegisteringPage { get; set; }

        /// <summary>The page currently being rendered in-game.</summary>
        public ModConfigPage ActiveDisplayPage { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="manifest">The manifest for the mod which registered the mod configuration.</param>
        /// <param name="revertToDefault">Reset the mod's config to its default values.</param>
        /// <param name="saveChanges">Save the mod's current config to the <c>config.json</c> file.</param>
        public ModConfig(IManifest manifest, Action revertToDefault, Action saveChanges)
        {
            this.ModManifest = manifest;
            this.RevertToDefault = revertToDefault;
            this.SaveChanges = saveChanges;
            this.Options.Add("", this.ActiveRegisteringPage = new ModConfigPage(""));
        }
    }
}
