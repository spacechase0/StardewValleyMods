using System;
using System.Collections.Generic;
using System.Linq;
using Magic.Framework.Spells;
using Microsoft.Xna.Framework.Graphics;

namespace Magic.Framework.Schools
{
    internal class School
    {
        /*********
        ** Fields
        *********/
        private static Dictionary<string, School> Schools;
        private readonly Lazy<Texture2D> IconImpl;


        /*********
        ** Accessors
        *********/
        public string Id { get; }

        /// <summary>The display name for the school.</summary>
        public string DisplayName => I18n.GetByKey($"school.{this.Id}.name");

        public Texture2D Icon => this.IconImpl.Value;


        /*********
        ** Public methods
        *********/
        public virtual Spell[] GetSpellsTier1() { return Array.Empty<Spell>(); }
        public virtual Spell[] GetSpellsTier2() { return Array.Empty<Spell>(); }
        public virtual Spell[] GetSpellsTier3() { return Array.Empty<Spell>(); }

        /// <summary>Get all spell tiers.</summary>
        public IEnumerable<Spell[]> GetAllSpellTiers()
        {
            return new[] { GetSpellsTier1(), GetSpellsTier2(), GetSpellsTier3() }
                .Where(p => p?.Length > 0);
        }

        public static void RegisterSchool(School school)
        {
            if (School.Schools == null)
                School.Init();

            School.Schools.Add(school.Id, school);
        }

        public static School GetSchool(string id)
        {
            if (School.Schools == null)
                School.Init();

            return School.Schools[id];
        }

        public static ICollection<string> GetSchoolList()
        {
            if (School.Schools == null)
                School.Init();

            return School.Schools.Keys;
        }


        /*********
        ** Protected methods
        *********/
        protected School(string id)
        {
            this.Id = id;
            this.IconImpl = new(() => Content.LoadTexture($"magic/{id}/school-icon.png"));
        }

        private static void Init()
        {
            School.Schools = new Dictionary<string, School>();
            School.RegisterSchool(new ArcaneSchool());
            School.RegisterSchool(new ElementalSchool());
            School.RegisterSchool(new NatureSchool());
            School.RegisterSchool(new LifeSchool());
            School.RegisterSchool(new EldritchSchool());
            School.RegisterSchool(new ToilSchool());
        }
    }
}
