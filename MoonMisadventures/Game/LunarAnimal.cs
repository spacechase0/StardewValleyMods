using System;
using System.Xml.Serialization;

using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;

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
                    Sprite = new AnimatedSprite( Mod.instance.Helper.ModContent.GetInternalAssetName("assets/cow.png").BaseName, 0, 32, 32 );
                    break;
                case LunarAnimalType.Chicken:
                    Sprite = new AnimatedSprite( Mod.instance.Helper.ModContent.GetInternalAssetName("assets/chicken.png").BaseName, 0, 16, 16 );
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
                if ( !this.isSwimming.Value && location.IsOutdoors && this.fullness.Value < 195 && Game1.random.NextDouble() < 0.1 && FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick )
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
                if ( !this.isSwimming.Value && environment.IsOutdoors && this.fullness.Value < 195 && Game1.random.NextDouble() < 0.1 && FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick )
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
                this.happiness.Value = ( byte ) Math.Max( 0, this.happiness.Value - this.happinessDrain.Value * 5 );
            }
            this.wasPet.Value = false;
            this.wasAutoPet.Value = false;
            this.daysOwned.Value++;
            Random r = new Random((int)this.myID.Value / 2 + (int)Game1.stats.DaysPlayed);
            if ( this.fullness.Value > 200 || r.NextDouble() < ( double ) ( this.fullness.Value - 30 ) / 170.0 )
            {
                this.age.Value++;
                this.happiness.Value = ( byte ) Math.Min( 255, this.happiness.Value + this.happinessDrain.Value * 2 );
            }
            if ( this.fullness.Value < 200 )
            {
                this.happiness.Value = ( byte ) Math.Max( 0, this.happiness.Value - 100 );
                this.friendshipTowardFarmer.Value = Math.Max( 0, this.friendshipTowardFarmer.Value - 20 );
            }
            bool produceToday = this.daysSinceLastLay.Value >= this.daysToLay.Value - ((this.type.Value.Equals("Sheep") && Game1.getFarmer(this.ownerID).professions.Contains(3)) ? 1 : 0) && r.NextDouble() < (double)(int)this.fullness.Value / 200.0 && r.NextDouble() < (double)(int)this.happiness.Value / 70.0;
            int whichProduce;
            if ( !produceToday || this.age.Value < this.ageWhenMature.Value )
            {
                whichProduce = -1;
            }
            else
            {
                whichProduce = this.defaultProduceIndex.Value;
                if (r.NextDouble() < this.happiness.Value / 150.0 )
                {
                    float happinessModifier = ((this.happiness.Value > 200) ? ((float)(int)this.happiness.Value * 1.5f) : ((float)((this.happiness.Value <= 100) ? (this.happiness.Value - 100) : 0)));
                    this.daysSinceLastLay.Value = 0;
                    Game1.stats.ChickenEggsLayed++;
                    double chanceForQuality = this.friendshipTowardFarmer.Value / 1000f - (1f - this.happiness.Value / 225f);
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
            if ( this.harvestType.Value == 1 && produceToday )
            {
                this.currentProduce.Value = whichProduce;
                whichProduce = -1;
            }
            if ( whichProduce != -1 /*&& this.home != null*/ )
            {
                bool spawn_object = true;
                foreach ( StardewValley.Object location_object in loc.objects.Values )
                {
                    if (location_object.bigCraftable.Value && location_object.ParentSheetIndex == 165 && location_object.heldObject.Value != null && ( location_object.heldObject.Value as Chest ).addItem( new CustomObject( DynamicGameAssets.Mod.Find( lunarType.Value == LunarAnimalType.Chicken ? ItemIds.GalaxyEgg : ItemIds.GalaxyMilk ) as ObjectPackData )
                    {
                        Quality = this.produceQuality.Value
                    } ) == null )
                    {
                        location_object.showNextIndex.Value = true;
                        spawn_object = false;
                        break;
                    }
                }
                if ( spawn_object && !loc.Objects.ContainsKey( base.getTileLocation() ) )
                {
                    loc.Objects.Add( base.getTileLocation(), new CustomObject( DynamicGameAssets.Mod.Find( ItemIds.GalaxyEgg ) as ObjectPackData )
                    {
                        CanBeGrabbed = true,
                        IsSpawnedObject = true,
                        Quality = this.produceQuality.Value
                    } );
                }
            }
            //if ( !wasLeftOutLastNight )
            {
                if ( this.fullness.Value < 30 )
                {
                    this.moodMessage.Value = 4;
                }
                else if ( this.happiness.Value < 30 )
                {
                    this.moodMessage.Value = 3;
                }
                else if ( this.happiness.Value < 200 )
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
                this.fullness.Value = ( byte ) Math.Max( 0, this.fullness.Value - this.fullnessDrain.Value * ( 1700 - Game1.timeOfDay ) / 100 );
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
