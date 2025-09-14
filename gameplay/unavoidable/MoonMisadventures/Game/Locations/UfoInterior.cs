using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using xTile;
using xTile.Dimensions;

namespace MoonMisadventures.Game.Locations
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_UfoInterior" )]
    public class UfoInterior : GameLocation
    {
        public readonly NetArray<string, NetString> artifacts = new(6);

        private Vector2[] artifactSpots;

        public UfoInterior() { }
        public UfoInterior( IModContentHelper content )
        : base( content.GetInternalAssetName( "assets/maps/UfoInterior.tmx" ).BaseName, "Custom_MM_UfoInterior" )
        {
            artifactSpots = new Vector2[ artifacts.Length ];

            for (int ix = 0; ix < Map.Layers[0].LayerWidth; ++ix)
            {
                for (int iy = 0; iy < Map.Layers[0].LayerHeight; ++iy)
                {
                    string[] prop = GetTilePropertySplitBySpaces("Action", "Buildings", ix, iy);
                    if ((prop?.Length ?? 0) < 2 || prop[0] != "MoonArtifact")
                        continue;

                    artifactSpots[int.Parse(prop[1])] = new(ix, iy);
                }
            }
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(artifacts, nameof(this.artifacts));
        }

        public override bool performAction(string[] action, Farmer who, Location tileLocation)
        {
            if (action[0] == "MoonArtifact")
            {
                List<string> left = new(new[]
                {
                    "(O)spacechase0.MoonMisadventures_MoonArtifact0",
                    "(O)spacechase0.MoonMisadventures_MoonArtifact1",
                    "(O)spacechase0.MoonMisadventures_MoonArtifact2",
                    "(O)spacechase0.MoonMisadventures_MoonArtifact3",
                    "(O)spacechase0.MoonMisadventures_MoonArtifact4",
                    "(O)spacechase0.MoonMisadventures_MoonArtifact5"
                });
                left.RemoveAll(s => artifacts.ToList().Contains(s));

                int slot = int.Parse(action[1]);

                if (artifacts[slot] == null && left.Contains(who.ActiveObject?.QualifiedItemId))
                {
                    artifacts[slot] = who.ActiveObject.QualifiedItemId;
                    who.reduceActiveItemByOne();
                    Game1.playSound("questcomplete");
                    return true;
                }
                else
                {
                    Game1.drawObjectDialogue(I18n.MoonArtifactSlot());
                    return true;
                }
            }
            return base.performAction(action, who, tileLocation);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            float yoffset = -64 - (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds) * -12;

            for (int i = 0; i < artifactSpots.Length; ++i)
            {
                Vector2 tilePos = artifactSpots[i];
                string artifact = artifacts[i];
                if (artifact == null)
                    continue;

                ParsedItemData pid = ItemRegistry.GetDataOrErrorItem(artifact);
                var tex = pid.GetTexture();
                var src = pid.GetSourceRect();

                Vector2 pos = tilePos * Game1.tileSize + new Vector2(0, yoffset);
                b.Draw(tex, Game1.GlobalToLocal( pos ), src, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, pos.Y / 10000);
            }
        }

        public override void performTouchAction(string[] action, Vector2 playerStandingPosition)
        {
            if (action[0] == "MapTeleport")
            {
                lastTouchActionLocation = playerStandingPosition;
                int x = int.Parse(action[1]) * Game1.tileSize - Game1.tileSize / 2;
                int y = int.Parse(action[2]) * Game1.tileSize - Game1.tileSize / 2;
                Game1.player.Position = new(x, y);
                playSound("wand");
            }
            else if (action[0] == "ArtifactDoor")
            {
                if (artifacts.ToArray().All(b => b != null))
                {
                    Game1.warpFarmer("Custom_MM_UfoInteriorArsenal", 7, 11, false);
                    return;
                }

                // Copied from "MagicalSeal" touch action
                Game1.player.Position -= Game1.player.getMostRecentMovementVector() * 2f;
                Game1.player.yVelocity = 0f;
                Game1.player.Halt();
                Game1.player.TemporaryPassableTiles.Clear();
                if (Game1.player.Tile == this.lastTouchActionLocation)
                {
                    if (Game1.player.position.Y > this.lastTouchActionLocation.Y * 64f + 32f)
                    {
                        Game1.player.position.Y += 4f;
                    }
                    else
                    {
                        Game1.player.position.Y -= 4f;
                    }
                    this.lastTouchActionLocation = Vector2.Zero;
                }
                Game1.drawObjectDialogue(I18n.ForceField());
                for (int i = 0; i < 40; i++)
                {
                    Game1.Multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(13f, 12f) * 64f + new Vector2(-8 + i % 4 * 16, -(i / 4) * 64 / 4), flicker: false, flipped: false)
                    {
                        layerDepth = 0.1152f + (float)i / 10000f,
                        color = new Color(100 + i * 4, i * 5, 120 + i * 4),
                        pingPong = true,
                        delayBeforeAnimationStart = i * 10,
                        scale = 4f,
                        alphaFade = 0.01f
                    });
                    Game1.Multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(13.5f, 10f) * 64f + new Vector2(-8 + i % 4 * 16, i / 4 * 64 / 4), flicker: false, flipped: false)
                    {
                        layerDepth = 0.1152f + (float)i / 10000f,
                        color = new Color(232 - i * 4, 192 - i * 6, 255 - i * 4),
                        pingPong = true,
                        delayBeforeAnimationStart = 320 + i * 10,
                        scale = 4f,
                        alphaFade = 0.01f
                    });
                    Game1.Multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(14f, 12f) * 64f + new Vector2(-8 + i % 4 * 16, -(i / 4) * 64 / 4), flicker: false, flipped: false)
                    {
                        layerDepth = 0.1152f + (float)i / 10000f,
                        color = new Color(100 + i * 4, i * 6, 120 + i * 4),
                        pingPong = true,
                        delayBeforeAnimationStart = 640 + i * 10,
                        scale = 4f,
                        alphaFade = 0.01f
                    });
                }
                Game1.player.jitterStrength = 2f;
                Game1.player.freezePause = 500;
                this.playSound("debuffHit");
            }

            base.performTouchAction(action, playerStandingPosition);
        }
    }
}
