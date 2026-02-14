using Data;
using UnityEngine;
using UnityEngine.Events;



namespace Server
{
    public class AuthService : MonoBehaviour
    {
        [SerializeField] private ApiClient apiClient;
        public UnityEvent OnLoginSuccess;

        public void Login(string username, string password)
        {
            LoginPayLoad loginPayLoad = new LoginPayLoad(username, password);

            StartCoroutine(
                apiClient.SendRequest<LoginResponse>(
                    "/api/auth/login",
                    "POST",
                    loginPayLoad,
                    (result) =>
                    {
                        if (result.IsSuccess)
                        {
                            Debug.Log($"<color=green>Login Successful!</color> Token: {result.Data.token}");
                            apiClient.SetToken(result.Data.token);

                            OnLoginSuccess?.Invoke();

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