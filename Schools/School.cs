using Magic.Spells;
using System.Collections.Generic;

namespace Magic.Schools
{
    public class School
    {
        public string Id { get; }

        public virtual Spell[] GetSpellsTier1() { return new Spell[0]; }
        public virtual Spell[] GetSpellsTier2() { return new Spell[0]; }
        public virtual Spell[] GetSpellsTier3() { return new Spell[0]; }

        protected School( string id )
        {
            Id = id;
        }

        private static Dictionary<string, School> schools;
        public static void registerSchool( School school )
        {
            if (schools == null)
                init();

            schools.Add(school.Id, school);
        }

        public static School getSchool( string id )
        {
            if (schools == null)
                init();

            return schools[id];
        }

        public static ICollection< string > getSchoolList()
        {
            if (schools == null)
                init();

            return schools.Keys;
        }

        private static void init()
        {
            schools = new Dictionary<string, School>();
            registerSchool(new ElementalSchool());
            registerSchool(new NatureSchool());
            registerSchool(new LifeSchool());
            registerSchool(new EldritchSchool());
            registerSchool(new ToilSchool());
        }
    }
}
