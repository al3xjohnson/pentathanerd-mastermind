using System;
using System.Collections.Generic;
using System.Linq;

namespace Pentathanerd.Mastermind
{
    public class ChallengeGenerator
    {
        private readonly List<LevelConfiguration> _levelConfigurations;
        public ChallengeGenerator(List<LevelConfiguration> levelConfigurations)
        {
            _levelConfigurations = levelConfigurations;
        }

        private List<ChallengeColor> _challengeColors;
        public List<ChallengeColor> ChallengeColors
        {
            get
            {
                if (_challengeColors == null)
                {
                    _challengeColors = new List<ChallengeColor>();
                    var enumCount = Enum.GetValues(typeof(Color)).Length - 1;

                    foreach (var levelConfiguration in _levelConfigurations)
                    {
                        var colorCount = levelConfiguration.AvailableColorCount;
                        if (levelConfiguration.AvailableColorCount < GameConfiguration.MinimumColorCount)
                        {
                            colorCount = GameConfiguration.MinimumColorCount;
                        }
                        else if (levelConfiguration.AvailableColorCount > GameConfiguration.MaximumColorCount)
                        {
                            colorCount = GameConfiguration.MaximumColorCount;
                        }

                        if (colorCount > enumCount)
                        {
                            colorCount = enumCount;
                        }

                        _challengeColors.AddRange(GenerateChallengeColors(colorCount, levelConfiguration.DuplicatesAllowed));

                    }
                }
                return _challengeColors;
            }
        }

        private List<ChallengeColor> GenerateChallengeColors(int colorCount, bool duplicates)
        {
            var retValue = new List<ChallengeColor>();

            for (var i = 1; i <= colorCount; i++)
            {
                for (var j = 1; j <= colorCount; j++)
                {
                    for (var k = 1; k <= colorCount; k++)
                    {
                        for (var l = 1; l <= colorCount; l++)
                        {
                            var challengeColor = new ChallengeColor
                            {
                                AvailableColors = colorCount,
                                Colors = new [] { (Color)i, (Color)j, (Color)k, (Color)l },
                                DuplicateColors = duplicates
                            };

                            var disinctCount = 4;
                            if (duplicates)
                            {
                                disinctCount--;
                            }
                            if (challengeColor.Colors.Distinct().Count() >= disinctCount)
                            {
                                retValue.Add(challengeColor);
                            }
                        }
                    }
                }
            }

            for (var i = 1; i <= colorCount; i++)
            {
                for (var j = 1; j <= colorCount; j++)
                {
                    for (var k = 1; k <= colorCount; k++)
                    {
                        for (var l = 1; l <= colorCount; l++)
                        {
                            for (var m = 1; m <= colorCount; m++)
                            {
                                var challengeColor = new ChallengeColor
                                {
                                    AvailableColors = colorCount,
                                    Colors = new[] { (Color)i, (Color)j, (Color)k, (Color)l, (Color)m },
                                    DuplicateColors = duplicates
                                };

                                var disinctCount = 5;
                                if (duplicates)
                                {
                                    disinctCount--;
                                }
                                if (challengeColor.Colors.Distinct().Count() >= disinctCount)
                                {
                                    retValue.Add(challengeColor);
                                }
                            }
                        }
                    }
                }
            }

            return retValue;
        }
    }
}