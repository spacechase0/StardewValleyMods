using System;
using System.Collections.Generic;
using System.Linq;
using JsonAssets.Data;

namespace JsonAssets.Framework.ContentPatcher
{
    internal class SpriteCoordinateToken : BaseToken
    {
        public readonly bool CoordinateIsX;

        private readonly Func<IDictionary<string, string>> IdsFunc;
        private IDictionary<string, string> Ids;

        public SpriteCoordinateToken(string type, bool coordinateIsX, Func<IDictionary<string,string>> func)
            : base(type, "Sprite" + (coordinateIsX ? "X" : "Y"))
        {
            this.CoordinateIsX = coordinateIsX;
            this.IdsFunc = func;
        }

        public bool HasBoundedRangeValues(string input, out int min, out int max)
        {
            min = 0;
            max = 4096;
            return true;
        }

        public override IEnumerable<string> GetValidInputs()
        {
            return this.Ids.Keys;
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
                return Array.Empty<string>();

            if (input == "")
                return this.Ids.Values.Select(p => "0").ToArray();

            if (this.Ids.ContainsKey(input))
                return new[] { "0" };

            return Array.Empty<string>();
        }

        protected override void UpdateContextImpl()
        {
            this.Ids = this.IdsFunc();
        }
    }
}
