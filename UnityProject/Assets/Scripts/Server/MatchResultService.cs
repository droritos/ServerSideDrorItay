using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Server
{
   public class MatchResultService : MonoBehaviour
   {
      [SerializeField] ApiClient apiClient;
      private const string sumbitEndPoint = "/api/match/submit";
      private const string leaderboardEndPoint = "/api/match/leaderboard";
   
      public void SumbitMatch(int score, float durationSeconds, string levelName)
      {
         MatchResult result = new MatchResult()
         {
            score = score,
            durationSeconds = durationSeconds,
            levelName = levelName,
         };

         StartCoroutine(
            apiClient.SendRequest<SubmitResponse>(sumbitEndPoint, GlobalData.POST, result,
               (apiResult) => // Renamed 'result' to 'apiResult' to avoid confusion
               {
                  // Now we check apiResult.Data.success
                  if (apiResult.IsSuccess && apiResult.Data.success)
                  {
                     Debug.Log($"<color=green>Match Submitted!</color>");
                  }
                  else
                  {
                     Debug.LogError($"<color=red>Cant Submit Match:</color> {apiResult.Error}");
                  }
               }
            )
         );
      }
   
      public void GetLeaderboard()
      {
         StartCoroutine(
            apiClient.SendRequest<LeaderboardResponse>(leaderboardEndPoint, GlobalData.GET, null,
               (result) =>
               {
                  if (result.IsSuccess)
                  {
                     if (result.Data.list == null)
                     {
                        Debug.LogWarning("<color=red>Leaderboard list is null! (Did you restart the server?)");
                        return;
                     }
                     
                     Debug.Log("--- LEADERBOARD ---");
                     foreach (MatchResult entry in result.Data.list)
                     {
                        Debug.Log($"<color=cyan>Score: {entry.score} | Level: {entry.levelName}");
                     }
                  }
                  else
                  {
                     Debug.LogError("Failed to fetch leaderboard: " + result.Error);
                  }
               }
            )
         );
      }
   }
}
