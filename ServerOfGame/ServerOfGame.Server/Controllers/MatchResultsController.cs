using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Models;
using System.Linq;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    [Route("api/match")] // Matching like in Unity
    public class MatchResultsController : ControllerBase
    {
        private static List<MatchResult> matchResults = new List<MatchResult>();

        [HttpPost("submit")]
        public IActionResult SumbitMatch([FromBody] MatchResult matchResult)
        {
            matchResults.Add(matchResult);

            return Ok(new SubmitResponse { success = true });
        }

        [HttpGet("leaderboard")]
        public IActionResult GetLeaderboard()
        {
            List<MatchResult> sortedlist = matchResults.OrderByDescending(x => x.score).Take(5).ToList();

            LeaderboardResponse response = new LeaderboardResponse();
            response.list = sortedlist;

            return Ok(response);
        }
    }

}