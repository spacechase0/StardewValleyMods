namespace Magic
{
    public class AnalyzeEventArgs
    {
        /*********
        ** Accessors
        *********/
        public int TargetX;
        public int TargetY;


        /*********
        ** Public methods
        *********/
        public AnalyzeEventArgs(int tx, int ty)
        {
            this.TargetX = tx;
            this.TargetY = ty;
        }
    }
}
