using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace TheftOfTheWinterStar.Framework
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

        /// <summary>Get a translation equivalent to "Frosty Stardrop".</summary>
        public static string Recipe_FrostyStardrop_Name()
        {
            return I18n.GetByKey("recipe.frosty-stardrop.name");
        }

        /// <summary>Get a translation equivalent to "Festive Big Key (A)".</summary>
        public static string Item_FestiveBigKeyA_Name()
        {
            return I18n.GetByKey("item.festive-big-key-a.name");
        }

        /// <summary>Get a translation equivalent to "Half of a 'big key' used in the Frost Dungeon.".</summary>
        public static string Item_FestiveBigKeyA_Description()
        {
            return I18n.GetByKey("item.festive-big-key-a.description");
        }

        /// <summary>Get a translation equivalent to "Festive Big Key (B)".</summary>
        public static string Item_FestiveBigKeyB_Name()
        {
            return I18n.GetByKey("item.festive-big-key-b.name");
        }

        /// <summary>Get a translation equivalent to "Half of a 'big key' used in the Frost Dungeon.".</summary>
        public static string Item_FestiveBigKeyB_Description()
        {
            return I18n.GetByKey("item.festive-big-key-b.description");
        }

        /// <summary>Get a translation equivalent to "Festive Key".</summary>
        public static string Item_FestiveKey_Name()
        {
            return I18n.GetByKey("item.festive-key.name");
        }

        /// <summary>Get a translation equivalent to "A key used in the Frost Dungeon.".</summary>
        public static string Item_FestiveKey_Description()
        {
            return I18n.GetByKey("item.festive-key.description");
        }

        /// <summary>Get a translation equivalent to "Frosty Stardrop Piece".</summary>
        public static string Item_FrostyStardropPiece_Name()
        {
            return I18n.GetByKey("item.frosty-stardrop-piece.name");
        }

        /// <summary>Get a translation equivalent to "A piece of a stardrop. Five can be combined for a full one.".</summary>
        public static string Item_FrostyStardropPiece_Description()
        {
            return I18n.GetByKey("item.frosty-stardrop-piece.description");
        }

        /// <summary>Get a translation equivalent to "Tempus Globe".</summary>
        public static string Item_TempusGlobe_Name()
        {
            return I18n.GetByKey("item.tempus-globe.name");
        }

        /// <summary>Get a translation equivalent to "A mystical item that allows crops around it to grow year-long. It also waters crops near it.\n5x5 area, centered on the object.".</summary>
        public static string Item_TempusGlobe_Description()
        {
            return I18n.GetByKey("item.tempus-globe.description");
        }

        /// <summary>Get a translation equivalent to "Festive Scepter".</summary>
        public static string Weapon_FestiveScepter_Name()
        {
            return I18n.GetByKey("weapon.festive-scepter.name");
        }

        /// <summary>Get a translation equivalent to "It shoots a frosty beam.".</summary>
        public static string Weapon_FestiveScepter_Description()
        {
            return I18n.GetByKey("weapon.festive-scepter.description");
        }

        /// <summary>Get a translation equivalent to "Hello, @.#$b#I was making preparations for the Feast of the Winter Star and... I can't find any of the decorations!$s#$b#It seems someone stole the decorations.$4#$b#I'm not sure why somebody would do this... but decorations don't just disappear by themselves!$s#$b#Anyways, I was hoping you could retrieve them for us?$h#$b#There was a trail of broken decorations leading down the tunnel to the left of the bus stop. We'd all appreciate it if you could do this for us.$n#$b#Or we could hire Marlo...".</summary>
        public static string Event_LewisSpeech()
        {
            return I18n.GetByKey("event.lewis-speech");
        }

        /// <summary>Get a translation equivalent to "This door is locked right now.".</summary>
        public static string MapMessages_LockedEntrance()
        {
            return I18n.GetByKey("map-messages.locked-entrance");
        }

        /// <summary>Get a translation equivalent to "This door is locked. It probably needs a key.".</summary>
        public static string MapMessages_LockedDoor()
        {
            return I18n.GetByKey("map-messages.locked-door");
        }

        /// <summary>Get a translation equivalent to "This giant door is locked. Perhaps something nearby can open it.".</summary>
        public static string MapMessages_LockedBoss()
        {
            return I18n.GetByKey("map-messages.locked-boss");
        }

        /// <summary>Get a translation equivalent to "The door has been unlocked.".</summary>
        public static string MapMessages_Unlocked()
        {
            return I18n.GetByKey("map-messages.unlocked");
        }

        /// <summary>Get a translation equivalent to "There seems to be a silhouette on the pedestal.".</summary>
        public static string MapMessages_ItemPuzzle()
        {
            return I18n.GetByKey("map-messages.item-puzzle");
        }

        /// <summary>Get a translation equivalent to "A target.".</summary>
        public static string MapMessages_Target()
        {
            return I18n.GetByKey("map-messages.target");
        }

        /// <summary>Get a translation equivalent to "Some festive lights.".</summary>
        public static string MapMessages_TrailLights()
        {
            return I18n.GetByKey("map-messages.trail-lights");
        }

        /// <summary>Get a translation equivalent to "A smashed candy cane.".</summary>
        public static string MapMessages_TrailCandyCane()
        {
            return I18n.GetByKey("map-messages.trail-candy-cane");
        }

        /// <summary>Get a translation equivalent to "Some festive ornaments.".</summary>
        public static string MapMessages_TrailOrnaments()
        {
            return I18n.GetByKey("map-messages.trail-ornaments");
        }

        /// <summary>Get a translation equivalent to "A smashed miniature tree.".</summary>
        public static string MapMessages_TrailTree()
        {
            return I18n.GetByKey("map-messages.trail-tree");
        }

        /// <summary>Get a translation equivalent to "How DARE they have fun without me! They'll never get their decorations back!".</summary>
        public static string FinalBoss_Speech()
        {
            return I18n.GetByKey("final-boss.speech");
        }

        /// <summary>Get a translation equivalent to "You got the decorations back!\nYou also learned the recipe for the 'Tempus Globe'!".</summary>
        public static string FinalBoss_VictoryMessage()
        {
            return I18n.GetByKey("final-boss.victory-message");
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

