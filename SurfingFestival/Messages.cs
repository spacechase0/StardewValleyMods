namespace SurfingFestival
{
    public class UseItemMessage
    {
        public const string TYPE = nameof(UseItemMessage);
        public Item ItemUsed { get; set; }
    }
}
