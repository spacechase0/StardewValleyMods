using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SpaceShared;
using StardewValley.GameData;

namespace SpaceCore.Spawnables
{
    public class ItemData
    {
        public string QualifiedId { get; set; }
        public int Quantity { get; set; }
        public int Quality { get; set; }
    }

    public class SpawnableDefinitionData
    {
        public enum SpawnableType
        {
            SetPiece,
            Forageable,
            Minable,
            LargeMinable,
            Breakable,
            LootChest,
            Furniture,
            Monster,
            //Critter,
            WildTree,
            FruitTree,
        }

        public enum MinableType
        {
            Pickaxe,
            Axe,
        }

        public enum LargeMinableType
        {
            GiantCrop
        }

        public SpawnableType Type { get; set; }
        public string Condition { get; set; } = "TRUE";

        public string SetPiecesMap { get; set; }
        public int SetPieceSizeX { get; set; }
        public int SetPieceSizeY { get; set; }
        public int SetPieceCount { get; set; }

        // Items must be objects unless IsTillSpot is true
        public List<Weighted<GenericSpawnItemDataWithCondition>> ForageableItemData { get; set; } = new();
        public bool ForageableExpiresWeekly { get; set; } = true;
        public bool ForageableIsTillSpot { get; set; } = false;

        public MinableType MinableTool { get; set; }
        public string MinableObjectId { get; set; }
        public int MineableHealth { get; set; }
        public List<List<Weighted<GenericSpawnItemDataWithCondition>>> MinableDrops { get; set; } = new();
        public int MinableExperienceGranted { get; set; } = 0; // Type dependant on MinableTool

        public MinableType LargeMinableTool { get; set; }
        public int LargeMinableRequiredToolTier { get; set; } = 0;
        public int LargeMinableHealth { get; set; }
        public int LargeMinableSizeX { get; set; } = 2;
        public int LargeMinableSizeY { get; set; } = 2;
        public string LargeMinableTexture { get; set; } // null for vanilla spritesheet
        public int LargeMinableSpriteIndex { get; set; }
        public List<List<Weighted<GenericSpawnItemDataWithCondition>>> LargeMinableDrops { get; set; } = new();
        public List<Weighted<GenericSpawnItemDataWithCondition>> LargeMinableShavingDrop { get; set; } = new();
        public int LargeMinableExperienceGranted { get; set; } = 0; // Type dependant on LargeMinableTool

        public string BreakableBigCraftableId { get; set; }
        public int BreakableHealth { get; set; }
        public string BreakableHitSound { get; set; } = "woodWhack";
        public string BreakableBrokenSound { get; set; } = "barrelBreak";
        public List<List<Weighted<GenericSpawnItemDataWithCondition>>> BreakableDrops { get; set; } = new();

        public string LootChestBigCraftableId { get; set; }
        public List<List<Weighted<GenericSpawnItemDataWithCondition>>> LootChestDrops { get; set; } = new();

        public string FurnitureQualifiedId { get; set; }
        public int FurnitureRotation { get; set; }
        public bool FurnitureIsOn { get; set; } = false;
        public List<Weighted<GenericSpawnItemDataWithCondition>> FurnitureHeldObject { get; set; } = new();
        public bool FurnitureCanPickUp { get; set; } = true;

        public string MonsterType { get; set; }
        public string MonsterName { get; set; } // Stats are pulled from Data/Monsters for this
        public string MonsterTextureOverride { get; set; }
        public List<List<Weighted<GenericSpawnItemDataWithCondition>>> MonsterDropOverride { get; set; } // if set, normal monster drops don't happen
        public Dictionary<string, object> MonsterAdditionalData { get; set; } = new();

        //public string CritterType { get; set; }
        // This won't work with a lot of them so unfortunately not happening (for now)...
        //public string CritterTextureOverride { get; set; }
        //public int CritterSpriteIndexOverride { get; set; }

        public string WildTreeType { get; set; }

        public string FruitTreeType { get; set; }
    }

    public class SpawnableSpawningGroupData
    {
        public class ToSpawn
        {
            public List<Weighted<string>> SpawnableIds { get; set; } = new();

            public int Minimum { get; set; }
            public int Maximum { get; set; }
        }
        public List<ToSpawn> SpawnablesToSpawn { get; set; } = new();
    }
}
