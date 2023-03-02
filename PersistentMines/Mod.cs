using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace PersistentMines
{
    [XmlType( "Mods_spacechase0_PersistentMines_Data" )]
    public class MineLevelData : INetObject<NetFields> // Interface required to make "net code" happy (we don't do any networking but it is needed for the serializer)
    {
        public NetFields NetFields { get; } = new();

        // These can't be normal Dictionary<Vector2, ...>s because the serializer complains
        public NetVector2Dictionary<StardewValley.Object, NetRef<StardewValley.Object>> objects = new();
        public NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = new();

        // In 1.6 support buildings and animals?
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        internal static NetIntDictionary<MineLevelData, NetRef<MineLevelData>> mineLevelData = new();
        private XmlSerializer serializer;

        public const string DataFile = "spacechase0.PersistentMines.xml";
        public const string DataKey = "minePersistent";

        public override void Entry(StardewModdingAPI.IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;

            Helper.Events.GameLoop.SaveCreated += this.GameLoop_SaveCreated;
            Helper.Events.GameLoop.Saved += this.GameLoop_Saved;
            Helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;

            Helper.Events.World.ObjectListChanged += this.World_ObjectListChanged;
            Helper.Events.World.TerrainFeatureListChanged += this.World_TerrainFeatureListChanged; ;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(MineLevelData));
        }

        private void GameLoop_SaveCreated(object sender, SaveCreatedEventArgs e)
        {
            mineLevelData = new();
        }

        private void GameLoop_Saved(object sender, SavedEventArgs e)
        {
            if (serializer == null)
                serializer = SaveGame.GetSerializer(mineLevelData.GetType());

            using var fs = File.Open(Path.Combine(Constants.CurrentSavePath, DataFile), FileMode.Create);
            serializer.Serialize(fs, mineLevelData);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!File.Exists(Path.Combine(Constants.CurrentSavePath, DataFile)))
            {
                mineLevelData = new();
                return;
            }

            if (serializer == null)
                serializer = SaveGame.GetSerializer(mineLevelData.GetType());

            using var fs = File.OpenRead(Path.Combine(Constants.CurrentSavePath, DataFile));
            mineLevelData = (NetIntDictionary<MineLevelData, NetRef<MineLevelData>>)serializer.Deserialize(fs);
        }

        private void World_ObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (e.Location is MineShaft ms)
            {
                foreach (var added in e.Added)
                {
                    added.Value.modData.Add(DataKey, "T");
                }
            }
        }

        private void World_TerrainFeatureListChanged(object sender, TerrainFeatureListChangedEventArgs e)
        {
            if (e.Location is MineShaft ms)
            {
                foreach (var added in e.Added)
                {
                    added.Value.modData.Add(DataKey, "T");
                }
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction))]
    public static class ObjectPlacementActionAllowChestsPatch
    {
        public static bool Prefix(StardewValley.Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            if (__instance.ParentSheetIndex != 130 && __instance.ParentSheetIndex != 232) return true;

            Vector2 placementTile = new Vector2(x / 64, y / 64);
            __instance.setHealth(10);
            if (who != null)
            {
                __instance.owner.Value = who.UniqueMultiplayerID;
            }
            else
            {
                __instance.owner.Value = Game1.player.UniqueMultiplayerID;
            }

            location.objects.Add(placementTile, new Chest(playerChest: true, placementTile, __instance.parentSheetIndex)
            {
                shakeTimer = 50
            });
            location.playSound(((int)__instance.parentSheetIndex == 130) ? "axe" : "hammer");
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(MineShaft), "populateLevel")]
    public static class MineShaftPopulateLevelRestoreSavedPatch
    {
        public static void DoLoad(MineShaft ms)
        {
            if (!Mod.mineLevelData.ContainsKey(ms.mineLevel))
                return;
            Log.Debug("Loading " + ms.mineLevel);
            var data = Mod.mineLevelData[ms.mineLevel];

            foreach (var pair in data.objects.Pairs)
            {
                ms.objects.Add(pair.Key, pair.Value);
            }
            foreach (var pair in data.terrainFeatures.Pairs)
            {
                ms.terrainFeatures.Add(pair.Key, pair.Value);
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
        {
            List<CodeInstruction> ret = new();

            foreach (var insn in insns)
            {
                ret.Add(insn);
                if (insn.opcode == OpCodes.Callvirt && insn.operand is MethodInfo meth && meth == typeof(NetCollection<NPC>).GetMethod("Clear"))
                {
                    ret.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    ret.Add(new CodeInstruction(OpCodes.Call, typeof(MineShaftPopulateLevelRestoreSavedPatch).GetMethod("DoLoad")));
                }
            }

            return ret;
        }
    }

    [HarmonyPatch(typeof(List<MineShaft>), nameof(List<MineShaft>.RemoveAll))]
    public static class MineShaftClearInactiveMinesButSavePatch
    {
        private static void DoSave(MineShaft ms)
        {
            if (ms.mineLevel > MineShaft.bottomOfMineLevel && ms.mineLevel != MineShaft.quarryMineShaft)
                return;
            Log.Debug("Saving " + ms.mineLevel);

            if (Mod.mineLevelData.ContainsKey(ms.mineLevel))
                Mod.mineLevelData.Remove(ms.mineLevel);

            MineLevelData data = new();
            foreach (var pair in ms.objects.Pairs)
            {
                if (pair.Value.modData.ContainsKey(Mod.DataKey))
                    data.objects.Add(pair.Key, pair.Value);
            }
            foreach (var pair in ms.terrainFeatures.Pairs)
            {
                if (pair.Value.modData.ContainsKey(Mod.DataKey))
                    data.terrainFeatures.Add(pair.Key, pair.Value);
            }
            Mod.mineLevelData.Add(ms.mineLevel, data);
        }

        public static bool Prefix(
            List<MineShaft> __instance, Predicate<MineShaft> match, ref int __result,
            ref int ____size, ref MineShaft[] ____items, ref int ____version
            )
        {
            if (__instance != MineShaft.activeMines)
                return true;

            Predicate<MineShaft> theMatch = (MineShaft ms) =>
            {
                bool ret = match(ms);
                if ( ret )
                    DoSave(ms);
                return ret;
            };

            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            int i;
            for (i = 0; i < ____size && !theMatch(____items[i]); i++)
            {
            }
            if (i >= ____size)
            {
                __result = 0;
                return false;
            }
            int j = i + 1;
            while (j < ____size)
            {
                for (; j < ____size && theMatch(____items[j]); j++)
                {
                }
                if (j < ____size)
                {
                    ____items[i++] = ____items[j++];
                }
            }
            if (RuntimeHelpers.IsReferenceOrContainsReferences<MineShaft>())
            {
                Array.Clear(____items, i, ____size - i);
            }
            int result = ____size - i;
            ____size = i;
            ____version++;

            __result = result;
            return false;
        }
    }
}
