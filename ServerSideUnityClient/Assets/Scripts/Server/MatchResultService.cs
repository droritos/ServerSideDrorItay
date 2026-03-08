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
        [SerializeField] private GUIChannel guiChannel;

        private const string submitEndPoint      = "/api/match/submit";
        private const string leaderboardEndPoint = "/api/match/leaderboard";

        private void Start()
        {
            servicesChannel.Subscribe(ServiceEventType.EndGameMatch, HandleEndGameEvent);
            servicesChannel.Subscribe(ServiceEventType.Connect,      GetLeaderboardAsyncVoid);
        }

        private void OnDestroy()
        {
            servicesChannel.Unsubscribe(ServiceEventType.EndGameMatch, HandleEndGameEvent);
            servicesChannel.Unsubscribe(ServiceEventType.Connect,      GetLeaderboardAsyncVoid);
        }

        // Called when match ends — data is the winner's username, not a score
        // We just refresh the leaderboard, no need to submit here
        private async void HandleEndGameEvent(string data)
        {
            await GetLeaderboardAsync();
        }

        private async void GetLeaderboardAsyncVoid()
        {
            await GetLeaderboardAsync();
        }

        public async Task SubmitMatchAsync(int score)
        {
            string currentUsername = PlayerPrefs.GetString("LastUsername", "Unknown");

            // Build the request body manually as JSON string
            // because JsonUtility handles simple key-value pairs reliably
            string json = $"{{\"username\":\"{currentUsername}\",\"score\":{score}}}";

            Debug.Log("<color=yellow>Submitting match results...</color>");

            var apiResult = await apiClient.SendRequestRawAsync<SubmitResponse>(submitEndPoint, GlobalData.POST, json);

            if (apiResult.IsSuccess && apiResult.Data.success)
                Debug.Log("<color=green>Match Submitted Successfully!</color>");
            else
                Debug.LogError($"<color=red>Submit Failed:</color> {apiResult.Error}");
        }

        public async Task GetLeaderboardAsync()
        {
            Debug.Log("<color=cyan>Fetching Leaderboard...</color>");

            // Fetch raw JSON string so we can parse it ourselves
            var result = await apiClient.SendRequestAsync<string>(leaderboardEndPoint, GlobalData.GET, null);

            if (!result.IsSuccess)
            {
                Debug.LogError($"Leaderboard Error: {result.Error}");
                return;
            }

            // Parse manually — JsonUtility struggles with nested lists
            // Server returns: {"list":[{"username":"itay","score":5}]}
            try
            {
                LeaderboardResponse parsed = JsonUtility.FromJson<LeaderboardResponse>(result.Data);

                if (parsed.list == null || parsed.list.Count == 0)
                {
                    Debug.LogWarning("Leaderboard is empty.");
                    return;
                }

                Debug.Log("--- LEADERBOARD ---");
                foreach (MatchResult entry in parsed.list)
                    Debug.Log($"<color=cyan>{entry.username}: {entry.score}</color>");

                guiChannel.RaiseLeaderboardChanged(parsed.list);
            }
            catch (Exception e)
            {
                Debug.LogError($"Leaderboard parse error: {e.Message}\nRaw: {result.Data}");
            }
        }
    }
}
