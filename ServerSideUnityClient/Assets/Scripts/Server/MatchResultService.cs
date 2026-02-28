using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Scriptable_Objects;
using UnityEngine;

namespace Server
{
    public class MatchResultService : MonoBehaviour
    {
        [SerializeField] private ApiClient apiClient;
        [SerializeField] private ServicesChannel servicesChannel;
        
        private const string sumbitEndPoint = "/api/match/submit";
        private const string leaderboardEndPoint = "/api/match/leaderboard";

        private void Start()
        {
            // Subscribe to the end of the match event
            servicesChannel.Subscribe(ServiceEventType.EndGameMatch, HandleEndGameEvent);
        }

        private void OnDestroy()
        {
            servicesChannel.Unsubscribe(ServiceEventType.EndGameMatch, HandleEndGameEvent);
        }

        private async void HandleEndGameEvent(string scoreAsString)
        {
            if (int.TryParse(scoreAsString, out int finalScore))
            {
                // Now we call our async REST method to save the score!
                await SubmitMatchAsync(finalScore);
        
                // After submitting, let's refresh the leaderboard to see our new rank!
                await GetLeaderboardAsync();
            }
        }

        public async Task SubmitMatchAsync(int score)
        {
            MatchResult result = new MatchResult()
            {
                score = score,
            };

            Debug.Log("<color=yellow>Submitting match results...</color>");
            
            // Using our new async SendRequest
            var apiResult = await apiClient.SendRequestAsync<SubmitResponse>(sumbitEndPoint, GlobalData.POST, result);

            if (apiResult.IsSuccess && apiResult.Data.success)
            {
                Debug.Log("<color=green>Match Submitted Successfully!</color>");
            }
            else
            {
                Debug.LogError($"<color=red>Submit Failed:</color> {apiResult.Error}");
            }
        }

        public async Task GetLeaderboardAsync()
        {
            Debug.Log("<color=cyan>Fetching Leaderboard...</color>");
            
            var result = await apiClient.SendRequestAsync<LeaderboardResponse>(leaderboardEndPoint, GlobalData.GET, null);

            if (result.IsSuccess)
            {
                if (result.Data.list == null)
                {
                    Debug.LogWarning("Leaderboard is empty.");
                    return;
                }

                Debug.Log("--- LEADERBOARD ---");
                foreach (MatchResult entry in result.Data.list)
                {
                    Debug.Log($"Score: {entry.score}");
                }
            }
            else
            {
                Debug.LogError($"Leaderboard Error: {result.Error}");
            }
        }
    }
}