using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Stardew3D.Data;
using Stardew3D.Handlers.Game;
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
    public readonly IGameHandler GameHandler;
    public readonly int CullRange;

    public FarmerWorldControlsBaseHandler(IGameHandler handler, Farmer obj, int cullRange)
        : base( obj )
    {
        GameHandler = handler;
        CullRange = cullRange;
    }
    public virtual void Update(IUpdateHandler.UpdateContext ctx)
    {
        if (this.Object != Game1.player || Game1.player.currentLocation != Game1.currentLocation)
            return;

        foreach (var cursor in GameHandler.Cursors)
            HandleCursor(ctx, cursor);
    }

    protected virtual void HandleCursor(IUpdateHandler.UpdateContext ctx, IGameCursor cursor)
    {
        List<(IEnumerable Values, Func<object, Vector2> Position2D, Func<object, Matrix> Transform)> check =
        [
            new(Game1.player.currentLocation.terrainFeatures.Values.ToArray(),
                (obj) => (obj as TerrainFeature).getBoundingBox().Center.ToVector2(),
                (obj) => Matrix.CreateTranslation((obj as TerrainFeature).getBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation.Map))),
            new(Game1.player.currentLocation.resourceClumps.ToArray(),
                (obj) => (obj as TerrainFeature).getBoundingBox().Center.ToVector2(),
                (obj) => Matrix.CreateTranslation((obj as TerrainFeature).getBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation.Map))),
            new(Game1.player.currentLocation.largeTerrainFeatures.ToArray(),
                (obj) => (obj as TerrainFeature).getBoundingBox().Center.ToVector2(),
                (obj) => Matrix.CreateTranslation((obj as TerrainFeature).getBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation.Map))),
            new(Game1.player.currentLocation.Objects.Values.ToArray(),
                (obj) => (obj as StardewValley.Object).TileLocation * Game1.tileSize + new Vector2( 0.5f, 0.5f ),
                (obj) => Matrix.CreateTranslation((obj as StardewValley.Object).TileLocation.ToPoint().To3D(Game1.player.currentLocation.Map))),
            new(Game1.player.currentLocation.furniture.ToArray(),
                (obj) => (obj as Furniture).GetBoundingBox().Center.ToVector2(),
                (obj) => Matrix.CreateTranslation((obj as Furniture).GetBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation.Map))),
            new(Game1.player.currentLocation.animals.Values.ToArray(),
                (obj) => (obj as FarmAnimal).GetBoundingBox().Center.ToVector2(),
                (obj) => Matrix.CreateTranslation((obj as FarmAnimal).GetBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation.Map))),
            new(Game1.player.currentLocation.buildings.ToArray(),
                (obj) => (obj as Building).GetBoundingBox().Center.ToVector2(),
                (obj) => Matrix.CreateTranslation((obj as Building).GetBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation.Map))),
            new(Game1.player.currentLocation.characters.ToArray(),
                (obj) => (obj as NPC).StandingPixel.ToVector2(),
                (obj) => Matrix.CreateTranslation((obj as NPC).StandingPixel3D))
        ];

        // TODO: optimize more
        foreach (var container in check)
        {
            foreach (var entry in container.Values)
            {
                if (Vector2.DistanceSquared(Game1.player.Position, container.Position2D(entry)) >= MathF.Pow(Game1.tileSize * CullRange, 2))
                    continue;

                InteractionData interaction = null;
                foreach (var idEntry in entry.GetExtendedQualifiedIds())
                    interaction ??= InteractionData.Get(idEntry);

                if (interaction == null)
                    continue;

                var objTransform = container.Transform(entry);

                foreach (var area in interaction.Areas)
                {
                    if (!CheckInteractionPurpose(area.Purpose))
                        continue;

                    HandleCursor(ctx, cursor, entry, objTransform, interaction, area);
                }
            }
        }
    }

    protected abstract bool CheckInteractionPurpose(string purpose);
    protected abstract void HandleCursor(IUpdateHandler.UpdateContext ctx, IGameCursor cursor, object obj, Matrix transform, InteractionData interaction, InteractionArea area);
}
