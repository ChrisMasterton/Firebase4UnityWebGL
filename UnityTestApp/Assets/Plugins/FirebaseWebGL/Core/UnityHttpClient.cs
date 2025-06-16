using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Firebase.Core
{
    public class UnityHttpClient : IHttpClient
    {
        public async Task<HttpResponse> SendRequestAsync(string url, string method = "GET", string jsonData = null, Dictionary<string, string> headers = null)
        {
            using var request = CreateUnityWebRequest(url, method, jsonData, headers);
            
            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            return CreateHttpResponse(request);
        }
        
        private UnityWebRequest CreateUnityWebRequest(string url, string method, string jsonData, Dictionary<string, string> headers)
        {
            var request = new UnityWebRequest(url, method);
            
            if (!string.IsNullOrEmpty(jsonData))
            {
                var bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }
            
            request.downloadHandler = new DownloadHandlerBuffer();
            
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }
            
            return request;
        }
        
        private HttpResponse CreateHttpResponse(UnityWebRequest request)
        {
            var response = new HttpResponse
            {
                IsSuccess = request.result == UnityWebRequest.Result.Success,
                ResponseCode = request.responseCode,
                Text = request.downloadHandler?.text,
                Data = request.downloadHandler?.data,
                Error = request.error
            };
            
            if (request.GetResponseHeaders() != null)
            {
                foreach (var header in request.GetResponseHeaders())
                {
                    response.ResponseHeaders[header.Key] = header.Value;
                }
            }
            
            return response;
        }
    }
}