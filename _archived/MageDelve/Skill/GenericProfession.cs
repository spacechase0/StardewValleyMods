namespace MageDelve.Skill
{
    public class GenericProfession : SpaceCore.Skills.Skill.Profession
    {
        /*********
        ** Public methods
        *********/
        public GenericProfession(ArcanaSkill skill, string theId)
            : base(skill, theId) { }

        public override string GetName()
        {
            return I18n.GetByKey("profession." + Id + ".name");
        }

        public override string GetDescription()
        {
            return I18n.GetByKey("profession." + Id + ".description");
        }
    }
}
