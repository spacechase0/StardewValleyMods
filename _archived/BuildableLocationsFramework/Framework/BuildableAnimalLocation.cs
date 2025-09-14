using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace BuildableLocationsFramework.Framework
{
    internal class BuildableAnimalLocation : BuildableGameLocation, IAnimalLocation
    {
        public NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> Animals { get; } = new();

        private void MyWarpHome(FarmAnimal farmAnimal)
        {
            if (farmAnimal.home?.indoors.Value is not AnimalHouse animalHouse)
                return;

            animalHouse.animals.Add(farmAnimal.myID.Value, farmAnimal);
            this.Animals.Remove(farmAnimal.myID.Value);
            farmAnimal.controller = null;
            farmAnimal.setRandomPosition(animalHouse);
            ++farmAnimal.home.currentOccupants.Value;
        }

        public BuildableAnimalLocation()
        {
        }

        public BuildableAnimalLocation(string mapPath, string name)
            : base(mapPath, name) { }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.Animals);
        }

        public override void DayUpdate(int dayOfMonth)
        {
            for (int index = this.Animals.Count() - 1; index >= 0; --index)
                this.Animals.Pairs.ElementAt(index).Value.dayUpdate(this);
            base.DayUpdate(dayOfMonth);
        }

        public override bool performToolAction(Tool t, int tileX, int tileY)
        {
            if (t is MeleeWeapon weapon)
            {
                foreach (FarmAnimal farmAnimal in this.Animals.Values)
                {
                    if (farmAnimal.GetBoundingBox().Intersects(weapon.mostRecentArea))
                        farmAnimal.hitWithWeapon(weapon);
                }
            }
            return base.performToolAction(t, tileX, tileY);
        }

        public override void timeUpdate(int timeElapsed)
        {
            base.timeUpdate(timeElapsed);
            if (Game1.IsMasterGame)
            {
                foreach (FarmAnimal farmAnimal in this.Animals.Values)
                    farmAnimal.updatePerTenMinutes(Game1.timeOfDay, this);
            }

            foreach (Building building in this.buildings)
            {
                if (building.daysOfConstructionLeft.Value <= 0)
                {
                    building.performTenMinuteAction(timeElapsed);
                    if (building.indoors.Value != null && !Game1.locations.Contains(building.indoors.Value) && timeElapsed >= 10)
                    {
                        building.indoors.Value.performTenMinuteUpdate(Game1.timeOfDay);
                        if (timeElapsed > 10)
                            building.indoors.Value.passTimeForObjects(timeElapsed - 10);
                    }
                }
            }
        }

        public override bool isCollidingPosition(
            Rectangle position,
            xTile.Dimensions.Rectangle viewport,
            bool isFarmer,
            int damagesFarmer,
            bool glider,
            Character character,
            bool pathfinding,
            bool projectile = false,
            bool ignoreCharacterRequirement = false)
        {
            if (!glider)
            {
                foreach (KeyValuePair<long, FarmAnimal> pair in this.Animals.Pairs)
                {
                    if (character != null && !character.Equals(pair.Value) && (character is not FarmAnimal && position.Intersects(pair.Value.GetBoundingBox())) && (!isFarmer || !Game1.player.GetBoundingBox().Intersects(pair.Value.GetBoundingBox())))
                    {
                        if (isFarmer && character is Farmer farmer)
                        {
                            if (farmer.TemporaryPassableTiles.Intersects(position))
                                break;
                        }
                        pair.Value.farmerPushing();
                        return true;
                    }
                }
            }
            return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
        }

        public bool CheckPetAnimal(Vector2 position, Farmer who)
        {
            foreach (KeyValuePair<long, FarmAnimal> pair in this.Animals.Pairs)
            {
                if (!pair.Value.wasPet.Value && pair.Value.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
                {
                    pair.Value.pet(who);
                    return true;
                }
            }
            return false;
        }

        public bool CheckPetAnimal(Rectangle rect, Farmer who)
        {
            foreach (KeyValuePair<long, FarmAnimal> pair in this.Animals.Pairs)
            {
                if (!pair.Value.wasPet.Value && pair.Value.GetBoundingBox().Intersects(rect))
                {
                    pair.Value.pet(who);
                    return true;
                }
            }
            return false;
        }

        public bool CheckInspectAnimal(Vector2 position, Farmer who)
        {
            foreach (KeyValuePair<long, FarmAnimal> pair in this.Animals.Pairs)
            {
                if (pair.Value.wasPet.Value && pair.Value.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
                {
                    pair.Value.pet(who);
                    return true;
                }
            }
            return false;
        }

        public bool CheckInspectAnimal(Rectangle rect, Farmer who)
        {
            foreach (KeyValuePair<long, FarmAnimal> pair in this.Animals.Pairs)
            {
                if (pair.Value.wasPet.Value && pair.Value.GetBoundingBox().Intersects(rect))
                {
                    pair.Value.pet(who);
                    return true;
                }
            }
            return false;
        }

        public override bool isTileOccupied(
            Vector2 tileLocation,
            string characterToIgnore = "",
            bool ignoreAllCharacters = false)
        {
            foreach (KeyValuePair<long, FarmAnimal> pair in this.Animals.Pairs)
            {
                if (pair.Value.getTileLocation().Equals(tileLocation))
                    return true;
            }
            return base.isTileOccupied(tileLocation, characterToIgnore, ignoreAllCharacters);
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            if (Game1.timeOfDay >= 1830)
            {
                for (int index = this.Animals.Count() - 1; index >= 0; --index)
                {
                    KeyValuePair<long, FarmAnimal> keyValuePair = this.Animals.Pairs.ElementAt(index);
                    FarmAnimal farmAnimal = keyValuePair.Value;
                    this.MyWarpHome(farmAnimal);
                }
            }

            Building underConstruction = this.getBuildingUnderConstruction();
            if (underConstruction?.daysOfConstructionLeft.Value > 0 && Game1.getCharacterFromName("Robin").currentLocation.Equals(this))
            {
                this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(399, 262, underConstruction.daysOfConstructionLeft.Value == 1 ? 29 : 9, 43), new Vector2(underConstruction.tileX.Value + underConstruction.tilesWide.Value / 2, underConstruction.tileY.Value + underConstruction.tilesHigh.Value / 2) * 64f + new Vector2(-16f, -144f), false, 0.0f, Color.White)
                {
                    id = 16846f,
                    scale = 4f,
                    interval = 999999f,
                    animationLength = 1,
                    totalNumberOfLoops = 99999,
                    layerDepth = ((underConstruction.tileY.Value + underConstruction.tilesHigh.Value / 2) * 64 + 32) / 10000f
                });
            }
            else
                this.removeTemporarySpritesWithIDLocal(16846f);
        }

        public override void pokeTileForConstruction(Vector2 tile)
        {
            base.pokeTileForConstruction(tile);
            foreach (KeyValuePair<long, FarmAnimal> pair in this.Animals.Pairs)
            {
                if (pair.Value.getTileLocation().Equals(tile))
                    pair.Value.Poke();
            }
        }

        public override bool isTileOccupiedForPlacement(Vector2 tileLocation, SObject toPlace = null)
        {
            foreach (KeyValuePair<long, FarmAnimal> pair in this.Animals.Pairs)
            {
                if (pair.Value.getTileLocation().Equals(tileLocation))
                    return true;
            }
            return base.isTileOccupiedForPlacement(tileLocation, toPlace);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            foreach (KeyValuePair<long, FarmAnimal> pair in this.Animals.Pairs)
                pair.Value.draw(b);
        }

        public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
        {
            base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
            if (Game1.currentLocation.Equals(this))
                return;
            NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>.PairsCollection pairs = this.Animals.Pairs;
            for (int index = pairs.Count() - 1; index >= 0; --index)
                pairs.ElementAt(index).Value.updateWhenNotCurrentLocation(null, time, this);
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            if (this.wasUpdated && Game1.gameMode != 0)
                return;
            base.UpdateWhenCurrentLocation(time);

            List<KeyValuePair<long, FarmAnimal>> tempAnimals = new List<KeyValuePair<long, FarmAnimal>>();
            foreach (KeyValuePair<long, FarmAnimal> pair in this.Animals.Pairs)
                tempAnimals.Add(pair);
            foreach (KeyValuePair<long, FarmAnimal> tempAnimal in tempAnimals)
            {
                if (tempAnimal.Value.updateWhenCurrentLocation(time, this))
                    this.Animals.Remove(tempAnimal.Key);
            }
            tempAnimals.Clear();
        }

        public override bool CanRefillWateringCanOnTile(int tileX, int tileY)
        {
            Building buildingAt = this.getBuildingAt(new Vector2(tileX, tileY));
            return buildingAt != null && buildingAt.CanRefillWateringCan() || base.CanRefillWateringCanOnTile(tileX, tileY);
        }

        public override bool isTileBuildingFishable(int tileX, int tileY)
        {
            Vector2 tile = new Vector2(tileX, tileY);
            foreach (Building building in this.buildings)
            {
                if (building.isTileFishable(tile))
                    return true;
            }
            return base.isTileBuildingFishable(tileX, tileY);
        }

        public override SObject getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
        {
            if (locationName != null && locationName != this.Name)
                return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
            if (bobberTile != Vector2.Zero)
            {
                foreach (Building building in this.buildings)
                {
                    if (building is FishPond pond && pond.isTileFishable(bobberTile))
                        return pond.CatchFish();
                }
            }
            return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
        }
    }
}
