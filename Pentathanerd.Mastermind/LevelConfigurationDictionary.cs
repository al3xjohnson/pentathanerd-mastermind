using System.Collections.Generic;

namespace Pentathanerd.Mastermind
{
    internal static class LevelConfigurationDictionary
    {
        private static List<LevelConfiguration> _levelConfigurations;
        public static List<LevelConfiguration> LevelConfigurations
        {
            get
            {
                if (_levelConfigurations == null)
                {
                    _levelConfigurations = new List<LevelConfiguration>
                    {
                        new LevelConfiguration
                        {
                            Level = 1,
                            ChallengeSize = 4,
                            AvailableColorCount = 4,
                            DuplicatesAllowed = false,
                            AvailableGuesses = 10
                        },
                        new LevelConfiguration
                        {
                            Level = 2,
                            ChallengeSize = 4,
                            AvailableColorCount = 5,
                            DuplicatesAllowed = false,
                            AvailableGuesses = 10
                        },
                        new LevelConfiguration
                        {
                            Level = 3,
                            ChallengeSize = 4,
                            AvailableColorCount = 6,
                            DuplicatesAllowed = false,
                            AvailableGuesses = 10
                        },
                        new LevelConfiguration
                        {
                            Level = 4,
                            ChallengeSize = 4,
                            AvailableColorCount = 6,
                            DuplicatesAllowed = true,
                            AvailableGuesses = 10
                        },
                        new LevelConfiguration
                        {
                            Level = 5,
                            ChallengeSize = 4,
                            AvailableColorCount = 7,
                            DuplicatesAllowed = true,
                            AvailableGuesses = 10
                        },
                        new LevelConfiguration
                        {
                            Level = 6,
                            ChallengeSize = 4,
                            AvailableColorCount = 8,
                            DuplicatesAllowed = true,
                            AvailableGuesses = 10
                        },
                        new LevelConfiguration
                        {
                            Level = 7,
                            ChallengeSize = 5,
                            AvailableColorCount = 8,
                            DuplicatesAllowed = true,
                            AvailableGuesses = 15
                        }
                    };
                }
                return _levelConfigurations;
            }
        }
    }
}