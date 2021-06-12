namespace SurfingFestival
{
    public class UseItemMessage
    {
        public const string Type = nameof(UseItemMessage);
        public Item ItemUsed { get; set; }
    }
}
