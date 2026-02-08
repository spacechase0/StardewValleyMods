namespace Microsoft.Xna.Framework.Audio;

public static class SoundEffectInstanceExtensions
{
    extension(SoundEffectInstance instance)
    {
        public bool IsLooped
        {
            get => instance.LoopCount == 255u;
            set => instance.LoopCount = value ? 255u : 0u;
        }
    }
}
