using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace MultiFertilizer
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static string KEY_FERT => $"{Mod.instance.ModManifest.UniqueID}/FertilizerLevel";
        public static string KEY_RETAIN => $"{Mod.instance.ModManifest.UniqueID}/WaterRetainLevel";
        public static string KEY_SPEED => $"{Mod.instance.ModManifest.UniqueID}/SpeedGrowLevel";

        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.plant))]
    public static class HoeDirtPlantPatch
    {
        public static bool Prefix(HoeDirt __instance, int index, int tileX, int tileY, Farmer who, bool isFertilizer, GameLocation location)
        {
            if (isFertilizer)
            {
                if (__instance.crop != null && __instance.crop.currentPhase != 0)
                    return false;

                int level = 0;
                string key = "";
                switch (index)
                {
                    case 368: level = 1; key = Mod.KEY_FERT; break;
                    case 369: level = 2; key = Mod.KEY_FERT; break;
                    case 919: level = 3; key = Mod.KEY_FERT; break;
                    case 370: level = 1; key = Mod.KEY_RETAIN; break;
                    case 371: level = 2; key = Mod.KEY_RETAIN; break;
                    case 920: level = 3; key = Mod.KEY_RETAIN; break;
                    case 465: level = 1; key = Mod.KEY_SPEED; break;
                    case 466: level = 2; key = Mod.KEY_SPEED; break;
                    case 918: level = 3; key = Mod.KEY_SPEED; break;
                }

                if (__instance.modData.ContainsKey(key))
                    return false;
                else
                {
                    __instance.modData[key] = level.ToString();
                    if (key == Mod.KEY_SPEED)
                        Mod.instance.Helper.Reflection.GetMethod(__instance, "applySpeedIncreases").Invoke(who);
                    location.playSound("dirtyHit");
                    return true;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.DrawOptimized))]
    public static class HoeDirtDrawPatchTranspiler
    {
        public static void DrawMultiFertilizer(SpriteBatch spriteBatch, Texture2D tex, Vector2 pos, Rectangle? sourceRect, Color col, float rot, Vector2 origin, float scale, SpriteEffects fx, float depth, HoeDirt __instance)
        {
            List<int> fertilizers = new List<int>();
            if (__instance.modData.ContainsKey(Mod.KEY_FERT))
            {
                int level = int.Parse(__instance.modData[Mod.KEY_FERT]);
                int index = 0;
                switch (level)
                {
                    case 1: index = 368; break;
                    case 2: index = 369; break;
                    case 3: index = 919; break;
                }
                if (index != 0)
                    fertilizers.Add(index);
            }
            if (__instance.modData.ContainsKey(Mod.KEY_RETAIN))
            {
                int level = int.Parse(__instance.modData[Mod.KEY_RETAIN]);
                int index = 0;
                switch (level)
                {
                    case 1: index = 370; break;
                    case 2: index = 371; break;
                    case 3: index = 920; break;
                }
                if (index != 0)
                    fertilizers.Add(index);
            }
            if (__instance.modData.ContainsKey(Mod.KEY_SPEED))
            {
                int level = int.Parse(__instance.modData[Mod.KEY_SPEED]);
                int index = 0;
                switch (level)
                {
                    case 1: index = 465; break;
                    case 2: index = 466; break;
                    case 3: index = 918; break;
                }
                if (index != 0)
                    fertilizers.Add(index);
            }
            foreach (int fertilizer in fertilizers)
            {
                if (fertilizer != 0)
                {
                    int fertilizerIndex = 0;
                    switch (fertilizer)
                    {
                        case 369:
                            fertilizerIndex = 1;
                            break;
                        case 370:
                            fertilizerIndex = 3;
                            break;
                        case 371:
                            fertilizerIndex = 4;
                            break;
                        case 920:
                            fertilizerIndex = 5;
                            break;
                        case 465:
                            fertilizerIndex = 6;
                            break;
                        case 466:
                            fertilizerIndex = 7;
                            break;
                        case 918:
                            fertilizerIndex = 8;
                            break;
                        case 919:
                            fertilizerIndex = 2;
                            break;
                    }
                    spriteBatch.Draw(Game1.mouseCursors, pos, new Rectangle(173 + fertilizerIndex / 3 * 16, 462 + fertilizerIndex % 3 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.9E-08f);
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            bool foundFert = false;
            bool stopCaring = false;

            // When we find the the fertilizer reference, replace the next draw with our call
            // Add the HoeDirt instance at the end of the argument list

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (stopCaring)
                {
                    newInsns.Add(insn);
                    continue;
                }

                if (insn.opcode == OpCodes.Ldfld && (insn.operand as FieldInfo).Name == "fertilizer")
                {
                    foundFert = true;
                }
                else if (foundFert &&
                          insn.opcode == OpCodes.Callvirt && (insn.operand as MethodInfo).Name == "Draw")
                {
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_0));

                    insn.opcode = OpCodes.Call;
                    insn.operand = typeof(HoeDirtDrawPatchTranspiler).GetMethod("DrawMultiFertilizer");
                    newInsns.Add(insn);

                    stopCaring = true;
                }
                else
                    newInsns.Add(insn);
            }

            return newInsns;
        }
    }

    /*
    [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.draw))]
    [HarmonyBefore( "Platonymous.TMXLoader" )]
    public static class HoeDirtDrawPatch
    {
        public static bool Prefix(HoeDirt __instance, SpriteBatch spriteBatch, Vector2 tileLocation )
        {
            int state = __instance.state.Value;
            if ( state != HoeDirt.invisible )
            {
                var texField = Mod.instance.Helper.Reflection.GetField<Texture2D>(__instance, "texture" );
                if ( texField.GetValue() == null )
                {
                    texField.SetValue( ( ( Game1.currentLocation.Name.Equals( "Mountain" ) || Game1.currentLocation.Name.Equals( "Mine" ) || ( Game1.currentLocation is MineShaft && ( Game1.currentLocation as MineShaft ).shouldShowDarkHoeDirt() ) || Game1.currentLocation is VolcanoDungeon ) ? HoeDirt.darkTexture : HoeDirt.lightTexture ) );
                    if ( ( Game1.GetSeasonForLocation( Game1.currentLocation ).Equals( "winter" ) && !( Game1.currentLocation is Desert ) && !Game1.currentLocation.IsGreenhouse && !Game1.currentLocation.SeedsIgnoreSeasonsHere() && !( Game1.currentLocation is MineShaft ) ) || ( Game1.currentLocation is MineShaft && ( Game1.currentLocation as MineShaft ).shouldUseSnowTextureHoeDirt() ) )
                    {
                        texField.SetValue( HoeDirt.snowTexture );
                    }
                }

                byte drawSum = (byte)(Mod.instance.Helper.Reflection.GetField<byte>(__instance, "neighborMask" ).GetValue() & 0xF);
                int sourceRectPosition = HoeDirt.drawGuide[drawSum];
                int wateredRectPosition = HoeDirt.drawGuide[Mod.instance.Helper.Reflection.GetField<byte>(__instance, "wateredNeighborMask" ).GetValue()];
                Vector2 drawPos = Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f));
                var c = Mod.instance.Helper.Reflection.GetField<NetColor>( __instance, "c" ).GetValue();
                spriteBatch.Draw( texField.GetValue(), drawPos, new Rectangle( sourceRectPosition % 4 * 16, sourceRectPosition / 4 * 16, 16, 16 ), c, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f );

                if ( state == HoeDirt.watered )
                {
                    spriteBatch.Draw( texField.GetValue(), drawPos, new Rectangle( wateredRectPosition % 4 * 16 + ( __instance.paddyWaterCheck( Game1.currentLocation, tileLocation ) ? 128 : 64 ), wateredRectPosition / 4 * 16, 16, 16 ), c, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.2E-08f );
                }
                List<int> fertilizers = new List<int>();
                if ( __instance.modData.ContainsKey( Mod.KEY_FERT ) )
                {
                    int level = int.Parse( __instance.modData[ Mod.KEY_FERT ] );
                    int index = 0;
                    switch ( level )
                    {
                        case 1: index = 368; break;
                        case 2: index = 369; break;
                        case 3: index = 919; break;
                    }
                    if ( index != 0 )
                        fertilizers.Add( index );
                }
                if ( __instance.modData.ContainsKey( Mod.KEY_RETAIN ) )
                {
                    int level = int.Parse( __instance.modData[ Mod.KEY_RETAIN ] );
                    int index = 0;
                    switch ( level )
                    {
                        case 1: index = 370; break;
                        case 2: index = 371; break;
                        case 3: index = 920; break;
                    }
                    if ( index != 0 )
                        fertilizers.Add( index );
                }
                if ( __instance.modData.ContainsKey( Mod.KEY_SPEED ) )
                {
                    int level = int.Parse( __instance.modData[ Mod.KEY_SPEED ] );
                    int index = 0;
                    switch ( level )
                    {
                        case 1: index = 465; break;
                        case 2: index = 466; break;
                        case 3: index = 918; break;
                    }
                    if ( index != 0 )
                        fertilizers.Add( index );
                }
                foreach ( int fertilizer in fertilizers )
                {
                    if ( fertilizer != 0 )
                    {
                        int fertilizerIndex = 0;
                        switch ( fertilizer )
                        {
                            case 369:
                                fertilizerIndex = 1;
                                break;
                            case 370:
                                fertilizerIndex = 3;
                                break;
                            case 371:
                                fertilizerIndex = 4;
                                break;
                            case 920:
                                fertilizerIndex = 5;
                                break;
                            case 465:
                                fertilizerIndex = 6;
                                break;
                            case 466:
                                fertilizerIndex = 7;
                                break;
                            case 918:
                                fertilizerIndex = 8;
                                break;
                            case 919:
                                fertilizerIndex = 2;
                                break;
                        }
                        spriteBatch.Draw( Game1.mouseCursors, drawPos, new Rectangle( 173 + fertilizerIndex / 3 * 16, 462 + fertilizerIndex % 3 * 16, 16, 16 ), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.9E-08f );
                    }
                }
            }
            if ( __instance.crop != null )
            {
                __instance.crop.draw( spriteBatch, tileLocation, ( state == HoeDirt.watered && ( int ) __instance.crop.currentPhase == 0 && __instance.crop.shouldDrawDarkWhenWatered() ) ? ( new Color( 180, 100, 200 ) * 1f ) : Color.White, Mod.instance.Helper.Reflection.GetField< float >( __instance, "shakeRotation" ).GetValue() );
            }
            return false;
        }
    }*/

    [HarmonyPatch(typeof(HoeDirt), "applySpeedIncreases")]
    public static class HoeDirtSpeedIncreasePatch
    {
        public static void Prefix(HoeDirt __instance, Farmer who)
        {
            if (!__instance.modData.ContainsKey(Mod.KEY_SPEED))
                return;

            int index = 0;
            switch (int.Parse(__instance.modData[Mod.KEY_SPEED]))
            {
                case 1: index = 465; break;
                case 2: index = 466; break;
                case 3: index = 918; break;
            }

            __instance.fertilizer.Value = index;
        }

        public static void Postfix(HoeDirt __instance, Farmer who)
        {
            __instance.fertilizer.Value = 0;
        }
    }

    [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.canPlantThisSeedHere))]
    public static class HoeDirtCanPlantSeedHerePatch
    {
        public static bool Prefix(HoeDirt __instance, int objectIndex, int tileX, int tileY, bool isFertilizer, ref bool __result)
        {
            if (isFertilizer)
            {
                int level = 0;
                string key = "";
                switch (objectIndex)
                {
                    case 368: level = 1; key = Mod.KEY_FERT; break;
                    case 369: level = 2; key = Mod.KEY_FERT; break;
                    case 919: level = 3; key = Mod.KEY_FERT; break;
                    case 370: level = 1; key = Mod.KEY_RETAIN; break;
                    case 371: level = 2; key = Mod.KEY_RETAIN; break;
                    case 920: level = 3; key = Mod.KEY_RETAIN; break;
                    case 465: level = 1; key = Mod.KEY_SPEED; break;
                    case 466: level = 2; key = Mod.KEY_SPEED; break;
                    case 918: level = 3; key = Mod.KEY_SPEED; break;
                }

                __result = !__instance.modData.ContainsKey(key);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.dayUpdate))]
    public static class HoeDirtDayUpdatePatch
    {
        public static void Prefix(HoeDirt __instance, GameLocation environment, Vector2 tileLocation)
        {
            if (!__instance.modData.ContainsKey(Mod.KEY_RETAIN))
                return;

            int index = 0;
            switch (int.Parse(__instance.modData[Mod.KEY_RETAIN]))
            {
                case 1: index = 370; break;
                case 2: index = 371; break;
                case 3: index = 920; break;
            }

            __instance.fertilizer.Value = index;
        }

        public static void Postfix(HoeDirt __instance, GameLocation environment, Vector2 tileLocation)
        {
            __instance.fertilizer.Value = 0;
        }
    }

    [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.seasonUpdate))]
    public static class HoeDirtSeasonUpdatePatch
    {
        public static void Prefix(HoeDirt __instance, bool onLoad)
        {
            if (!onLoad && (__instance.crop == null || (bool)__instance.crop.dead || !__instance.crop.seasonsToGrowIn.Contains(Game1.currentLocation.GetSeasonForLocation())))
            {
                __instance.modData.Remove(Mod.KEY_FERT);
                __instance.modData.Remove(Mod.KEY_RETAIN);
                __instance.modData.Remove(Mod.KEY_SPEED);
            }
        }
    }

    [HarmonyPatch(typeof(Crop), nameof(Crop.harvest))]
    public static class CropHarvestPatch
    {
        public static void Prefix(Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester)
        {
            if (!soil.modData.ContainsKey(Mod.KEY_FERT))
                return;

            int index = 0;
            switch (int.Parse(soil.modData[Mod.KEY_FERT]))
            {
                case 1: index = 368; break;
                case 2: index = 369; break;
                case 3: index = 919; break;
            }

            soil.fertilizer.Value = index;
        }

        public static void Postfix(Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester)
        {
            soil.fertilizer.Value = 0;
        }
    }

    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isTileOccupiedForPlacement))]
    public static class GameLocationTileOccupiedPlacementPatch
    {
        public static bool PatchedSection(GameLocation __instance, Vector2 tileLocation, StardewValley.Object toPlace)
        {
            if (toPlace.Category == -19 && __instance.terrainFeatures.ContainsKey(tileLocation) && __instance.terrainFeatures[tileLocation] is HoeDirt)
            {
                HoeDirt hoe_dirt = __instance.terrainFeatures[tileLocation] as HoeDirt;

                int level = 0;
                string key = "";
                switch (toPlace.ParentSheetIndex)
                {
                    case 368: level = 1; key = Mod.KEY_FERT; break;
                    case 369: level = 2; key = Mod.KEY_FERT; break;
                    case 919: level = 3; key = Mod.KEY_FERT; break;
                    case 370: level = 1; key = Mod.KEY_RETAIN; break;
                    case 371: level = 2; key = Mod.KEY_RETAIN; break;
                    case 920: level = 3; key = Mod.KEY_RETAIN; break;
                    case 465: level = 1; key = Mod.KEY_SPEED; break;
                    case 466: level = 2; key = Mod.KEY_SPEED; break;
                    case 918: level = 3; key = Mod.KEY_SPEED; break;
                }

                if (hoe_dirt.modData.ContainsKey(key))
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            bool stopCaring = false;
            bool foundFertCategory = false;

            // When we find -19, after the next instruction:
            // Place our patched section function call. If it returns true, return from the function true.

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (stopCaring)
                {
                    newInsns.Add(insn);
                    continue;
                }

                if (insn.opcode == OpCodes.Ldc_I4_S && (sbyte)insn.operand == (sbyte)-19)
                {
                    newInsns.Add(insn);
                    foundFertCategory = true;
                }
                else if (foundFertCategory)
                {
                    newInsns.Add(insn);

                    var branchPastOld = new CodeInstruction(OpCodes.Br, insn.operand);
                    branchPastOld.labels.Add(gen.DefineLabel());

                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_1));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_2));
                    newInsns.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GameLocationTileOccupiedPlacementPatch), nameof(GameLocationTileOccupiedPlacementPatch.PatchedSection))));

                    newInsns.Add(new CodeInstruction(OpCodes.Brfalse, branchPastOld.labels[0]));

                    newInsns.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                    newInsns.Add(new CodeInstruction(OpCodes.Ret));

                    newInsns.Add(branchPastOld);

                    foundFertCategory = false;
                    stopCaring = true;
                }
                else
                    newInsns.Add(insn);
            }

            return newInsns;
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.canBePlacedHere))]
    public static class ObjectCanPlaceHerePatch
    {
        public static bool PatchedSection(StardewValley.Object __instance, GameLocation l, Vector2 tile)
        {
            if (l.isTileHoeDirt(tile))
            {
                int level = 0;
                string key = "";
                switch (__instance.ParentSheetIndex)
                {
                    case 368: level = 1; key = Mod.KEY_FERT; break;
                    case 369: level = 2; key = Mod.KEY_FERT; break;
                    case 919: level = 3; key = Mod.KEY_FERT; break;
                    case 370: level = 1; key = Mod.KEY_RETAIN; break;
                    case 371: level = 2; key = Mod.KEY_RETAIN; break;
                    case 920: level = 3; key = Mod.KEY_RETAIN; break;
                    case 465: level = 1; key = Mod.KEY_SPEED; break;
                    case 466: level = 2; key = Mod.KEY_SPEED; break;
                    case 918: level = 3; key = Mod.KEY_SPEED; break;
                }

                if (__instance.ParentSheetIndex == 805)
                {
                    return true;
                }
                if (l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is HoeDirt && (l.terrainFeatures[tile] as HoeDirt).modData.ContainsKey(key))
                {
                    return true;
                }
                if (l.objects.ContainsKey(tile) && l.objects[tile] is IndoorPot && (l.objects[tile] as IndoorPot).hoeDirt.Value.modData.ContainsKey(key))
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            bool stopCaring = false;
            int fertCategoryCounter = 0;

            // When we find the second -19, after the next instruction:
            // Place our patched section function call. If it returns true, return from the function false.

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (stopCaring)
                {
                    newInsns.Add(insn);
                    continue;
                }

                if (insn.opcode == OpCodes.Ldc_I4_S && (sbyte)insn.operand == (sbyte)-19)
                {
                    newInsns.Add(insn);
                    fertCategoryCounter++;
                }
                else if (fertCategoryCounter == 2)
                {
                    newInsns.Add(insn);

                    var branchPastOld = new CodeInstruction(OpCodes.Br, insn.operand);
                    branchPastOld.labels.Add(gen.DefineLabel());

                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_1));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_2));
                    newInsns.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectCanPlaceHerePatch), nameof(ObjectCanPlaceHerePatch.PatchedSection))));

                    newInsns.Add(new CodeInstruction(OpCodes.Brfalse, branchPastOld.labels[0]));

                    newInsns.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                    newInsns.Add(new CodeInstruction(OpCodes.Ret));

                    newInsns.Add(branchPastOld);

                    ++fertCategoryCounter;
                    stopCaring = true;
                }
                else
                    newInsns.Add(insn);
            }

            return newInsns;
        }
    }

    [HarmonyPatch(typeof(StardewValley.Utility), nameof(StardewValley.Utility.tryToPlaceItem))]
    public static class UtilityTryToPlacePatch
    {
        public static bool PatchedSection(GameLocation location, Item item, int x, int y)
        {
            int level = 0;
            string key = "";
            switch (item.ParentSheetIndex)
            {
                case 368: level = 1; key = Mod.KEY_FERT; break;
                case 369: level = 2; key = Mod.KEY_FERT; break;
                case 919: level = 3; key = Mod.KEY_FERT; break;
                case 370: level = 1; key = Mod.KEY_RETAIN; break;
                case 371: level = 2; key = Mod.KEY_RETAIN; break;
                case 920: level = 3; key = Mod.KEY_RETAIN; break;
                case 465: level = 1; key = Mod.KEY_SPEED; break;
                case 466: level = 2; key = Mod.KEY_SPEED; break;
                case 918: level = 3; key = Mod.KEY_SPEED; break;
            }


            Vector2 tileLocation = new Vector2(x / 64, y / 64);
            if (!location.terrainFeatures.ContainsKey(tileLocation))
                return true;
            HoeDirt hoe_dirt = location.terrainFeatures[tileLocation] as HoeDirt;
            if ((int)(location.terrainFeatures[tileLocation] as HoeDirt).fertilizer != 0)
            {
                if ((location.terrainFeatures[tileLocation] as HoeDirt).modData.ContainsKey(key))
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916-2"));
                }
                return true;
            }
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            bool stopCaring = false;
            int fertCategoryCounter = 0;

            // When we find the -19, after the next instruction:
            // Place our patched section function call. If it returns true, return from the function false.
            // Then skip the old section

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (stopCaring)
                {
                    newInsns.Add(insn);
                    continue;
                }

                if (insn.opcode == OpCodes.Ldc_I4_S && (sbyte)insn.operand == (sbyte)-19)
                {
                    newInsns.Add(insn);
                    fertCategoryCounter++;
                }
                else if (fertCategoryCounter == 1)
                {
                    newInsns.Add(insn);

                    var branchPastOld = new CodeInstruction(OpCodes.Br, insn.operand);
                    branchPastOld.labels.Add(gen.DefineLabel());

                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_1));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_2));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_3));
                    newInsns.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UtilityTryToPlacePatch), nameof(UtilityTryToPlacePatch.PatchedSection))));

                    newInsns.Add(new CodeInstruction(OpCodes.Brfalse, branchPastOld.labels[0]));

                    newInsns.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                    newInsns.Add(new CodeInstruction(OpCodes.Ret));

                    newInsns.Add(branchPastOld);

                    ++fertCategoryCounter;
                    stopCaring = true;
                }
                else
                    newInsns.Add(insn);
            }

            return newInsns;
        }
    }
}
