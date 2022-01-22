using System;
using System.Collections.Generic;
using System.Linq;
using JsonAssets.Data;
using Microsoft.Xna.Framework;

namespace JsonAssets.Framework.ContentPatcher
{
    internal class SpriteTilesheetToken : BaseToken
    {
        private readonly Func<List<DataNeedsIdWithTexture>> ObjsFunc;
        private IDictionary<string, string> Tilesheets = new Dictionary<string, string>();

        public SpriteTilesheetToken(string type, Func<List<DataNeedsIdWithTexture>> func)
            : base(type, "SpriteTilesheet")
        {
            this.ObjsFunc = func;
        }

        public override IEnumerable<string> GetValidInputs()
        {
            return this.Tilesheets.Keys;
        }

        public override bool TryValidateInput(string input, out string error)
        {
            error = "";
            if (!this.Tilesheets.ContainsKey(input))
            {
                error = $"Invalid name for {this.Type}: {input}";
                return false;
            }
            return true;
        }

        public override bool IsReady()
        {
            return base.IsReady() && this.Tilesheets?.Count > 0 && !string.IsNullOrEmpty(this.Tilesheets.First().Value);
        }

        public override IEnumerable<string> GetValues(string input)
        {
            if (!this.IsReady())
                return Array.Empty<string>();

            if (input == "")
                return this.Tilesheets.Values.Select(p => p.ToString()).ToArray();

            return this.Tilesheets.TryGetValue(input, out string value) && !string.IsNullOrEmpty(value)
                ? new[] { $"JA/{Type}/{input}" }
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
            if (!string.IsNullOrEmpty(obj.Tilesheet) && this.Tilesheets.Count > 0 && string.IsNullOrEmpty(this.Tilesheets.First().Value))
            {
                this.UpdateContextImpl();
                return true;
            }

            return false;
        }

        protected override void UpdateContextImpl()
        {
            var dict = new Dictionary<string, string>();
            var objs = this.ObjsFunc();
            foreach (var obj in objs)
            {
                dict.Add(obj.Name, obj.Tilesheet);
            }
            this.Tilesheets = dict;
        }
    }
}
