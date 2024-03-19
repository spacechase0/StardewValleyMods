using StardewModdingAPI.Utilities;
using StardewModdingAPI;

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

        public bool SocialInteractions_AlwaysTrigger { get; set; } = false;
        public KeybindList SocialInteractions_TriggerModifier { get; set; } = new KeybindList(SButton.LeftControl);
    }
}
