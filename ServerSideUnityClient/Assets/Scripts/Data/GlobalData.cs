using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public static class GlobalData
    {
        public const string GET = "GET";
        public const string POST = "POST";
        public const string DELETE = "DELETE";
        public const string PUT = "PUT";
    }
    public struct ApiResult<T>
    {
        public T Data;
        public bool IsSuccess;
        public string Error;
    }
    
    public struct LoginPayLoad
    {
        public string username;
        public string password;

        public LoginPayLoad(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
    [System.Serializable]
    public class AuthRequest
    {
        public string Username;
        public string Password;
    }

    [System.Serializable]
    public class AuthResponse
    {
        public string token;
        public string message;
    }
    public struct PlayerProfile
    {
        public string username;   // Must match the server's JSON key (case-sensitive!)
        public int level;
        public int xp;
    }
    public struct InfoPopupArgs
    {
        public string Text;
        public InfoPopupType Type;
    }
    public struct PurchaseRequest
    {
        public int itemID;
    }

    public struct PurchaseResponse
    {
        public bool isSuccess;
        public string error;
        public int newBalance;
    }
    
    [System.Serializable]
    public struct MatchResult
    {
        public int score;
        public float durationSeconds;
        public string levelName;
    }
    
    [System.Serializable]
    public struct LeaderboardResponse
    {
        // Unity needs the list to be inside a variable, not the root
        public List<MatchResult> list;
    }
    
    [System.Serializable]
    public struct SubmitResponse
    {
        public bool success;
    }
    
    #region << Global Enums>>
    public enum InfoPopupType
    {
        Log,
        Warning,
        Error
    }
    #endregion
}