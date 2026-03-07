using System.Threading.Tasks;
using Data;
using UnityEngine;

namespace Server
{
    /// <summary>
    /// Reads and updates the player's server-side profile.
    /// ApiClient automatically attaches the JWT.
    /// </summary>
    public class PlayerService : MonoBehaviour
    {
        [SerializeField] private ApiClient apiClient;
        private const string Endpoint = "/api/player/profile";

        public async Task<PlayerProfile> GetProfile()
        {
            var result = await apiClient.SendRequestAsync<PlayerProfile>(
                Endpoint, GlobalData.GET, null);

            if (result.IsSuccess)
            {
                Debug.Log($"<color=green>Profile: {result.Data.username} Lvl {result.Data.level}</color>");
                return result.Data;
            }

            Debug.LogError($"<color=red>GetProfile failed: {result.Error}</color>");
            return default;
        }

        public async void UpdateProfile(string username, int newLevel, int newXp)
        {
            var data   = new PlayerProfile { username = username, level = newLevel, xp = newXp };
            var result = await apiClient.SendRequestAsync<PlayerProfile>(
                Endpoint, GlobalData.PUT, data);

            if (result.IsSuccess)
                Debug.Log($"<color=green>Profile updated: Lvl {result.Data.level}, XP {result.Data.xp}</color>");
            else
                Debug.LogError($"<color=red>Update failed: {result.Error}</color>");
        }
    }
}
