using SpaceCore;

namespace CookingSkill.Framework
{
    internal class GenericProfession : Skills.Skill.Profession
    {
        public GenericProfession(Skill skill, string theId)
            : base(skill, theId) { }

        internal string Name { get; set; }
        internal string Description { get; set; }

        public override string GetName()
        {
            return this.Name;
        }

        public override string GetDescription()
        {
            return this.Description;
        }
    }
}
