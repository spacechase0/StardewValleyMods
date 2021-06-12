using System;
using System.Collections.Generic;
using System.Linq;
using JsonAssets.Data;
using SpaceShared.APIs;

namespace JsonAssets.Framework
{
    internal abstract class BaseToken
    {
        /// CP at the moment (in the beta I got) doesn't like public getters
        internal string Type { get; }
        internal string TokenName { get; }
        private int OldGen = -1;

        public BaseToken(string type, string name)
        {
            this.Type = type;
            this.TokenName = this.Type + name;
        }

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
            if (this.OldGen != ContentPatcherIntegration.IdsAssignedGen)
            {
                this.OldGen = ContentPatcherIntegration.IdsAssignedGen;
                this.UpdateContextImpl();
                return true;
            }
            return false;
        }

        protected abstract void UpdateContextImpl();
    }

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
                return this.Ids.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!this.Ids.ContainsKey(input))
                return new string[0];
            return new[] {this.Ids[input].ToString() };
        }

        protected override void UpdateContextImpl()
        {
            this.Ids = this.IdsFunc();
        }
    }

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
            return base.IsReady() && this.Tilesheets != null && this.Tilesheets.Count > 0 && !string.IsNullOrEmpty(this.Tilesheets.First().Value);
        }

        public override IEnumerable<string> GetValues(string input)
        {
            if (!this.IsReady())
                return new string[0];

            if (input == "")
                return this.Tilesheets.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!this.Tilesheets.ContainsKey(input) || string.IsNullOrEmpty(this.Tilesheets[input]))
                return new string[0];
            return new[] { this.Tilesheets[input].ToString() };
        }

        public override bool UpdateContext()
        {
            if (base.UpdateContext())
                return true;

            var objs = this.ObjsFunc();
            if (objs.Count == 0)
                return false;

            var obj = objs[0];
            if (!string.IsNullOrEmpty(obj.tilesheet) && this.Tilesheets.Count > 0 && string.IsNullOrEmpty(this.Tilesheets.First().Value))
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
                dict.Add(obj.Name, obj.tilesheet);
            }
            this.Tilesheets = dict;
        }
    }

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
                return new string[0];

            if (input == "")
                return this.Coordinates.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!this.Coordinates.ContainsKey(input))
                return new string[0];
            return new[] { (this.Coordinates[input]/*-(coordinateIsX?0:16)*/).ToString() };
        }

        public override bool UpdateContext()
        {
            if (base.UpdateContext())
                return true;

            var objs = this.ObjsFunc();
            if (objs.Count == 0)
                return false;

            var obj = objs[0];
            if (!string.IsNullOrEmpty(obj.tilesheet) && this.Coordinates.Count > 0 && this.Coordinates.First().Value == 0)
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
                dict.Add(obj.Name, this.CoordinateIsX ? obj.tilesheetX : obj.tilesheetY);
            }
            this.Coordinates = dict;
        }
    }

    internal class ContentPatcherIntegration
    {
        private static IContentPatcherApi Cp;
        private static IApi Ja;

        internal static bool IdsAssigned;
        internal static int IdsAssignedGen = -1;

        private static List<BaseToken> Tokens;

        public static void Initialize()
        {
            ContentPatcherIntegration.Cp = Mod.instance.Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");
            ContentPatcherIntegration.Ja = Mod.instance.Helper.ModRegistry.GetApi<IApi>("spacechase0.JsonAssets");
            if (ContentPatcherIntegration.Cp == null)
                return;

            ContentPatcherIntegration.Ja.IdsAssigned += (s, e) => ContentPatcherIntegration.IdsAssigned = true;
            ContentPatcherIntegration.Ja.IdsAssigned += (s, e) => ContentPatcherIntegration.IdsAssignedGen++;
            Mod.instance.Helper.Events.GameLoop.ReturnedToTitle += (s, e) => ContentPatcherIntegration.IdsAssigned = false;

            ContentPatcherIntegration.Tokens = new List<BaseToken>();
            ContentPatcherIntegration.Tokens.Add(new IdToken("Object", Mod.StartingObjectId, ContentPatcherIntegration.Ja.GetAllObjectIds));
            ContentPatcherIntegration.Tokens.Add(new IdToken("Crop", Mod.StartingCropId, ContentPatcherIntegration.Ja.GetAllCropIds));
            ContentPatcherIntegration.Tokens.Add(new IdToken("FruitTree", Mod.StartingFruitTreeId, ContentPatcherIntegration.Ja.GetAllFruitTreeIds));
            ContentPatcherIntegration.Tokens.Add(new IdToken("BigCraftable", Mod.StartingBigCraftableId, ContentPatcherIntegration.Ja.GetAllBigCraftableIds));
            ContentPatcherIntegration.Tokens.Add(new IdToken("Hat", Mod.StartingHatId, ContentPatcherIntegration.Ja.GetAllHatIds));
            ContentPatcherIntegration.Tokens.Add(new IdToken("Weapon", Mod.StartingWeaponId, ContentPatcherIntegration.Ja.GetAllWeaponIds));
            ContentPatcherIntegration.Tokens.Add(new IdToken("Clothing", Mod.StartingClothingId, ContentPatcherIntegration.Ja.GetAllClothingIds));
            ContentPatcherIntegration.Tokens.Add(new SpriteTilesheetToken("Object", () => Mod.instance.Objects.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("Object", true, () => Mod.instance.Objects.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("Object", false, () => Mod.instance.Objects.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteTilesheetToken("Crop", () => Mod.instance.Crops.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("Crop", true, () => Mod.instance.Crops.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("Crop", false, () => Mod.instance.Crops.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteTilesheetToken("FruitTree", () => Mod.instance.FruitTrees.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("FruitTree", true, () => Mod.instance.FruitTrees.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("FruitTree", false, () => Mod.instance.FruitTrees.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteTilesheetToken("BigCraftable", () => Mod.instance.BigCraftables.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("BigCraftable", true, () => Mod.instance.BigCraftables.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("BigCraftable", false, () => Mod.instance.BigCraftables.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteTilesheetToken("Hat", () => Mod.instance.Hats.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("Hat", true, () => Mod.instance.Hats.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("Hat", false, () => Mod.instance.Hats.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteTilesheetToken("Weapon", () => Mod.instance.Weapons.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("Weapon", true, () => Mod.instance.Weapons.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.Tokens.Add(new SpriteCoordinateToken("Weapon", false, () => Mod.instance.Weapons.ToList<DataNeedsIdWithTexture>()));
            // TODO: Shirt tilesheet
            // TODO: Shirt x
            // TODO: Shirt y
            // TODO: Pants tilesheet
            // TODO: Pants x
            // TODO: Pants y

            foreach (var token in ContentPatcherIntegration.Tokens)
            {
                //cp.RegisterToken(Mod.instance.ModManifest, token.TokenName, token.UpdateContext, token.IsReady, token.GetValue, true, true);
                ContentPatcherIntegration.Cp.RegisterToken(Mod.instance.ModManifest, token.TokenName, token);
            }
        }
    }
}
