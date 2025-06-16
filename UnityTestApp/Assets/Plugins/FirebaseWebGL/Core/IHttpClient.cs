using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Firebase.Core
{
    public interface IHttpClient
    {
        Task<HttpResponse> SendRequestAsync(string url, string method = "GET", string jsonData = null, Dictionary<string, string> headers = null);
    }

    [Serializable]
    public class HttpResponse
    {
        public bool IsSuccess { get; set; }
        public long ResponseCode { get; set; }
        public string Text { get; set; }
        public byte[] Data { get; set; }
        public string Error { get; set; }
        public Dictionary<string, string> ResponseHeaders { get; set; }

        public HttpResponse()
        {
            ResponseHeaders = new Dictionary<string, string>();
        }
    }
}