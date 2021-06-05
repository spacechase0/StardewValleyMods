namespace Magic
{
    public class AnalyzeEventArgs
    {
        public int TargetX;
        public int TargetY;

        public AnalyzeEventArgs(int tx, int ty)
        {
            TargetX = tx;
            TargetY = ty;
        }
    }
}
