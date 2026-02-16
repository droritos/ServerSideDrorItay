using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Data;
using Scriptable_Objects;
using UnityEngine;
using UnityEngine.Events;


namespace Server
{
    public class AuthService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] ServicesChannel servicesChannel;
        [SerializeField] LoginUIManager loginUIManager;
        
        private const string BaseUrl = "http://localhost:5235/api/auth"; 
        private readonly HttpClient _httpClient = new HttpClient();

        private void Start()
        {
            loginUIManager.LoginResponse += Login;
            loginUIManager.RegisterResponse += Register;
        }

        private void OnDestroy()
        {
            loginUIManager.LoginResponse -= Login;
            loginUIManager.RegisterResponse -= Register;
        }

        private void OnValidate()
        {
           // if(!servicesChannel)
           //     servicesChannel = Resources.Load<ServicesChannel>("ServicesEvents");
            
            if(!loginUIManager)
                loginUIManager = FindFirstObjectByType<LoginUIManager>();
        }

        public async void Register(string username, string password)
        {
            await SendAuthRequest(username, password, "/register");

            //PopUpGUIHandler.Instance.HandlePopupRequest("Register Success!", InfoPopupType.Log);
            servicesChannel.Raise(ServiceEventType.Register);
        }

        public async void Login(string username, string password)
        {
            string token = await SendAuthRequest(username, password, "/login");

            if (!string.IsNullOrEmpty(token))
            {
                //PopUpGUIHandler.Instance.HandlePopupRequest($"Login Success! Token - {token}", InfoPopupType.Log);
                servicesChannel.Raise(ServiceEventType.Login, token);
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
                    PopUpGUIHandler.Instance.HandlePopupRequest(authResponse.message,InfoPopupType.Log);
                    return authResponse.token;
                }
                else
                {
                    Debug.LogError($"Auth Error: {responseText}");
                    PopUpGUIHandler.Instance.HandlePopupRequest(responseText,InfoPopupType.Error);
                    return null;
                }
            }
            catch (Exception e)
            {
                string error = $"Network Error: {e.Message}";
                //Debug.LogError(error);
                PopUpGUIHandler.Instance.HandlePopupRequest(error,InfoPopupType.Error);
                return null;
            }
        }
    }
}