namespace SurfingFestival.Framework
{
    internal class UseItemMessage
    {
        public const string Type = nameof(UseItemMessage);
        public Item ItemUsed { get; set; }
    }
}
