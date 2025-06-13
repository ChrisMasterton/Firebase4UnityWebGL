using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Firebase.Core
{
    public static class FirebaseUtils
    {
        public static string ToJson<T>(T obj)
        {
            return JsonUtility.ToJson(obj);
        }
        
        public static T FromJson<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
        
        public static string EncodeKey(string key)
        {
            return UnityWebRequest.EscapeURL(key);
        }
        
        public static string BuildQueryParams(Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return string.Empty;
                
            var sb = new StringBuilder("?");
            var first = true;
            
            foreach (var kvp in parameters)
            {
                if (!first)
                    sb.Append("&");
                    
                sb.Append($"{UnityWebRequest.EscapeURL(kvp.Key)}={UnityWebRequest.EscapeURL(kvp.Value)}");
                first = false;
            }
            
            return sb.ToString();
        }
        
        public static UnityWebRequest CreateRequest(string url, string method = "GET", string jsonData = null)
        {
            var request = new UnityWebRequest(url, method);
            
            if (!string.IsNullOrEmpty(jsonData))
            {
                var bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }
            
            request.downloadHandler = new DownloadHandlerBuffer();
            return request;
        }
        
        public static void SetAuthHeader(UnityWebRequest request, string idToken)
        {
            if (!string.IsNullOrEmpty(idToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {idToken}");
            }
        }
        
        public static FirebaseException HandleError(UnityWebRequest request)
        {
            if (request.result == UnityWebRequest.Result.Success)
                return null;
                
            var errorMessage = "Unknown error";
            var errorCode = "unknown";
            
            if (!string.IsNullOrEmpty(request.downloadHandler?.text))
            {
                try
                {
                    var errorResponse = JsonUtility.FromJson<FirebaseErrorResponse>(request.downloadHandler.text);
                    if (errorResponse?.error != null)
                    {
                        errorMessage = errorResponse.error.message ?? errorMessage;
                        errorCode = errorResponse.error.code?.ToString() ?? errorCode;
                    }
                }
                catch
                {
                    errorMessage = request.downloadHandler.text;
                }
            }
            else if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                errorMessage = "Connection error";
                errorCode = "connection_error";
            }
            else if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                errorMessage = $"HTTP Error {request.responseCode}";
                errorCode = $"http_{request.responseCode}";
            }
            
            return new FirebaseException(errorCode, errorMessage);
        }
        
        public static string GenerateRandomString(int length = 20)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new StringBuilder(length);
            
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
            }
            
            return result.ToString();
        }
        
        public static long GetUnixTimestamp()
        {
            return ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
        }
        
        public static DateTime FromUnixTimestamp(long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }
    }
    
    [Serializable]
    public class FirebaseErrorResponse
    {
        public FirebaseError error;
    }
    
    [Serializable]
    public class FirebaseError
    {
        public int? code;
        public string message;
        public FirebaseErrorDetail[] details;
    }
    
    [Serializable]
    public class FirebaseErrorDetail
    {
        [SerializeField] private string type;
        [SerializeField] private string reason;
        [SerializeField] private string domain;
        [SerializeField] private object metadata;
    }
    
    public class FirebaseException : Exception
    {
        public string ErrorCode { get; }
        
        public FirebaseException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
        
        public FirebaseException(string errorCode, string message, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}