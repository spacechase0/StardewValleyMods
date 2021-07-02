// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Locations.Event
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using StardewValley.Events;
using SDV = StardewValley;

namespace Paritee.StardewValley.Core.Locations
{
  public class Event
  {
    public const int BlueChicken = 3900074;
    public const int SoundInTheNightAnimalEaten = 2;

    public static SDV.Event GetEventInLocation(SDV.GameLocation location)
    {
      return location.currentEvent;
    }

    public static bool IsEventOccurringInLocation(SDV.GameLocation location)
    {
      return GetEventInLocation(location) != null;
    }

    public static void GoToNextEventCommandInLocation(SDV.GameLocation location)
    {
      if (!IsEventOccurringInLocation(location))
        return;
      ++GetEventInLocation(location).CurrentCommand;
    }

    public static bool TryGetFarmEvent<T>(out T farmEvent)
    {
      farmEvent = default;
      if (SDV.Game1.farmEvent == null || !(SDV.Game1.farmEvent is T))
        return false;
      farmEvent = (T) SDV.Game1.farmEvent;
      return true;
    }

    public static bool IsFarmEventOccurring<T>(out T farmEvent)
    {
      return TryGetFarmEvent(out farmEvent);
    }

    public static void ForceQuestionEventToProceed(QuestionEvent questionEvent)
    {
      questionEvent.forceProceed = true;
    }
  }
}