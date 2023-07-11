using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using xTile;

namespace MoonMisadventures.Game.Locations
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_UfoInterior" )]
    public class UfoInterior : GameLocation
    {
        public readonly NetArray<bool, NetBool> artifacts = new(6);

        public UfoInterior() { }
        public UfoInterior( IModContentHelper content )
        : base( content.GetInternalAssetName( "assets/maps/UfoInterior.tmx" ).BaseName, "Custom_MM_UfoInterior" )
        {
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(artifacts, nameof(this.artifacts));
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
                if (artifacts.ToArray().All(b => b))
                {
                    // do warp
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
