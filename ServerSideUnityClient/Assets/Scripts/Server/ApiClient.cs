using System;
using System.Collections;
using Data;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    
    private string _baseUrl = "http://localhost:5235"; 
    private string _authToken;
    
    public void SetToken(string token)  
    {
        _authToken = token;
    }
    
    public IEnumerator SendRequest<T>(string endpoint, string method, object body, Action<ApiResult<T>> callback)
    {
        
        using (UnityWebRequest www = new UnityWebRequest(_baseUrl + endpoint, method))
        {
            if (!String.IsNullOrEmpty(_authToken)) 
            {
                www.SetRequestHeader("Authorization", "Bearer " + _authToken);
            }
            
            
            if (body != null)
            {
                string json = JsonUtility.ToJson(body);
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.SetRequestHeader("Content-Type", "application/json");
            }
            
           
            www.downloadHandler = new DownloadHandlerBuffer();

           
            yield return www.SendWebRequest();
            Debug.Log($"<color=magenta>RAW SERVER RESPONSE: {www.downloadHandler.text}");
            
            
            ApiResult<T> apiResult = new ApiResult<T>();

            if (www.result == UnityWebRequest.Result.Success)
            {
                apiResult.IsSuccess = true;
                
               
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
                
                
                apiResult.Error = $"{www.error}: {www.downloadHandler.text}";
            }

            callback(apiResult);
        }
    }
}
