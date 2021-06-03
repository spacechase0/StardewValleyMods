using JsonAssets.Data;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Type = type;
            TokenName = Type + name;
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
            if (oldGen != ContentPatcherIntegration.idsAssignedGen)
            {
                oldGen = ContentPatcherIntegration.idsAssignedGen;
                UpdateContextImpl();
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
        :   base(type, "Id")
        {
            StartingId = startingId;
            idsFunc = theIdsFunc;
        }

        public override IEnumerable<string> GetValidInputs()
        {
            return ids.Keys;
        }

        public bool HasBoundedRangeValues(string input, out int min, out int max)
        {
            min = StartingId;
            max = int.MaxValue;
            return true;
        }

        public override bool TryValidateInput(string input, out string error)
        {
            error = "";
            if (!ids.ContainsKey(input))
            {
                error = $"Invalid name for {Type}: {input}";
                return false;
            }
            return true;
        }

        public override IEnumerable<string> GetValues(string input)
        {
            if (!IsReady())
                return new string[0];

            if (input == "")
                return ids.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!ids.ContainsKey(input))
                return new string[0];
            return new string[] { ids[input].ToString() };
        }

        protected override void UpdateContextImpl()
        {
            ids = idsFunc();
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
            return tilesheets.Keys;
        }

        public override bool TryValidateInput(string input, out string error)
        {
            error = "";
            if (!tilesheets.ContainsKey(input))
            {
                error = $"Invalid name for {Type}: {input}";
                return false;
            }
            return true;
        }

        public override bool IsReady()
        {
            return base.IsReady() && tilesheets != null && tilesheets.Count > 0 && !string.IsNullOrEmpty(tilesheets.First().Value);
        }

        public override IEnumerable<string> GetValues(string input)
        {
            if (!IsReady())
                return new string[0];

            if (input == "")
                return tilesheets.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!tilesheets.ContainsKey(input) || string.IsNullOrEmpty(tilesheets[input]))
                return new string[0];
            return new string[] { tilesheets[input].ToString() };
        }

        public override bool UpdateContext()
        {
            if (base.UpdateContext())
                return true;

            var objs = objsFunc();
            if (objs.Count == 0)
                return false;

            var obj = objs[0];
            if ( !string.IsNullOrEmpty(obj.tilesheet) && tilesheets.Count > 0 && string.IsNullOrEmpty(tilesheets.First().Value) )
            {
                UpdateContextImpl();
                return true;
            }

            return false;
        }

        protected override void UpdateContextImpl()
        {
            var dict = new Dictionary<string, string>();
            var objs = objsFunc();
            foreach (var obj in objs)
            {
                dict.Add(obj.Name, obj.tilesheet);
            }
            tilesheets = dict;
        }
    }

    public class SpriteCoordinateToken : BaseToken
    {
        public readonly bool coordinateIsX;
        private Func<List<DataNeedsIdWithTexture>> objsFunc;
        private IDictionary<string, int> coordinates = new Dictionary<string, int>();
        
        public SpriteCoordinateToken(string type, bool coordinateIsX, Func<List<DataNeedsIdWithTexture>> func )
        : base(type, "Sprite" + (coordinateIsX ? "X" : "Y"))
        {
            this.coordinateIsX = coordinateIsX;
            this.objsFunc = func;
        }

        public override IEnumerable<string> GetValidInputs()
        {
            return coordinates.Keys;
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
            if (!coordinates.ContainsKey(input))
            {
                error = $"Invalid name for {Type}: {input}";
                return false;
            }
            return true;
        }

        public override IEnumerable<string> GetValues(string input)
        {
            if (!IsReady())
                return new string[0];

            if (input == "")
                return coordinates.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!coordinates.ContainsKey(input))
                return new string[0];
            return new string[] { (coordinates[input]/*-(coordinateIsX?0:16)*/).ToString() };
        }

        public override bool UpdateContext()
        {
            if (base.UpdateContext())
                return true;

            var objs = objsFunc();
            if (objs.Count == 0)
                return false;

            var obj = objs[0];
            if (!string.IsNullOrEmpty(obj.tilesheet) && coordinates.Count > 0 && coordinates.First().Value == 0)
            {
                UpdateContextImpl();
                return true;
            }

            return false;
        }

        protected override void UpdateContextImpl()
        {
            var dict = new Dictionary<string, int>();
            var objs = objsFunc();
            foreach (var obj in objs)
            {
                dict.Add(obj.Name, coordinateIsX ? obj.tilesheetX : obj.tilesheetY);
            }
            coordinates = dict;
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
            cp = Mod.instance.Helper.ModRegistry.GetApi<ContentPatcherAPI>("Pathoschild.ContentPatcher");
            ja = Mod.instance.Helper.ModRegistry.GetApi<IApi>("spacechase0.JsonAssets");
            if (cp == null)
                return;

            ja.IdsAssigned += (s, e) => idsAssigned = true;
            ja.IdsAssigned += (s, e) => idsAssignedGen++;
            Mod.instance.Helper.Events.GameLoop.ReturnedToTitle += (s, e) => idsAssigned = false;

            tokens = new List<BaseToken>();
            tokens.Add(new IdToken("Object", Mod.StartingObjectId, ja.GetAllObjectIds));
            tokens.Add(new IdToken("Crop", Mod.StartingCropId, ja.GetAllCropIds));
            tokens.Add(new IdToken("FruitTree", Mod.StartingFruitTreeId, ja.GetAllFruitTreeIds));
            tokens.Add(new IdToken("BigCraftable", Mod.StartingBigCraftableId, ja.GetAllBigCraftableIds));
            tokens.Add(new IdToken("Hat", Mod.StartingHatId, ja.GetAllHatIds));
            tokens.Add(new IdToken("Weapon", Mod.StartingWeaponId, ja.GetAllWeaponIds));
            tokens.Add(new IdToken("Clothing", Mod.StartingClothingId, ja.GetAllClothingIds));
            tokens.Add(new SpriteTilesheetToken("Object", () => Mod.instance.objects.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Object", true, () => Mod.instance.objects.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Object", false, () => Mod.instance.objects.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteTilesheetToken("Crop", () => Mod.instance.crops.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Crop", true, () => Mod.instance.crops.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Crop", false, () => Mod.instance.crops.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteTilesheetToken("FruitTree", () => Mod.instance.fruitTrees.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("FruitTree", true, () => Mod.instance.fruitTrees.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("FruitTree", false, () => Mod.instance.fruitTrees.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteTilesheetToken("BigCraftable", () => Mod.instance.bigCraftables.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("BigCraftable", true, () => Mod.instance.bigCraftables.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("BigCraftable", false, () => Mod.instance.bigCraftables.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteTilesheetToken("Hat", () => Mod.instance.hats.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Hat", true, () => Mod.instance.hats.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Hat", false, () => Mod.instance.hats.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteTilesheetToken("Weapon", () => Mod.instance.weapons.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Weapon", true, () => Mod.instance.weapons.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Weapon", false, () => Mod.instance.weapons.ToList<DataNeedsIdWithTexture>()));
            // TODO: Shirt tilesheet
            // TODO: Shirt x
            // TODO: Shirt y
            // TODO: Pants tilesheet
            // TODO: Pants x
            // TODO: Pants y

            foreach (var token in tokens)
            {
                //cp.RegisterToken(Mod.instance.ModManifest, token.TokenName, token.UpdateContext, token.IsReady, token.GetValue, true, true);
                cp.RegisterToken(Mod.instance.ModManifest, token.TokenName, token);
            }
        }
    }
}
