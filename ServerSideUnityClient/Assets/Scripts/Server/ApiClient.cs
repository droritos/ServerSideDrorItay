using System;
using System.Text;
using System.Threading.Tasks;
using Data;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Central HTTP client for all REST calls.
/// Call SetToken() once after login so every subsequent request carries the JWT.
/// </summary>
public class ApiClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:5235";

    private string _authToken = string.Empty;

    public void SetToken(string token)
    {
        _authToken = token;
        Debug.Log("[ApiClient] JWT token stored.");
    }

    public string GetToken() => _authToken;

    // Standard request — deserializes with JsonUtility
    public async Task<ApiResult<T>> SendRequestAsync<T>(string endpoint, string method, object body)
    {
        using var www = new UnityWebRequest(baseUrl + endpoint, method);

        if (!string.IsNullOrEmpty(_authToken))
            www.SetRequestHeader("Authorization", "Bearer " + _authToken);

        if (body != null)
        {
            string json = JsonUtility.ToJson(body);
            byte[] raw  = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(raw);
            www.SetRequestHeader("Content-Type", "application/json");
        }

        www.downloadHandler = new DownloadHandlerBuffer();
        var op = www.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        var result = new ApiResult<T>();
        if (www.result == UnityWebRequest.Result.Success)
        {
            result.IsSuccess = true;
            result.Data = typeof(T) == typeof(string)
                ? (T)(object)www.downloadHandler.text
                : JsonUtility.FromJson<T>(www.downloadHandler.text);
        }
        else
        {
            result.IsSuccess = false;
            result.Error     = $"{www.error}: {www.downloadHandler.text}";
            Debug.LogWarning($"[ApiClient] {method} {endpoint} failed: {result.Error}");
        }
        return result;
    }

    // Raw JSON string body — use when JsonUtility.ToJson won't serialize correctly
    public async Task<ApiResult<T>> SendRequestRawAsync<T>(string endpoint, string method, string rawJson)
    {
        using var www = new UnityWebRequest(baseUrl + endpoint, method);

        if (!string.IsNullOrEmpty(_authToken))
            www.SetRequestHeader("Authorization", "Bearer " + _authToken);

        if (!string.IsNullOrEmpty(rawJson))
        {
            byte[] raw = Encoding.UTF8.GetBytes(rawJson);
            www.uploadHandler = new UploadHandlerRaw(raw);
            www.SetRequestHeader("Content-Type", "application/json");
        }

        www.downloadHandler = new DownloadHandlerBuffer();
        var op = www.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        var result = new ApiResult<T>();
        if (www.result == UnityWebRequest.Result.Success)
        {
            result.IsSuccess = true;
            result.Data      = JsonUtility.FromJson<T>(www.downloadHandler.text);
        }
        else
        {
            result.IsSuccess = false;
            result.Error     = $"{www.error}: {www.downloadHandler.text}";
            Debug.LogWarning($"[ApiClient] {method} {endpoint} failed: {result.Error}");
        }
        return result;
    }
}
