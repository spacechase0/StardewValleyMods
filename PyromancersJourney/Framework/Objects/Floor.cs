using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Framework.Objects
{
    internal class Floor : BaseObject
    {
        private readonly Texture2D TexInside;
        private readonly Texture2D TexOutside;
        private readonly VertexBuffer Buffer;
        private readonly VertexBuffer BufferGlow;
        private readonly int TriCount;
        private readonly int TriCountGlow;
        private readonly bool Outside;

        public Floor(World world, bool theOutside)
            : base(world)
        {
            this.Outside = theOutside;
            this.TexInside = Game1.content.Load<Texture2D>("Maps\\Mines\\volcano_dungeon");
            this.TexOutside = Game1.content.Load<Texture2D>("Maps\\Mines\\volcano_caldera");
            float tx = 16f / this.TexInside.Width;
            float ty = 16f / this.TexInside.Height;
            Vector2 t = new Vector2(tx, ty);

            var vertices = new List<VertexEverything>();
            var verticesGlow = new List<VertexEverything>();
            for (int ix = 0; ix < world.Map.Size.X; ++ix)
            {
                for (int iy = 0; iy < world.Map.Size.Y; ++iy)
                {
                    Vector2[][] texCoordMap = {
                        new[]
                        {
                            new Vector2( 1, 0 ) * t,
                            new Vector2( 2, 0 ) * t,
                            new Vector2( 2, 1 ) * t,
                            new Vector2( 1, 1 ) * t
                        },
                        new[]
                        {
                            new Vector2( 0, 20 ) * t,
                            new Vector2( 1, 20 ) * t,
                            new Vector2( 1, 21 ) * t,
                            new Vector2( 0, 21 ) * t
                        }
                    };
                    if (this.Outside)
                    {
                        float tx2 = 16f / this.TexOutside.Width;
                        float ty2 = 16f / this.TexOutside.Height;
                        Vector2 t2 = new Vector2(tx2, ty2);
                        texCoordMap[0] = new[]
                        {
                            new Vector2( 1, 3 ) * t2,
                            new Vector2( 2, 3 ) * t2,
                            new Vector2( 2, 4 ) * t2,
                            new Vector2( 1, 4 ) * t2
                        };
                    }
                    int tile = (int)world.Map.Floor[ix, iy];


                    float y = 0;
                    var targetVert = vertices;
                    ref int tri = ref this.TriCount;
                    if (tile == (int)FloorTile.Lava)
                    {
                        y = -0.1f;
                        targetVert = verticesGlow;
                        tri = ref this.TriCountGlow;
                    }
                    else
                    {
                        var n = new Vector3(0, 0, -1);
                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 0), n, texCoordMap[tile][0]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 0), n, texCoordMap[tile][1]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, -1, iy + 0), n, texCoordMap[tile][2]));
                        ++tri;

                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 0), n, texCoordMap[tile][0]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, -1, iy + 0), n, texCoordMap[tile][2]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, -1, iy + 0), n, texCoordMap[tile][3]));
                        ++tri;

                        n = new Vector3(-1, 0, 0);
                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 0), n, texCoordMap[tile][0]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 1), n, texCoordMap[tile][1]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, -1, iy + 1), n, texCoordMap[tile][2]));
                        ++tri;

                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 0), n, texCoordMap[tile][0]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, -1, iy + 1), n, texCoordMap[tile][2]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, -1, iy + 0), n, texCoordMap[tile][3]));
                        ++tri;

                        n = new Vector3(1, 0, 0);
                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 0), n, texCoordMap[tile][0]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 1), n, texCoordMap[tile][1]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, -1, iy + 1), n, texCoordMap[tile][2]));
                        ++tri;

                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 0), n, texCoordMap[tile][0]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, -1, iy + 1), n, texCoordMap[tile][2]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, -1, iy + 0), n, texCoordMap[tile][3]));
                        ++tri;

                        n = new Vector3(0, 0, 1);
                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 1), n, texCoordMap[tile][0]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, 0, iy + 1), n, texCoordMap[tile][1]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, -1, iy + 1), n, texCoordMap[tile][2]));
                        ++tri;

                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, 0, iy + 1), n, texCoordMap[tile][0]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 1, -1, iy + 1), n, texCoordMap[tile][2]));
                        targetVert.Add(new VertexEverything(new Vector3(ix + 0, -1, iy + 1), n, texCoordMap[tile][3]));
                        ++tri;
                    }

                    targetVert.Add(new VertexEverything(new Vector3(ix + 0, y, iy + 0), Vector3.Up, texCoordMap[tile][0]));
                    targetVert.Add(new VertexEverything(new Vector3(ix + 1, y, iy + 0), Vector3.Up, texCoordMap[tile][1]));
                    targetVert.Add(new VertexEverything(new Vector3(ix + 1, y, iy + 1), Vector3.Up, texCoordMap[tile][2]));
                    ++tri;

                    targetVert.Add(new VertexEverything(new Vector3(ix + 0, y, iy + 0), Vector3.Up, texCoordMap[tile][0]));
                    targetVert.Add(new VertexEverything(new Vector3(ix + 1, y, iy + 1), Vector3.Up, texCoordMap[tile][2]));
                    targetVert.Add(new VertexEverything(new Vector3(ix + 0, y, iy + 1), Vector3.Up, texCoordMap[tile][3]));
                    ++tri;
                }
            }

            this.Buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexEverything), vertices.Count, BufferUsage.WriteOnly);
            this.Buffer.SetData(vertices.ToArray());

            this.BufferGlow = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexEverything), verticesGlow.Count, BufferUsage.WriteOnly);
            this.BufferGlow.SetData(verticesGlow.ToArray());
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

            BaseObject.Effect.LightingEnabled = false;
            BaseObject.Effect.Texture = this.TexInside;
            foreach (EffectPass pass in BaseObject.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(this.BufferGlow);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, this.TriCountGlow);
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            this.Buffer?.Dispose();
            this.BufferGlow?.Dispose();
        }
    }
}
