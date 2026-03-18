using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using SpaceShared;
using Stardew3D.DataModels;
using Stardew3D.GameModes;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace Stardew3D.Handlers.Gameplay;

public class FarmerMotionControlsHandler : FarmerWorldControlsBaseHandler
{
    public FarmerMotionControlsHandler(IGameMode mode, Farmer player)
        : base( mode, player, 2 )
    {
    }

    public Matrix GetHeldTransformFor(IGameCursor cursor, bool forDisplay = false)
    {
        return Matrix.Identity
            * Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f))
            * Matrix.CreateScale(1f / 3)
            //* Matrix.CreateTranslation(new Vector3(0.075f, 0.15f, -0.0f))
            * Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f))
            * Matrix.CreateFromQuaternion // Was getting a gimbal lock otherwise
            (
                  Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(MathHelper.ToRadians(-45)))
                * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(-90)))
                * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationZ(MathHelper.ToRadians(0)))
            )
            * Matrix.CreateTranslation(new Vector3(0f, -0.125f, -0.0f))
            * cursor.Grip;
    }

    public override void Update(IUpdateHandler.UpdateContext ctx)
    {

        base.Update(ctx);
    }

    private class ToolAreaData
    {
        public InteractionArea area;
        public Matrix transform;
        public Matrix prevTransform;
        public Vector3[] verts;
        public Vector3[] prevVerts;
    }

    private Tool tool;
    private Matrix baseTransform;
    private Matrix basePrevTransform;
    private InteractionData interaction;
    private ToolAreaData[] toolAreas;
    private ConditionalWeakTable<IGameCursor, Holder<Matrix>> lastGrips = new();
    protected override void HandleCursor(IUpdateHandler.UpdateContext ctx, IGameCursor cursor)
    {
        if (cursor.Holding is not Tool tool)
            return;
        this.tool = tool;

        interaction = null;
        foreach (var idEntry in cursor.Holding.GetExtendedQualifiedIds())
            interaction ??= InteractionData.Get(idEntry);
        if (interaction == null)
            return;

        baseTransform = GetHeldTransformFor(cursor);
        basePrevTransform = lastGrips.GetOrCreateValue(cursor).Value;

        toolAreas ??= new ToolAreaData[interaction.Areas.Count];
        if (toolAreas.Length != interaction.Areas.Count) Array.Resize(ref toolAreas, interaction.Areas.Count);
        for (int i = 0; i < interaction.Areas.Count; i++)
        {
            toolAreas[i] ??= new ToolAreaData();
            toolAreas[i].area = interaction.Areas[i];
            toolAreas[i].transform = interaction.Areas[i].Transform * baseTransform;
            toolAreas[i].prevTransform = interaction.Areas[i].Transform * basePrevTransform;
            toolAreas[i].verts = interaction.Areas[i].GetShape().Transform(toolAreas[i].transform);
            toolAreas[i].prevVerts = interaction.Areas[i].GetShape().Transform(toolAreas[i].prevTransform);
        }

        base.HandleCursor(ctx, cursor);

        lastGrips.AddOrUpdate(cursor, new(baseTransform));
    }

    protected override bool CheckInteractionPurpose(string purpose)
    {
        return purpose == $"{Mod.Instance.ModManifest.UniqueID}/Action";
    }

    protected override void HandleCursor(IUpdateHandler.UpdateContext ctx, IGameCursor cursor, object obj, Matrix tfTransform, InteractionData tfInteraction, InteractionArea tfArea)
    {
        for ( int i = 0; i < toolAreas.Length; i++)
        {
            if (!toolAreas[i].area.Purpose.StartsWith($"{Mod.Instance.ModManifest.UniqueID}/ToolAction/"))
                continue;

            var treeVerts = tfArea.GetTransformedShape().Transform(tfTransform);

            if (!GJK_EPA_BCP.CheckIntersection(toolAreas[i].verts, treeVerts, out var contact, out var depth, out var normal))
                continue;
            if (GJK_EPA_BCP.CheckIntersection(toolAreas[i].prevVerts, treeVerts, out _, out _, out _))
                continue;

            Vector2 objVel = Vector2.Zero;
            if (obj is Character c)
                objVel = new Vector2(c.xVelocity, -c.yVelocity) / Game1.tileSize; // y is negative because that's how vanilla treats it

            Vector3 vel = cursor.LinearVelocity;// + new Vector3(objVel.X, 0, objVel.Y);
            // TODO: Fix the following stuff for angular velocity
            //vel += cursor.AngularVelocity * Vector3.Distance( baseTransform.Translation, transform.Translation );
            //Log.Debug($"vel:{vel.Length()} {vel} - ({cursor.LinearVelocity} {cursor.AngularVelocity} {Vector3.Distance(baseTransform.Translation, transform.Translation)})");

            if (vel.Length() < 0.625f)
                continue;

            if (toolAreas[i].area.Purpose == $"{Mod.Instance.ModManifest.UniqueID}/ToolAction/Impact")
            {
                // Only allow hits that are going similarly angled to the tool's angle
                // If you hit it pointing the wrong way, the tool will be oriented the wrong way, so the hit will be ignored
                if (Vector3.Dot(toolAreas[i].transform.Left.Normalized(), vel.Normalized()) < 0.5) // about 60 degrees in any direction
                    continue;
            }

            tool.lastUser = Game1.player;
            tool.swingTicker++;

            switch (obj)
            {
                case TerrainFeature tf:
                    if (tf.performToolAction(tool, 0, tf.Tile))
                    {
                        if (tf is ResourceClump clump && Game1.player.currentLocation.resourceClumps.Contains(clump))
                            Game1.player.currentLocation.resourceClumps.Remove(clump);
                        else if (tf is LargeTerrainFeature large && Game1.player.currentLocation.largeTerrainFeatures.Contains(large))
                            Game1.player.currentLocation.largeTerrainFeatures.Remove(large);
                        else if (Game1.player.currentLocation.terrainFeatures.TryGetValue(tf.Tile, out var atKey) && atKey == tf)
                            Game1.player.currentLocation.terrainFeatures.Remove(tf.Tile);
                    }
                    break;

                case StardewValley.Object o:
                    if (o.performToolAction(tool))
                    {
                        if (Game1.player.currentLocation.Objects.TryGetValue(o.TileLocation, out var atKey) && atKey == o)
                            Game1.player.currentLocation.Objects.Remove(o.TileLocation);
                    }
                    break;

                case NPC n:
                    if (n is Monster monster && tool is MeleeWeapon weapon)
                    {
                        // TODO: This whole thing should be reverse patched from MeleeWeapon.DoDamage...
                        // And also GameLocation.damageMonster, considering we don't actually want to iterate over every monster here...
                        float effectiveCritChance = weapon.critChance.Value;
                        if (weapon.type.Value == 1)
                        {
                            effectiveCritChance += 0.005f;
                            effectiveCritChance *= 1.12f;
                        }
                        Game1.player.currentLocation.damageMonster(monster.GetBoundingBox(), (int)(weapon.minDamage.Value * (1f + Game1.player.buffs.AttackMultiplier)), (int)(weapon.maxDamage.Value * (1f + Game1.player.buffs.AttackMultiplier)), isBomb: false, weapon.knockback.Value * (1f + Game1.player.buffs.KnockbackMultiplier), (int)(weapon.addedPrecision.Value * (1f + Game1.player.buffs.WeaponPrecisionMultiplier)), effectiveCritChance * (1f + Game1.player.buffs.CriticalChanceMultiplier), weapon.critMultiplier.Value * (1f + Game1.player.buffs.CriticalPowerMultiplier), weapon.type.Value != 1 || !weapon.isOnSpecial, Game1.player);
                    }
                    else
                    {
                        n.hitWithTool(tool);
                    }
                    break;
            }
        }
    }
}
