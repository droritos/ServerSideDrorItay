using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Models;
using System.Linq;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    [Route("api/match")] // Matching like in Unity
    public class MatchController : ControllerBase
    {
        private static List<MatchResult> matchResults = new List<MatchResult>();

        [HttpPost("submit")]
        public IActionResult SumbitMatch([FromBody] MatchResult matchResult)
        {
            matchResults.Add(matchResult);

            // FIX: Don't return "true". Return an object containing true.
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