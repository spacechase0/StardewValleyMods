using System;

namespace LuckSkill.Framework
{
    /// <summary>A luck skill profession.</summary>
    internal class Profession : IProfession
    {
        /*********
        ** Private methods
        *********/
        /// <summary>Get the translated name.</summary>
        private readonly Func<string> GetName;

        /// <summary>Get the translated description.</summary>
        private readonly Func<string> GetDescription;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public string DefaultName { get; }

        /// <inheritdoc />
        public string DefaultDescription { get; }

        /// <inheritdoc />
        public string Name => this.GetName();

        /// <inheritdoc />
        public string Description => this.GetDescription();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id"><inheritdoc cref="IProfession.Id" path="/summary"/></param>
        /// <param name="defaultName"><inheritdoc cref="IProfession.DefaultName" path="/summary"/></param>
        /// <param name="defaultDescription"><inheritdoc cref="IProfession.DefaultDescription" path="/summary"/></param>
        /// <param name="name"><inheritdoc cref="IProfession.Name" path="/summary"/></param>
        /// <param name="description"><inheritdoc cref="IProfession.Description" path="/summary"/></param>
        public Profession(int id, string defaultName, string defaultDescription, Func<string> name, Func<string> description)
        {
            this.Id = id;
            this.DefaultName = defaultName;
            this.DefaultDescription = defaultDescription;
            this.GetName = name;
            this.GetDescription = description;
        }
    }
}
