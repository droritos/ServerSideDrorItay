using System;
using System.Threading.Tasks;
using Data;
using Scriptable_Objects;
using UnityEngine;

namespace Server
{
    /// <summary>
    /// Handles register and login REST calls.
    /// On success: stores JWT in ApiClient and raises ServiceEventType.Login so
    /// ConnectionManager opens the WebSocket.
    /// </summary>
    public class AuthService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ServicesChannel servicesChannel;
        [SerializeField] private LoginUIManager  loginUIManager;
        [SerializeField] private ApiClient       apiClient;

        private const string BaseEndpoint = "/api/auth";

        private void Start()
        {
            loginUIManager.LoginResponse    += Login;
            loginUIManager.RegisterResponse += Register;
        }

        private void OnDestroy()
        {
            loginUIManager.LoginResponse    -= Login;
            loginUIManager.RegisterResponse -= Register;
        }

        private void OnValidate()
        {
            if (!loginUIManager) loginUIManager = FindFirstObjectByType<LoginUIManager>();
            if (!apiClient)      apiClient      = FindFirstObjectByType<ApiClient>();
        }

        // ── Register ──────────────────────────────────────────

        public async void Register(string username, string password)
        {
            var result = await SendAuth(username, password, "/register");
            if (result.IsSuccess)
            {
                PopUpGUIHandler.Instance.HandlePopupRequest("Registered! Please log in.", InfoPopupType.Log);
                servicesChannel.Raise(ServiceEventType.Register);
            }
        }

        // ── Login ─────────────────────────────────────────────

        public async void Login(string username, string password)
        {
            var result = await SendAuth(username, password, "/login");
            if (!result.IsSuccess) return;

            // Store username for leaderboard submission
            PlayerPrefs.SetString("LastUsername", username);
            PlayerPrefs.Save();

            // Give the JWT to ApiClient so future REST calls are authenticated
            apiClient.SetToken(result.Data.token);

            // Signal ConnectionManager to open the WebSocket
            servicesChannel.Raise(ServiceEventType.Login, result.Data.token);
        }

        // ── Shared helper ─────────────────────────────────────

        private async Task<ApiResult<AuthResponse>> SendAuth(string username, string password, string path)
        {
            var body   = new AuthRequest { Username = username, Password = password };
            var result = await apiClient.SendRequestAsync<AuthResponse>(
                BaseEndpoint + path, GlobalData.POST, body);

            if (result.IsSuccess)
                PopUpGUIHandler.Instance.HandlePopupRequest(result.Data.message, InfoPopupType.Log);
            else
                PopUpGUIHandler.Instance.HandlePopupRequest(result.Error, InfoPopupType.Error);

            return result;
        }
    }
}
