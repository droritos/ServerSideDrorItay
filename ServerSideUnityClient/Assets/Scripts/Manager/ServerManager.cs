using Server;
using UnityEngine;

namespace Manager
{
    public class ServerManager : MonoBehaviour
    {
        [SerializeField] ApiClient apiClient;
        [SerializeField] AuthService  authService;
        [SerializeField] PlayerService   playerService;
        [SerializeField] InventoryService InventoryService;
        [SerializeField] MatchResultService MatchResultService;
        
        string[] levelNames = { "Snow Island", "Lava Core", "Green Forest", "Sky City","Underground Palace" };
        

        private void Start()
        {
            SetUpServer();
        }

        private void SetUpServer()
        {
            authService.Login();
            playerService.GetProfile();
            InsertRandomMatches();
        }

        public void UpdateInfromation()
        {
            // Moshiko is also exits in the server variables
            playerService.UpdateProfile("Moshiko",8,13);
        }
        
        public void PurchaseItem()
        {
            InventoryService.PurchaseItem(3);
        }

        [ContextMenu("Insert Random Matches")]
        private void InsertRandomMatches()
        {
            for (int i = 0; i < 5; i++)
            {
                // Random.Range(min, max) gives us random numbers.
                int randomScore = Random.Range(500, 50000); 
                float randomDuration = Random.Range(30.0f, 300.0f); // 30s to 5 mins
        
                // Pick a random level name from our list
                string randomLevel = levelNames[Random.Range(0, levelNames.Length)];

                // (Make sure this matches the function name in your class!)
                MatchResultService.SumbitMatch(randomScore, randomDuration, randomLevel);
            }
            Debug.Log("All 5 Match Read");
        }

        public void GetLeaderBoard()
        {
            MatchResultService.GetLeaderboard();
        }

    }
}
