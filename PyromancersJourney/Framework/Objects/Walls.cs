using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Framework.Objects
{
    internal class Walls : BaseObject
    {
        private readonly Texture2D TexInside;
        private readonly Texture2D TexOutside;
        private readonly VertexBuffer Buffer;
        private readonly int TriCount;
        private readonly bool Outside;

        public Walls(World world, bool theOutside)
            : base(world)
        {
            this.Outside = theOutside;
            this.TexInside = Game1.content.Load<Texture2D>("Maps\\Mines\\volcano_dungeon");
            this.TexOutside = Game1.content.Load<Texture2D>("Maps\\Mines\\volcano_caldera");
            float tx = 16f / this.TexInside.Width;
            float ty = 16f / this.TexInside.Height;
            Vector2 t = new Vector2(tx, ty);

            var vertices = new List<VertexEverything>();
            for (int ix = 0; ix < world.Map.Size.X; ++ix)
            {
                for (int iy = 0; iy < world.Map.Size.Y; ++iy)
                {
                    Vector2[][] texCoordMap = {
                        Array.Empty<Vector2>(),
                        new[]
                        {
                            new Vector2( 1, 0 ) * t,
                            new Vector2( 2, 0 ) * t,
                            new Vector2( 2, 1 ) * t,
                            new Vector2( 1, 1 ) * t
                        }
                    };
                    if (this.Outside)
                    {
                        float tx2 = 16f / this.TexOutside.Width;
                        float ty2 = 16f / this.TexOutside.Height;
                        Vector2 t2 = new Vector2(tx2, ty2);
                        texCoordMap[1] = new[]
                        {
                            new Vector2( 1, 3 ) * t2,
                            new Vector2( 2, 3 ) * t2,
                            new Vector2( 2, 4 ) * t2,
                            new Vector2( 1, 4 ) * t2
                        };
                    }
                    int tile = (int)world.Map.Walls[ix, iy];

                    if (tile != (int)WallTile.Empty)
                    {
                        var n = new Vector3(0, 0, -1);
                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 1, iy + 0), n, texCoordMap[tile][0]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 1, iy + 0), n, texCoordMap[tile][1]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 0), n, texCoordMap[tile][2]));
                        ++this.TriCount;

                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 1, iy + 0), n, texCoordMap[tile][0]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 0), n, texCoordMap[tile][2]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 0), n, texCoordMap[tile][3]));
                        ++this.TriCount;

                        n = new Vector3(-1, 0, 0);
                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 1, iy + 0), n, texCoordMap[tile][0]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 1, iy + 1), n, texCoordMap[tile][1]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 1), n, texCoordMap[tile][2]));
                        ++this.TriCount;

                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 1, iy + 0), n, texCoordMap[tile][0]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 1), n, texCoordMap[tile][2]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 0), n, texCoordMap[tile][3]));
                        ++this.TriCount;

                        n = new Vector3(1, 0, 0);
                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 1, iy + 0), n, texCoordMap[tile][0]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 1, iy + 1), n, texCoordMap[tile][1]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 1), n, texCoordMap[tile][2]));
                        ++this.TriCount;

                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 1, iy + 0), n, texCoordMap[tile][0]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 1), n, texCoordMap[tile][2]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 0), n, texCoordMap[tile][3]));
                        ++this.TriCount;

                        n = new Vector3(0, 0, 1);
                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 1, iy + 1), n, texCoordMap[tile][0]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 1, iy + 1), n, texCoordMap[tile][1]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 1), n, texCoordMap[tile][2]));
                        ++this.TriCount;

                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 1, iy + 1), n, texCoordMap[tile][0]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 1), n, texCoordMap[tile][2]));
                        vertices.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 1), n, texCoordMap[tile][3]));
                        ++this.TriCount;
                    }
                }
            }

            this.Buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexEverything), vertices.Count, BufferUsage.WriteOnly);
            this.Buffer.SetData(vertices.ToArray());
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);
            //effect.LightingEnabled = true;
            BaseObject.Effect.TextureEnabled = true;
            BaseObject.Effect.Texture = this.Outside ? this.TexOutside : this.TexInside;
            foreach (EffectPass pass in BaseObject.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(this.Buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, this.TriCount);
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            this.Buffer?.Dispose();
        }
    }
}
