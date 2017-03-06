using System;
using System.Collections.Generic;
using System.Linq;

namespace Pentathanerd.Mastermind
{
    public class ChallengeColor
    {
        public bool DuplicateColors { get; set; }

        public int AvailableColors { get; set; }

        public Color[] Colors { get; set; }

        public bool Used { get; set; }
    }
}