using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Pentathanerd.Mastermind.Models;

namespace Pentathanerd.Mastermind.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Leaderboard()
        {
            var leaderboard = GamePlayHub.GetLeaderboard();

            var models = new List<LeaderboardModel>();

            if (leaderboard != null)
            {
                models = ToLeaderboardModels(leaderboard).OrderByDescending(x => x.Score).ToList();
            }

            return View(models);
        }

        public ActionResult HardReset(string resetKey)
        {
            if (resetKey == "hardToGuessResetKey123")
                GamePlayHub.HardReset();

            return RedirectToAction("Index");
        }

        public ActionResult ClearLeaderboard(string resetKey)
        {
            if (resetKey == "hardToGuessResetLeaderboardKey123")
                GamePlayHub.ClearLeaderboardStats();

            return RedirectToAction("Leaderboard");
        }


        private static List<LeaderboardModel> ToLeaderboardModels(List<LeaderboardStats> stats)
        {
            var retValue = new List<LeaderboardModel>();

            foreach (var i in stats)
            {
                retValue.Add(new LeaderboardModel
                {
                    Score = i.Challenges.Sum(x => x.Score),
                    TeamName = string.IsNullOrEmpty(i.TeamName) ? "Anon" : i.TeamName,
                    RoundsCompleted = i.Challenges.Count(x=> x.Solved),
                    Fastest = CalculateFastestTime(i.Challenges),
                    FewestMoves = CalculateFewestMoves(i.Challenges)
                });
            }

            return retValue;
        }

        private static TimeSpan CalculateFastestTime(List<Challenge> challenges)
        {
            var timeSpans = new List<TimeSpan>();
            foreach (var challenge in challenges.Where(x => x.Solved))
            {
                timeSpans.Add(new TimeSpan(0, 0,0, Convert.ToInt32((challenge.EndTime - challenge.StartTime).TotalSeconds)));
            }
            return timeSpans.OrderBy(x => x.TotalSeconds).FirstOrDefault();
        }

        private static int CalculateFewestMoves(List<Challenge> challenges)
        {
            return challenges.Count(x => x.Solved) > 0 ? challenges.Where(x => x.Solved).Min(y => y.TotalGuessCount) : -1;
        }
    }
}