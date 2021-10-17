using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace MoreRings.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Crop"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class CropPatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>The last item modified by <see cref="ModifyCropQuality"/>.</summary>
        private static Item LastItem;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Crop>(nameof(Crop.harvest)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_Harvest))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="Crop.harvest"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_Harvest(IEnumerable<CodeInstruction> instructions)
        {
            return instructions
                .MethodReplacer(
                    from: PatchHelper.RequireMethod<Game1>(nameof(Game1.createItemDebris)),
                    to: PatchHelper.RequireMethod<CropPatcher>(nameof(Game_CreateItemDebris))
                )
                .MethodReplacer(
                    from: PatchHelper.RequireMethod<Farmer>(nameof(Farmer.addItemToInventoryBool)),
                    to: PatchHelper.RequireMethod<CropPatcher>(nameof(Farmer_AddItemToInventoryBool))
                );

        }

        /// <summary>Call <see cref="Game1.createItemDebris"/> after adjusting the item quality.</summary>
        public static Debris Game_CreateItemDebris(Item item, Vector2 origin, int direction, GameLocation location, int groundLevel)
        {
            CropPatcher.ModifyCropQuality(item);

            return Game1.createItemDebris(item, origin, direction, location, groundLevel);
        }


        /// <summary>Call <see cref="Farmer.addItemToInventoryBool"/> after adjusting the item quality.</summary>
        public static bool Farmer_AddItemToInventoryBool(Farmer farmer, Item item, bool makeActiveObject)
        {
            CropPatcher.ModifyCropQuality(item);

            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        /// <summary>Increase the harvested crop quality if the player has the <see cref="Mod.RingQuality"/> ring equipped.</summary>
        /// <param name="item">The item to modify.</param>
        private static void ModifyCropQuality(Item item)
        {
            if (item is not SObject obj || object.ReferenceEquals(item, CropPatcher.LastItem))
                return;

            CropPatcher.LastItem = item;
            if (Game1.random.NextDouble() < Mod.Instance.CountRingsEquipped(Mod.Instance.RingQuality) * Mod.Instance.Config.QualityRing_ChancePerRing)
            {
                obj.Quality = obj.Quality switch
                {
                    SObject.lowQuality => SObject.medQuality,
                    SObject.medQuality => SObject.highQuality,
                    SObject.highQuality => SObject.bestQuality,
                    _ => obj.Quality
                };
            }
        }
    }
}
