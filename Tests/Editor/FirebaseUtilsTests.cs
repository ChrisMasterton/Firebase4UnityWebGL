using System;
using System.Collections.Generic;
using NUnit.Framework;
using Firebase.Core;
using Firebase.Tests.Editor.TestUtilities;

namespace Firebase.Tests.Editor
{
    public class FirebaseUtilsTests
    {
        [Test]
        public void ToJson_ValidObject_ReturnsJsonString()
        {
            var testUser = new FirebaseTestData.TestUserData("John Doe", "john@example.com", 30);
            
            var json = FirebaseUtils.ToJson(testUser);
            
            Assert.IsNotNull(json);
            TestHelpers.AssertValidJson(json);
            Assert.IsTrue(json.Contains("John Doe"));
            Assert.IsTrue(json.Contains("john@example.com"));
            Assert.IsTrue(json.Contains("30"));
        }

        [Test]
        public void FromJson_ValidJsonString_ReturnsDeserializedObject()
        {
            var originalUser = new FirebaseTestData.TestUserData("Jane Smith", "jane@example.com", 25);
            var json = FirebaseUtils.ToJson(originalUser);
            
            var deserializedUser = FirebaseUtils.FromJson<FirebaseTestData.TestUserData>(json);
            
            Assert.IsNotNull(deserializedUser);
            Assert.AreEqual(originalUser.name, deserializedUser.name);
            Assert.AreEqual(originalUser.email, deserializedUser.email);
            Assert.AreEqual(originalUser.age, deserializedUser.age);
        }

        [Test]
        public void FromJson_InvalidJson_ThrowsException()
        {
            var invalidJson = "{ invalid json }";
            
            Assert.Throws<ArgumentException>(() => {
                FirebaseUtils.FromJson<FirebaseTestData.TestUserData>(invalidJson);
            });
        }

        [Test]
        public void EncodeKey_StringWithSpecialCharacters_ReturnsEncodedString()
        {
            var keyWithSpaces = "user name with spaces";
            var keyWithSymbols = "user@example.com";
            
            var encodedSpaces = FirebaseUtils.EncodeKey(keyWithSpaces);
            var encodedSymbols = FirebaseUtils.EncodeKey(keyWithSymbols);
            
            Assert.IsNotNull(encodedSpaces);
            Assert.IsNotNull(encodedSymbols);
            Assert.AreNotEqual(keyWithSpaces, encodedSpaces);
            Assert.AreNotEqual(keyWithSymbols, encodedSymbols);
            Assert.IsFalse(encodedSpaces.Contains(" "));
            Assert.IsFalse(encodedSymbols.Contains("@"));
        }

        [Test]
        public void BuildQueryParams_EmptyDictionary_ReturnsEmptyString()
        {
            var emptyParams = new Dictionary<string, string>();
            
            var result = FirebaseUtils.BuildQueryParams(emptyParams);
            
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void BuildQueryParams_NullDictionary_ReturnsEmptyString()
        {
            var result = FirebaseUtils.BuildQueryParams(null);
            
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void BuildQueryParams_SingleParameter_ReturnsFormattedQuery()
        {
            var parameters = new Dictionary<string, string>
            {
                { "orderBy", "name" }
            };
            
            var result = FirebaseUtils.BuildQueryParams(parameters);
            
            Assert.AreEqual("?orderBy=name", result);
        }

        [Test]
        public void BuildQueryParams_MultipleParameters_ReturnsFormattedQuery()
        {
            var parameters = new Dictionary<string, string>
            {
                { "orderBy", "name" },
                { "limitToFirst", "10" },
                { "startAt", "A" }
            };
            
            var result = FirebaseUtils.BuildQueryParams(parameters);
            
            Assert.IsTrue(result.StartsWith("?"));
            Assert.IsTrue(result.Contains("orderBy=name"));
            Assert.IsTrue(result.Contains("limitToFirst=10"));
            Assert.IsTrue(result.Contains("startAt=A"));
            Assert.IsTrue(result.Contains("&"));
        }

        [Test]
        public void BuildQueryParams_ParametersWithSpecialCharacters_EncodesCorrectly()
        {
            var parameters = new Dictionary<string, string>
            {
                { "filter", "name == \"John Doe\"" },
                { "orderBy", "created@timestamp" }
            };
            
            var result = FirebaseUtils.BuildQueryParams(parameters);
            
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StartsWith("?"));
            Assert.IsFalse(result.Contains("\""));
            Assert.IsFalse(result.Contains("@"));
        }

        [Test]
        public void GenerateRandomString_DefaultLength_Returns20Characters()
        {
            var randomString = FirebaseUtils.GenerateRandomString();
            
            Assert.IsNotNull(randomString);
            Assert.AreEqual(20, randomString.Length);
        }

        [Test]
        public void GenerateRandomString_CustomLength_ReturnsCorrectLength()
        {
            var customLength = 15;
            
            var randomString = FirebaseUtils.GenerateRandomString(customLength);
            
            Assert.IsNotNull(randomString);
            Assert.AreEqual(customLength, randomString.Length);
        }

        [Test]
        public void GenerateRandomString_MultipleCallsReturnDifferentStrings()
        {
            var string1 = FirebaseUtils.GenerateRandomString();
            var string2 = FirebaseUtils.GenerateRandomString();
            
            Assert.AreNotEqual(string1, string2);
        }

        [Test]
        public void GetUnixTimestamp_ReturnsValidTimestamp()
        {
            var timestamp = FirebaseUtils.GetUnixTimestamp();
            
            TestHelpers.AssertValidTimestamp(timestamp);
        }

        [Test]
        public void FromUnixTimestamp_ValidTimestamp_ReturnsCorrectDateTime()
        {
            var originalDateTime = DateTime.UtcNow;
            var timestamp = ((DateTimeOffset)originalDateTime).ToUnixTimeSeconds();
            
            var convertedDateTime = FirebaseUtils.FromUnixTimestamp(timestamp);
            
            Assert.AreEqual(originalDateTime.Date, convertedDateTime.Date);
            Assert.AreEqual(originalDateTime.Hour, convertedDateTime.Hour);
            Assert.AreEqual(originalDateTime.Minute, convertedDateTime.Minute);
        }

        [Test]
        public void FromUnixTimestamp_ZeroTimestamp_ReturnsEpoch()
        {
            var epochDateTime = FirebaseUtils.FromUnixTimestamp(0);
            
            Assert.AreEqual(1970, epochDateTime.Year);
            Assert.AreEqual(1, epochDateTime.Month);
            Assert.AreEqual(1, epochDateTime.Day);
        }
    }
}