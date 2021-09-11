using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonAssets.Framework.ContentPatcher
{
    internal class IdToken : BaseToken
    {
        private readonly int StartingId;
        private readonly Func<IDictionary<string, int>> IdsFunc;
        private IDictionary<string, int> Ids = new Dictionary<string, int>();

        public IdToken(string type, int startingId, Func<IDictionary<string, int>> theIdsFunc)
            : base(type, "Id")
        {
            this.StartingId = startingId;
            this.IdsFunc = theIdsFunc;
        }

        public override IEnumerable<string> GetValidInputs()
        {
            return this.Ids.Keys;
        }

        public bool HasBoundedRangeValues(string input, out int min, out int max)
        {
            min = this.StartingId;
            max = int.MaxValue;
            return true;
        }

        public override bool TryValidateInput(string input, out string error)
        {
            error = "";
            if (!this.Ids.ContainsKey(input))
            {
                error = $"Invalid name for {this.Type}: {input}";
                return false;
            }
            return true;
        }

        public override IEnumerable<string> GetValues(string input)
        {
            if (!this.IsReady())
                return new string[0];

            if (input == "")
                return this.Ids.Values.Select(p => p.ToString()).ToArray();

            return this.Ids.TryGetValue(input, out int value)
                ? new[] { value.ToString() }
                : new string[0];
        }

        protected override void UpdateContextImpl()
        {
            this.Ids = this.IdsFunc();
        }
    }
}
