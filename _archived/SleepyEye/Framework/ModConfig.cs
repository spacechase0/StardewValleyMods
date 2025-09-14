namespace SleepyEye.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The number of seconds until the tent tool should trigger a save.</summary>
        public int SecondsUntilSave { get; set; } = 7;
    }
}
