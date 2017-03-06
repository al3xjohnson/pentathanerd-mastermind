using System;
using System.Collections.Generic;

namespace Pentathanerd.Mastermind
{
    internal class PlayerStats
    {
        public PlayerStats()
        {
            Challenges = new List<Challenge>();
        }

        private string _username;
        public string Username
        {
            get
            {
                if (string.IsNullOrEmpty(_username))
                {
                    return ConnectionId;
                }
                return _username;
            }
            set { _username = value; }
        }

        public string ConnectionId { get; set; }

        public List<Challenge> Challenges { get; set; }

        public TimerExtension PlayClock { get; set; }

        public double Score => CalculateScore();

        public bool Ready { get; set; }

        public int Level => CalculateLevel();

        private int CalculateLevel()
        {
            var solvedCount = 0;
            foreach (var challenge in Challenges)
            {
                if (challenge.Solved)
                {
                    solvedCount++;
                }
            }
            return solvedCount + 1;
        }

        private double CalculateScore()
        {
            var retValue = 0.0;
            foreach (var challenge in Challenges)
            {
                retValue += challenge.Score;
            }

            return retValue;
        }
    }
}