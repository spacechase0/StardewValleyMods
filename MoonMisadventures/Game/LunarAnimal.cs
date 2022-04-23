using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;

namespace MoonMisadventures.Game
{
    public enum LunarAnimalType
    {
        Cow,
        Chicken,
    }

    [XmlType( "Mods_spacechase0_MoonMisadventures_LunarAnimal" )]
    public class LunarAnimal : FarmAnimal
    {
        public readonly NetEnum<LunarAnimalType> lunarType = new();

        public static string GetVanillaTypeFromLunarType( LunarAnimalType type )
        {
            switch ( type )
            {
                case LunarAnimalType.Cow:
                    return "White Cow";
                case LunarAnimalType.Chicken:
                    return "White Chicken";
            }

            throw new ArgumentException( "Invalid lunar animal type" );
        }

        public LunarAnimal() { }
        public LunarAnimal( LunarAnimalType type, Vector2 pos, long id )
        :   base( GetVanillaTypeFromLunarType( type ), id, 0 )
        {
            lunarType.Value = type;
            position.Value = pos;
            age.Value = ageWhenMature.Value;

            switch ( type )
            {
                case LunarAnimalType.Cow:
                    displayType = "Lunar Cow";
                    break;
                case LunarAnimalType.Chicken:
                    displayType = "Lunar Chicken";
                    break;
            }
            reloadData();
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields( lunarType );
        }

        public override void reloadData()
        {
            base.reloadData();
            switch ( lunarType.Value )
            {
                case LunarAnimalType.Cow:
                    Sprite = new AnimatedSprite( Mod.instance.Helper.Content.GetActualAssetKey( "assets/cow.png" ), 0, 32, 32 );
                    break;
                case LunarAnimalType.Chicken:
                    Sprite = new AnimatedSprite( Mod.instance.Helper.Content.GetActualAssetKey( "assets/chicken.png" ), 0, 16, 16 );
                    break;
            }
            fullnessDrain.Value *= 2;
            happinessDrain.Value *= 2;
        }

        public override bool CanHavePregnancy()
        {
            return lunarType.Value == LunarAnimalType.Cow;
        }

        public override bool updateWhenCurrentLocation( GameTime time, GameLocation location )
        {
            //SpaceShared.Log.Debug( displayName + " full " + fullness.Value + " " + FarmAnimal.NumPathfindingThisTick );
            if ( !Game1.IsClient )
            {
                // Eat more aggressively since they can't go home to eat
                if ( !this.isSwimming.Value && location.IsOutdoors && ( byte ) this.fullness < 195 && Game1.random.NextDouble() < 0.1 && FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick )
                {
                    FarmAnimal.NumPathfindingThisTick++;
                    base.controller = new PathFindController( this, location, grassEndPointFunction, -1, eraseOldPathController: false, behaviorAfterFindingGrassPatch, 200, Point.Zero );
                    this._followTarget = null;
                    this._followTargetPosition = null;
                }
            }
            return base.updateWhenCurrentLocation( time, location );
        }

        public override void updateWhenNotCurrentLocation( Building currentBuilding, GameTime time, GameLocation environment )
        {
            base.updateWhenNotCurrentLocation( currentBuilding, time, environment );
            //SpaceShared.Log.Debug( displayName + " full " + fullness.Value+" "+FarmAnimal.NumPathfindingThisTick );
            if ( !Game1.IsClient )
            {
                // Eat more aggressively since they can't go home to eat
                if ( !this.isSwimming.Value && environment.IsOutdoors && ( byte ) this.fullness < 195 && Game1.random.NextDouble() < 0.1 && FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick )
                {
                    FarmAnimal.NumPathfindingThisTick++;
                    base.controller = new PathFindController( this, environment, grassEndPointFunction, -1, eraseOldPathController: false, behaviorAfterFindingGrassPatch, 200, Point.Zero );
                    this._followTarget = null;
                    this._followTargetPosition = null;
                }
            }
        }

        public void ActualDayUpdate( GameLocation loc )
        {
            if ( this.daysOwned.Value < 0 )
            {
                this.daysOwned.Value = this.age.Value;
            }
            this.StopAllActions();
            this.health.Value = 3;
            this.daysSinceLastLay.Value++;
            if ( !this.wasPet.Value && !this.wasAutoPet.Value )
            {
                this.friendshipTowardFarmer.Value = Math.Max( 0, ( int ) this.friendshipTowardFarmer - ( 10 - ( int ) this.friendshipTowardFarmer / 200 ) );
                this.happiness.Value = ( byte ) Math.Max( 0, ( byte ) this.happiness - ( byte ) this.happinessDrain * 5 );
            }
            this.wasPet.Value = false;
            this.wasAutoPet.Value = false;
            this.daysOwned.Value++;
            Random r = new Random((int)(long)this.myID / 2 + (int)Game1.stats.DaysPlayed);
            if ( ( byte ) this.fullness > 200 || r.NextDouble() < ( double ) ( ( byte ) this.fullness - 30 ) / 170.0 )
            {
                this.age.Value++;
                this.happiness.Value = ( byte ) Math.Min( 255, ( byte ) this.happiness + ( byte ) this.happinessDrain * 2 );
            }
            if ( this.fullness.Value < 200 )
            {
                this.happiness.Value = ( byte ) Math.Max( 0, ( byte ) this.happiness - 100 );
                this.friendshipTowardFarmer.Value = Math.Max( 0, ( int ) this.friendshipTowardFarmer - 20 );
            }
            bool produceToday = (byte)this.daysSinceLastLay >= (byte)this.daysToLay - ((this.type.Value.Equals("Sheep") && Game1.getFarmer(this.ownerID).professions.Contains(3)) ? 1 : 0) && r.NextDouble() < (double)(int)(byte)this.fullness / 200.0 && r.NextDouble() < (double)(int)(byte)this.happiness / 70.0;
            string whichProduce;
            if ( !produceToday || ( int ) this.age < ( byte ) this.ageWhenMature )
            {
                whichProduce = "-1";
            }
            else
            {
                whichProduce = this.defaultProduceIndex;
                if ( r.NextDouble() < ( double ) ( int ) ( byte ) this.happiness / 150.0 )
                {
                    float happinessModifier = (((byte)this.happiness > 200) ? ((float)(int)(byte)this.happiness * 1.5f) : ((float)(((byte)this.happiness <= 100) ? ((byte)this.happiness - 100) : 0)));
                    this.daysSinceLastLay.Value = 0;
                    Game1.stats.ChickenEggsLayed++;
                    double chanceForQuality = (float)(int)this.friendshipTowardFarmer / 1000f - (1f - (float)(int)(byte)this.happiness / 225f);
                    if ( ( !this.isCoopDweller() && Game1.getFarmer( this.ownerID ).professions.Contains( 3 ) ) || ( this.isCoopDweller() && Game1.getFarmer( this.ownerID ).professions.Contains( 2 ) ) )
                    {
                        chanceForQuality += 0.33;
                    }
                    if ( chanceForQuality >= 0.95 && r.NextDouble() < chanceForQuality / 2.0 )
                    {
                        this.produceQuality.Value = 4;
                    }
                    else if ( r.NextDouble() < chanceForQuality / 2.0 )
                    {
                        this.produceQuality.Value = 2;
                    }
                    else if ( r.NextDouble() < chanceForQuality )
                    {
                        this.produceQuality.Value = 1;
                    }
                    else
                    {
                        this.produceQuality.Value = 0;
                    }
                }
            }
            if ( ( byte ) this.harvestType == 1 && produceToday )
            {
                this.currentProduce.Value = whichProduce;
                whichProduce = "-1";
            }
            if ( whichProduce != "-1" /*&& this.home != null*/ )
            {
                bool spawn_object = true;
                foreach ( StardewValley.Object location_object in loc.objects.Values )
                {
                    if ( ( bool ) location_object.bigCraftable && location_object.ItemID == "165" && location_object.heldObject.Value != null && ( location_object.heldObject.Value as Chest ).addItem( new StardewValley.Object( lunarType.Value == LunarAnimalType.Chicken ? ItemIds.GalaxyEgg : ItemIds.GalaxyMilk, 1 )
                    {
                        Quality = this.produceQuality
                    } ) == null )
                    {
                        location_object.showNextIndex.Value = true;
                        spawn_object = false;
                        break;
                    }
                }
                if ( spawn_object && !loc.Objects.ContainsKey( base.getTileLocation() ) )
                {
                    loc.Objects.Add( base.getTileLocation(), new StardewValley.Object( ItemIds.GalaxyEgg, 1 )
                    {
                        CanBeGrabbed = true,
                        IsSpawnedObject = true,
                        Quality = this.produceQuality
                    } );
                }
            }
            //if ( !wasLeftOutLastNight )
            {
                if ( ( byte ) this.fullness < 30 )
                {
                    this.moodMessage.Value = 4;
                }
                else if ( ( byte ) this.happiness < 30 )
                {
                    this.moodMessage.Value = 3;
                }
                else if ( ( byte ) this.happiness < 200 )
                {
                    this.moodMessage.Value = 2;
                }
                else
                {
                    this.moodMessage.Value = 1;
                }
            }
            if ( Game1.timeOfDay < 1700 )
            {
                this.fullness.Value = ( byte ) Math.Max( 0, ( byte ) this.fullness - ( byte ) this.fullnessDrain * ( 1700 - Game1.timeOfDay ) / 100 );
            }
            this.fullness.Value = 0;
            if ( Utility.isFestivalDay( Game1.dayOfMonth, Game1.currentSeason ) )
            {
                this.fullness.Value = 250;
            }
            //this.reload( this.home );
        }
    }
}
