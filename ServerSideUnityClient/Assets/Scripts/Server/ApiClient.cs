using System;
using System.Text;
using System.Threading.Tasks;
using Data;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    private string _baseUrl = "http://localhost:5235"; 
    private string _authToken;

    // Notice we now return a Task<ApiResult<T>>
    public async Task<ApiResult<T>> SendRequestAsync<T>(string endpoint, string method, object body)
    {
        using (UnityWebRequest www = new UnityWebRequest(_baseUrl + endpoint, method))
        {
            if (!string.IsNullOrEmpty(_authToken)) 
                www.SetRequestHeader("Authorization", "Bearer " + _authToken);

            if (body != null)
            {
                string json = JsonUtility.ToJson(body);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.SetRequestHeader("Content-Type", "application/json");
            }

            www.downloadHandler = new DownloadHandlerBuffer();

            // This is the magic: we AWAIT the request instead of yielding it
            var operation = www.SendWebRequest();
            while (!operation.isDone) await Task.Yield(); 

            ApiResult<T> apiResult = new ApiResult<T>();

            if (www.result == UnityWebRequest.Result.Success)
            {
                apiResult.IsSuccess = true;
                apiResult.Data = typeof(T) == typeof(string) 
                    ? (T)(object)www.downloadHandler.text 
                    : JsonUtility.FromJson<T>(www.downloadHandler.text);
            }
            else
            {
                apiResult.IsSuccess = false;
                apiResult.Error = $"{www.error}: {www.downloadHandler.text}";
            }

            return apiResult;
        }
    }
}