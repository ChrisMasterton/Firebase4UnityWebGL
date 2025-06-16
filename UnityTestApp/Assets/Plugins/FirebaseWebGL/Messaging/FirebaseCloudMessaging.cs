using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Firebase.Core;
using Firebase.Models;

namespace Firebase.Messaging
{
    public class FirebaseCloudMessaging
    {
        private readonly FirebaseConfig config;
        private readonly string serverKey;
        
        public FirebaseCloudMessaging(FirebaseConfig config, string serverKey)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.serverKey = serverKey ?? throw new ArgumentNullException(nameof(serverKey));
        }
        
        public async Task<FcmResponse> SendToTokenAsync(string token, FcmNotification notification, Dictionary<string, string> data = null)
        {
            var message = new FcmMessage
            {
                to = token,
                notification = notification,
                data = data,
                priority = "high"
            };
            
            return await SendMessageAsync(message);
        }
        
        public async Task<FcmResponse> SendToTokensAsync(string[] tokens, FcmNotification notification, Dictionary<string, string> data = null)
        {
            if (tokens == null || tokens.Length == 0)
                throw new ArgumentException("Tokens array cannot be null or empty");
                
            if (tokens.Length > 1000)
                throw new ArgumentException("Cannot send to more than 1000 tokens at once");
                
            var message = new FcmMessage
            {
                registration_ids = tokens,
                notification = notification,
                data = data,
                priority = "high"
            };
            
            return await SendMessageAsync(message);
        }
        
        public async Task<FcmResponse> SendToTopicAsync(string topic, FcmNotification notification, Dictionary<string, string> data = null)
        {
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentException("Topic cannot be null or empty");
                
            topic = topic.StartsWith("/topics/") ? topic : $"/topics/{topic}";
            
            var message = new FcmMessage
            {
                to = topic,
                notification = notification,
                data = data,
                priority = "high"
            };
            
            return await SendMessageAsync(message);
        }
        
        public async Task<FcmResponse> SendDataMessageAsync(string token, Dictionary<string, string> data)
        {
            if (data == null || data.Count == 0)
                throw new ArgumentException("Data cannot be null or empty");
                
            var message = new FcmMessage
            {
                to = token,
                data = data,
                priority = "high",
                content_available = true
            };
            
            return await SendMessageAsync(message);
        }
        
        public async Task<FcmResponse> SendSilentNotificationAsync(string token, Dictionary<string, string> data)
        {
            var message = new FcmMessage
            {
                to = token,
                data = data,
                priority = "high",
                content_available = true
            };
            
            return await SendMessageAsync(message);
        }
        
        public async Task<FcmResponse> SendWithConditionAsync(string condition, FcmNotification notification, Dictionary<string, string> data = null)
        {
            if (string.IsNullOrEmpty(condition))
                throw new ArgumentException("Condition cannot be null or empty");
                
            var message = new FcmMessage
            {
                to = condition,
                notification = notification,
                data = data,
                priority = "high"
            };
            
            return await SendMessageAsync(message);
        }
        
        public async Task<FcmResponse> SendMessageAsync(FcmMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
                
            var json = FirebaseUtils.ToJson(message);
            
            using var request = FirebaseUtils.CreateRequest(config.FcmBaseUrl, "POST", json);
            request.SetRequestHeader("Authorization", $"key={serverKey}");
            
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            return FirebaseUtils.FromJson<FcmResponse>(request.downloadHandler.text);
        }
        
        public async Task SubscribeToTopicAsync(string token, string topic)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token cannot be null or empty");
                
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentException("Topic cannot be null or empty");
                
            var url = $"https://iid.googleapis.com/iid/v1/{token}/rel/topics/{topic}";
            
            using var request = FirebaseUtils.CreateRequest(url, "POST");
            request.SetRequestHeader("Authorization", $"key={serverKey}");
            
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
        }
        
        public async Task UnsubscribeFromTopicAsync(string token, string topic)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token cannot be null or empty");
                
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentException("Topic cannot be null or empty");
                
            var url = $"https://iid.googleapis.com/iid/v1/{token}/rel/topics/{topic}";
            
            using var request = FirebaseUtils.CreateRequest(url, "DELETE");
            request.SetRequestHeader("Authorization", $"key={serverKey}");
            
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
        }
        
        public async Task SubscribeTokensToTopicAsync(string[] tokens, string topic)
        {
            if (tokens == null || tokens.Length == 0)
                throw new ArgumentException("Tokens array cannot be null or empty");
                
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentException("Topic cannot be null or empty");
                
            if (tokens.Length > 1000)
                throw new ArgumentException("Cannot subscribe more than 1000 tokens at once");
                
            var url = "https://iid.googleapis.com/iid/v1:batchAdd";
            var requestData = new
            {
                to = $"/topics/{topic}",
                registration_tokens = tokens
            };
            
            var json = FirebaseUtils.ToJson(requestData);
            
            using var request = FirebaseUtils.CreateRequest(url, "POST", json);
            request.SetRequestHeader("Authorization", $"key={serverKey}");
            
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
        }
        
        public async Task UnsubscribeTokensFromTopicAsync(string[] tokens, string topic)
        {
            if (tokens == null || tokens.Length == 0)
                throw new ArgumentException("Tokens array cannot be null or empty");
                
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentException("Topic cannot be null or empty");
                
            if (tokens.Length > 1000)
                throw new ArgumentException("Cannot unsubscribe more than 1000 tokens at once");
                
            var url = "https://iid.googleapis.com/iid/v1:batchRemove";
            var requestData = new
            {
                to = $"/topics/{topic}",
                registration_tokens = tokens
            };
            
            var json = FirebaseUtils.ToJson(requestData);
            
            using var request = FirebaseUtils.CreateRequest(url, "POST", json);
            request.SetRequestHeader("Authorization", $"key={serverKey}");
            
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
        }
        
        public FcmNotification CreateNotification(string title, string body, string icon = null, string sound = "default")
        {
            return new FcmNotification
            {
                title = title,
                body = body,
                icon = icon,
                sound = sound
            };
        }
        
        public FcmMessage CreateMessage()
        {
            return new FcmMessage
            {
                priority = "high",
                time_to_live = 3600
            };
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
    
    public static class FcmConditions
    {
        public static string And(params string[] topics)
        {
            if (topics == null || topics.Length == 0)
                return "";
                
            var conditions = Array.ConvertAll(topics, topic => $"'{topic}' in topics");
            return string.Join(" && ", conditions);
        }
        
        public static string Or(params string[] topics)
        {
            if (topics == null || topics.Length == 0)
                return "";
                
            var conditions = Array.ConvertAll(topics, topic => $"'{topic}' in topics");
            return string.Join(" || ", conditions);
        }
        
        public static string Not(string topic)
        {
            return $"!'{topic}' in topics";
        }
    }
}