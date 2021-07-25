using System.Diagnostics.CodeAnalysis;
using SpaceShared;
using StardewModdingAPI;

namespace Magic.Framework
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    internal class Configuration
    {
        /*********
        ** Accessors
        *********/
        public SButton Key_SwapSpells { get; set; } = SButton.Tab;
        public SButton Key_Cast { get; set; } = SButton.Q;
        public SButton Key_Spell1 { get; set; } = SButton.D1;
        public SButton Key_Spell2 { get; set; } = SButton.D2;
        public SButton Key_Spell3 { get; set; } = SButton.D3;
        public SButton Key_Spell4 { get; set; } = SButton.D4;
        public SButton Key_Spell5 { get; set; } = SButton.D5;

        /// <summary>The name of the map asset in which to add the alter.</summary>
        public string AltarLocation { get; set; } = "SeedShop";

        /// <summary>The X tile position for the top-left corner of the altar.</summary>
        public int AltarX { get; set; } = 36;

        /// <summary>The Y tile position for the top-left corner of the altar.</summary>
        public int AltarY { get; set; } = 16;
    }
}
