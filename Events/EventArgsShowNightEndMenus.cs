namespace SpaceCore.Events
{
    public class EventArgsShowNightEndMenus
    {
        public EventStage Stage { get; set; }
        public bool ProcessShippedItems { get; set; } = true;
    }
}