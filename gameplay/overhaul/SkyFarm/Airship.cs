using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;

namespace SkyFarm;

[XmlType("Mods_spacechase0_SkyFarm_Airship")]
public class Airship : Horse
{
    private IReflectedField<bool> roomForHorseAtDismountTile_;
    private IReflectedField<Vector2> dismountTile_;

    private float propeller = 0;

    public Airship()
    {
        Name = "Airship";
        roomForHorseAtDismountTile_ = Mod.instance.Helper.Reflection.GetField<bool>(this, "roomForHorseAtDismountTile");
        dismountTile_ = Mod.instance.Helper.Reflection.GetField<Vector2>(this, "dismountTile");
    }

    public override bool hasSpecialCollisionRules()
    {
        return true;
    }

    public virtual bool isColliding(GameLocation l, Vector2 tile)
    {
        return false;
    }

    public override bool checkAction(Farmer who, GameLocation l)
    {
        if (rider == null)
        {
            mutex.RequestLock(() =>
            {
                if (who.mount != null || rider != null || who.FarmerSprite.PauseForSingleAnimation || base.currentLocation != who.currentLocation)
                {
                    mutex.ReleaseLock();
                }
                else
                {
                    rider = who;
                    rider.freezePause = 5000;
                    rider.synchronizedJump(6f);
                    rider.Halt();
                    if (rider.Position.X < base.Position.X)
                    {
                        rider.faceDirection(1);
                    }

                    l.playSound("dwop");
                    mounting.Value = true;
                    rider.isAnimatingMount = true;
                    rider.completelyStopAnimatingOrDoingAction();
                    rider.faceGeneralDirection(Utility.PointToVector2(base.StandingPixel), 0, opposite: false, useTileCalculations: false);
                }
            });
        }
        else
        {
            dismounting.Value = true;
            rider.isAnimatingMount = true;
            farmerPassesThrough = false;
            rider.TemporaryPassableTiles.Clear();
            Vector2 value = Utility.recursiveFindOpenTileForCharacter(rider, rider.currentLocation, base.Tile, 8);
            base.Position = new Vector2(value.X * 64f + 32f - (float)(GetBoundingBox().Width / 2), value.Y * 64f + 4f);
            roomForHorseAtDismountTile_.SetValue(!base.currentLocation.isCollidingPosition(GetBoundingBox(), Game1.viewport, isFarmer: true, 0, glider: false, this));
            base.Position = rider.Position;
            dismounting.Value = false;
            rider.isAnimatingMount = false;
            Halt();
            if (!value.Equals(Vector2.Zero) && Vector2.Distance(value, base.Tile) < 2f)
            {
                rider.synchronizedJump(6f);
                l.playSound("dwop");
                rider.freezePause = 5000;
                rider.Halt();
                rider.xOffset = 0f;
                dismounting.Value = true;
                rider.isAnimatingMount = true;
                dismountTile_.SetValue(value);
            }
            else
            {
                dismount();
            }
        }

        return true;
    }

    public override void update(GameTime time, GameLocation location)
    {
        Vector2 oldPos = Position;

        base.update(time, location);

        bool horizontal = FacingDirection == Game1.left || FacingDirection == Game1.right;
        float diff = (Position - oldPos).Length();
        if (diff > 0)
        {
            propeller += diff / 15;
        }
        int propellerFrames = (horizontal ? 5 : 6);
        if (propeller >= propellerFrames)
        {
            propeller -= propellerFrames;
            location.TemporarySprites.Add(new TemporaryAnimatedSprite(5, Position + getPropellerOffset() + new Vector2(-16, -16-Game1.tileSize * 2), Color.White));
            currentLocation.localSound($"{Mod.instance.ModManifest.UniqueID}_AirshipPutter", base.Tile);
        }

        if (rider != null && !dismounting.Value)
        {
            rider.TemporaryPassableTiles.Clear();
            rider.TemporaryPassableTiles.Add(new Rectangle(0, 0, 999999, 999999));
        }
    }

    public override void draw(SpriteBatch b)
    {
        var tex = Mod.instance.AirshipTex;

        bool horizontal = FacingDirection == Game1.left || FacingDirection == Game1.right;

        Vector2 pos = getLocalPosition(Game1.viewport) - new Vector2(0, Game1.tileSize * 2);
        pos += new Vector2(0, horizontal ? -8 : -16);

        b.Draw(tex, pos, new Rectangle(0, 48 * FacingDirection, 32, 48), Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, (base.Position.Y + 0) / 10000f);
        b.Draw(tex, pos, new Rectangle(32, 48 * FacingDirection, 32, 48), Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, (base.Position.Y + Game1.tileSize) / 10000f);

        int frameCount = (horizontal ? 5 : 6);
        int frameSize = 6;
        b.Draw(tex, pos + getPropellerOffset(), new Rectangle(64, 48 * FacingDirection + (int)propeller * frameSize, frameSize, frameSize), Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, (base.Position.Y + Game1.tileSize / 2) / 10000f);
    }

    private Vector2 getPropellerOffset()
    {
        switch (FacingDirection)
        {
            case Game1.up: return new Vector2(13, 42 - 48 * 0) * Game1.pixelZoom;
            case Game1.right: return new Vector2(1, 77 - 48 * 1) * Game1.pixelZoom;
            case Game1.down: return new Vector2(13, 114 - 48 * 2) * Game1.pixelZoom;
            case Game1.left: return new Vector2(26, 173 - 48 * 3) * Game1.pixelZoom;
        }
        return Vector2.Zero;
    }

    public override void PerformDefaultHorseFootstep(string step_type)
    {
    }
}
