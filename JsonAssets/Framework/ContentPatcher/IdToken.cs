using System;
using System.Collections.Generic;
using System.Linq;
using SpaceShared;

namespace JsonAssets.Framework.ContentPatcher
{
    internal class IdToken : BaseToken
    {
        public IdToken(string type)
            : base(type, "Id")
        {
        }

        public override IEnumerable<string> GetValidInputs()
        {
            return new string[0];
        }

        public override bool TryValidateInput(string input, out string error)
        {
            error = "";
            return true;
        }

        public override IEnumerable<string> GetValues(string input)
        { 
            if (!this.IsReady())
                return Array.Empty<string>();

            return new[] { input };
        }

        protected override void UpdateContextImpl()
        {
        }
    }
}
