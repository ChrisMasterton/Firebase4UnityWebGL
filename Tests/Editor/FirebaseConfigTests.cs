using NUnit.Framework;
using Firebase.Core;

namespace Firebase.Tests.Editor
{
    public class FirebaseConfigTests
    {
        private const string TestApiKey = "test-api-key";
        private const string TestAuthDomain = "test-project.firebaseapp.com";
        private const string TestDatabaseURL = "https://test-project-default-rtdb.firebaseio.com";
        private const string TestProjectId = "test-project-id";
        private const string TestStorageBucket = "test-project.appspot.com";
        private const string TestMessagingSenderId = "123456789";
        private const string TestAppId = "1:123456789:web:abcdef123456";

        [Test]
        public void FirebaseConfig_ValidConfiguration_IsValid()
        {
            var config = new FirebaseConfig(
                TestApiKey,
                TestAuthDomain,
                TestDatabaseURL,
                TestProjectId,
                TestStorageBucket,
                TestMessagingSenderId,
                TestAppId
            );

            Assert.IsTrue(config.IsValid());
        }

        [Test]
        public void FirebaseConfig_MissingApiKey_IsInvalid()
        {
            var config = new FirebaseConfig(
                null,
                TestAuthDomain,
                TestDatabaseURL,
                TestProjectId,
                TestStorageBucket,
                TestMessagingSenderId,
                TestAppId
            );

            Assert.IsFalse(config.IsValid());
        }

        [Test]
        public void FirebaseConfig_MissingProjectId_IsInvalid()
        {
            var config = new FirebaseConfig(
                TestApiKey,
                TestAuthDomain,
                TestDatabaseURL,
                null,
                TestStorageBucket,
                TestMessagingSenderId,
                TestAppId
            );

            Assert.IsFalse(config.IsValid());
        }

        [Test]
        public void FirebaseConfig_MissingDatabaseURL_IsInvalid()
        {
            var config = new FirebaseConfig(
                TestApiKey,
                TestAuthDomain,
                null,
                TestProjectId,
                TestStorageBucket,
                TestMessagingSenderId,
                TestAppId
            );

            Assert.IsFalse(config.IsValid());
        }

        [Test]
        public void FirebaseConfig_PropertiesAccessible()
        {
            var config = new FirebaseConfig(
                TestApiKey,
                TestAuthDomain,
                TestDatabaseURL,
                TestProjectId,
                TestStorageBucket,
                TestMessagingSenderId,
                TestAppId
            );

            Assert.AreEqual(TestApiKey, config.ApiKey);
            Assert.AreEqual(TestAuthDomain, config.AuthDomain);
            Assert.AreEqual(TestDatabaseURL, config.DatabaseURL);
            Assert.AreEqual(TestProjectId, config.ProjectId);
            Assert.AreEqual(TestStorageBucket, config.StorageBucket);
            Assert.AreEqual(TestMessagingSenderId, config.MessagingSenderId);
            Assert.AreEqual(TestAppId, config.AppId);
        }

        [Test]
        public void FirebaseConfig_BaseUrlsGeneratedCorrectly()
        {
            var config = new FirebaseConfig(
                TestApiKey,
                TestAuthDomain,
                TestDatabaseURL,
                TestProjectId,
                TestStorageBucket,
                TestMessagingSenderId,
                TestAppId
            );

            Assert.AreEqual("https://identitytoolkit.googleapis.com/v1", config.AuthBaseUrl);
            Assert.AreEqual(TestDatabaseURL, config.DatabaseBaseUrl);
            Assert.AreEqual($"https://firestore.googleapis.com/v1/projects/{TestProjectId}/databases/(default)/documents", config.FirestoreBaseUrl);
            Assert.AreEqual($"https://firebasestorage.googleapis.com/v0/b/{TestStorageBucket}/o", config.StorageBaseUrl);
            Assert.AreEqual("https://fcm.googleapis.com/fcm/send", config.FcmBaseUrl);
        }

        [Test]
        public void FirebaseConfig_JsonSerialization_WorksCorrectly()
        {
            var config = new FirebaseConfig(
                TestApiKey,
                TestAuthDomain,
                TestDatabaseURL,
                TestProjectId,
                TestStorageBucket,
                TestMessagingSenderId,
                TestAppId
            );

            var json = config.ToJson();
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains(TestApiKey));
            Assert.IsTrue(json.Contains(TestProjectId));

            var deserializedConfig = FirebaseConfig.CreateFromJson(json);
            Assert.IsNotNull(deserializedConfig);
            Assert.AreEqual(config.ApiKey, deserializedConfig.ApiKey);
            Assert.AreEqual(config.ProjectId, deserializedConfig.ProjectId);
        }
    }
}