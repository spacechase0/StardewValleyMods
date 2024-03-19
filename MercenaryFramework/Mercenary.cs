using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;

namespace MercenaryFramework
{
    [XmlType("Mods_spacechase0_MercenaryFramework_Mercenary")]
    public class Mercenary : Character
    {
        private readonly NetString correspondingNpc = new();
        public string CorrespondingNpc => correspondingNpc.Value;

        public Mercenary()
        {
        }

        public Mercenary(string npc, Vector2 pos)
        : base(new AnimatedSprite($"Characters\\{npc}", 0, 16, 32), pos, 2, npc)
        {
            correspondingNpc.Value = npc;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(correspondingNpc, "CorrespondingNPC");
        }

        public void OnLeave()
        {
            NPC npc = Game1.getCharacterFromName(name, true);
            npc.TryLoadSchedule();
            npc.warpToPathControllerDestination();
        }

        public void UpdateForFarmer(Farmer farmer, int numInFormation, GameTime gameTime)
        {
            var trail = Mod.instance.trails.GetOrCreateValue(farmer);
            int ind = Math.Min(trail.Count - 1, ( numInFormation + 1 ) * Mod.TrailingDistance);
            Vector2 targetPos = ind < 0 ? farmer.Position : trail[ind];

            Vector2 posDiff = targetPos - Position;
            int targetDir = -3;
            if (posDiff.Y < 0)
            {
                if (MathF.Abs(posDiff.X) < -posDiff.Y)
                    targetDir = 0;
                else if (posDiff.X < 0)
                    targetDir = 3;
                else if (posDiff.X > 0)
                    targetDir = 1;
            }
            else if (posDiff.Y > 0)
            {
                if (MathF.Abs(posDiff.X) < posDiff.Y)
                    targetDir = 2;
                else if (posDiff.X < 0)
                    targetDir = 3;
                else if (posDiff.X > 0)
                    targetDir = 1;
            }
            else
            {
                if (posDiff.X < 0)
                    targetDir = 3;
                else if (posDiff.X > 0)
                    targetDir = 1;
            }

            if (Game1.player == farmer)
            {
                faceDirection(targetDir);

                // todo - pathfind if larger than dist from farmer
                if (Position != targetPos)
                {
                    Position = targetPos;
                    animateInFacingDirection(gameTime);
                }
            }

            updateGlow();
            updateEmote(gameTime);
            
        }

        public override void draw(SpriteBatch b, float alpha = 1)
        {
            b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(this.GetSpriteWidthForPositioning() * 4 / 2, this.GetBoundingBox().Height / 2), this.Sprite.SourceRect, Color.White * alpha, 0, new Vector2(this.Sprite.SpriteWidth / 2, (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, base.Scale) * 4f, (base.flip || (this.Sprite.CurrentAnimation != null && this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
            if (this.IsEmoting)
            {
                Vector2 emotePosition = this.getLocalPosition(Game1.viewport);
                emotePosition.Y -= 96f;
                b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(this.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, this.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.StandingPixel.Y / 10000f);
            }
        }
    }
}
