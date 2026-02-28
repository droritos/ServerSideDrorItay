using Server;
using UnityEngine;

namespace Manager
{
    public class ServerManager : MonoBehaviour
    {
        [SerializeField] ApiClient apiClient;
        [SerializeField] AuthService  authService;
        [SerializeField] MatchResultService MatchResultService;
        
        string[] levelNames = { "Snow Island", "Lava Core", "Green Forest", "Sky City","Underground Palace" };
    }
}
