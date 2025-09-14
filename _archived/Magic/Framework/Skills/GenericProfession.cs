namespace Magic.Framework.Skills
{
    internal class GenericProfession : SpaceCore.Skills.Skill.Profession
    {
        /*********
        ** Accessors
        *********/
        public string Name { get; set; }
        public string Description { get; set; }


        /*********
        ** Public methods
        *********/
        public GenericProfession(Skill skill, string theId)
            : base(skill, theId) { }

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
