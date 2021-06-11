using System;
using System.Collections.Generic;
using System.Linq;
using JsonAssets.Data;
using SpaceShared.APIs;

namespace JsonAssets.Other.ContentPatcher
{
    public abstract class BaseToken
    {
        /// CP at the moment (in the beta I got) doesn't like public getters
        internal string Type { get; }
        internal string TokenName { get; }
        private int oldGen = -1;

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
            return ContentPatcherIntegration.idsAssigned;
        }

        public abstract IEnumerable<string> GetValues(string input);

        public virtual bool UpdateContext()
        {
            if (this.oldGen != ContentPatcherIntegration.idsAssignedGen)
            {
                this.oldGen = ContentPatcherIntegration.idsAssignedGen;
                this.UpdateContextImpl();
                return true;
            }
            return false;
        }

        protected abstract void UpdateContextImpl();
    }

    public class IdToken : BaseToken
    {
        private int StartingId;
        private Func<IDictionary<string, int>> idsFunc;
        private IDictionary<string, int> ids = new Dictionary<string, int>();

        public IdToken(string type, int startingId, Func<IDictionary<string, int>> theIdsFunc)
            : base(type, "Id")
        {
            this.StartingId = startingId;
            this.idsFunc = theIdsFunc;
        }

        public override IEnumerable<string> GetValidInputs()
        {
            return this.ids.Keys;
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
            if (!this.ids.ContainsKey(input))
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
                return this.ids.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!this.ids.ContainsKey(input))
                return new string[0];
            return new[] {this.ids[input].ToString() };
        }

        protected override void UpdateContextImpl()
        {
            this.ids = this.idsFunc();
        }
    }

    public class SpriteTilesheetToken : BaseToken
    {
        private Func<List<DataNeedsIdWithTexture>> objsFunc;
        private IDictionary<string, string> tilesheets = new Dictionary<string, string>();

        public SpriteTilesheetToken(string type, Func<List<DataNeedsIdWithTexture>> func)
            : base(type, "SpriteTilesheet")
        {
            this.objsFunc = func;
        }

        public override IEnumerable<string> GetValidInputs()
        {
            return this.tilesheets.Keys;
        }

        public override bool TryValidateInput(string input, out string error)
        {
            error = "";
            if (!this.tilesheets.ContainsKey(input))
            {
                error = $"Invalid name for {this.Type}: {input}";
                return false;
            }
            return true;
        }

        public override bool IsReady()
        {
            return base.IsReady() && this.tilesheets != null && this.tilesheets.Count > 0 && !string.IsNullOrEmpty(this.tilesheets.First().Value);
        }

        public override IEnumerable<string> GetValues(string input)
        {
            if (!this.IsReady())
                return new string[0];

            if (input == "")
                return this.tilesheets.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!this.tilesheets.ContainsKey(input) || string.IsNullOrEmpty(this.tilesheets[input]))
                return new string[0];
            return new[] { this.tilesheets[input].ToString() };
        }

        public override bool UpdateContext()
        {
            if (base.UpdateContext())
                return true;

            var objs = this.objsFunc();
            if (objs.Count == 0)
                return false;

            var obj = objs[0];
            if (!string.IsNullOrEmpty(obj.tilesheet) && this.tilesheets.Count > 0 && string.IsNullOrEmpty(this.tilesheets.First().Value))
            {
                this.UpdateContextImpl();
                return true;
            }

            return false;
        }

        protected override void UpdateContextImpl()
        {
            var dict = new Dictionary<string, string>();
            var objs = this.objsFunc();
            foreach (var obj in objs)
            {
                dict.Add(obj.Name, obj.tilesheet);
            }
            this.tilesheets = dict;
        }
    }

    public class SpriteCoordinateToken : BaseToken
    {
        public readonly bool coordinateIsX;
        private Func<List<DataNeedsIdWithTexture>> objsFunc;
        private IDictionary<string, int> coordinates = new Dictionary<string, int>();

        public SpriteCoordinateToken(string type, bool coordinateIsX, Func<List<DataNeedsIdWithTexture>> func)
            : base(type, "Sprite" + (coordinateIsX ? "X" : "Y"))
        {
            this.coordinateIsX = coordinateIsX;
            this.objsFunc = func;
        }

        public override IEnumerable<string> GetValidInputs()
        {
            return this.coordinates.Keys;
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
            if (!this.coordinates.ContainsKey(input))
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
                return this.coordinates.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!this.coordinates.ContainsKey(input))
                return new string[0];
            return new[] { (this.coordinates[input]/*-(coordinateIsX?0:16)*/).ToString() };
        }

        public override bool UpdateContext()
        {
            if (base.UpdateContext())
                return true;

            var objs = this.objsFunc();
            if (objs.Count == 0)
                return false;

            var obj = objs[0];
            if (!string.IsNullOrEmpty(obj.tilesheet) && this.coordinates.Count > 0 && this.coordinates.First().Value == 0)
            {
                this.UpdateContextImpl();
                return true;
            }

            return false;
        }

        protected override void UpdateContextImpl()
        {
            var dict = new Dictionary<string, int>();
            var objs = this.objsFunc();
            foreach (var obj in objs)
            {
                dict.Add(obj.Name, this.coordinateIsX ? obj.tilesheetX : obj.tilesheetY);
            }
            this.coordinates = dict;
        }
    }

    public class ContentPatcherIntegration
    {
        private static ContentPatcherAPI cp;
        private static IApi ja;

        internal static bool idsAssigned = false;
        internal static int idsAssignedGen = -1;

        private static List<BaseToken> tokens;

        public static void Initialize()
        {
            ContentPatcherIntegration.cp = Mod.instance.Helper.ModRegistry.GetApi<ContentPatcherAPI>("Pathoschild.ContentPatcher");
            ContentPatcherIntegration.ja = Mod.instance.Helper.ModRegistry.GetApi<IApi>("spacechase0.JsonAssets");
            if (ContentPatcherIntegration.cp == null)
                return;

            ContentPatcherIntegration.ja.IdsAssigned += (s, e) => ContentPatcherIntegration.idsAssigned = true;
            ContentPatcherIntegration.ja.IdsAssigned += (s, e) => ContentPatcherIntegration.idsAssignedGen++;
            Mod.instance.Helper.Events.GameLoop.ReturnedToTitle += (s, e) => ContentPatcherIntegration.idsAssigned = false;

            ContentPatcherIntegration.tokens = new List<BaseToken>();
            ContentPatcherIntegration.tokens.Add(new IdToken("Object", Mod.StartingObjectId, ContentPatcherIntegration.ja.GetAllObjectIds));
            ContentPatcherIntegration.tokens.Add(new IdToken("Crop", Mod.StartingCropId, ContentPatcherIntegration.ja.GetAllCropIds));
            ContentPatcherIntegration.tokens.Add(new IdToken("FruitTree", Mod.StartingFruitTreeId, ContentPatcherIntegration.ja.GetAllFruitTreeIds));
            ContentPatcherIntegration.tokens.Add(new IdToken("BigCraftable", Mod.StartingBigCraftableId, ContentPatcherIntegration.ja.GetAllBigCraftableIds));
            ContentPatcherIntegration.tokens.Add(new IdToken("Hat", Mod.StartingHatId, ContentPatcherIntegration.ja.GetAllHatIds));
            ContentPatcherIntegration.tokens.Add(new IdToken("Weapon", Mod.StartingWeaponId, ContentPatcherIntegration.ja.GetAllWeaponIds));
            ContentPatcherIntegration.tokens.Add(new IdToken("Clothing", Mod.StartingClothingId, ContentPatcherIntegration.ja.GetAllClothingIds));
            ContentPatcherIntegration.tokens.Add(new SpriteTilesheetToken("Object", () => Mod.instance.objects.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("Object", true, () => Mod.instance.objects.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("Object", false, () => Mod.instance.objects.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteTilesheetToken("Crop", () => Mod.instance.crops.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("Crop", true, () => Mod.instance.crops.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("Crop", false, () => Mod.instance.crops.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteTilesheetToken("FruitTree", () => Mod.instance.fruitTrees.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("FruitTree", true, () => Mod.instance.fruitTrees.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("FruitTree", false, () => Mod.instance.fruitTrees.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteTilesheetToken("BigCraftable", () => Mod.instance.bigCraftables.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("BigCraftable", true, () => Mod.instance.bigCraftables.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("BigCraftable", false, () => Mod.instance.bigCraftables.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteTilesheetToken("Hat", () => Mod.instance.hats.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("Hat", true, () => Mod.instance.hats.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("Hat", false, () => Mod.instance.hats.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteTilesheetToken("Weapon", () => Mod.instance.weapons.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("Weapon", true, () => Mod.instance.weapons.ToList<DataNeedsIdWithTexture>()));
            ContentPatcherIntegration.tokens.Add(new SpriteCoordinateToken("Weapon", false, () => Mod.instance.weapons.ToList<DataNeedsIdWithTexture>()));
            // TODO: Shirt tilesheet
            // TODO: Shirt x
            // TODO: Shirt y
            // TODO: Pants tilesheet
            // TODO: Pants x
            // TODO: Pants y

            foreach (var token in ContentPatcherIntegration.tokens)
            {
                //cp.RegisterToken(Mod.instance.ModManifest, token.TokenName, token.UpdateContext, token.IsReady, token.GetValue, true, true);
                ContentPatcherIntegration.cp.RegisterToken(Mod.instance.ModManifest, token.TokenName, token);
            }
        }
    }
}
