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
        #region << Events >>
        public event UnityAction<string> OnLoginToken 
        {
            add    => servicesChannel.Subscribe(ServiceEventType.Login, value);
            remove => servicesChannel.Unsubscribe(ServiceEventType.Login, value);
        }
        public event UnityAction<string> OnRegisterToken
        {
            add    => servicesChannel.Subscribe(ServiceEventType.Register, value);
            remove => servicesChannel.Unsubscribe(ServiceEventType.Register, value);
        }
        #endregion
        
        [Header("References")]
        [SerializeField] ServicesChannel servicesChannel;
        [SerializeField] LoginUIManager loginUIManager;
        
        private const string BaseUrl = "http://localhost:5235/api/auth"; 
        private readonly HttpClient _httpClient = new HttpClient();
        
        
        
        public async void Register(string username, string password)
        {
            string token = await SendAuthRequest(username, password, "/register");

            if (!string.IsNullOrEmpty(token))
            {
                PopUpGUIHandler.Instance.HandlePopupRequest("Register Success!", InfoPopupType.Log);
                servicesChannel.Raise(ServiceEventType.Register, token);
            }
        }

        public async void Login(string username, string password)
        {
            string token = await SendAuthRequest(username, password, "/login");

            if (!string.IsNullOrEmpty(token))
            {
                PopUpGUIHandler.Instance.HandlePopupRequest($"Login Success! Token - {token}", InfoPopupType.Log);
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
                    return authResponse.token;
                }
                else
                {
                    //Debug.LogError($"Auth Error: {responseText}");
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