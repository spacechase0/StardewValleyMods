using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace RealtimeMinimap.Framework
{
    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>
    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for consistency and to match translation conventions.")]
    internal static class I18n
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's translation helper.</summary>
        private static ITranslationHelper Translations;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">The mod's translation helper.</param>
        public static void Init(ITranslationHelper translations)
        {
            I18n.Translations = translations;
        }

        /// <summary>Get a translation equivalent to "Show By Default".</summary>
        public static string Config_ShowByDefault_Name()
        {
            return I18n.GetByKey("config.show-by-default.name");
        }

        /// <summary>Get a translation equivalent to "Whether the minimap should be shown by default.\nYou must restart the game for this to take effect.".</summary>
        public static string Config_ShowByDefault_Tooltip()
        {
            return I18n.GetByKey("config.show-by-default.tooltip");
        }

        /// <summary>Get a translation equivalent to "Toggle Key".</summary>
        public static string Config_ToggleKey_Name()
        {
            return I18n.GetByKey("config.toggle-key.name");
        }

        /// <summary>Get a translation equivalent to "The key which shows or hides the minimap.".</summary>
        public static string Config_ToggleKey_Tooltip()
        {
            return I18n.GetByKey("config.toggle-key.tooltip");
        }

        /// <summary>Get a translation equivalent to "Update Interval".</summary>
        public static string Config_UpdateInterval_Name()
        {
            return I18n.GetByKey("config.update-interval.name");
        }

        /// <summary>Get a translation equivalent to "The interval in milliseconds between each minimap update. 0 will be every frame; -1 will only do it when entering a new location. (Markers update every frame regardless.)".</summary>
        public static string Config_UpdateInterval_Tooltip()
        {
            return I18n.GetByKey("config.update-interval.tooltip");
        }

        /// <summary>Get a translation equivalent to "Positioning & Size".</summary>
        public static string Config_PositioningAndSize_Text()
        {
            return I18n.GetByKey("config.positioning-and-size.text");
        }

        /// <summary>Get a translation equivalent to "Options pertaining to the placement of the minimap.".</summary>
        public static string Config_PositioningAndSize_Tooltip()
        {
            return I18n.GetByKey("config.positioning-and-size.tooltip");
        }

        /// <summary>Get a translation equivalent to "Minimap Anchor X".</summary>
        public static string Config_AnchorX_Name()
        {
            return I18n.GetByKey("config.anchor-x.name");
        }

        /// <summary>Get a translation equivalent to "The percentage of the screen's width where the top-left of the minimap will be placed.".</summary>
        public static string Config_AnchorX_Tooltip()
        {
            return I18n.GetByKey("config.anchor-x.tooltip");
        }

        /// <summary>Get a translation equivalent to "Minimap Anchor Y".</summary>
        public static string Config_AnchorY_Name()
        {
            return I18n.GetByKey("config.anchor-y.name");
        }

        /// <summary>Get a translation equivalent to "The percentage of the screen's height where the top-left of the minimap will be placed.".</summary>
        public static string Config_AnchorY_Tooltip()
        {
            return I18n.GetByKey("config.anchor-y.tooltip");
        }

        /// <summary>Get a translation equivalent to "Minimap Offset X".</summary>
        public static string Config_OffsetX_Name()
        {
            return I18n.GetByKey("config.offset-x.name");
        }

        /// <summary>Get a translation equivalent to "The X offset from the anchor that the minimap will be placed at.".</summary>
        public static string Config_OffsetX_Tooltip()
        {
            return I18n.GetByKey("config.offset-x.tooltip");
        }

        /// <summary>Get a translation equivalent to "Minimap Offset Y".</summary>
        public static string Config_OffsetY_Name()
        {
            return I18n.GetByKey("config.offset-y.name");
        }

        /// <summary>Get a translation equivalent to "The Y offset from the anchor that the minimap will be placed at.".</summary>
        public static string Config_OffsetY_Tooltip()
        {
            return I18n.GetByKey("config.offset-y.tooltip");
        }

        /// <summary>Get a translation equivalent to "Minimap Size".</summary>
        public static string Config_Size_Name()
        {
            return I18n.GetByKey("config.size.name");
        }

        /// <summary>Get a translation equivalent to "The size of the minimap, in pixels (before UI scale).".</summary>
        public static string Config_Size_Tooltip()
        {
            return I18n.GetByKey("config.size.tooltip");
        }

        /// <summary>Get a translation equivalent to "Markers".</summary>
        public static string Config_Markers_Text()
        {
            return I18n.GetByKey("config.markers.text");
        }

        /// <summary>Get a translation equivalent to "Options pertaining to rendering markers on the map.".</summary>
        public static string Config_Markers_Tooltip()
        {
            return I18n.GetByKey("config.markers.tooltip");
        }

        /// <summary>Get a translation equivalent to "Player Heads".</summary>
        public static string Config_PlayerMarkerScale_Name()
        {
            return I18n.GetByKey("config.player-marker-scale.name");
        }

        /// <summary>Get a translation equivalent to "Render scale for the head of a player. 0 disables it.".</summary>
        public static string Config_PlayerMarkerScale_Tooltip()
        {
            return I18n.GetByKey("config.player-marker-scale.tooltip");
        }

        /// <summary>Get a translation equivalent to "NPC Heads".</summary>
        public static string Config_NpcMarkerScale_Name()
        {
            return I18n.GetByKey("config.npc-marker-scale.name");
        }

        /// <summary>Get a translation equivalent to "Render scale for the head of an NPC. 0 disables it.".</summary>
        public static string Config_NpcMarkerScale_Tooltip()
        {
            return I18n.GetByKey("config.npc-marker-scale.tooltip");
        }

        /// <summary>Get a translation equivalent to "Wood Signs".</summary>
        public static string Config_WoodSignMarkerScale_Name()
        {
            return I18n.GetByKey("config.wood-sign-marker-scale.name");
        }

        /// <summary>Get a translation equivalent to "Render scale for items held on wooden signs. 0 disables it.".</summary>
        public static string Config_WoodSignMarkerScale_Tooltip()
        {
            return I18n.GetByKey("config.wood-sign-marker-scale.tooltip");
        }

        /// <summary>Get a translation equivalent to "Stone Signs".</summary>
        public static string Config_StoneSignMarkerScale_Name()
        {
            return I18n.GetByKey("config.stone-sign-marker-scale.name");
        }

        /// <summary>Get a translation equivalent to "Render scale for items held on stone signs. 0 disables it.".</summary>
        public static string Config_StoneSignMarkerScale_Tooltip()
        {
            return I18n.GetByKey("config.stone-sign-marker-scale.tooltip");
        }

        /// <summary>Get a translation equivalent to "Dark Signs".</summary>
        public static string Config_DarkSignMarkerScale_Name()
        {
            return I18n.GetByKey("config.dark-sign-marker-scale.name");
        }

        /// <summary>Get a translation equivalent to "Render scale for items held on dark signs. 0 disables it.".</summary>
        public static string Config_DarkSignMarkerScale_Tooltip()
        {
            return I18n.GetByKey("config.dark-sign-marker-scale.tooltip");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a translation by its key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        private static Translation GetByKey(string key, object tokens = null)
        {
            if (I18n.Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(I18n.Init)} from the mod's entry method before reading translations.");
            return I18n.Translations.Get(key, tokens);
        }
    }
}

