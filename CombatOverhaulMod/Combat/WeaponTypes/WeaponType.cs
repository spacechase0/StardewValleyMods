using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;

namespace CombatOverhaulMod.Combat.WeaponTypes
{
    internal abstract class WeaponType
    {
        public static readonly int Spear = 10;
        public static readonly int Gloves = 11;

        public virtual bool StopsPlayer { get; } = true;

        public virtual float BaseStaminaUsage { get; } = 0.2f;

        public virtual float BaseSwipeSpeed { get; } = 400;
        public virtual float SwipeSpeedModifier { get; } = 40;

        public abstract void StartSwipe(MeleeWeapon weapon, Farmer who);

        public virtual void DoSwipe(MeleeWeapon weapon, Farmer who, float speed)
        {
            if (who == null || who.CurrentTool != weapon)
                return;

            if (who.IsLocalPlayer)
            {
                who.TemporaryPassableTiles.Clear();
                who.currentLocation.lastTouchActionLocation = Vector2.Zero;
            }

            // vanilla has a swipeSpeed *= 1.3f;, but I'm skipping it
            // and I'm not sure how it affects things at this point

            DoSwipeAnimation(weapon, who, speed);
        }

        public virtual void DoSwipeAnimation(MeleeWeapon weapon, Farmer who, float speed)
        {
            throw new NotImplementedException();
        }

        public abstract void DrawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, bool isOnSpecial, Texture2D texture, Rectangle sourceRect, float sort_behind_layer, float sort_layer);

        public abstract Rectangle GetNormalDamageArea(MeleeWeapon weapon, int x, int y, int facingDirection, Rectangle wielderBoundingBox, int indexInCurrentAnimation);
    }
}
