using System;
using System.Collections.Generic;

namespace JsonAssets.Framework.ContentPatcher
{
    internal abstract class BaseToken
    {
        /// CP at the moment (in the beta I got) doesn't like public getters
        internal string Type { get; }
        internal string TokenName { get; }
        private int OldGen = -1;

        public bool AllowsInput()
        {
            return true;
        }

        public bool RequiresInput()
        {
            return true;
        }

        public bool CanHaveMultipleValues(string input)
        {
            return false;
        }

        public abstract IEnumerable<string> GetValidInputs();

        public abstract bool TryValidateInput(string input, out string error);

        public virtual bool IsReady()
        {
            Console.WriteLine("meow is ready? " + TokenName);
            return ContentPatcherIntegration.IdsAssigned;
        }

        public abstract IEnumerable<string> GetValues(string input);

        public virtual bool UpdateContext()
        {
            if (this.OldGen != ContentPatcherIntegration.IdsAssignedGen)
            {
                this.OldGen = ContentPatcherIntegration.IdsAssignedGen;
                this.UpdateContextImpl();
                return true;
            }
            else Console.WriteLine("not updating meow " + this.TokenName);
            return false;
        }

        protected BaseToken(string type, string name)
        {
            this.Type = type;
            this.TokenName = this.Type + name;
        }

        protected abstract void UpdateContextImpl();
    }
}
