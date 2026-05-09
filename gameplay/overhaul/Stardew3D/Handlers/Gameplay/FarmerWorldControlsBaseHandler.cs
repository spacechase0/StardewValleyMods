using System.Collections;
using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using Stardew3D.GameModes;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace Stardew3D.Handlers.Gameplay;

// Inheriting from RendererFor because some subclasses need it, and we don't have multi-inheritance
// However, we don't override the necessary methods because they need to do those themselves, and
// they won't trigger unless registered as a renderer anyways
public abstract class FarmerWorldControlsBaseHandler : RendererFor<ModelData, Farmer>, IUpdateHandler
{
    public readonly IGameMode GameMode;
    public readonly int CullRange;

    public FarmerWorldControlsBaseHandler(IGameMode mode, Farmer obj, int cullRange)
        : base( obj )
    {
        GameMode = mode;
        CullRange = cullRange;
    }
    public virtual void Update(IUpdateHandler.UpdateContext ctx)
    {
        if (this.Object != Game1.player || Game1.player.currentLocation != Game1.currentLocation)
            return;

        foreach (var cursor in GameMode.Cursors)
            HandleCursor(ctx, cursor);
    }

    private IEnumerable<(object Value, object ValueHolder, Vector2 Position2D, Matrix Transform)> GetToCheck()
    {
        foreach (var entry in Game1.player.currentLocation.terrainFeatures.Values.ToArray())
        {
            Vector2 pos = entry.getBoundingBox().Center.ToVector2();
            if (Vector2.DistanceSquared(Game1.player.Position, pos) >= MathF.Pow(Game1.tileSize * (CullRange + 1), 2))
                continue;

            yield return new(entry, Game1.player.currentLocation.terrainFeatures, pos, Matrix.CreateTranslation(entry.getBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation)));

            if (entry is HoeDirt hd && hd.crop != null)
                yield return new(hd.crop, hd, pos, Matrix.CreateTranslation(entry.getBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation)));
        }

        foreach (var entry in Game1.player.currentLocation.resourceClumps.ToArray())
        {
            Vector2 pos = entry.getBoundingBox().Center.ToVector2();
            if (Vector2.DistanceSquared(Game1.player.Position, pos) >= MathF.Pow(Game1.tileSize * (CullRange + 1), 2))
                continue;

            yield return new(entry, Game1.player.currentLocation.resourceClumps, pos, Matrix.CreateTranslation(entry.getBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation)));
        }

        foreach (var entry in Game1.player.currentLocation.largeTerrainFeatures.ToArray())
        {
            Vector2 pos = entry.getBoundingBox().Center.ToVector2();
            if (Vector2.DistanceSquared(Game1.player.Position, pos) >= MathF.Pow(Game1.tileSize * (CullRange + 1), 2))
                continue;

            yield return new(entry, Game1.player.currentLocation.largeTerrainFeatures, pos, Matrix.CreateTranslation(entry.getBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation)));
        }

        foreach (var entry in Game1.player.currentLocation.Objects.Values.ToArray())
        {
            Vector2 pos = entry.TileLocation * Game1.tileSize + new Vector2(0.5f, 0.5f);
            if (Vector2.DistanceSquared(Game1.player.Position, pos) >= MathF.Pow(Game1.tileSize * (CullRange + 1), 2))
                continue;

            yield return new(entry, Game1.player.currentLocation.Objects, pos, Matrix.CreateTranslation(entry.TileLocation.ToPoint().To3D(Game1.player.currentLocation)));
        }

        foreach (var entry in Game1.player.currentLocation.furniture.ToArray())
        {
            Vector2 pos = entry.GetBoundingBox().Center.ToVector2();
            if (Vector2.DistanceSquared(Game1.player.Position, pos) >= MathF.Pow(Game1.tileSize * (CullRange + 1), 2))
                continue;

            yield return new(entry, Game1.player.currentLocation.furniture, pos, Matrix.CreateTranslation(entry.GetBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation)));
        }

        foreach (var entry in Game1.player.currentLocation.animals.Values.ToArray())
        {
            Vector2 pos = entry.GetBoundingBox().Center.ToVector2();
            if (Vector2.DistanceSquared(Game1.player.Position, pos) >= MathF.Pow(Game1.tileSize * (CullRange + 1), 2))
                continue;

            yield return new(entry, Game1.player.currentLocation.animals, pos, Matrix.CreateTranslation(entry.GetBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation)));
        }

        foreach (var entry in Game1.player.currentLocation.buildings.ToArray())
        {
            Vector2 pos = entry.GetBoundingBox().Center.ToVector2();
            if (Vector2.DistanceSquared(Game1.player.Position, pos) >= MathF.Pow(Game1.tileSize * (CullRange + 1), 2))
                continue;

            yield return new(entry, Game1.player.currentLocation.buildings, pos, Matrix.CreateTranslation(entry.GetBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation)));
        }

        foreach (var entry in Game1.player.currentLocation.characters.ToArray())
        {
            Vector2 pos = entry.StandingPixel.ToVector2();
            if (Vector2.DistanceSquared(Game1.player.Position, pos) >= MathF.Pow(Game1.tileSize * (CullRange + 1), 2))
                continue;

            yield return new(entry, Game1.player.currentLocation.characters, pos, Matrix.CreateTranslation(entry.StandingPixel3D));
        }
    }

    protected virtual void HandleCursor(IUpdateHandler.UpdateContext ctx, IGameCursor cursor)
    {
        // TODO: optimize more
        foreach (var entry in GetToCheck())
        {
            if (Vector2.DistanceSquared(Game1.player.Position, entry.Position2D) >= MathF.Pow(Game1.tileSize * (CullRange + 1), 2))
                continue;

            InteractionData interaction = InteractionData.Get(entry.Value, out Vector3 interactionSize, out _);
            if (interaction == null)
                continue;

            var objTransform = Matrix.CreateScale(interactionSize) * entry.Transform * Matrix.CreateTranslation( 0, interactionSize.Y / 2, 0 );

            foreach (var area in interaction.Areas)
            {
                if (!CheckInteractionPurpose(area.Purpose))
                    continue;

                Vector3 size3 = area.GetBoundingBox().Max * interactionSize - area.GetBoundingBox().Min * interactionSize;
                float size = Math.Max( size3.X, size3.Z );

                if (Vector3.DistanceSquared(cursor.PointerPosition, objTransform.Translation + area.Translation * interactionSize) >= MathF.Pow(CullRange + size / 2, 2))
                    continue;

                HandleCursor(ctx, cursor, entry.Value, entry.ValueHolder, objTransform, interaction, area);
            }
        }
    }

    protected abstract bool CheckInteractionPurpose(string purpose);
    protected abstract void HandleCursor(IUpdateHandler.UpdateContext ctx, IGameCursor cursor, object obj, object objHolder, Matrix transform, InteractionData interaction, InteractionArea area);
}
