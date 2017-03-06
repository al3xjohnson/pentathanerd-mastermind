namespace Pentathanerd.Mastermind
{
    public class LevelConfiguration
    {
        public int Level { get; set; }

        public int ChallengeSize { get; set; }

        public int AvailableColorCount { get; set; }

        public bool DuplicatesAllowed { get; set; }

        public int AvailableGuesses { get; set; }
    }
}