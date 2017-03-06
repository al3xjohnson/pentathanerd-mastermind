using System;
using System.Collections.Generic;
using System.Linq;

namespace Pentathanerd.Mastermind
{
    public class ScoreCalculator
    {
        private readonly Color[] WorkingAnswer;
        private Color[] WorkingGuess;

        private Color[] Answer { get; set; }

        private Challenge Challenge { get; set; }

        public ScoreCalculator(Challenge challenge)
        {
            Answer = challenge.ChallengeColor.Colors.ToArray();
            WorkingAnswer = Answer;
            Challenge = challenge;
        }

        public Challenge CalculateScore(Color[] guess)
        {
            WorkingGuess = guess;
            Challenge.Guesses.Add(guess);
            Challenge.TotalGuessCount++;

            var guessResponse = CalculateGuessResponse();
            Challenge.GuessResponseses.Add(guessResponse);

            var correctColorAndPositionGuessesCount = guessResponse.Count(x => x == GuessResponse.ColorAndPosition);
            Challenge.CorrectColorAndPositionGuessCount += correctColorAndPositionGuessesCount;
            Challenge.Score += correctColorAndPositionGuessesCount * GameConfiguration.CorrectColorAndPositionScore;

            if (correctColorAndPositionGuessesCount == guess.Length)
            {
                Challenge.Solved = true;
                Challenge.ActiveChallenge = false;
                var roundBonus = CalculateRoundBonus();
                Challenge.Score += roundBonus;
                var timeBonus = CalculateTimeBonus();
                Challenge.Score += timeBonus;
                return Challenge;
            }

            var correctColorGuessCount = guessResponse.Count(x => x == GuessResponse.Color);
            Challenge.CorrectColorGuessCount += correctColorGuessCount;
            Challenge.Score += correctColorGuessCount * GameConfiguration.CorrectColorScore;

            var incorrectGuessCount = guessResponse.Count(x => x == GuessResponse.None);
            Challenge.IncorrectGuessCount += incorrectGuessCount;
            Challenge.Score += incorrectGuessCount * GameConfiguration.IncorrectGuessScore;

            Challenge.Score += GameConfiguration.IncompleteChallengeBonus;

            if (Challenge.TotalGuessCount >= Challenge.AvailableGuesses)
            {
                Challenge.Solved = false;
                Challenge.ActiveChallenge = false;
                Challenge.Score -= GameConfiguration.IncompleteRoundBonus;
            }

            return Challenge;
        }

        private double CalculateRoundBonus()
        {
            var retValue = GameConfiguration.RoundBonus;
            var roundCompletionMultiplier = (Challenge.AvailableGuesses + 1 - Challenge.TotalGuessCount) * GameConfiguration.RoundMulitiplier;

            retValue += roundCompletionMultiplier;

            return Math.Round(retValue);
        }

        private double CalculateTimeBonus()
        {
            var timeBonusUpperBound = GameConfiguration.TimeBonusUpperBound;

            var challengeDuration = (Challenge.EndTime - Challenge.StartTime).Seconds;

            var retValue = (timeBonusUpperBound - challengeDuration) * (GameConfiguration.TimeBonus * (Challenge.AvailableGuesses + 1 - Challenge.TotalGuessCount));

            return Math.Round(retValue);
        }

        private List<GuessResponse> CalculateGuessResponse()
        {
            var response = new List<GuessResponse>();
            var correctColorAndPositionResponses = GetCorrectColorAndPosition();
            var correctColorResponses = GetCorrectColor();
            var incorrectResponses = GetIncorrect();

            response.AddRange(correctColorAndPositionResponses);
            response.AddRange(correctColorResponses);
            response.AddRange(incorrectResponses);

            return response;
        }

        private List<GuessResponse> GetCorrectColorAndPosition()
        {
            var retVal = new List<GuessResponse>();
            for (var i = 0; i < WorkingGuess.Length; i++)
            {
                var guessColor = WorkingGuess[i];
                var correctColor = Answer[i];
                if (guessColor == correctColor)
                {
                    retVal.Add(GuessResponse.ColorAndPosition);
                    WorkingAnswer[i] = Color.Unknown;
                    WorkingGuess[i] = Color.Unknown;
                }
            }
            return retVal;
        }

        private List<GuessResponse> GetCorrectColor()
        {
            var retVal = new List<GuessResponse>();
            for (var i = 0; i < WorkingGuess.Length; i++)
            {
                var guessColor = WorkingGuess[i];
                if (WorkingAnswer.Contains(guessColor) && guessColor != Color.Unknown)
                {
                    retVal.Add(GuessResponse.Color);
                    var colorIndex = Array.IndexOf(WorkingAnswer, guessColor);
                    WorkingAnswer[colorIndex] = Color.Unknown;
                    WorkingGuess[i] = Color.Unknown;
                }
            }
            return retVal;
        }

        private List<GuessResponse> GetIncorrect()
        {
            var remainingColors = WorkingAnswer.Where(x => x != Color.Unknown);
            return remainingColors.Select(remainingColor => GuessResponse.None).ToList();
        }
    }
}