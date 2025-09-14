using System.Collections.Generic;

namespace LuckSkill.Framework
{
    public class LuckSkillApi : ILuckSkillApi
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public int FortunateProfessionId => Mod.FortunateProfessionId;

        /// <inheritdoc />
        public int PopularHelperProfessionId => Mod.PopularHelperProfessionId;

        /// <inheritdoc />
        public int LuckyProfessionId => Mod.LuckyProfessionId;

        /// <inheritdoc />
        public int UnUnluckyProfessionId => Mod.UnUnluckyProfessionId;

        /// <inheritdoc />
        public int ShootingStarProfessionId => Mod.ShootingStarProfessionId;

        /// <inheritdoc />
        public int SpiritChildProfessionId => Mod.SpiritChildProfessionId;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public IDictionary<int, IProfession> GetProfessions()
        {
            return Mod.Instance.GetProfessions();
        }
    }
}
