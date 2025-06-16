using System;
using NUnit.Framework;
using Firebase.Core;
using Firebase.Tests.Editor.Mocks;

namespace Firebase.Tests.Editor.TestUtilities
{
    public static class TestHelpers
    {
        public static MockHttpClient CreateMockHttpClient()
        {
            return new MockHttpClient();
        }

        public static MockHttpClient CreateAuthenticatedMockClient()
        {
            return new MockHttpClient()
                .ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
        }

        public static void AssertValidHttpRequest(HttpRequest request, string expectedMethod = "GET")
        {
            Assert.IsNotNull(request, "Request should not be null");
            Assert.IsNotNull(request.Url, "Request URL should not be null");
            Assert.AreEqual(expectedMethod, request.Method, $"Request method should be {expectedMethod}");
        }

        public static void AssertAuthenticationHeader(HttpRequest request, string expectedToken = null)
        {
            Assert.IsNotNull(request.Headers, "Request headers should not be null");
            
            if (!string.IsNullOrEmpty(expectedToken))
            {
                Assert.IsTrue(request.Headers.ContainsKey("Authorization"), "Request should contain Authorization header");
                Assert.AreEqual($"Bearer {expectedToken}", request.Headers["Authorization"], "Authorization header should contain the correct token");
            }
        }

        public static void AssertContentTypeHeader(HttpRequest request, string expectedContentType = "application/json")
        {
            Assert.IsNotNull(request.Headers, "Request headers should not be null");
            Assert.IsTrue(request.Headers.ContainsKey("Content-Type"), "Request should contain Content-Type header");
            Assert.AreEqual(expectedContentType, request.Headers["Content-Type"], $"Content-Type should be {expectedContentType}");
        }

        public static void AssertJsonContent(HttpRequest request, string expectedJsonSubstring)
        {
            Assert.IsNotNull(request.JsonData, "Request JSON data should not be null");
            Assert.IsTrue(request.JsonData.Contains(expectedJsonSubstring), 
                $"Request JSON should contain '{expectedJsonSubstring}'. Actual: {request.JsonData}");
        }

        public static void AssertFirebaseException(Action action, string expectedErrorCode)
        {
            var exception = Assert.Throws<FirebaseException>(action.Invoke);
            Assert.AreEqual(expectedErrorCode, exception.ErrorCode, $"Exception error code should be {expectedErrorCode}");
        }

        public static void AssertUrlContains(string url, params string[] expectedSubstrings)
        {
            Assert.IsNotNull(url, "URL should not be null");
            
            foreach (var substring in expectedSubstrings)
            {
                Assert.IsTrue(url.Contains(substring), 
                    $"URL should contain '{substring}'. Actual URL: {url}");
            }
        }

        public static void AssertValidFirebaseConfig(FirebaseConfig config)
        {
            Assert.IsNotNull(config, "Config should not be null");
            Assert.IsTrue(config.IsValid(), "Config should be valid");
            Assert.IsNotNull(config.ApiKey, "API Key should not be null");
            Assert.IsNotNull(config.ProjectId, "Project ID should not be null");
            Assert.IsNotNull(config.DatabaseURL, "Database URL should not be null");
        }

        public static void AssertValidTimestamp(long timestamp, long maxAgeSeconds = 60)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var age = Math.Abs(now - timestamp);
            
            Assert.IsTrue(age <= maxAgeSeconds, 
                $"Timestamp should be within {maxAgeSeconds} seconds of now. Age: {age} seconds");
        }

        public static void AssertValidEmail(string email)
        {
            Assert.IsNotNull(email, "Email should not be null");
            Assert.IsTrue(email.Contains("@"), "Email should contain '@' symbol");
            Assert.IsTrue(email.Contains("."), "Email should contain '.' symbol");
        }

        public static T DeserializeTestJson<T>(string json)
        {
            try
            {
                return FirebaseUtils.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to deserialize JSON: {ex.Message}. JSON: {json}");
                return default(T);
            }
        }

        public static string SerializeTestObject<T>(T obj)
        {
            try
            {
                return FirebaseUtils.ToJson(obj);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to serialize object: {ex.Message}. Object: {obj}");
                return null;
            }
        }

        public static void AssertValidJson(string json)
        {
            Assert.IsNotNull(json, "JSON should not be null");
            Assert.IsTrue(json.Trim().StartsWith("{") || json.Trim().StartsWith("["), 
                "JSON should start with '{' or '['");
            Assert.IsTrue(json.Trim().EndsWith("}") || json.Trim().EndsWith("]"), 
                "JSON should end with '}' or ']'");
        }
    }
}