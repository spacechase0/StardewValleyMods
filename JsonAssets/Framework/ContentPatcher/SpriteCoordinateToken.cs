using System;
using System.Collections.Generic;
using System.Linq;
using JsonAssets.Data;

namespace JsonAssets.Framework.ContentPatcher
{
    internal class SpriteCoordinateToken : BaseToken
    {
        public readonly bool CoordinateIsX;
        private readonly Func<List<DataNeedsIdWithTexture>> ObjsFunc;
        private IDictionary<string, int> Coordinates = new Dictionary<string, int>();

        public SpriteCoordinateToken(string type, bool coordinateIsX, Func<List<DataNeedsIdWithTexture>> func)
            : base(type, "Sprite" + (coordinateIsX ? "X" : "Y"))
        {
            this.CoordinateIsX = coordinateIsX;
            this.ObjsFunc = func;
        }

        public override IEnumerable<string> GetValidInputs()
        {
            return this.Coordinates.Keys;
        }

        public bool HasBoundedRangeValues(string input, out int min, out int max)
        {
            min = 0;
            max = 4096;
            return true;
        }

        public override bool TryValidateInput(string input, out string error)
        {
            error = "";
            if (!this.Coordinates.ContainsKey(input))
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
                return this.Coordinates.Values.Select(p => p.ToString()).ToArray();

            return this.Coordinates.TryGetValue(input, out int value)
                ? new[] { "0" }
                : Array.Empty<string>();
        }

        public override bool UpdateContext()
        {
            if (base.UpdateContext())
                return true;

            var objs = this.ObjsFunc();
            if (objs.Count == 0)
                return false;

            var obj = objs[0];
            if (!string.IsNullOrEmpty(obj.Tilesheet) && this.Coordinates.Count > 0 && this.Coordinates.First().Value == 0)
            {
                this.UpdateContextImpl();
                return true;
            }

            return false;
        }

        protected override void UpdateContextImpl()
        {
            var dict = new Dictionary<string, int>();
            var objs = this.ObjsFunc();
            foreach (var obj in objs)
            {
                dict.Add(obj.Name, this.CoordinateIsX ? obj.TilesheetX : obj.TilesheetY);
            }
            this.Coordinates = dict;
        }
    }
}
