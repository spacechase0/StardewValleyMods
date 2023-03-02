using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ConstellationVoyage
{
    [XmlType("Mods_spacechase0_ConstellationVoyageLocation")]
    public class VoyageLocation : GameLocation
    {
        public class Island
        {
            public Vector2 tilePos;
            public bool lit;
            public Vector2 scaleFactor = Vector2.Zero;

            public Rectangle GetBoundingBox()
            {
                return new Rectangle((int)tilePos.X * Game1.tileSize, (int)tilePos.Y * Game1.tileSize, VoyageLocation.islandMap.DisplaySize.Width, VoyageLocation.islandMap.DisplaySize.Height);
            }
        }


        private Texture2D boatTex;
        private Vector2 currBoatPos = new Vector2( 50, 50 ) * Game1.tileSize;
        private float currBoatRot = 0;

        private Vector2 targetBoatPos;

        private List<Island> islands = new();

        private static xTile.Map islandMap;

        public VoyageLocation() { }
        public VoyageLocation(IModContentHelper content)
        :   base( content.GetInternalAssetName( "assets/empty.tmx" ).Name, "Custom_ConstellationVoyageLocation" )
        {
            boatTex = content.Load<Texture2D>("assets/magic-boat.png");

            if (islandMap == null)
            {
                islandMap = content.Load<xTile.Map>("assets/star-island.tmx");
            }
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();
            Game1.ambientLight = Game1.outdoorLight = Color.White;
            Game1.background = new SpaceBackground();

            currBoatPos = new Vector2(50, 50) * Game1.tileSize;
            currBoatRot = 0;
            targetBoatPos = Vector2.Zero;

            islands.Clear();

            foreach (var ts in islandMap.TileSheets)
            {
                Game1.mapDisplayDevice.LoadTileSheet(ts);
            }
        }

        public override void cleanupBeforePlayerExit()
        {
            base.cleanupBeforePlayerExit();
            Game1.ambientLight = Game1.outdoorLight = Color.Black;
            Game1.background = null;
        }

        public override bool isActionableTile(int xTile, int yTile, Farmer who)
        {
            if (xTile == 51 && yTile == 53)
                return true;
            return base.isActionableTile(xTile, yTile, who);
        }

        public override bool checkAction(xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            if (tileLocation.X == 51 && tileLocation.Y == 53)
            {
                targetBoatPos = currBoatPos + new Vector2( 15, 15 ) * Game1.tileSize;
                islands.Add(new()
                {
                    tilePos = new Vector2((int)(targetBoatPos.X / Game1.tileSize), (int)(targetBoatPos.Y / Game1.tileSize)) + new Vector2(-4, 7)
                }); ;
            }
            return base.checkAction(tileLocation, viewport, who);
        }

        private float angleDist(float curr, float target)
        {
            float a = target - currBoatRot;
            float b = target - currBoatRot + MathF.PI * 2;
            float c = target - currBoatRot - MathF.PI * 3;

            float smallest = MathF.Min(a, MathF.Min(b, c));
            if (smallest > 0)
                return curr - target;
            else
                return target - curr;
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);

            foreach (var island in islands)
            {
                island.scaleFactor = new(Math.Min(4, island.scaleFactor.X + 0.025f), Math.Min(4, island.scaleFactor.Y + 0.025f));
            }

            const float ROTATE_SPEED = 0.025f;
            if (targetBoatPos != Vector2.Zero)
            {
                float targetBoatRot = MathF.Atan2(targetBoatPos.Y - currBoatPos.Y, targetBoatPos.X - currBoatPos.X);
                if (currBoatRot != targetBoatRot)
                {
                    float dist = angleDist(currBoatRot, targetBoatRot);
                    currBoatRot += MathF.Sign(dist) * ROTATE_SPEED;
                    if (MathF.Abs(currBoatRot - targetBoatRot) < ROTATE_SPEED)
                        currBoatRot = targetBoatRot;
                }
                else
                {
                    const float MOVE_SPEED = 5;
                    var oldBoatPos = currBoatPos;
                    if (Vector2.Distance(targetBoatPos, currBoatPos) <= MOVE_SPEED)
                    {
                        currBoatPos = targetBoatPos;
                        targetBoatPos = Vector2.Zero;
                    }
                    else
                        currBoatPos += new Vector2(MathF.Cos(targetBoatRot) * MOVE_SPEED, MathF.Sin(targetBoatRot) * MOVE_SPEED);
                    (Game1.background as SpaceBackground).offset += new Vector2(MOVE_SPEED, 0);
                }
            }
            else if ( currBoatRot != 0 )
            {
                float dist = angleDist(currBoatRot, 0);
                currBoatRot += MathF.Sign(dist) * ROTATE_SPEED;
                if (MathF.Abs(currBoatRot - 0) < ROTATE_SPEED)
                    currBoatRot = 0;
            }
        }

        public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
        {
            var boatMapPos = new Vector2(50, 50) * Game1.tileSize;
            if (position.Intersects(new((int)boatMapPos.X + 0 * 4, (int)boatMapPos.Y + 0 * 4, 16 * 4, 120 * 4)))
                return true;
            if (position.Intersects(new((int)boatMapPos.X + 0 * 4, (int)boatMapPos.Y + 0 * 4, 160 * 4, 48 * 4)))
                return true;
            if (position.Intersects(new((int)boatMapPos.X + 70 * 4, (int)boatMapPos.Y + 0 * 4, 90 * 4, 120 * 4)))
                return true;
            if (position.Intersects(new((int)boatMapPos.X + 0 * 4, (int)boatMapPos.Y + 92 * 4, 48 * 4, 28 * 4)))
                return true;
            if (position.Intersects(new((int)boatMapPos.X + 64 * 4, (int)boatMapPos.Y + 92 * 4, 96 * 4, 28 * 4)))
                return true;

            if (currBoatRot != 0)
            {
                if (position.Intersects(new((int)boatMapPos.X + 0 * 4, (int)boatMapPos.Y + 92 * 4, 160 * 4, 28 * 4)))
                    return true;
            }
            else
            {
                var buildings = islandMap.GetLayer("Buildings");

                foreach (var island in islands)
                {
                    int dx = (int)(currBoatPos.X - boatMapPos.X);
                    int dy = (int)(currBoatPos.Y - boatMapPos.Y);
                    Rectangle relPosition = position;
                    relPosition.X += dx;
                    relPosition.Y += dy;
                    if (island.GetBoundingBox().Intersects(relPosition))
                    {
                        int startX = (int)(relPosition.X - (island.tilePos.X * Game1.tileSize)) / Game1.tileSize;
                        int startY = (int)(relPosition.Y - (island.tilePos.Y * Game1.tileSize)) / Game1.tileSize;
                        int endX = startX + (int)Math.Ceiling((double)relPosition.Width / Game1.tileSize);
                        int endY = startY + (int)Math.Ceiling((double)relPosition.Height / Game1.tileSize);

                        for (int ix = Math.Max(0, startX); ix < Math.Min(islandMap.Layers[0].LayerWidth, endX); ++ix)
                        {
                            for (int iy = Math.Max(0, startY); iy < Math.Min(islandMap.Layers[0].LayerHeight, endY); ++iy)
                            {
                                bool hasTile = buildings.Tiles[ix, iy] != null;
                                if (hasTile)
                                {
                                    Rectangle r = new((int)(island.tilePos.X + ix) * Game1.tileSize, (int)(island.tilePos.Y + iy) * Game1.tileSize, Game1.tileSize, Game1.tileSize);
                                    if (relPosition.Intersects(r))
                                        return true;
                                }
                            }
                        }
                    }
                }
            }

            return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
        }

        public override void draw(SpriteBatch b)
        {
            var boatMapPos = new Vector2(50, 50) * Game1.tileSize;
            base.draw(b);

            var dreamscape = Game1.content.Load<Texture2D>("Maps/EmilyDreamscapeTiles");
            currBoatRot = 20 * MathF.PI / 180;
            foreach (var island in islands)
            {
                Vector2 pos = new((int)island.tilePos.X * Game1.tileSize, (int)island.tilePos.Y * Game1.tileSize);
                //pos = Game1.GlobalToLocal(pos);

                var back = islandMap.GetLayer("Back");

                int i = 0;
                foreach (var tile in back.Tiles.Array)
                {
                    Vector2 tilePos = new(i / back.LayerWidth, i % back.LayerWidth);
                    tilePos *= Game1.tileSize;

                    /*
                    Matrix m2 = Matrix.Identity;
                    m2 *= (Matrix.CreateTranslation(pos.X, pos.Y, 0));
                    m2 *= Matrix.CreateRotationZ(-currBoatRot);
                    //m2 *= (Matrix.CreateScale(island.scaleFactor.X / 4, island.scaleFactor.Y / 4, 1));

                    m2 *= (Matrix.CreateTranslation(boatMapPos.X - currBoatPos.X, boatMapPos.Y - currBoatPos.Y, 0));

                    Vector2 tilePosTmp = Vector2.Transform(Vector2.Zero, m2);

                    // I give up on figuring out this matrix stuff completely
                    Vector2 a = pos + boatMapPos - currBoatPos;
                    Vector2 d = a - tilePosTmp;
                    float angle2 = MathF.Atan2(d.Y, d.X);
                    m2 *= Matrix.CreateTranslation(MathF.Cos(angle2) * d.Length(), MathF.Sin(angle2) * d.Length(), 0);
                    //*/

                    Vector2 v0 = (pos + tilePos) - (currBoatPos - boatMapPos);
                    Vector2 v1 = new(MathF.Cos(-currBoatRot) * v0.X + MathF.Sin(-currBoatRot) * v0.Y,
                                     -MathF.Sin(-currBoatRot) * v0.X + MathF.Cos(-currBoatRot) * v0.Y);

                    tilePos = (currBoatPos - boatMapPos) + v1;

                    //tilePos += new Vector2(back.LayerWidth * Game1.tileSize / 2 + 16f, back.LayerHeight * Game1.tileSize / 2 + 16f);
                    //tilePos += pos;
                    tilePos = Game1.GlobalToLocal(tilePos);
                    b.Draw(dreamscape, tilePos, new Rectangle(tile.TileIndex % 8 * 16, tile.TileIndex / 8 * 16, 16, 16), Color.White, currBoatRot, Vector2.Zero, island.scaleFactor, SpriteEffects.None, 0);
                    ++i;
                }

                Matrix m = Matrix.Identity;
                m *= Matrix.CreateScale(island.scaleFactor.X / 4, island.scaleFactor.Y / 4, 1);
                m *= Matrix.CreateRotationZ(currBoatRot);
                m *= Matrix.CreateTranslation(pos.X, pos.Y, 0);
                m *= Matrix.CreateTranslation(-currBoatPos.X, -currBoatPos.Y, 0);
                m *= Matrix.CreateTranslation(boatMapPos.X, boatMapPos.Y, 0);
                m *= Matrix.CreateTranslation(new Vector3(7.75f * Game1.tileSize, 10.75f * Game1.tileSize, 0));
                //SpaceShared.Log.Debug("rot:" + currBoatRot);
                Vector2 starPos = Vector2.Transform(Vector2.Zero, m);
                //starPos += new Vector2(back.LayerWidth * Game1.tileSize / 2, back.LayerHeight * Game1.tileSize / 2);
                //starPos += pos;

                Vector2 localStarPos = Game1.GlobalToLocal(starPos);
                b.Draw(Game1.objectSpriteSheet, localStarPos, new Rectangle(0, 512, 16, 16), Color.White, MathF.Sin(Game1.currentGameTime.TotalGameTime.Milliseconds / 250f) * 15 * MathF.PI / 180, new Vector2(8, 8), new Vector2( 6, 6 ), SpriteEffects.None, (starPos.Y + 70 /* why 70? The world may never know (probably position of the tile) */ * Game1.tileSize) / 10000f );
                //SpaceShared.Log.Trace(""+localStarPos);
            }

            b.Draw(boatTex, Game1.GlobalToLocal(boatMapPos), new Microsoft.Xna.Framework.Rectangle(0, 0, 160, 118), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, boatMapPos.Y / 10000f);
            b.Draw(boatTex, Game1.GlobalToLocal(boatMapPos + new Vector2(12f, 0f) * 4f), new Microsoft.Xna.Framework.Rectangle(0, 160, 128, 96), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (boatMapPos.Y + 408f) / 10000f);
        }
    }
}
