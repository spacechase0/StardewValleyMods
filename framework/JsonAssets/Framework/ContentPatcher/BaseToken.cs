using System;
using System.Collections.Generic;
using SpaceShared;

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
            return ContentPatcherIntegration.IdsAssigned;
        }

        public abstract IEnumerable<string> GetValues(string input);

        public virtual bool UpdateContext()
        {
            try
            {
                if (this.OldGen != ContentPatcherIntegration.IdsAssignedGen)
                {
                    this.OldGen = ContentPatcherIntegration.IdsAssignedGen;
                    this.UpdateContextImpl();
                    return true;
                }
            }
            catch (Exception e) { Log.Error("exception:"+e); throw e; }
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
