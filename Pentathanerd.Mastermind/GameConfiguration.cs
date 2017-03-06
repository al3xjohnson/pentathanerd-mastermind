using System;
using System.Configuration;

namespace Pentathanerd.Mastermind
{
    internal static class GameConfiguration
    {
        private const double DefaultChallengeTimeInMinutes = 3;
        private const double DefaultGameTimeInMinutes = 5;
        private const double DefualtMinimumScore = 300;
        private const double DefualtIncompleteChallengeBonus = 0;
        private const double DefualtIncompleteRoundBonus = -100;
        private const double DefualtRoundBonus = 50;
        private const double DefualtRoundMultiplier = 20;
        private const int DefaultCorrectColorAndPositionScore = 0;
        private const int DefaultCorrectColorScore = 0;
        private const int DefaultIncorrectGuessScore = 0;
        private const int DefaultNumberOfTeams = 2;
        private const int DefaultMinimumColorCount = 4;
        private const int DefaultMaximumColorCount = 8;
        private const int DefaultTimeBonusUpperBound = 360;
        private const double DefaultTimeBonus = .1;
        private const double DefaultSecondsToAddAfterChallengeSolved = 60;
        public const GameMode DefaultGameMode = GameMode.SinglePlayer;

        public static double ChallengeTime {
            get
            {
                var appSetting = GetAppSetting("ChallengeTime");
                return string.IsNullOrEmpty(appSetting) ? DefaultChallengeTimeInMinutes : Convert.ToDouble(appSetting);
            }
        }

        public static double GameTime {
            get
            {
                var appSetting = GetAppSetting("GameTime");
                return string.IsNullOrEmpty(appSetting) ? DefaultGameTimeInMinutes : Convert.ToDouble(appSetting);
            }
        }

        public static double MinimumScore {
            get
            {
                var appSetting = GetAppSetting("MinimumScore");
                return string.IsNullOrEmpty(appSetting) ? DefualtMinimumScore : Convert.ToDouble(appSetting);
            }
        }

        public static int CorrectColorAndPositionScore {
            get
            {
                var appSetting = GetAppSetting("CorrectColorAndPositionScore");
                return string.IsNullOrEmpty(appSetting) ? DefaultCorrectColorAndPositionScore : Convert.ToInt32(appSetting);
            }
        }

        public static int CorrectColorScore
        {
            get
            {
                var appSetting = GetAppSetting("CorrectColorScore");
                return string.IsNullOrEmpty(appSetting) ? DefaultCorrectColorScore : Convert.ToInt32(appSetting);
            }
        }

        public static int IncorrectGuessScore {
            get
            {
                var appSetting = GetAppSetting("IncorrectGuessCount");
                return string.IsNullOrEmpty(appSetting) ? DefaultIncorrectGuessScore : Convert.ToInt32(appSetting);
            }
        }

        public static int NumberOfTeams {
            get
            {
                var appSetting = GetAppSetting("IncorrectGuessCount");
                return string.IsNullOrEmpty(appSetting) ? DefaultNumberOfTeams : Convert.ToInt32(appSetting);
            }
        }

        public static GameMode GameMode {
            get
            {
                var appSetting = GetAppSetting("GameMode");
                if (!string.IsNullOrEmpty(appSetting))
                {
                    GameMode gameMode;
                    if (Enum.TryParse(appSetting, true, out gameMode))
                    {
                        return gameMode;
                    }
                }
                return DefaultGameMode;
            }
        }

        public static int MinimumColorCount {
            get
            {
                var appSetting = GetAppSetting("MinimumGuessCount");
                return string.IsNullOrEmpty(appSetting) ? DefaultMinimumColorCount : Convert.ToInt32(appSetting);
            }
        }

        public static int MaximumColorCount
        {
            get
            {
                var appSetting = GetAppSetting("MaximumGuessCount");
                return string.IsNullOrEmpty(appSetting) ? DefaultMaximumColorCount : Convert.ToInt32(appSetting);
            }
        }

        public static double IncompleteRoundBonus
        {
            get
            {
                var appSetting = GetAppSetting("IncompleteRoundBonus");
                return string.IsNullOrEmpty(appSetting) ? DefualtIncompleteRoundBonus : Convert.ToDouble(appSetting);
            }
        }

        public static double IncompleteChallengeBonus
        {
            get
            {
                var appSetting = GetAppSetting("IncompleteRoundBonus");
                return string.IsNullOrEmpty(appSetting) ? DefualtIncompleteChallengeBonus : Convert.ToDouble(appSetting);
            }
        }

        public static double RoundBonus {
            get
            {
                var appSetting = GetAppSetting("RounBonus");
                return string.IsNullOrEmpty(appSetting) ? DefualtRoundBonus : Convert.ToDouble(appSetting);
            }
        }
        public static double RoundMulitiplier {
            get
            {
                var appSetting = GetAppSetting("RoundMultiplier");
                return string.IsNullOrEmpty(appSetting) ? DefualtRoundMultiplier : Convert.ToDouble(appSetting);
            }
        }

        public static int TimeBonusUpperBound {
            get
            {
                var appSetting = GetAppSetting("TimeBonusUpperBound");
                return string.IsNullOrEmpty(appSetting) ? DefaultTimeBonusUpperBound : Convert.ToInt32(appSetting);
            }
        }

        public static double TimeBonus
        {
            get
            {
                var appSetting = GetAppSetting("TimeBonus");
                return string.IsNullOrEmpty(appSetting) ? DefaultTimeBonus : Convert.ToDouble(appSetting);
            }
        }

        public static double SecondsToAddAfterChallengeSolved {
            get
            {
                var appSetting = GetAppSetting("SecondsToAddAfterChallengeSolved");
                return string.IsNullOrEmpty(appSetting) ? DefaultSecondsToAddAfterChallengeSolved : Convert.ToDouble(appSetting);
            }
        }

        private static string GetAppSetting(string settingName)
        {
            string retValue = null;
            if (ConfigurationManager.AppSettings[settingName] != null)
            {
                retValue = ConfigurationManager.AppSettings[settingName];
            }
            return retValue;
        }
    }
}