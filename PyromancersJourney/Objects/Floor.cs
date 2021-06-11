using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Objects
{
    public class Floor : BaseObject
    {
        private Texture2D texInside;
        private Texture2D texOutside;
        private VertexBuffer buffer;
        private VertexBuffer bufferGlow;
        private int triCount;
        private int triCountGlow;
        private bool outside;

        public Floor(World world, bool theOutside)
            : base(world)
        {
            this.outside = theOutside;
            this.texInside = Game1.content.Load<Texture2D>("Maps\\Mines\\volcano_dungeon");
            this.texOutside = Game1.content.Load<Texture2D>("Maps\\Mines\\volcano_caldera");
            float tx = 16f / this.texInside.Width;
            float ty = 16f / this.texInside.Height;
            Vector2 t = new Vector2(tx, ty);

            var vertices = new List<VertexEverything>();
            var verticesGlow = new List<VertexEverything>();
            for (int ix = 0; ix < world.map.Size.X; ++ix)
            {
                for (int iy = 0; iy < world.map.Size.Y; ++iy)
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
                    if (this.outside)
                    {
                        float tx2 = 16f / this.texOutside.Width;
                        float ty2 = 16f / this.texOutside.Height;
                        Vector2 t2 = new Vector2(tx2, ty2);
                        texCoordMap[0] = new[]
                        {
                            new Vector2( 1, 3 ) * t2,
                            new Vector2( 2, 3 ) * t2,
                            new Vector2( 2, 4 ) * t2,
                            new Vector2( 1, 4 ) * t2
                        };
                    }
                    int tile = (int)world.map.Floor[ix, iy];


                    float y = 0;
                    var targetVert = vertices;
                    ref int tri = ref this.triCount;
                    if (tile == (int)FloorTile.Lava)
                    {
                        y = -0.1f;
                        targetVert = verticesGlow;
                        tri = ref this.triCountGlow;
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

            this.buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexEverything), vertices.Count, BufferUsage.WriteOnly);
            this.buffer.SetData(vertices.ToArray());

            this.bufferGlow = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexEverything), verticesGlow.Count, BufferUsage.WriteOnly);
            this.bufferGlow.SetData(verticesGlow.ToArray());
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);
            //effect.LightingEnabled = true;
            BaseObject.effect.TextureEnabled = true;
            BaseObject.effect.Texture = this.outside ? this.texOutside : this.texInside;
            for (int e = 0; e < BaseObject.effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = BaseObject.effect.CurrentTechnique.Passes[e];
                pass.Apply();
                device.SetVertexBuffer(this.buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, this.triCount);
            }

            BaseObject.effect.LightingEnabled = false;
            BaseObject.effect.Texture = this.texInside;
            for (int e = 0; e < BaseObject.effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = BaseObject.effect.CurrentTechnique.Passes[e];
                pass.Apply();
                device.SetVertexBuffer(this.bufferGlow);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, this.triCountGlow);
            }
        }
    }
}
