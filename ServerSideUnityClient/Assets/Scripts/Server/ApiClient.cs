using System;
using System.Collections;
using Data;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    // Ensure this matches your actual server port/IP
    private string _baseUrl = "http://localhost:5235"; 
    private string _authToken;
    
    public void SetToken(string token)  
    {
        _authToken = token;
    }
    
    public IEnumerator SendRequest<T>(string endpoint, string method, object body, Action<ApiResult<T>> callback)
    {
        // Setup the Web Request
        using (UnityWebRequest www = new UnityWebRequest(_baseUrl + endpoint, method))
        {
            if (!String.IsNullOrEmpty(_authToken)) 
            {
                www.SetRequestHeader("Authorization", "Bearer " + _authToken);
            }
            
            // Handle Body (JSON)
            if (body != null)
            {
                string json = JsonUtility.ToJson(body);
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.SetRequestHeader("Content-Type", "application/json");
            }
            
            // We always need a download handler to read the response (even for errors)
            www.downloadHandler = new DownloadHandlerBuffer();

            // Send
            yield return www.SendWebRequest();
            Debug.Log($"<color=magenta>RAW SERVER RESPONSE: {www.downloadHandler.text}");
            
            // Handle Result
            ApiResult<T> apiResult = new ApiResult<T>();

            if (www.result == UnityWebRequest.Result.Success)
            {
                apiResult.IsSuccess = true;
                
                // Handle raw String response vs JSON Object response
                if (typeof(T) == typeof(string))
                {
                    apiResult.Data = (T)(object)www.downloadHandler.text;
                }
                else
                {
                    try 
                    {
                        apiResult.Data = JsonUtility.FromJson<T>(www.downloadHandler.text);
                    }
                    catch (Exception ex)
                    {
                        apiResult.IsSuccess = false;
                        apiResult.Error = "JSON Parse Error: " + ex.Message;
                    }
                }
            }
            else
            {
                apiResult.IsSuccess = false;
                
                // This lets you see "Invalid Password" instead of just "400 Bad Request"
                apiResult.Error = $"{www.error}: {www.downloadHandler.text}";
            }

            callback(apiResult);
        }
    }
}
