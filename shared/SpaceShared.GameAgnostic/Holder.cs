#if !DEPENDENCY_HAS_SPACESHARED
namespace SpaceShared
{
    public class Holder<T>
    {
        public T Value;

        public Holder() { }

        public Holder(T value)
        {
            this.Value = value;
        }
    }
}
#endif
