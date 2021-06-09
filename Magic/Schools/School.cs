using System.Collections.Generic;
using Magic.Spells;

namespace Magic.Schools
{
    public class School
    {
        public string Id { get; }

        public virtual Spell[] GetSpellsTier1() { return new Spell[0]; }
        public virtual Spell[] GetSpellsTier2() { return new Spell[0]; }
        public virtual Spell[] GetSpellsTier3() { return new Spell[0]; }

        protected School(string id)
        {
            this.Id = id;
        }

        private static Dictionary<string, School> schools;
        public static void registerSchool(School school)
        {
            if (School.schools == null)
                School.init();

            School.schools.Add(school.Id, school);
        }

        public static School getSchool(string id)
        {
            if (School.schools == null)
                School.init();

            return School.schools[id];
        }

        public static ICollection<string> getSchoolList()
        {
            if (School.schools == null)
                School.init();

            return School.schools.Keys;
        }

        private static void init()
        {
            School.schools = new Dictionary<string, School>();
            School.registerSchool(new ArcaneSchool());
            School.registerSchool(new ElementalSchool());
            School.registerSchool(new NatureSchool());
            School.registerSchool(new LifeSchool());
            School.registerSchool(new EldritchSchool());
            School.registerSchool(new ToilSchool());
        }
    }
}
