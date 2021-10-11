using System;
using System.Collections.Generic;
using GenericModConfigMenu.ModOption;
using StardewModdingAPI;

namespace GenericModConfigMenu.Framework
{
    /// <summary>The config UI for a specific mod.</summary>
    internal class ModConfig
    {
        /*********
        ** Fields
        *********/
        /// <summary>The page to which any new fields should be added.</summary>
        private ModConfigPage ActiveRegisteringPage;


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

        /// <summary>The page currently being rendered in-game.</summary>
        public ModConfigPage ActiveDisplayPage { get; set; }

        /// <summary>The callbacks to invoke when an option value changes.</summary>
        public List<Action<string, object>> ChangeHandlers { get; } = new();


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

            this.SetActiveRegisteringPage("");
        }

        /// <summary>Set the active page to which options should be added, creating it if needed.</summary>
        /// <param name="pageId">The unique page ID.</param>
        public void SetActiveRegisteringPage(string pageId)
        {
            if (this.Options.TryGetValue(pageId, out ModConfigPage page))
                this.ActiveRegisteringPage = page;
            else
                this.Options[pageId] = this.ActiveRegisteringPage = new ModConfigPage(pageId);
        }

        /// <summary>Add an option to the active registering page.</summary>
        /// <param name="option">The option to add.</param>
        public void AddOption(BaseModOption option)
        {
            this.ActiveRegisteringPage.Options.Add(option);

            if (this.DefaultEditableInGame)
                this.AnyEditableInGame = true;
        }
    }
}
