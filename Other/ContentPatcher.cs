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
        public string Type { get; }
        public string TokenName { get; }
        private int oldGen = -1;

        public BaseToken(string type, string name)
        {
            Type = type;
            TokenName = Type + name;
        }

        public virtual bool IsReady()
        {
            return ContentPatcherIntegration.idsAssigned;
        }

        public bool UpdateContext()
        {
            if (oldGen != ContentPatcherIntegration.idsAssignedGen)
            {
                oldGen = ContentPatcherIntegration.idsAssignedGen;
                UpdateContextImpl();
                return true;
            }
            return false;
        }

        public abstract IEnumerable<string> GetValue(string input);

        protected abstract void UpdateContextImpl();
    }

    public class IdToken : BaseToken
    {
        private Func<IDictionary<string, int>> idsFunc;
        private IDictionary<string, int> ids;

        public IdToken(string type, Func<IDictionary<string, int>> theIdsFunc)
        :   base(type, "Id")
        {
            idsFunc = theIdsFunc;
        }

        protected override void UpdateContextImpl()
        {
            ids = idsFunc();
        }

        public override IEnumerable<string> GetValue(string input)
        {
            if (!IsReady())
                return new string[0];

            if (input == "")
                return ids.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!ids.ContainsKey(input))
                return new string[0];
            return new string[] { ids[input].ToString() };
        }
    }
    
    public class SpriteTilesheetToken : BaseToken
    {
        private List<DataNeedsIdWithTexture> objs;
        private IDictionary<string, string> tilesheets;

        public SpriteTilesheetToken(string type, List<DataNeedsIdWithTexture> objs)
        : base(type, "SpriteTilesheet")
        {
            this.objs = objs;
        }

        protected override void UpdateContextImpl()
        {
            var dict = new Dictionary<string, string>();
            foreach (var obj in objs)
            {
                dict.Add(obj.Name, obj.tilesheet);
            }
            tilesheets = dict;
        }

        public override IEnumerable<string> GetValue(string input)
        {
            if (!IsReady())
                return new string[0];

            if (input == "")
                return tilesheets.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!tilesheets.ContainsKey(input))
                return new string[0];
            return new string[] { tilesheets[input].ToString() };
        }
    }

    public class SpriteCoordinateToken : BaseToken
    {
        public readonly bool coordinateIsX;
        private List<DataNeedsIdWithTexture> objs;
        private IDictionary<string, int> coordinates;
        
        public SpriteCoordinateToken(string type, bool coordinateIsX, List<DataNeedsIdWithTexture> objs )
        : base(type, "Sprite" + (coordinateIsX ? "X" : "Y"))
        {
            this.coordinateIsX = coordinateIsX;
            this.objs = objs;
        }

        protected override void UpdateContextImpl()
        {
            var dict = new Dictionary<string, int>();
            foreach ( var obj in objs )
            {
                dict.Add(obj.Name, coordinateIsX ? obj.tilesheetX : obj.tilesheetY);
            }
            coordinates = dict;
        }

        public override IEnumerable<string> GetValue(string input)
        {
            if (!IsReady())
                return new string[0];

            if (input == "")
                return coordinates.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!coordinates.ContainsKey(input))
                return new string[0];
            return new string[] { coordinates[input].ToString() };
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
            tokens.Add(new IdToken("Object", ja.GetAllObjectIds));
            tokens.Add(new IdToken("Crop", ja.GetAllCropIds));
            tokens.Add(new IdToken("FruitTree", ja.GetAllFruitTreeIds));
            tokens.Add(new IdToken("BigCraftable", ja.GetAllBigCraftableIds));
            tokens.Add(new IdToken("Hat", ja.GetAllHatIds));
            tokens.Add(new IdToken("Weapon", ja.GetAllWeaponIds));
            tokens.Add(new IdToken("Clothing", ja.GetAllClothingIds));
            tokens.Add(new SpriteTilesheetToken("Object", Mod.instance.objects.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Object", true, Mod.instance.objects.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Object", false, Mod.instance.objects.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteTilesheetToken("Crop", Mod.instance.crops.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Crop", true, Mod.instance.crops.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Crop", false, Mod.instance.crops.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteTilesheetToken("FruitTree", Mod.instance.fruitTrees.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("FruitTree", true, Mod.instance.fruitTrees.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("FruitTree", false, Mod.instance.fruitTrees.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteTilesheetToken("BigCraftable", Mod.instance.bigCraftables.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("BigCraftable", true, Mod.instance.bigCraftables.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("BigCraftable", false, Mod.instance.bigCraftables.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteTilesheetToken("Hat", Mod.instance.hats.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Hat", true, Mod.instance.hats.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Hat", false, Mod.instance.hats.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteTilesheetToken("Weapon", Mod.instance.weapons.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Weapon", true, Mod.instance.weapons.ToList<DataNeedsIdWithTexture>()));
            tokens.Add(new SpriteCoordinateToken("Weapon", false, Mod.instance.weapons.ToList<DataNeedsIdWithTexture>()));
            // TODO: Shirt tilesheet
            // TODO: Shirt x
            // TODO: Shirt y
            // TODO: Pants tilesheet
            // TODO: Pants x
            // TODO: Pants y

            foreach (var token in tokens)
            {
                cp.RegisterToken(Mod.instance.ModManifest, token.TokenName, token.UpdateContext, token.IsReady, token.GetValue, true, true);
            }
        }
    }
}
