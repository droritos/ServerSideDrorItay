using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Data;
using UnityEngine;
using UnityEngine.Events;



namespace Server
{
    public class AuthService : MonoBehaviour
    {
        private const string BaseUrl = "http://localhost:5235/api/auth"; 
        private readonly HttpClient _httpClient = new HttpClient();
        
        [Header("Temporary References")]
        [SerializeField] private ChatService chatService; // Link this in Inspector!
        [SerializeField] private LoginUIManager uiManager; // Link this to show errors

        private void Start()
        {
            uiManager.LoginResponse += Login;
            uiManager.RegisterResponse += Register;
        }

        private void OnDestroy()
        {
            uiManager.LoginResponse -= Login;
            uiManager.RegisterResponse -= Register;
        }

        public async void Register(string username, string password)
        {
            await SendAuthRequest(username, password, "/register");
        }
        
        public async void Login(string username, string password)
        {
            string token = await SendAuthRequest(username, password, "/login");
            
            if (!string.IsNullOrEmpty(token))
            {
                chatService.Connect(token);  // Connect the Chat!
                Debug.Log($"<color=green>Login Success! Token:</color> {token}");
            }
        }

        private async Task<string> SendAuthRequest(string username, string password, string endpoint)
        {
            try
            {
                AuthRequest request = new AuthRequest { Username = username, Password = password };
                string json = JsonUtility.ToJson(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(BaseUrl + endpoint, content);

                string responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    AuthResponse authResponse = JsonUtility.FromJson<AuthResponse>(responseText);
                    return authResponse.token;
                }
                else
                {
                    Debug.LogError($"Auth Error: {responseText}");
                    // Apply UI Pop!
                    return null;
                }
            }
            catch (Exception e)
            {
                string error = $"Network Error: {e.Message}";
                Debug.LogError(error);
                PopUpGUIHandler.Instance.HandlePopupRequest(error,InfoPopupType.Error);
                return null;
            }
        }
    }
}