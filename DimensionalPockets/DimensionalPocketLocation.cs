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
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Tools;

namespace DimensionalPockets
{
    [XmlType( "Mods_DimensionalPockets_DimensionalPocket" )]
    public class DimensionalPocketLocation : BuildableGameLocation, IAnimalLocation
    {
        public readonly NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = new();
        public NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> Animals => animals;

        public DimensionalPocketLocation()
        {
        }

        public DimensionalPocketLocation(string mapPath, string name)
            : base(mapPath, name)
        {
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(Animals);
        }

        public override void TransferDataFromSavedLocation(GameLocation l)
        {
            var other = l as DimensionalPocketLocation;
            Animals.MoveFrom(other.Animals);
            foreach (var animal in Animals.Values)
            {
                animal.reload(null);
            }

            base.TransferDataFromSavedLocation(l);
        }

        public bool CheckInspectAnimal(Vector2 position, Farmer who)
        {
            foreach (var animal in Animals.Values)
            {
                if (animal.wasPet.Value && animal.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
                {
                    animal.pet(who);
                    return true;
                }
            }

            return false;
        }

        public bool CheckInspectAnimal(Rectangle rect, Farmer who)
        {
            foreach (var animal in Animals.Values)
            {
                if (animal.wasPet.Value && animal.GetBoundingBox().Intersects(rect))
                {
                    animal.pet(who);
                    return true;
                }
            }

            return false;
        }

        public bool CheckPetAnimal(Vector2 position, Farmer who)
        {
            foreach (var animal in Animals.Values)
            {
                if (!animal.wasPet.Value && animal.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
                {
                    animal.pet(who);
                    return true;
                }
            }

            return false;
        }

        public bool CheckPetAnimal(Rectangle rect, Farmer who)
        {
            foreach (var animal in Animals.Values)
            {
                if (!animal.wasPet.Value && animal.GetBoundingBox().Intersects(rect))
                {
                    animal.pet(who);
                    return true;
                }
            }

            return false;
        }

        public override void DayUpdate(int dayOfMonth)
        {
            for (int i = this.animals.Count() - 1; i >= 0; i--)
            {
                var animal = this.animals.Pairs.ElementAt(i).Value;
                animal.dayUpdate(this);
            }
            base.DayUpdate(dayOfMonth);
        }

        public override bool performToolAction(Tool t, int tileX, int tileY)
        {
            if (t is MeleeWeapon)
            {
                foreach (FarmAnimal a in this.animals.Values)
                {
                    if (a.GetBoundingBox().Intersects((t as MeleeWeapon).mostRecentArea))
                    {
                        a.hitWithWeapon(t as MeleeWeapon);
                    }
                }
            }
            return base.performToolAction(t, tileX, tileY);
        }

        public override void performTenMinuteUpdate(int timeOfDay)
        {
            base.performTenMinuteUpdate(timeOfDay);
            if (Game1.IsMasterGame)
            {
                foreach (FarmAnimal value in this.animals.Values)
                {
                    value.updatePerTenMinutes(Game1.timeOfDay, this);
                }
            }
        }

        public override bool isCollidingPosition(Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
        {
            if (!glider)
            {
                if (character != null && !(character is FarmAnimal))
                {
                    Microsoft.Xna.Framework.Rectangle playerBox = Game1.player.GetBoundingBox();
                    Farmer farmer = (isFarmer ? (character as Farmer) : null);
                    foreach (FarmAnimal animal in this.animals.Values)
                    {
                        if (position.Intersects(animal.GetBoundingBox()) && (!isFarmer || !playerBox.Intersects(animal.GetBoundingBox())))
                        {
                            if (farmer != null && farmer.TemporaryPassableTiles.Intersects(position))
                            {
                                break;
                            }
                            animal.farmerPushing();
                            return true;
                        }
                    }
                }
            }
            return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
        }

        public override bool isTileOccupied(Vector2 tileLocation, string characterToIgnore = "", bool ignoreAllCharacters = false)
        {
            foreach (KeyValuePair<long, FarmAnimal> pair in this.animals.Pairs)
            {
                if (pair.Value.getTileLocation().Equals(tileLocation))
                {
                    return true;
                }
            }
            return base.isTileOccupied(tileLocation, characterToIgnore, ignoreAllCharacters);
        }

        public override bool isTileOccupiedForPlacement(Vector2 tileLocation, StardewValley.Object toPlace = null)
        {
            foreach (KeyValuePair<long, FarmAnimal> pair in this.animals.Pairs)
            {
                if (pair.Value.getTileLocation().Equals(tileLocation))
                {
                    return true;
                }
            }
            return base.isTileOccupiedForPlacement(tileLocation, toPlace);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            foreach (KeyValuePair<long, FarmAnimal> pair in this.animals.Pairs)
            {
                pair.Value.draw(b);
            }
        }
        public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
        {
            base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
            if (!Game1.currentLocation.Equals(this))
            {
                NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>.PairsCollection pairs = this.animals.Pairs;
                for (int i = pairs.Count() - 1; i >= 0; i--)
                {
                    pairs.ElementAt(i).Value.updateWhenNotCurrentLocation(null, time, this);
                }
            }
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);
            foreach (KeyValuePair<long, FarmAnimal> kvp in this.Animals.Pairs)
            {
                kvp.Value.updateWhenCurrentLocation(time, this);
            }
        }
    }
}
