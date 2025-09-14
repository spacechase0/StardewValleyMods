namespace SurfingFestival.Framework
{
    internal class UseItemMessage
    {
        public const string Type = nameof(UseItemMessage);
        public SurfItem ItemUsed { get; set; }
    }
}
