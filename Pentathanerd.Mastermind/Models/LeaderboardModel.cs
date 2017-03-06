using System;
using System.Collections.Generic;
using System.Linq;

namespace Pentathanerd.Mastermind.Models
{
    public class LeaderboardModel
    {
        public string TeamName { get; set; }
        public double Score { get; set; }

        public int RoundsCompleted { get; set; }

        public TimeSpan Fastest { get; set; }

        public int FewestMoves { get; set; }
    }
}