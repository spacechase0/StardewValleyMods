namespace SpaceCore.Framework
{
    internal class Configuration
    {
        /// <summary>Whether to show the custom skill page. This will move the wallet so that there is room for more skills.</summary>
        public bool CustomSkillPage { get; set; } = true;
        public bool WalletLegacyStyle { get; set; }
        public bool WalletOnRightOfSkillPage { get; set; }

        /// <summary>When All Professions is installed, whether to automatically apply custom professions registered through SpaceCore when their level is reached.</summary>
        public bool SupportAllProfessionsMod { get; set; } = true;

        /// <summary>Whether to dispose extended tilesheet textures when they're no longer used by SpaceCore.</summary>
        /// <remarks>This can reduce memory usage, but can cause crashes if other mods don't handle disposal correctly.</remarks>
        public bool DisposeOldTextures { get; set; }
    }
}
