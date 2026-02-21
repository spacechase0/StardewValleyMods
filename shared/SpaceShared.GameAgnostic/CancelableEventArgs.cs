#if !DEPENDENCY_HAS_SPACESHARED
namespace SpaceShared
{
    public class CancelableEventArgs
    {
        public bool Cancel { get; set; } = false;
    }
}
#endif
