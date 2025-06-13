using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Firebase.Core;
using Firebase.Models;

namespace Firebase.Storage
{
    public class FirebaseStorage
    {
        private readonly FirebaseConfig config;
        private readonly FirebaseAuth auth;
        
        public FirebaseStorage(FirebaseConfig config, FirebaseAuth auth = null)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.auth = auth;
        }
        
        public StorageReference GetReference(string path = "")
        {
            return new StorageReference(this, path);
        }
        
        public async Task<string> UploadBytesAsync(string path, byte[] data, string contentType = "application/octet-stream", Dictionary<string, string> metadata = null)
        {
            path = path.TrimStart('/');
            var url = $"{config.StorageBaseUrl}/{FirebaseUtils.EncodeKey(path)}";
            
            var parameters = new Dictionary<string, string>
            {
                { "uploadType", "media" },
                { "name", path }
            };
            
            url += FirebaseUtils.BuildQueryParams(parameters);
            
            using var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", contentType);
            
            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    request.SetRequestHeader($"x-goog-meta-{kvp.Key}", kvp.Value);
                }
            }
            
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            var response = FirebaseUtils.FromJson<StorageUploadResponse>(request.downloadHandler.text);
            return GetDownloadUrl(path, response.downloadTokens);
        }
        
        public async Task<string> UploadFileAsync(string path, string filePath, string contentType = null, Dictionary<string, string> metadata = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");
                
            var data = File.ReadAllBytes(filePath);
            contentType = contentType ?? GetContentType(filePath);
            
            return await UploadBytesAsync(path, data, contentType, metadata);
        }
        
        public async Task<string> UploadTextureAsync(string path, Texture2D texture, string format = "PNG", Dictionary<string, string> metadata = null)
        {
            byte[] data;
            string contentType;
            
            switch (format.ToUpper())
            {
                case "PNG":
                    data = texture.EncodeToPNG();
                    contentType = "image/png";
                    break;
                case "JPG":
                case "JPEG":
                    data = texture.EncodeToJPG();
                    contentType = "image/jpeg";
                    break;
                default:
                    throw new ArgumentException($"Unsupported format: {format}");
            }
            
            return await UploadBytesAsync(path, data, contentType, metadata);
        }
        
        public async Task<byte[]> DownloadBytesAsync(string path)
        {
            path = path.TrimStart('/');
            var url = $"{config.StorageBaseUrl}/{FirebaseUtils.EncodeKey(path)}";
            
            var parameters = new Dictionary<string, string> { { "alt", "media" } };
            url += FirebaseUtils.BuildQueryParams(parameters);
            
            using var request = FirebaseUtils.CreateRequest(url, "GET");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            return request.downloadHandler.data;
        }
        
        public async Task<string> DownloadTextAsync(string path)
        {
            var data = await DownloadBytesAsync(path);
            return System.Text.Encoding.UTF8.GetString(data);
        }
        
        public async Task<Texture2D> DownloadTextureAsync(string path)
        {
            var data = await DownloadBytesAsync(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            return texture;
        }
        
        public async Task<string> GetDownloadUrlAsync(string path)
        {
            path = path.TrimStart('/');
            var url = $"{config.StorageBaseUrl}/{FirebaseUtils.EncodeKey(path)}";
            
            using var request = FirebaseUtils.CreateRequest(url, "GET");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            var storageObject = FirebaseUtils.FromJson<StorageObject>(request.downloadHandler.text);
            return GetDownloadUrl(path, storageObject.downloadTokens);
        }
        
        public async Task<StorageObject> GetMetadataAsync(string path)
        {
            path = path.TrimStart('/');
            var url = $"{config.StorageBaseUrl}/{FirebaseUtils.EncodeKey(path)}";
            
            using var request = FirebaseUtils.CreateRequest(url, "GET");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            return FirebaseUtils.FromJson<StorageObject>(request.downloadHandler.text);
        }
        
        public async Task<StorageObject> UpdateMetadataAsync(string path, Dictionary<string, string> metadata)
        {
            path = path.TrimStart('/');
            var url = $"{config.StorageBaseUrl}/{FirebaseUtils.EncodeKey(path)}";
            
            var updateData = new { metadata = metadata };
            var json = FirebaseUtils.ToJson(updateData);
            
            using var request = FirebaseUtils.CreateRequest(url, "PATCH", json);
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            return FirebaseUtils.FromJson<StorageObject>(request.downloadHandler.text);
        }
        
        public async Task DeleteAsync(string path)
        {
            path = path.TrimStart('/');
            var url = $"{config.StorageBaseUrl}/{FirebaseUtils.EncodeKey(path)}";
            
            using var request = FirebaseUtils.CreateRequest(url, "DELETE");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
        }
        
        public async Task<StorageObject[]> ListAsync(string path = "", int maxResults = 1000, string pageToken = null)
        {
            var url = $"{config.StorageBaseUrl}";
            
            var parameters = new Dictionary<string, string>
            {
                { "maxResults", maxResults.ToString() }
            };
            
            if (!string.IsNullOrEmpty(path))
                parameters["prefix"] = path.TrimStart('/');
                
            if (!string.IsNullOrEmpty(pageToken))
                parameters["pageToken"] = pageToken;
                
            url += FirebaseUtils.BuildQueryParams(parameters);
            
            using var request = FirebaseUtils.CreateRequest(url, "GET");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            var response = FirebaseUtils.FromJson<StorageListResponse>(request.downloadHandler.text);
            return response.items ?? new StorageObject[0];
        }
        
        private string GetDownloadUrl(string path, string downloadToken)
        {
            return $"https://firebasestorage.googleapis.com/v0/b/{config.StorageBucket}/o/{FirebaseUtils.EncodeKey(path)}?alt=media&token={downloadToken}";
        }
        
        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".txt" => "text/plain",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".pdf" => "application/pdf",
                ".zip" => "application/zip",
                ".mp3" => "audio/mpeg",
                ".mp4" => "video/mp4",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                _ => "application/octet-stream"
            };
        }
        
        private async Task AddAuthIfNeeded(UnityWebRequest request)
        {
            if (auth != null && auth.IsSignedIn)
            {
                try
                {
                    var token = await auth.GetIdTokenAsync();
                    FirebaseUtils.SetAuthHeader(request, token);
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
    
    public class StorageReference
    {
        private readonly FirebaseStorage storage;
        private readonly string path;
        
        public string Name => Path.GetFileName(path);
        public string Path => path;
        public string Bucket => storage.config.StorageBucket;
        public StorageReference Parent => string.IsNullOrEmpty(path) ? null : new StorageReference(storage, GetParentPath(path));
        public StorageReference Root => new StorageReference(storage, "");
        
        internal StorageReference(FirebaseStorage storage, string path)
        {
            this.storage = storage;
            this.path = path?.Trim('/') ?? "";
        }
        
        public StorageReference Child(string pathString)
        {
            var childPath = string.IsNullOrEmpty(path) ? pathString : $"{path}/{pathString}";
            return new StorageReference(storage, childPath);
        }
        
        public async Task<string> PutBytesAsync(byte[] data, string contentType = "application/octet-stream", Dictionary<string, string> metadata = null)
        {
            return await storage.UploadBytesAsync(path, data, contentType, metadata);
        }
        
        public async Task<string> PutFileAsync(string filePath, string contentType = null, Dictionary<string, string> metadata = null)
        {
            return await storage.UploadFileAsync(path, filePath, contentType, metadata);
        }
        
        public async Task<string> PutTextureAsync(Texture2D texture, string format = "PNG", Dictionary<string, string> metadata = null)
        {
            return await storage.UploadTextureAsync(path, texture, format, metadata);
        }
        
        public async Task<byte[]> GetBytesAsync()
        {
            return await storage.DownloadBytesAsync(path);
        }
        
        public async Task<string> GetTextAsync()
        {
            return await storage.DownloadTextAsync(path);
        }
        
        public async Task<Texture2D> GetTextureAsync()
        {
            return await storage.DownloadTextureAsync(path);
        }
        
        public async Task<string> GetDownloadUrlAsync()
        {
            return await storage.GetDownloadUrlAsync(path);
        }
        
        public async Task<StorageObject> GetMetadataAsync()
        {
            return await storage.GetMetadataAsync(path);
        }
        
        public async Task<StorageObject> UpdateMetadataAsync(Dictionary<string, string> metadata)
        {
            return await storage.UpdateMetadataAsync(path, metadata);
        }
        
        public async Task DeleteAsync()
        {
            await storage.DeleteAsync(path);
        }
        
        public async Task<StorageReference[]> ListAllAsync()
        {
            var objects = await storage.ListAsync(path);
            return Array.ConvertAll(objects, obj => new StorageReference(storage, obj.name));
        }
        
        private string GetParentPath(string childPath)
        {
            var lastSlash = childPath.LastIndexOf('/');
            return lastSlash > 0 ? childPath.Substring(0, lastSlash) : "";
        }
    }
    
    [Serializable]
    internal class StorageListResponse
    {
        public StorageObject[] items;
        public string nextPageToken;
    }
}