using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatOverhaulMod.Elements
{
    public static class Extensions
    {
        public static void TakeDamage( this Character character, string element, int amount, Character src = null, bool overrideParry = false )
        {
            if ( character is Farmer farmer )
            {
                if ( element == null )
                    farmer.takeDamage( amount, overrideParry, ( src as Monster ) );
                else
                {
                    // Elemental damage can't be parried
                    // TODO: Make it use a variable in ElementData
                    farmer.TakeElementalDamage( element, amount, ( src as Monster ) );
                }
            }
            else if ( character is Monster monster )
            {
                monster.TakeElementalDamage( element, amount, ( src as Farmer ) );
            }
        }

        public static void TakeElementalDamage( this Farmer farmer, string element, int amount, Monster src )
        {
            if ( Game1.eventUp || farmer.FarmerSprite.isPassingOut() )
            {
                return;
            }
            var elem = Game1.content.Load<Dictionary<string, ElementData>>( "spacechase0.CombatOverhaulMod\\Elements" )[ element ];

            // Following vanilla logic here... Mostly
            amount += Game1.random.Next( Math.Min( -1, -amount / 8 ), Math.Max( 1, amount / 8 ) );
            double resist = farmer.GetElementalResistance( element );

            if (farmer.isWearingRing( Ring.YobaRingId ) && !Game1.player.buffs.IsApplied( Buff.yobaBlessing ) && Game1.random.NextDouble() < (0.9 - (double)((float) farmer.health / 100f)) / (double)(3 - farmer.LuckLevel / 10) + (( farmer.health <= 15) ? 0.2 : 0.0))
            {
                farmer.currentLocation.playSound("yoba");
                Game1.player.buffs.Apply(new Buff(Buff.yobaBlessing));
                return;
            }

            if ( amount > 0 && src != null && farmer.isWearingRing( "839" ) )
            {
                Microsoft.Xna.Framework.Rectangle monsterBox = src.GetBoundingBox();
                Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(monsterBox, farmer);
                trajectory /= 2f;
                int damageToMonster = amount;
                int farmerDamage = ( int ) Math.Max(1, amount * ( 1 - resist ));
                if ( farmerDamage < 10 )
                {
                    damageToMonster = ( int ) Math.Ceiling( ( double ) ( damageToMonster + farmerDamage ) / 2.0 );
                }
                // TODO: Elements for the following line
                src.takeDamage( damageToMonster, ( int ) trajectory.X, ( int ) trajectory.Y, isBomb: false, 1.0, farmer );
                src.currentLocation.debris.Add( new Debris( damageToMonster, new Vector2( monsterBox.Center.X + 16, monsterBox.Center.Y ), new Color( 255, 130, 0 ), 1f, src ) );
            }
            if ( amount > 0 && farmer.isWearingRing(Ring.YobaRingId) && !Game1.player.buffs.IsApplied(Buff.yobaBlessing) && Game1.random.NextDouble() < ( 0.9 - ( double ) ( ( float ) farmer.health / 100f ) ) / ( double ) ( 3 - farmer.LuckLevel / 10 ) + ( ( farmer.health <= 15 ) ? 0.2 : 0.0 ) )
            {
                farmer.currentLocation.playSound( "yoba" );
                Game1.player.buffs.Apply(new Buff(Buff.yobaBlessing));
                return;
            }
            Rumble.rumble( 0.75f, 150f );
            amount = ( int ) ( amount * ( 1 - resist ) );//( int ) Math.Max( 1, amount * ( 1 - resist ) );
            farmer.health = Math.Max( 0, farmer.health - amount );
            if ( farmer.health <= 0 && farmer.GetEffectsOfRingMultiplier( "863" ) > 0 && !farmer.hasUsedDailyRevive.Value )
            {
                Game1.player.startGlowing( new Color( 255, 255, 0 ), border: false, 0.25f );
                DelayedAction.functionAfterDelay( delegate
                {
                    farmer.stopGlowing();
                }, 500 );
                Game1.playSound( "yoba" );
                for ( int i = 0; i < 13; i++ )
                {
                    float xPos = Game1.random.Next(-32, 33);
                    farmer.currentLocation.temporarySprites.Add( new TemporaryAnimatedSprite( "LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle( 114, 46, 2, 2 ), 200f, 5, 1, new Vector2( xPos + 32f, -96f ), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f )
                    {
                        attachedCharacter = farmer,
                        positionFollowsAttachedCharacter = true,
                        motion = new Vector2( xPos / 32f, -3f ),
                        delayBeforeAnimationStart = i * 50,
                        alphaFade = 0.001f,
                        acceleration = new Vector2( 0f, 0.1f )
                    } );
                }
                farmer.currentLocation.temporarySprites.Add( new TemporaryAnimatedSprite( "LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle( 157, 280, 28, 19 ), 2000f, 1, 1, new Vector2( -20f, -16f ), flicker: false, flipped: false, 1E-06f, 0f, Color.White, 4f, 0f, 0f, 0f )
                {
                    attachedCharacter = farmer,
                    positionFollowsAttachedCharacter = true,
                    alpha = 0.1f,
                    alphaFade = -0.01f,
                    alphaFadeFade = -0.00025f
                } );
                farmer.health = ( int ) Math.Min( farmer.maxHealth, ( float ) farmer.maxHealth * 0.5f + ( float ) farmer.GetEffectsOfRingMultiplier( "863" ) );
                farmer.hasUsedDailyRevive.Value = true;
            }
            if ( amount > 0 )
            {
                farmer.temporarilyInvincible = true;
                farmer.temporaryInvincibilityTimer = 0;
                farmer.currentTemporaryInvincibilityDuration = 1200 + farmer.GetEffectsOfRingMultiplier( "861" ) * 400;
                farmer.currentLocation.playSound( "ow" );
                Game1.hitShakeTimer = 100 * amount;
            }
            farmer.currentLocation.debris.Add( new Debris( amount, new Vector2( farmer.StandingPixel.X + 8, farmer.StandingPixel.Y ), elem.Color, 1f, farmer ) );
        }

        public static void TakeElementalDamage( this Monster monster, string element, int amount, Farmer src )
        {
            var elem = Game1.content.Load<Dictionary<string, ElementData>>("spacechase0.CombatOverhaulMod\\Elements")[ element ];
            double resist = monster.GetElementalResistance( element );
            if ( src != null )
            {
                if ( src.professions.Contains( Farmer.fighter ) )
                    amount = (int)Math.Ceiling( amount * 1.1f );
                if ( src.professions.Contains( Farmer.brute ) )
                    amount = ( int ) Math.Ceiling( amount * 1.51f );
            }
            amount = ( int ) ( amount * ( 1 - resist ) );

            monster.currentLocation.removeDamageDebris( monster );
            monster.currentLocation.debris.Add( new Debris( amount, new Vector2( monster.GetBoundingBox().Center.X + 16, monster.GetBoundingBox().Center.Y ), elem.Color, 1, monster ) );
            if ( src != null )
            {
                foreach ( BaseEnchantment enchantment2 in src.enchantments )
                {
                    enchantment2.OnDealDamage( monster, monster.currentLocation, src, ref amount );
                }
            }

            if ( monster.Health <= 0 )
            {
                if ( !monster.currentLocation.isFarm )
                {
                    src.checkForQuestComplete( null, 1, 1, null, monster.Name, 4 );
                }
                if ( !monster.currentLocation.isFarm && Game1.player.team.specialOrders != null )
                {
                    foreach ( SpecialOrder order in Game1.player.team.specialOrders )
                    {
                        if ( order.onMonsterSlain != null )
                        {
                            order.onMonsterSlain( Game1.player, monster );
                        }
                    }
                }
                if ( src != null )
                {
                    foreach ( BaseEnchantment enchantment3 in src.enchantments )
                    {
                        enchantment3.OnMonsterSlay( monster, monster.currentLocation, src );
                    }
                }
                if ( src != null && src.leftRing.Value != null )
                {
                    src.leftRing.Value.onMonsterSlay( monster, monster.currentLocation, src );
                }
                if ( src != null && src.rightRing.Value != null )
                {
                    src.rightRing.Value.onMonsterSlay( monster, monster.currentLocation, src );
                }
                // TODO: Wear more rings API?
                if ( src != null && !monster.currentLocation.isFarm && ( !( monster is GreenSlime ) || ( bool ) ( monster as GreenSlime ).firstGeneration ) )
                {
                    if ( src.IsLocalPlayer )
                    {
                        Game1.stats.monsterKilled( monster.Name );
                    }
                    else if ( Game1.IsMasterGame )
                    {
                        src.queueMessage( 25, Game1.player, monster.Name );
                    }
                }
                monster.currentLocation.monsterDrop( monster, monster.GetBoundingBox().Center.X, monster.GetBoundingBox().Center.Y, src );
                if ( src != null && !monster.currentLocation.isFarm )
                {
                    src.gainExperience( 4, monster.ExperienceGained );
                }
                if ( ( bool ) monster.isHardModeMonster )
                {
                    Game1.stats.incrementStat( "hardModeMonstersKilled", 1 );
                }
                monster.currentLocation.characters.Remove( monster );
                Game1.stats.MonstersKilled++;
            }
            else if ( amount > 0 )
            {
                monster.shedChunks( Game1.random.Next( 1, 3 ) );
            }
        }

        public static double GetElementalResistance( this Character character, string element )
        {
            if ( character is Farmer farmer )
            {
                double ret = 0;
                ret += farmer.hat.Value?.GetElementalStat( element ) ?? 0;
                ret += farmer.shirtItem.Value?.GetElementalStat( element ) ?? 0;
                ret += farmer.pantsItem.Value?.GetElementalStat( element ) ?? 0;
                ret += farmer.boots.Value?.GetElementalStat( element ) ?? 0;
                ret += farmer.leftRing.Value?.GetElementalStat( element ) ?? 0;
                ret += farmer.rightRing.Value?.GetElementalStat( element ) ?? 0;
                return ret;
            }
            else
            {
                var data = Game1.content.Load< Dictionary< string, Dictionary< string, double > > >("spacechase0.CombatOverhaulMod\\DefaultElementalStats");
                if ( character.Name != null && data.ContainsKey( character.Name ) && data[ character.Name ].ContainsKey( element ) )
                    return data[ character.Name ][ element ];
                return 0;
            }
        }

        public static Dictionary<string, double> GetElementalResistances( this Character character )
        {
            var elements = Game1.content.Load< Dictionary< string, ElementData > >("spacechase0.CombatOverhaulMod\\Elements");

            Dictionary<string, double> ret = new();
            foreach ( var element in elements )
            {
                ret.Add( element.Key, character.GetElementalResistance( element.Key ) );
            }

            return ret;
        }

        public static double GetElementalStat( this Item item, string element )
        {
            var elems = item.get_ElementalStatOverrides();
            if ( !elems.ContainsKey( element ) )
                return item.GetDefaultElementalStat( element );
            return elems[ element ];
        }

        public static void SetElementalStat( this Item item, string element, double value )
        {
            var elems = item.get_ElementalStatOverrides();
            if ( !elems.ContainsKey( element ) )
                elems.Add( element, value );
            else
                elems[ element ] = value;
        }

        public static Dictionary<string, double> GetElementalStats( this Item item )
        {
            var elements = Game1.content.Load< Dictionary< string, ElementData > >( "spacechase0.CombatOverhaulMod\\Elements" );

            Dictionary<string, double> ret = new();
            foreach ( var element in elements )
            {
                ret.Add( element.Key, item.GetElementalStat( element.Key ) );
            }

            return ret;
        }

        public static double GetDefaultElementalStat( this Item item, string element )
        {
            var data = Game1.content.Load< Dictionary< string, Dictionary< string, double > > >("spacechase0.CombatOverhaulMod\\DefaultElementalStats");

            string idStr = item.QualifiedItemId.ToString();
            if ( data.ContainsKey( idStr ) && data[ idStr ].ContainsKey( element ) )
                return data[ idStr ][ element ];

            return 0;
        }
    }
}
