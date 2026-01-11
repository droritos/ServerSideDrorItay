using Data;
using UnityEngine;

namespace Server
{
    public class AuthService : MonoBehaviour
    {
        [SerializeField] private ApiClient apiClient;

        [SerializeField] string trainingUsername = "Moshe";
        [SerializeField] string trainingPassword = "1254!";
        
        //private LoginPayLoad loginPayLoad;

        public void Login()
        {
            LoginWithDevice(trainingUsername, trainingPassword);
        }

        private void LoginWithDevice(string username, string password)
        {
            LoginPayLoad loginPayLoad = new LoginPayLoad(username, password);

            StartCoroutine(
                apiClient.SendRequest<LoginResponse>( // <string> means we expect a String back (the Token)
                    "/api/auth/login", 
                    "POST", 
                    loginPayLoad, 
                
                    // 2. This is the Callback (The code to run when the server replies)
                    (result) => 
                    {
                        if (result.IsSuccess)
                        {
                            Debug.Log($"<color=green>Login Successful!</color> Token: {result.Data.token}");
                            apiClient.SetToken(result.Data.token);
                        }
                        else
                        {
                            Debug.LogError($"<color=red>Login Failed:</color> {result.Error}");
                        }
                    }
                )
            );
        }
    
    
    }

    
}