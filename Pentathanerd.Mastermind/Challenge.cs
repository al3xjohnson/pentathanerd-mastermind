using System;
using System.Collections.Generic;
using System.Linq;

namespace Pentathanerd.Mastermind
{
    public class Challenge
    {
        private readonly List<ChallengeColor> _challengeColors;
        private static Random _random;

        public Challenge(List<ChallengeColor> challengeColors, Random random, LevelConfiguration levelConfiguration)
        {
            _challengeColors = challengeColors;
            _random = random;

            Guesses = new List<Color[]>();
            GuessResponseses = new List<List<GuessResponse>>();
            ChallengeColor = GenerateChallengeColors(levelConfiguration.ChallengeSize, levelConfiguration.AvailableColorCount, levelConfiguration.DuplicatesAllowed);
            ActiveChallenge = true;
            StartTime = DateTime.Now;
            AvailableGuesses = levelConfiguration.AvailableGuesses;
        }

        private ChallengeColor GenerateChallengeColors(int challengeSize, int availableColorCount, bool duplicates)
        {
            var challengeColor = new ChallengeColor();
            if (_challengeColors.Any(x => x.Colors.Length == challengeSize))
            {
                do
                {
                    var randomIndex = _random.Next(_challengeColors.Count);
                    challengeColor = _challengeColors[randomIndex];
                } while (challengeColor.Used || challengeColor.Colors.Length != challengeSize || challengeColor.DuplicateColors != duplicates || challengeColor.AvailableColors != availableColorCount);

                challengeColor.Used = true;
            }
            
            return challengeColor;
        }

        public int AvailableGuesses { get; set; }

        public ChallengeColor ChallengeColor { get; set; }

        public List<Color[]> Guesses { get; set; }

        public List<List<GuessResponse>> GuessResponseses { get; set; }

        public int CorrectColorAndPositionGuessCount { get; set; }

        public int CorrectColorGuessCount { get; set; }

        public int IncorrectGuessCount { get; set; }

        public int TotalGuessCount { get; set; }

        public double Score { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        private bool _activeChallenge;
        public bool ActiveChallenge {
            get { return _activeChallenge; }
            set
            {
                if (!value)
                {
                    this.EndTime = DateTime.Now;
                }
                _activeChallenge = value;
            }
        }

        public bool Solved { get; set; }
    }
}