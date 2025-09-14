namespace ManaBar
{
    public class ModConfig
    {
        public bool RenderManaBar { get; set; }

        public int XManaBarOffset { get; set; }

        public int YManaBarOffset { get; set; }

        public float SizeMultiplier { get; set; }

        public ModConfig()
        {
            XManaBarOffset = 0;
            YManaBarOffset = 0;

            RenderManaBar = true;

            SizeMultiplier = 15f;
        }
    }
}
