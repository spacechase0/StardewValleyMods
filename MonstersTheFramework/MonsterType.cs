using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace MonstersTheFramework
{
    public class MonsterType
    {
        public class StateData : ICloneable
        {
            public class MovementData : ICloneable
            {
                public enum DirectionType
                {
                    Fixed,
                    TowardPlayer,
                    Pathfind, // Special case, ignores most other movement variables
                }

                public enum StyleType
                {
                    Constant,
                    Dash,
                }

                public DirectionType Direction { get; set; }
                public StyleType Style { get; set; }

                public Vector2 FixedDirection { get; set; }
                public float MovementSpeed { get; set; }
                public float DashFriction { get; set; }

                public object Clone()
                {
                    var ret = ( MovementData ) this.MemberwiseClone();
                    return ret;
                }
            }

            public class AnimationData : ICloneable
            {
                public string SpriteSheet { get; set; }
                public Vector2 FrameSize { get; set; }
                public int StartingIndex { get; set; }
                public int AnimationLength { get; set; } = 1;
                public int TicksPerFrame { get; set; } = 1;
                public bool Loops { get; set; } = true;
                public Vector2 Origin { get; set; }
                public Vector2 DeathChunkStartCoordinates { get; set; }

                public object Clone()
                {
                    var ret = ( AnimationData ) this.MemberwiseClone();
                    return ret;
                }
            }

            /* on start
             * on hurt by player
             * on hurt by bomb
             * on hit player
             * on animation end
             * on collide with wall/obstacle
             * on tick
             * on death
             **/
            public class EventData : ICloneable
            {
                public string Event { get; set; }

                public string When { get; set; }

                [JsonExtensionData]
                public Dictionary<string, object> Actions { get; set; }
                /* shoot projectile(s)
                 * change state
                 * heal
                 * make explosion
                 * play sound
                 * break nearby obstacles
                 * spawn monster
                 **/
                public object Clone()
                {
                    var ret = ( EventData ) this.MemberwiseClone();
                    ret.Actions = new();
                    foreach ( var action in Actions )
                        ret.Actions.Add( action.Key, action.Value );
                    return ret;
                }
            }

            public string InheritsFrom { get; set; }

            public Vector2? BoundingBoxSize { get; set; }
            public int? Defense { get; set; }

            public MovementData Movement { get; set; }
            public bool? IsGlider { get; set; }

            public string CanReceiveDamageFromPlayer { get; set; } // true, false, or enchantment name requirement
            public bool? CanReceiveDamageFromBomb { get; set; }
            public float? IncomingDamageMultiplier { get; set; }
            public string HitSound { get; set; }

            public AnimationData Animation { get; set; }

            public int? ContactDamage { get; set; }
            public string DamageElement { get; set; } // MCN only
            public string ApplyDebuff { get; set; }

            public List<EventData> Events { get; set; } = new();

            public object Clone()
            {
                var ret = ( StateData ) this.MemberwiseClone();
                ret.Movement = ( MovementData ) Movement?.Clone();
                ret.Animation = ( AnimationData ) Animation?.Clone();
                ret.Events = new();
                foreach ( var evt in Events )
                    ret.Events.Add( ( EventData ) evt.Clone() );
                return ret;
            }

            public StateData InheritFrom( StateData other, Dictionary<string, StateData> states )
            {
                var ret = ( StateData ) other.Clone();
                if ( !string.IsNullOrEmpty( ret.InheritsFrom ) && states.ContainsKey( ret.InheritsFrom ) )
                {
                    ret = ret.InheritFrom( states[ ret.InheritsFrom ], states );
                }
                if ( BoundingBoxSize.HasValue )
                    ret.BoundingBoxSize = BoundingBoxSize.Value;
                if ( Defense.HasValue )
                    ret.Defense = Defense.Value;
                if ( Movement != null )
                    ret.Movement = Movement;
                if ( IsGlider.HasValue )
                    ret.IsGlider = IsGlider.Value;
                if ( CanReceiveDamageFromPlayer != null )
                    ret.CanReceiveDamageFromPlayer = CanReceiveDamageFromPlayer;
                if ( CanReceiveDamageFromBomb.HasValue )
                    ret.CanReceiveDamageFromBomb = CanReceiveDamageFromBomb.Value;
                if ( IncomingDamageMultiplier.HasValue )
                    ret.IncomingDamageMultiplier = IncomingDamageMultiplier.Value;
                if ( HitSound != null )
                    ret.HitSound = HitSound;
                if ( Animation != null )
                    ret.Animation = Animation;
                // state length?
                if ( ContactDamage.HasValue )
                    ret.ContactDamage = ContactDamage.Value;
                if ( DamageElement != null )
                    ret.DamageElement = DamageElement;
                if ( ApplyDebuff != null )
                    ret.ApplyDebuff = ApplyDebuff;
                foreach ( var evt in Events )
                {
                    ret.Events.Add( evt );
                }
                return ret;
            }
        }
        public class ItemDrop
        {
            public string Drop { get; set; }
            public int Quantity { get; set; } = 1;
        }


        public string Name { get; set; }
        public string CorrespondingMonsterGoal { get; set; }
        public int MaxHealth { get; set; }

        public List<List<Weighted<ItemDrop>>> Drops { get; set; } = new();

        public string StartingState { get; set; }
        public Dictionary<string, StateData> States { get; set; }
    }
}
