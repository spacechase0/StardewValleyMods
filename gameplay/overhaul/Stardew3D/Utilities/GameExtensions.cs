using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Valve.VR;

namespace Stardew3D.Utilities;

public static class GameExtenions
{
    public static string GetExtendedQualifiedId(this object obj)
    {
        return (obj?.GetExtendedQualifiedIds() ?? [null])[0];
    }

    public static string[] GetExtendedQualifiedIds(this object obj)
    {
        // TODO: Dehardcode this
        if (obj is MeleeWeapon weapon)
            return
            [
                weapon.QualifiedItemId,
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/{weapon.GetItemTypeId().Substring(1)}{weapon.type.Value}",
                weapon.GetItemTypeId(),
            ];
        else if (obj is Item item)
            return
            [
                item.QualifiedItemId,
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/{item.GetItemTypeId().Substring(1)}{item.GetType().Name}",
                item.GetItemTypeId()
            ];

        else if (obj is GameLocation location)
            return [$"({Mod.Instance.ModManifest.UniqueID}/Location){location.Name}", $"({Mod.Instance.ModManifest.UniqueID}/Location)"];

        else if (obj is Grass grass)
            return [$"({Mod.Instance.ModManifest.UniqueID}/Grass){grass.grassType.Value}", $"({Mod.Instance.ModManifest.UniqueID}/Grass)"];
        else if (obj is ResourceClump clump)
            return [$"({Mod.Instance.ModManifest.UniqueID}/ResourceClump){clump.textureName.Value ?? Game1.objectSpriteSheetName}:{clump.parentSheetIndex.Value}", $"({Mod.Instance.ModManifest.UniqueID}/ResourceClump)"];
        else if (obj is Tree tree)
            return [$"({Mod.Instance.ModManifest.UniqueID}/Tree){tree.treeType.Value}", $"({Mod.Instance.ModManifest.UniqueID}/Tree)"];
        else if (obj is HoeDirt hoeDirt)
            return
            [
                $"({Mod.Instance.ModManifest.UniqueID}/HoeDirt){hoeDirt.sourceRectPosition}/{hoeDirt.fertilizer.Value}",
                $"({Mod.Instance.ModManifest.UniqueID}/HoeDirt){hoeDirt.sourceRectPosition}",
                $"({Mod.Instance.ModManifest.UniqueID}/HoeDirt)"
            ];
        else if (obj is Flooring flooring)
            return
            [
                $"({Mod.Instance.ModManifest.UniqueID}/Flooring){flooring.whichFloor.Value}/{flooring.whichView.Value}",
                $"({Mod.Instance.ModManifest.UniqueID}/Flooring){flooring.whichFloor.Value}",
                $"({Mod.Instance.ModManifest.UniqueID}/Flooring)"
            ];
        else if (obj is TerrainFeature)
            return [$"({Mod.Instance.ModManifest.UniqueID}/TerrainFeatureType){obj.GetType().Name}", $"({Mod.Instance.ModManifest.UniqueID}/TerrainFeatureType)"];

        else if (obj is Farmer farmer)
            return [$"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Farmer){farmer.Name}", $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Farmer)"];
        else if (obj is Monster monster)
            return
            [
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Monster){monster.Name}",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/CharacterType){monster.GetType().Name}",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Monster)",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Character)"
            ];
        else if (obj is FarmAnimal animal)
            return
            [
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/FarmAnimal){animal.Name}",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/CharacterType){animal.type.Value}",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/CharacterType){animal.GetType().Name}",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/FarmAnimal)",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Character)"
            ];
        else if (obj is NPC npc)
            return
            [
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/NPC){npc.Name}/{npc.LastAppearanceId}",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/NPC){npc.Name}",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/CharacterType){npc.GetType().Name}",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/NPC)",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Character)"
            ];
        else if (obj is Character character)
            return
            [
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Character){character.Name}",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/CharacterType){character.GetType().Name}",
                $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Character)"
            ];

        else if (obj is Building building)
            return
            [
                $"({Mod.Instance.ModManifest.UniqueID}/Building){building.id}/{building.skinId}",
                $"({Mod.Instance.ModManifest.UniqueID}/Building){building.id}",
                $"({Mod.Instance.ModManifest.UniqueID}/BuildingType){building.GetType().Name}",
                $"({Mod.Instance.ModManifest.UniqueID}/Building)"
            ];

        else if (obj is Crop crop)
            return
            [
                $"({Mod.Instance.ModManifest.UniqueID}/Crop){crop.netSeedIndex}/{crop.currentPhase.Value}",
                $"({Mod.Instance.ModManifest.UniqueID}/Crop){crop.netSeedIndex}",
                $"({Mod.Instance.ModManifest.UniqueID}/Crop)"
            ];

        else if (obj is IClickableMenu menu)
            return [$"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Menu){menu.GetType().Namespace}.{menu.GetType().Name}", $"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Menu)"];

        return [obj?.GetType()?.FullName ?? "null"];
    }
}
