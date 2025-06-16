using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Core;

namespace Firebase.Tests.Editor.Mocks
{
    public class MockHttpClient : IHttpClient
    {
        private readonly Dictionary<string, MockHttpResponse> configuredResponses;
        private readonly List<HttpRequest> requestHistory;

        public IReadOnlyList<HttpRequest> RequestHistory => requestHistory.AsReadOnly();

        public MockHttpClient()
        {
            configuredResponses = new Dictionary<string, MockHttpResponse>();
            requestHistory = new List<HttpRequest>();
        }

        public MockHttpClient ConfigureResponse(string urlPattern, HttpResponse response)
        {
            configuredResponses[urlPattern] = new MockHttpResponse { Response = response };
            return this;
        }

        public MockHttpClient ConfigureResponse(string urlPattern, string responseText, long responseCode = 200, bool isSuccess = true)
        {
            var response = new HttpResponse
            {
                IsSuccess = isSuccess,
                ResponseCode = responseCode,
                Text = responseText
            };
            return ConfigureResponse(urlPattern, response);
        }

        public MockHttpClient ConfigureFirebaseAuthResponse(string responseText, bool isSuccess = true)
        {
            return ConfigureResponse("identitytoolkit.googleapis.com", responseText, isSuccess ? 200 : 400, isSuccess);
        }

        public MockHttpClient ConfigureDatabaseResponse(string responseText, bool isSuccess = true)
        {
            return ConfigureResponse("firebaseio.com", responseText, isSuccess ? 200 : 400, isSuccess);
        }

        public MockHttpClient ConfigureFirestoreResponse(string responseText, bool isSuccess = true)
        {
            return ConfigureResponse("firestore.googleapis.com", responseText, isSuccess ? 200 : 400, isSuccess);
        }

        public MockHttpClient ConfigureStorageResponse(string responseText, bool isSuccess = true)
        {
            return ConfigureResponse("firebasestorage.googleapis.com", responseText, isSuccess ? 200 : 400, isSuccess);
        }

        public async Task<HttpResponse> SendRequestAsync(string url, string method = "GET", string jsonData = null, Dictionary<string, string> headers = null)
        {
            var request = new HttpRequest
            {
                Url = url,
                Method = method,
                JsonData = jsonData,
                Headers = headers ?? new Dictionary<string, string>()
            };
            
            requestHistory.Add(request);

            await Task.Yield();

            foreach (var kvp in configuredResponses)
            {
                if (url.Contains(kvp.Key))
                {
                    return kvp.Value.Response;
                }
            }

            return new HttpResponse
            {
                IsSuccess = false,
                ResponseCode = 404,
                Error = "Mock response not configured",
                Text = "Mock response not configured for URL: " + url
            };
        }

        public void ClearHistory()
        {
            requestHistory.Clear();
        }

        public HttpRequest GetLastRequest()
        {
            return requestHistory.Count > 0 ? requestHistory[requestHistory.Count - 1] : null;
        }

        public HttpRequest GetRequest(int index)
        {
            return index >= 0 && index < requestHistory.Count ? requestHistory[index] : null;
        }

        public bool HasRequestToUrl(string urlPattern)
        {
            return requestHistory.Exists(r => r.Url.Contains(urlPattern));
        }
    }

    public class MockHttpResponse
    {
        public HttpResponse Response { get; set; }
    }

    public class HttpRequest
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public string JsonData { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
}