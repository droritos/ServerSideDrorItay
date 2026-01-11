using Data;
using UnityEngine;

namespace Server
{
    public class PlayerService : MonoBehaviour
    {
        [SerializeField] ApiClient apiClient;
    
        private const string endPoint = "/api/player/profile";

        public void GetProfile()
        {
            StartCoroutine(apiClient.SendRequest<PlayerProfile>(endPoint,
                "GET",
                null,
                (result) => 
                {
                    if (result.IsSuccess)
                    {
                        Debug.Log($"<color=green>Profile !</color> Token: name: {result.Data.username} lvl: {result.Data.level} xp: {result.Data.xp}");
                    }
                    else
                    {
                        Debug.LogError($"<color=red>Login Failed:</color> {result.Error}");
                    }
                }
            ));
        }
        
        public void UpdateProfile(string nameId,int newLevel, int newXp) 
        {
            // 2. Create the data object we want to send to the server
            PlayerProfile dataToSend = new PlayerProfile
            {
                username = nameId,
                level = newLevel,
                xp = newXp
            };

            StartCoroutine(apiClient.SendRequest<PlayerProfile>(
                endPoint,
                GlobalData.PUT,     
                dataToSend,
                (result) => 
                {
                    if (result.IsSuccess)
                    {
                        Debug.Log($"<color=green>Update Success!</color> Server confirmed: {result.Data.level}, with {result.Data.xp} Xp");
                    }
                    else
                    {
                        Debug.LogError($"<color=red>Update Failed:</color> {result.Error}");
                    }
                }
            ));
        }
    }
}
