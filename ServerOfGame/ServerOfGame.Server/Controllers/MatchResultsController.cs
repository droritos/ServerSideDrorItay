using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    [Route("api/match")]
    public class MatchResultsController : ControllerBase
    {
        
        private static List<MatchResult> matchResults = LoadResults();

        private static string _matchFilePath = Path.Combine(Directory.GetCurrentDirectory(), "leaderboard.json");

        [HttpPost("submit")]
        public IActionResult SubmitMatch([FromBody] MatchResult matchResult)
        {
            // Find the position of the player in the list
            int index = matchResults.FindIndex(x => x.username == matchResult.username);

            if (index != -1)
            {
                // Get the current struct, update it, and put it back
                var existing = matchResults[index];
                existing.score += matchResult.score;
                matchResults[index] = existing;

                Console.WriteLine($"[DB] Updated {matchResult.username}'s total: {existing.score}");
            }
            else
            {
                // New player! than Add
                matchResults.Add(matchResult);
                Console.WriteLine($"[DB] Created new record for {matchResult.username}");
            }

            SaveResults();
            return Ok(new SubmitResponse { success = true });
        }

        [HttpGet("leaderboard")]
        public IActionResult GetLeaderboard()
        {
            List<MatchResult> sortedlist = matchResults
                .OrderByDescending(x => x.score)
                .Take(5)
                .ToList();

            LeaderboardResponse response = new LeaderboardResponse
            {
                list = sortedlist
            };

            return Ok(response);
        }

        private static List<MatchResult> LoadResults()
        {
            if (System.IO.File.Exists(_matchFilePath))
            {
                string json = System.IO.File.ReadAllText(_matchFilePath);
                return JsonSerializer.Deserialize<List<MatchResult>>(json) ?? new List<MatchResult>();
            }
            return new List<MatchResult>();
        }

        private void SaveResults()
        {
            // WriteIndented = true makes the JSON file readable if you open it in Notepad
            string json = JsonSerializer.Serialize(matchResults, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_matchFilePath, json);
        }
    }
}