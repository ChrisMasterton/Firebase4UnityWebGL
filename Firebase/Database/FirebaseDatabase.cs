using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Firebase.Core;
using Firebase.Models;

namespace Firebase.Database
{
    public class FirebaseDatabase
    {
        private readonly FirebaseConfig config;
        private readonly FirebaseAuth auth;
        
        public FirebaseDatabase(FirebaseConfig config, FirebaseAuth auth = null)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.auth = auth;
        }
        
        public async Task<T> GetAsync<T>(string path)
        {
            var url = BuildUrl(path) + ".json";
            
            using var request = FirebaseUtils.CreateRequest(url, "GET");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            var json = request.downloadHandler.text;
            if (string.IsNullOrEmpty(json) || json == "null")
                return default(T);
                
            return FirebaseUtils.FromJson<T>(json);
        }
        
        public async Task<Dictionary<string, T>> GetChildrenAsync<T>(string path)
        {
            var url = BuildUrl(path) + ".json";
            
            using var request = FirebaseUtils.CreateRequest(url, "GET");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            var json = request.downloadHandler.text;
            if (string.IsNullOrEmpty(json) || json == "null")
                return new Dictionary<string, T>();
                
            return FirebaseUtils.FromJson<Dictionary<string, T>>(json);
        }
        
        public async Task SetAsync<T>(string path, T value)
        {
            var url = BuildUrl(path) + ".json";
            var json = FirebaseUtils.ToJson(value);
            
            using var request = FirebaseUtils.CreateRequest(url, "PUT", json);
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
        }
        
        public async Task UpdateAsync<T>(string path, T value)
        {
            var url = BuildUrl(path) + ".json";
            var json = FirebaseUtils.ToJson(value);
            
            using var request = FirebaseUtils.CreateRequest(url, "PATCH", json);
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
        }
        
        public async Task<string> PushAsync<T>(string path, T value)
        {
            var url = BuildUrl(path) + ".json";
            var json = FirebaseUtils.ToJson(value);
            
            using var request = FirebaseUtils.CreateRequest(url, "POST", json);
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            var response = FirebaseUtils.FromJson<PushResponse>(request.downloadHandler.text);
            return response.name;
        }
        
        public async Task DeleteAsync(string path)
        {
            var url = BuildUrl(path) + ".json";
            
            using var request = FirebaseUtils.CreateRequest(url, "DELETE");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
        }
        
        public async Task<T> GetWithQueryAsync<T>(string path, DatabaseQuery query)
        {
            var url = BuildUrl(path) + ".json" + BuildQueryString(query);
            
            using var request = FirebaseUtils.CreateRequest(url, "GET");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            var json = request.downloadHandler.text;
            if (string.IsNullOrEmpty(json) || json == "null")
                return default(T);
                
            return FirebaseUtils.FromJson<T>(json);
        }
        
        public async Task<bool> ExistsAsync(string path)
        {
            try
            {
                var url = BuildUrl(path) + ".json";
                var parameters = new Dictionary<string, string> { { "shallow", "true" } };
                url += FirebaseUtils.BuildQueryParams(parameters);
                
                using var request = FirebaseUtils.CreateRequest(url, "GET");
                await AddAuthIfNeeded(request);
                await SendRequestAsync(request);
                
                var exception = FirebaseUtils.HandleError(request);
                if (exception != null)
                    return false;
                    
                var json = request.downloadHandler.text;
                return !string.IsNullOrEmpty(json) && json != "null";
            }
            catch
            {
                return false;
            }
        }
        
        public DatabaseReference GetReference(string path = "")
        {
            return new DatabaseReference(this, path);
        }
        
        private string BuildUrl(string path)
        {
            if (string.IsNullOrEmpty(config.DatabaseURL))
                throw new FirebaseException("missing_database_url", "Database URL not configured");
                
            path = path?.TrimStart('/') ?? "";
            return $"{config.DatabaseBaseUrl}/{path}";
        }
        
        private string BuildQueryString(DatabaseQuery query)
        {
            if (query == null)
                return "";
                
            var parameters = new Dictionary<string, string>();
            
            if (!string.IsNullOrEmpty(query.OrderBy))
                parameters["orderBy"] = $"\"{query.OrderBy}\"";
                
            if (query.LimitToFirst.HasValue)
                parameters["limitToFirst"] = query.LimitToFirst.Value.ToString();
                
            if (query.LimitToLast.HasValue)
                parameters["limitToLast"] = query.LimitToLast.Value.ToString();
                
            if (!string.IsNullOrEmpty(query.StartAt))
                parameters["startAt"] = $"\"{query.StartAt}\"";
                
            if (!string.IsNullOrEmpty(query.EndAt))
                parameters["endAt"] = $"\"{query.EndAt}\"";
                
            if (!string.IsNullOrEmpty(query.EqualTo))
                parameters["equalTo"] = $"\"{query.EqualTo}\"";
                
            if (query.Shallow)
                parameters["shallow"] = "true";
                
            return FirebaseUtils.BuildQueryParams(parameters);
        }
        
        private async Task AddAuthIfNeeded(UnityWebRequest request)
        {
            if (auth != null && auth.IsSignedIn)
            {
                try
                {
                    var token = await auth.GetIdTokenAsync();
                    var parameters = new Dictionary<string, string> { { "auth", token } };
                    var separator = request.url.Contains("?") ? "&" : "?";
                    request.url += separator + FirebaseUtils.BuildQueryParams(parameters).TrimStart('?');
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to add auth token: {ex.Message}");
                }
            }
        }
        
        private async Task SendRequestAsync(UnityWebRequest request)
        {
            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }
    }
    
    public class DatabaseReference
    {
        private readonly FirebaseDatabase database;
        private readonly string path;
        
        public string Key => path.Split('/')[^1];
        public string Path => path;
        public DatabaseReference Parent => string.IsNullOrEmpty(path) ? null : new DatabaseReference(database, GetParentPath(path));
        
        internal DatabaseReference(FirebaseDatabase database, string path)
        {
            this.database = database;
            this.path = path?.Trim('/') ?? "";
        }
        
        public DatabaseReference Child(string pathString)
        {
            var childPath = string.IsNullOrEmpty(path) ? pathString : $"{path}/{pathString}";
            return new DatabaseReference(database, childPath);
        }
        
        public async Task<T> GetValueAsync<T>()
        {
            return await database.GetAsync<T>(path);
        }
        
        public async Task SetValueAsync<T>(T value)
        {
            await database.SetAsync(path, value);
        }
        
        public async Task UpdateChildrenAsync<T>(T value)
        {
            await database.UpdateAsync(path, value);
        }
        
        public async Task<DatabaseReference> PushAsync<T>(T value)
        {
            var key = await database.PushAsync(path, value);
            return Child(key);
        }
        
        public async Task RemoveValueAsync()
        {
            await database.DeleteAsync(path);
        }
        
        private string GetParentPath(string childPath)
        {
            var lastSlash = childPath.LastIndexOf('/');
            return lastSlash > 0 ? childPath.Substring(0, lastSlash) : "";
        }
    }
    
    public class DatabaseQuery
    {
        public string OrderBy { get; set; }
        public int? LimitToFirst { get; set; }
        public int? LimitToLast { get; set; }
        public string StartAt { get; set; }
        public string EndAt { get; set; }
        public string EqualTo { get; set; }
        public bool Shallow { get; set; }
        
        public static DatabaseQuery OrderByChild(string path)
        {
            return new DatabaseQuery { OrderBy = path };
        }
        
        public static DatabaseQuery OrderByKey()
        {
            return new DatabaseQuery { OrderBy = "$key" };
        }
        
        public static DatabaseQuery OrderByValue()
        {
            return new DatabaseQuery { OrderBy = "$value" };
        }
        
        public DatabaseQuery LimitToFirst(int limit)
        {
            LimitToFirst = limit;
            return this;
        }
        
        public DatabaseQuery LimitToLast(int limit)
        {
            LimitToLast = limit;
            return this;
        }
        
        public DatabaseQuery StartAt(string value)
        {
            StartAt = value;
            return this;
        }
        
        public DatabaseQuery EndAt(string value)
        {
            EndAt = value;
            return this;
        }
        
        public DatabaseQuery EqualTo(string value)
        {
            EqualTo = value;
            return this;
        }
    }
    
    [Serializable]
    internal class PushResponse
    {
        public string name;
    }
}