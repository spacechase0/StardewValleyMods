namespace TheftOfTheWinterStar.Framework
{
    internal class SaveData
    {
        /*********
        ** Accessors
        *********/
        public ArenaStage ArenaStage { get; set; } = ArenaStage.NotTriggered;
        public bool DidProjectilePuzzle { get; set; } = false;
        public bool BeatBoss { get; set; } = false;
    }
}
