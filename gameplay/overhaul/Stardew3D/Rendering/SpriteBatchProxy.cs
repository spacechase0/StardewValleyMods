using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.Utilities;
using StardewValley;
using static StardewValley.BellsAndWhistles.PlayerStatusList;

namespace Stardew3D.Rendering;

public class SpriteBatchProxy : SpriteBatch
{
    private GameLocation relevantLocation;
    private Vector2 base2d;
    private Matrix baseTransform;
    private bool sameY3d;
    private Matrix? orientationOverride;
    private float scale = 1;

    public SpriteBatchProxy(GameLocation relevantLocation)
        : base(Game1.graphics.GraphicsDevice)
    {
        this.relevantLocation = relevantLocation;
    }

    public void Begin(Vector2 base2d, Matrix baseTransform, Matrix? orientationOverride = null, bool sameY3d = true, float scale = 1)
    {
        base.Begin(SpriteSortMode.FrontToBack);
        this.base2d = base2d;
        this.baseTransform = baseTransform;
        this.sameY3d = sameY3d;
        this.orientationOverride = orientationOverride;
        this.scale = scale;
    }

    public new void End(RenderBatcher output)
    {
        this._beginCalled = this._beginCalled ? false : throw new InvalidOperationException("Begin must be called before calling End.");

        switch (_sortMode)
        {
            case SpriteSortMode.Texture:
            case SpriteSortMode.BackToFront:
            case SpriteSortMode.FrontToBack:
                Array.Sort<SpriteBatchItem>(_batcher._batchItemList, 0, _batcher._batchItemCount);
                break;
        }

        float sameY = 0;
        float sameLayer = 0;
        if (sameY3d)
        {
            int amt = 0;
            for (int i = 0; i < _batcher._batchItemCount; ++i)
            {
                var item = _batcher._batchItemList[i];
                if (item.SortKey < 1f / 10000)
                    continue;
                ++amt;

                var sy = Math.Max(Math.Max(item.vertexTL.Position.Y, item.vertexTR.Position.Y), Math.Max(item.vertexBL.Position.Y, item.vertexBR.Position.Y));
                sameY = Math.Max(sameY, sy);
                sameLayer += item.SortKey;
            }
            if (amt > 0)
            {
                //sameY /= amt;
                sameLayer /= amt;
            }
        }

        for ( int i = 0; i < _batcher._batchItemCount; ++i )
        {
            var item = _batcher._batchItemList[i];

            float pos2dX = (item.vertexTL.Position.X + item.vertexTR.Position.X + item.vertexBL.Position.X + item.vertexBR.Position.X) / 4;
            float pos2dY = Math.Max(Math.Max(item.vertexTL.Position.Y, item.vertexTR.Position.Y), Math.Max(item.vertexBL.Position.Y, item.vertexBR.Position.Y));
            Vector2 basePos = new Vector2(pos2dX, sameY3d ? sameY : pos2dY);
            float yFromLayer = basePos.Y - ((sameY3d ? sameLayer : item.SortKey) * 10000);
            /*if (!sameY3d)
                basePos.Y -= yFromLayer;// / Game1.tileSize;
            /*
            Vector3 pos = new Vector3(basePos.X - base2d.X, 0, basePos.Y - base2d.Y) / Game1.tileSize;
            pos.Y = base2d.To3D(relevantLocation?.Map).Y;
            pos.Y += yFromLayer / Game1.tileSize;
            pos.Z -= yFromLayer / Game1.tileSize;
            //*/
            Vector3 pos = Vector3.Zero;
#if true
            Vector3 base3dFrom2d = base2d.To3D(relevantLocation?.Map);
            pos.X += basePos.X / Game1.tileSize - base3dFrom2d.X;
            if (!sameY3d)
            {
                if (item.SortKey < 1f / 10000)
                    pos.Z += (basePos.Y) / Game1.tileSize - base3dFrom2d.Z;
                else
                    pos.Z += (basePos.Y - yFromLayer) / Game1.tileSize - base3dFrom2d.Z;
            }
#endif
            //pos.Z -= yFromLayer / Game1.tileSize;
            if (orientationOverride.HasValue)
                output.AddSprite(basePos, pos + baseTransform.Translation, orientationOverride.Value * baseTransform.NoTranslation(), i, item, scale);
            else
                output.AddBillboardSprite(basePos, pos + baseTransform.Translation, i, item, scale);
        }

        _batcher._batchItemCount = 0;
    }
}
