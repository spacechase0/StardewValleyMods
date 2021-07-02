// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Locations.AnimalHouse
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using Paritee.StardewValley.Core.Characters;
using Paritee.StardewValley.Core.Objects;
using Paritee.StardewValley.Core.Utilities;
using StardewValley.Buildings;
using StardewValley.Objects;
using SDV = StardewValley;

namespace Paritee.StardewValley.Core.Locations
{
  public class AnimalHouse
  {
    public enum Size
    {
      Small,
      Big,
      Deluxe
    }

    public const string Coop = "Coop";
    public const string Barn = "Barn";
    public const string Incubator = "Incubator";
    public const int DefaultIncubatorItemIndex = 101;
    public const int OstrichIncubatorItemIndex = 254;
    public const int AutoGrabberItemIndex = 165;

    public static string FormatSize(string buildingName, Size size)
    {
      return size.Equals(Size.Small) ? buildingName : size + " " + buildingName;
    }

    public static List<SDV.Object> GetIncubators(SDV.AnimalHouse animalHouse)
    {
      return animalHouse.objects.Values.Where(o => Object.IsIncubator(o))
        .ToList();
    }

    public static SDV.Object GetIncubatorWithEggReadyToHatch(SDV.AnimalHouse animalHouse)
    {
      var incubators = GetIncubators(animalHouse);
      return !incubators.Any() ? null : incubators.FirstOrDefault(o => Object.IsHoldingObject(o) && Object.IsReady(o));
    }

    public static string GetRandomTypeFromIncubator(
      SDV.Object incubator,
      Dictionary<string, List<string>> restrictions)
    {
      return incubator.heldObject.Value == null
        ? null
        : FarmAnimal.GetRandomTypeFromProduce(incubator.heldObject.Value.ParentSheetIndex, restrictions);
    }

    public static void ResetIncubator(SDV.AnimalHouse animalHouse)
    {
      animalHouse.incubatingEgg.X = 0;
      animalHouse.incubatingEgg.Y = -1;
    }

    public static void ResetIncubator(SDV.AnimalHouse animalHouse, SDV.Object incubator)
    {
      incubator.heldObject.Value = null;
      if (animalHouse.getBuilding() is Barn _)
        incubator.ParentSheetIndex = OstrichIncubatorItemIndex;
      else
        incubator.ParentSheetIndex = DefaultIncubatorItemIndex;
      ResetIncubator(animalHouse);
    }

    public static SDV.AnimalHouse GetIndoors(Building building)
    {
      return Reflection.GetFieldValue<NetRef<SDV.GameLocation>>(building, "indoors").Value as SDV.AnimalHouse;
    }

    public static bool IsFull(SDV.AnimalHouse animalHouse)
    {
      return animalHouse.isFull();
    }

    public static bool IsFull(Building building)
    {
      return IsFull(GetIndoors(building));
    }

    public static bool IsEggReadyToHatch(SDV.AnimalHouse animalHouse)
    {
      return animalHouse.incubatingEgg.Y > 0 || animalHouse.incubatingEgg.X - 1 <= 0;
    }

    public static void AddAnimal(Building building, SDV.FarmAnimal animal)
    {
      AddAnimal(GetIndoors(building), animal);
    }

    public static void AddAnimal(SDV.AnimalHouse animalHouse, SDV.FarmAnimal animal)
    {
      animalHouse.animals.Add(animal.myID.Value, animal);
      if (animalHouse.animalsThatLiveHere.Contains(animal.myID.Value))
        return;
      animalHouse.animalsThatLiveHere.Add(animal.myID.Value);
    }

    public static Building GetBuilding(SDV.AnimalHouse animalHouse)
    {
      return animalHouse.getBuilding();
    }

    public static void SetCurrentEvent(SDV.AnimalHouse animalHouse, SDV.Event currentEvent)
    {
      animalHouse.currentEvent = currentEvent;
    }

    public static SDV.Event GetCurrentEvent(SDV.AnimalHouse animalHouse)
    {
      return animalHouse.currentEvent;
    }

    public static SDV.Event GetIncubatorHatchEvent(
      SDV.AnimalHouse animalHouse,
      string message = null)
    {
      return new SDV.Event("none/-1000 -1000/farmer 2 9 0/pause 250/message \"" +
                           (message ?? Content.LoadString(
                             "Strings\\Locations:AnimalHouse_Incubator_Hatch_RegularEgg")) +
                           "\"/pause 500/animalNaming/pause 500/end");
    }

    public static void SetIncubatorHatchEvent(SDV.AnimalHouse animalHouse)
    {
      var incubatorHatchEvent = GetIncubatorHatchEvent(animalHouse);
      SetCurrentEvent(animalHouse, incubatorHatchEvent);
    }

    public static void AutoGrabFromAnimals(SDV.AnimalHouse animalHouse, SDV.Object autoGrabber)
    {
      foreach (var pair in animalHouse.animals.Pairs)
        if (FarmAnimal.IsAProducer(pair.Value) && FarmAnimal.RequiresToolForHarvest(pair.Value) &&
            FarmAnimal.IsCurrentlyProducing(pair.Value) && !FarmAnimal.CanFindProduce(pair.Value) &&
            autoGrabber.heldObject.Value != null && autoGrabber.heldObject.Value is Chest chest)
        {
          SDV.Item obj = new SDV.Object(Vector2.Zero, FarmAnimal.GetCurrentProduce(pair.Value), null, false, true,
            false, false)
          {
            Quality = FarmAnimal.GetProduceQuality(pair.Value)
          };
          if (chest.addItem(obj) == null)
          {
            FarmAnimal.SetCurrentProduce(pair.Value, -1);
            if (FarmAnimal.IsSheared(pair.Value))
              FarmAnimal.ReloadSpriteTexture(pair.Value);
            autoGrabber.showNextIndex.Value = true;
          }
        }
    }

    public static bool AreAnimalDoorsOpen(Building building)
    {
      return building.animalDoorOpen.Value;
    }
  }
}