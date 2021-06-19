namespace SpaceCore.Framework
{
    internal class Configuration
    {
        public bool CustomSkillPage { get; set; } = true;
        public bool WalletLegacyStyle { get; set; }
        public bool WalletOnRightOfSkillPage { get; set; }

        /// <summary>Whether to dispose extended tilesheet textures when they're no longer used by SpaceCore.</summary>
        /// <remarks>This can reduce memory usage, but can cause crashes if other mods don't handle disposal correctly.</remarks>
        public bool DisposeOldTextures { get; set; }
    }
}
