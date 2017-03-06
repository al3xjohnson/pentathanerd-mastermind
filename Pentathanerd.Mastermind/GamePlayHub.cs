using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using Microsoft.AspNet.SignalR;

namespace Pentathanerd.Mastermind
{
    public class GamePlayHub : Hub
    {
        #region StuffAndThings
        private static IHubContext GamePlayHubContext => GlobalHost.ConnectionManager.GetHubContext<GamePlayHub>();
        private static readonly List<string> _connectedUsers = new List<string>();
        private static ConcurrentDictionary<string, PlayerStats> _connectedPlayers = new ConcurrentDictionary<string, PlayerStats>();
        private static Random _random;
        private static bool _multiplayerGameIsActive;

        private static List<LevelConfiguration> LevelConfigurations => LevelConfigurationDictionary.LevelConfigurations;
        private static TimerExtension GameClock { get; set; }

        private static List<LeaderboardStats> LeaderboardStats = new List<LeaderboardStats>();

        private static Random Random
        {
            get
            {
                if (_random == null)
                {
                    _random = new Random();
                }
                return _random;
            }
        }

        private static List<ChallengeColor> _challengeColors;
        private static List<ChallengeColor> ChallengeColors
        {
            get
            {
                if (_challengeColors == null)
                {
                    _challengeColors = new ChallengeGenerator(LevelConfigurations).ChallengeColors;
                }
                return _challengeColors;
            }
        }

        private static List<string> PlayerConnectionIds
        {
            get { return _connectedPlayers.Select(x => x.Value.ConnectionId).ToList(); }
        }

        private static GameMode GameMode => GameConfiguration.GameMode;
        #endregion

        public override Task OnConnected()
        {
            _connectedUsers.Add(Context.ConnectionId);

            if (GameMode == GameMode.SinglePlayer)
            {
                RegisterPlayer();
                Clients.Caller.enableTeamRegistration();
            }
            else
            {
                if (_connectedPlayers.Count == 0 && (GameMode == GameMode.Multiplayer))
                {
                    _multiplayerGameIsActive = false;
                }

                if (_multiplayerGameIsActive)
                {
                    UpdateViewingArea();
                }
                else
                {
                    if (GameMode == GameMode.Multiplayer)
                    {
                        Clients.Caller.enableTeamRegistration();
                    }
                    else
                    {
                        RegisterPlayer();
                    }
                }
            }

            Clients.All.updateConnectedCount(_connectedUsers.Count, _connectedPlayers.Count);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var connectionId = Context.ConnectionId;
            var player = GetPlayer(connectionId);

            if (player != null)
            {
                PlayerStats removedPlayer;
                _connectedPlayers.TryRemove(player.ConnectionId, out removedPlayer);
                var leaderboardStats = LeaderboardStats.FirstOrDefault(x => x.TeamName == player.Username);
                if (leaderboardStats != null)
                {
                    leaderboardStats.Challenges.AddRange(player.Challenges);
                }
                else
                {
                    LeaderboardStats.Add(new LeaderboardStats
                    {
                        TeamName = player.Username,
                        Challenges = player.Challenges
                    });
                }
            }
            _connectedUsers.Remove(connectionId);

            if (_connectedPlayers.Count == 0 && GameMode == GameMode.Multiplayer)
            {
                _multiplayerGameIsActive = false;
                EndGame();
            }

            Clients.All.updateConnectedCount(_connectedUsers.Count, _connectedPlayers.Count);

            return base.OnDisconnected(stopCalled);
        }

        public void RegisterTeam(string teamName)
        {
            if (GameMode == GameMode.SinglePlayer)
            {
                var connectionId = Context.ConnectionId;
                var playerStats = GetPlayer(connectionId);

                playerStats.Username = teamName;
                Clients.Caller.updateTeamName(teamName);
                Clients.Caller.enableStartButton();
            }
            else
            {
                if (!_connectedPlayers.Values.Any(x => x.Username == teamName))
                {
                    RegisterPlayer();
                    var connectionId = Context.ConnectionId;
                    var playerStats = GetPlayer(connectionId);

                    playerStats.Username = teamName;
                    Clients.Caller.updateTeamName(teamName);
                    Clients.All.updateConnectedCount(_connectedUsers.Count, _connectedPlayers.Count);
                }
                else
                {
                    Clients.Caller.enableTeamRegistration("Team has already registered.");
                }
            }
        }

        public void ReadyUpPlayer()
        {
            var connectionId = Context.ConnectionId;
            var playerStats = GetPlayer(connectionId);

            if (playerStats == null)
                return;

            if (_multiplayerGameIsActive)
                return;

            playerStats.Ready = true;

            if (_connectedPlayers.Values.Count(x => x.Ready) == _connectedPlayers.Values.Count)
            {
                StartGame();
            }
        }

        public void RegisterPlayer()
        {
            var connectionId = Context.ConnectionId;
            var playerStats = new PlayerStats
            {
                ConnectionId = connectionId,
                Challenges = new List<Challenge> { CreateChallenge(new PlayerStats()) }
            };
            _connectedPlayers.TryAdd(connectionId, playerStats);

            if (GameMode == GameMode.Multiplayer)
            {
                if (_connectedPlayers.Count >= GameConfiguration.NumberOfTeams)
                {
                    Clients.Clients(PlayerConnectionIds).enableReadyButton();
                }
            }
        }

        public void StartGame()
        {
            if (!_connectedPlayers.Keys.Contains(Context.ConnectionId))
                return;

            if (GameMode == GameMode.Multiplayer)
            {
                if (_multiplayerGameIsActive)
                    return;

                if (_connectedPlayers.Count(x => x.Value.Ready) == _connectedPlayers.Count && !_multiplayerGameIsActive)
                {
                    StartMultiPlayerGame();
                }
            }
            else
            {
                var connectionId = Context.ConnectionId;
                var playerStats = GetPlayer(connectionId);
                StartSinglePlayerGame(playerStats);
            }
        }

        private void StartMultiPlayerGame()
        {
            _multiplayerGameIsActive = true;
            var now = DateTime.Now;
            var challengeEndTime = now.AddMinutes(GameConfiguration.ChallengeTime);
            var gameEndTime = now.AddMinutes(GameConfiguration.GameTime);

            var levelConfiguration = LevelConfigurations.First(x => x.Level == 1);
            Clients.Clients(PlayerConnectionIds).startGame(gameEndTime, challengeEndTime, levelConfiguration.Level, levelConfiguration.AvailableColorCount, levelConfiguration.DuplicatesAllowed, levelConfiguration.AvailableGuesses);

            foreach (var playerStats in _connectedPlayers.Values)
            {
                playerStats.Challenges = new List<Challenge> { CreateChallenge(new PlayerStats()) };
                playerStats.Ready = false;
                InitializePlayerClock(playerStats, challengeEndTime);
                playerStats.PlayClock.Start();
            }

            InitializeGameClock(gameEndTime);
            GameClock.Start();
        }

        private void StartSinglePlayerGame(PlayerStats playerStats)
        {
            var now = DateTime.Now;
            var challengeEndTime = now.AddMinutes(GameConfiguration.ChallengeTime);

            var levelConfiguration = LevelConfigurations.OrderBy(x => x.Level).FirstOrDefault();
            Clients.Caller.startGame(0, challengeEndTime, levelConfiguration.Level, levelConfiguration.AvailableColorCount, levelConfiguration.DuplicatesAllowed, levelConfiguration.AvailableGuesses);

            playerStats.Challenges = new List<Challenge> { CreateChallenge(new PlayerStats()) };
            playerStats.Ready = false;
            InitializePlayerClock(playerStats, challengeEndTime);
            playerStats.PlayClock.Start();
        }

        private static void InitializeGameClock(DateTime? endTime = null)
        {
            if (endTime == null)
            {
                var gameTime = GetGameTime();
                GameClock = new TimerExtension(gameTime.TotalMilliseconds);
            }
            else
            {
                GameClock = new TimerExtension(endTime.Value);
            }
            
            GameClock.Elapsed += GameClockOnElapsed;
        }

        private static void GameClockOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            EndGame();
        }

        private static void InitializePlayerClock(PlayerStats player, DateTime? endTime = null)
        {
            TimerExtension gameClock;
            if (endTime == null)
            {
                var gameTime = GetChallengeTime();
                gameClock = new TimerExtension(gameTime.TotalMilliseconds);
            }
            else
            {
                gameClock = new TimerExtension(endTime.Value);
            }
            
            gameClock.Elapsed += (sender, args) => PlayerClockOnElapsed(sender, args, player);

            player.PlayClock = gameClock;
        }

        private static void PlayerClockOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs, PlayerStats player)
        {
            if (player.PlayClock != null)
            {
                player.PlayClock.Stop();
                player.PlayClock.Dispose();
                player.PlayClock = null;
            }

            GamePlayHubContext.Clients.Client(player.ConnectionId).playerClockGameOver();

            var activeChallenge = player.Challenges.FirstOrDefault(x => x.ActiveChallenge);
            if (activeChallenge != null)
            {
                var answer = activeChallenge.ChallengeColor.Colors.Select(color => color.ToString()).ToList();
                GamePlayHubContext.Clients.Client(player.ConnectionId).showAnswer(answer);
            }

            var leaderboardStats = LeaderboardStats.FirstOrDefault(x => x.TeamName == player.Username);
            if (leaderboardStats != null)
            {
                leaderboardStats.Challenges.AddRange(player.Challenges);
            }
            else
            {
                LeaderboardStats.Add(new LeaderboardStats
                {
                    TeamName = player.Username,
                    Challenges = player.Challenges
                });
            }

            foreach (var challenge in player.Challenges.Where(x => x.ActiveChallenge))
            {
                challenge.ActiveChallenge = false;
            }

            if (GameMode.SinglePlayer == GameConfiguration.GameMode)
            {
                GamePlayHubContext.Clients.Client(player.ConnectionId).enableStartButton();
            }
            else
            {
                var connectedPlayers = _connectedPlayers.Values;
                var activeChallenges = false;
                foreach (var connectedPlayer in connectedPlayers)
                {
                    if (connectedPlayer.Challenges.Any(x => x.ActiveChallenge))
                    {
                        activeChallenges = true;
                    }
                }

                if (!activeChallenges)
                {
                    EndGame();
                }
            }
        }

        private static void EndGame()
        {
            _multiplayerGameIsActive = false;
            StopGameClock();
            if (_connectedPlayers.Count >= GameConfiguration.NumberOfTeams)
            {
                GamePlayHubContext.Clients.Clients(PlayerConnectionIds).enableReadyButton();
            }
            GamePlayHubContext.Clients.All.mainClockGameOver();
        }

        private static void StopGameClock()
        {
            if (GameClock != null)
            {
                GameClock.Stop();
                GameClock.Dispose();
                GameClock.Elapsed -= GameClockOnElapsed;
            }
        }

        public static TimeSpan GetChallengeTime()
        {
            var challengeTimeInSeconds = GameConfiguration.ChallengeTime * 60.0 * 1000;
            return new TimeSpan(0, 0, 0, 0, Convert.ToInt32(challengeTimeInSeconds));
        }

        public static TimeSpan GetGameTime()
        {
            var challengeTimeInSeconds = GameConfiguration.GameTime * 60.0 * 1000;
            return new TimeSpan(0, 0, 0, 0, Convert.ToInt32(challengeTimeInSeconds));
        }

        public void SubmitGuess(string guessString)
        {
            var connectionId = Context.ConnectionId;
            var player = GetPlayer(connectionId);

            if (player == null)
                return;

            if (GameMode == GameMode.Multiplayer)
            {
                if (!_multiplayerGameIsActive)
                    return;
            }

            if (player.Challenges.Count(x => x.ActiveChallenge) == 0)
            {
                return;
            }

            lock (player)
            {
                var playerClockEndTime = player.PlayClock.EndTime;
                var updatedEndTime = playerClockEndTime;
                player.PlayClock.Stop();

                var guess = ConvertGuess(guessString);
                var solved = false;
                var gameOver = false;
                var challengeResult = player.Challenges.Where(x => x.ActiveChallenge).Select(x =>
                {
                    var scoreCalculator = new ScoreCalculator(x);
                    var challenge = scoreCalculator.CalculateScore(guess);
                    solved = challenge.Solved;
                    gameOver = !challenge.ActiveChallenge;
                    return challenge;
                }).FirstOrDefault();

                if (challengeResult != null)
                {
                    var levelConfiguration = GetLevelConfiguration(player.Level);
                    if (!challengeResult.ActiveChallenge)
                    {
                        if (challengeResult.Solved)
                            updatedEndTime = playerClockEndTime.AddSeconds(GameConfiguration.SecondsToAddAfterChallengeSolved);

                        var level = challengeResult.Solved ? player.Level - 1 : player.Level;
                        Clients.Caller.createChallengeHistory(player.Challenges.Count, level, challengeResult.Score, challengeResult.Guesses.Count, (challengeResult.EndTime - challengeResult.StartTime).TotalSeconds, challengeResult.Solved);

                        var newChallenge = CreateChallenge(player);
                        player.Challenges.Add(newChallenge);

                        if (!challengeResult.Solved)
                        {
                            var answer = challengeResult.ChallengeColor.Colors.Select(color => color.ToString()).ToList();
                            Clients.Client(player.ConnectionId).showAnswer(answer);
                        }
                        else
                        {
                            levelConfiguration = GetLevelConfiguration(player.Level);
                            if (levelConfiguration.ChallengeSize > 4)
                            {
                                Clients.Caller.addFifthColor();
                            }
                        }
                    }

                    if (GameMode == GameMode.Multiplayer)
                    {
                        if (GameClock.EndTime < updatedEndTime)
                        {
                            updatedEndTime = GameClock.EndTime;
                        }
                    }

                    Clients.Caller.updateGame(challengeResult.GuessResponseses.Count, challengeResult.GuessResponseses.Last(), gameOver, solved, player.Score, updatedEndTime, player.Challenges.Count(x => x.Solved), player.Challenges.Count(x => !x.Solved && !x.ActiveChallenge), player.Level, levelConfiguration.AvailableColorCount, levelConfiguration.DuplicatesAllowed, levelConfiguration.AvailableGuesses);
                    InitializePlayerClock(player, updatedEndTime);
                    player.PlayClock.Start();
                    UpdateViewingArea();
                }
            }
        }

        private List<string> GetPossibleColors(int availableColorCount)
        {
            var retValue = new List<string>();
            var enumCount = Enum.GetValues(typeof(Color)).Length - 1;

            if (availableColorCount > enumCount)
            {
                availableColorCount = enumCount;
            }

            for (var i = 1; i <= availableColorCount; i++)
            {
                retValue.Add(((Color)i).ToString());
            }

            return retValue;
        }

        private Challenge CreateChallenge(PlayerStats player)
        {
            var levelConfiguration = GetLevelConfiguration(player.Level);
            var challengeCount = ChallengeColors.Count(x => !x.Used && x.Colors.Length == levelConfiguration.ChallengeSize && x.DuplicateColors == levelConfiguration.DuplicatesAllowed && x.AvailableColors == levelConfiguration.AvailableColorCount);
            if (challengeCount == 0)
            {
                ResetChallengeColors(levelConfiguration);
            }
            var retValue = new Challenge(ChallengeColors, Random, levelConfiguration);

            var possibleColors = GetPossibleColors(levelConfiguration.AvailableColorCount);
            Clients.Caller.updateChallengeColors(possibleColors);
            return retValue;
        }

        private LevelConfiguration GetLevelConfiguration(int level)
        {
            var levelConfiguration = LevelConfigurations.FirstOrDefault(x => x.Level == level);
            if (levelConfiguration == null)
            {
                levelConfiguration = LevelConfigurations.OrderByDescending(x => x.Level).First();
            }
            return levelConfiguration;
        }

        private void ResetChallengeColors(LevelConfiguration levelConfiguration)
        {
            foreach (var challengeColor in ChallengeColors.Where(x=> x.Colors.Length == levelConfiguration.ChallengeSize && x.DuplicateColors == levelConfiguration.DuplicatesAllowed && x.AvailableColors == levelConfiguration.AvailableColorCount))
            {
                challengeColor.Used = false;
            }
        }

        private static Color[] ConvertGuess(string guess)
        {
            var guessesSplit = guess.Split(',');
            var retVal = new Color[guessesSplit.Length];
            for (var i = 0; i < guessesSplit.Length; i++)
            {
                var color = ToColor(guessesSplit[i]);
                retVal[i] = color;
            }

            return retVal;
        }

        private static Color ToColor(string s)
        {
            switch (s.ToLower().Trim())
            {
                case "red":
                    return Color.Red;
                case "green":
                    return Color.Green;
                case "yellow":
                    return Color.Yellow;
                case "white":
                    return Color.White;
                case "black":
                    return Color.Black;
                case "blue":
                    return Color.Blue;
                case "orange":
                    return Color.Orange;
                case "pink":
                    return Color.Pink;
                default:
                    return Color.Unknown;
            }
        }

        private static PlayerStats GetPlayer(string connectionId)
        {
            var connectedPlayer = _connectedPlayers.FirstOrDefault(x => x.Key == connectionId);
            return connectedPlayer.Value;
        }

        public static List<LeaderboardStats> GetLeaderboard()
        {
            var currentLeaderboardStats = new List<LeaderboardStats>();
            currentLeaderboardStats.AddRange(ConvertActivePlayersToStats());
            currentLeaderboardStats.AddRange(LeaderboardStats);
            return currentLeaderboardStats;
        }

        private static List<LeaderboardStats> ConvertActivePlayersToStats()
        {
            var retValue = new List<LeaderboardStats>();

            foreach (var connectedPlayer in _connectedPlayers.Values)
            {
                var leaderboardStats = new LeaderboardStats
                {
                    TeamName = connectedPlayer.Username,
                    Challenges = connectedPlayer.Challenges
                };
                retValue.Add(leaderboardStats);
            }
            return retValue;
        }

        public static void ClearLeaderboardStats()
        {
            LeaderboardStats = new List<LeaderboardStats>();
        }

        public static void HardReset()
        {
            ClearLeaderboardStats();
            _connectedPlayers = new ConcurrentDictionary<string, PlayerStats>();
            _multiplayerGameIsActive = true;
        }

        private static void UpdateViewingArea()
        {
            var scoreLeaderboard = BuildScoreLeaderBoard();
            var roundsLeaderboard = BuildRoundsLeaderBoard();
            var fastestLeaderboard = BuildFastestLeaderBoard();
            var fewestLeaderboard = BuildFewestLeaderBoard();

            GamePlayHubContext.Clients.AllExcept(PlayerConnectionIds.ToArray()).updateViewingArea(scoreLeaderboard, roundsLeaderboard, fastestLeaderboard, fewestLeaderboard);
        }

        private static List<LeaderboardStat> BuildScoreLeaderBoard()
        {
            var currentPlayerActiveStats = ConvertActivePlayersToStats();

            var retValue = new List<LeaderboardStat>();
            var rank = 1;
            foreach (var activePlayer in currentPlayerActiveStats.OrderByDescending(x => x.Challenges.Sum(y => y.Score)))
            {
                var leaderboardStat = new LeaderboardStat
                {
                    Rank = rank,
                    Name = activePlayer.TeamName,
                    Value = activePlayer.Challenges.Sum(x => x.Score).ToString()
                };
                retValue.Add(leaderboardStat);
                rank++;
            }
            return retValue;
        }

        private static List<LeaderboardStat> BuildRoundsLeaderBoard()
        {
            var currentPlayerActiveStats = ConvertActivePlayersToStats();

            var retValue = new List<LeaderboardStat>();
            var rank = 1;
            foreach (var activePlayer in currentPlayerActiveStats.OrderByDescending(x => x.Challenges.Count(y => y.Solved)))
            {
                var leaderboardStat = new LeaderboardStat
                {
                    Rank = rank,
                    Name = activePlayer.TeamName,
                    Value = activePlayer.Challenges.Count(x => x.Solved).ToString()
                };
                retValue.Add(leaderboardStat);
                rank++;
            }
            return retValue;
        }

        private static List<LeaderboardStat> BuildFastestLeaderBoard()
        {
            var currentPlayerActiveStats = ConvertActivePlayersToStats();

            var retValue = currentPlayerActiveStats.Select(activePlayer => new LeaderboardStat
            {
                Name = activePlayer.TeamName,
                Value = CalculateFastestTime(activePlayer.Challenges)
            }).ToList();

            var rank = 1;
            foreach (var leaderboardStat in retValue.OrderBy(x => ((TimeSpan)x.Value).TotalSeconds))
            {
                leaderboardStat.Rank = rank;
                leaderboardStat.Value = ((TimeSpan)leaderboardStat.Value).ToString(@"mm\:ss");
                rank++;
            }
            return retValue.OrderBy(x => x.Rank).ToList();
        }

        private static TimeSpan CalculateFastestTime(List<Challenge> challenges)
        {
            var timeSpans = new List<TimeSpan>();
            foreach (var challenge in challenges.Where(x => x.Solved))
            {
                timeSpans.Add(new TimeSpan(0, 0, 0, Convert.ToInt32((challenge.EndTime - challenge.StartTime).TotalSeconds)));
            }
            return timeSpans.OrderBy(x => x.TotalSeconds).FirstOrDefault();
        }

        private static List<LeaderboardStat> BuildFewestLeaderBoard()
        {
            var currentPlayerActiveStats = ConvertActivePlayersToStats();

            var retValue = currentPlayerActiveStats.Select(activePlayer => new LeaderboardStat
            {
                Name = activePlayer.TeamName,
                Value = activePlayer.Challenges.Count(x => x.Solved) > 0 ? activePlayer.Challenges.Where(x => x.Solved).Min(y => y.TotalGuessCount) : 0
            }).ToList();

            var rank = 1;
            foreach (var source in retValue.OrderBy(x =>x.Value))
            {
                source.Rank = rank;
                rank++;
            }
            return retValue;
        }
    }

    [Serializable]
    public class LeaderboardStat
    {
        public int Rank { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }
    }
}