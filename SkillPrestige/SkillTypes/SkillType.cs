using System;
using System.Linq;
using SkillPrestige.Logging;

namespace SkillPrestige.SkillTypes
{
    /// <summary>
    /// Represents a skill type in Stardew Valley. (e.g. Farming, Fishing, Foraging)
    /// </summary>
    [Serializable]
    public partial class SkillType
    {
        static SkillType()
        {
            Logger.LogInformation("Registering skill types...");
            var concreteSkillTypeRegistrations = AppDomain.CurrentDomain.GetNonSystemAssemblies().SelectMany(x => x.GetTypesSafely()).Where(x => typeof(ISkillTypeRegistration).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract).ToList();
            Logger.LogVerbose($"concerete skill type registration count: {concreteSkillTypeRegistrations.Count}");
            foreach (var registration in concreteSkillTypeRegistrations)
            {
                Logger.LogVerbose($"Creating instance of type {registration.FullName}...");
                ((ISkillTypeRegistration)Activator.CreateInstance(registration)).RegisterSkillTypes();
            }
            Logger.LogInformation("Skill types registered.");
        }

        public SkillType() { }

        // ReSharper disable once MemberCanBeProtected.Global - this time resharper is just out of it's gourd. this is used publically.
        public SkillType(string name, int ordinal)
        {
            Name = name;
            Ordinal = ordinal;
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - setter used by deserializer.
        public string Name { get; private set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - setter used by deserializer.
        /// <summary>
        /// The ordinal and lookup used to get the skill type from Stardew Valley.
        /// </summary>
        public int Ordinal { get; private set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SkillType)obj);
        }

        public bool Equals(SkillType other)
        {
            return string.Equals(Name, other.Name) && Ordinal == other.Ordinal;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode -- used by deserializer only
                return ((Name?.GetHashCode() ?? 0) * 397) ^ Ordinal;
            }
        }

        public static bool operator ==(SkillType left, SkillType right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (((object)left == null) || ((object)right == null)) return false;
            return left.Equals(right);
        }
        public static bool operator !=(SkillType left, SkillType right)
        {
            return !(left == right);
        }
    }
}

