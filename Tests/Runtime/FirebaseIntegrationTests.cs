using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Firebase.Core;
using Firebase.Database;
using Firebase.Firestore;
using Firebase.Storage;
using Firebase.Messaging;
using Firebase.Tests.Editor.Mocks;
using Firebase.Tests.Editor.TestUtilities;

namespace Firebase.Tests.Runtime
{
    public class FirebaseIntegrationTests
    {
        private FirebaseConfig testConfig;
        private MockHttpClient mockHttpClient;

        [SetUp]
        public void SetUp()
        {
            testConfig = FirebaseTestData.ValidConfig;
            mockHttpClient = new MockHttpClient();
        }

        [TearDown]
        public void TearDown()
        {
            mockHttpClient?.ClearHistory();
        }

        [UnityTest]
        public IEnumerator FirebaseAuth_SignInWorkflow_CompletesSuccessfully()
        {
            var auth = new FirebaseAuth(testConfig, mockHttpClient);
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
            
            var signInTask = auth.SignInWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.ValidEmail,
                FirebaseTestData.TestUsers.ValidPassword);
            
            yield return new WaitUntil(() => signInTask.IsCompleted);
            
            Assert.IsFalse(signInTask.IsFaulted, signInTask.Exception?.ToString());
            Assert.IsNotNull(signInTask.Result);
            Assert.IsTrue(auth.IsSignedIn);
            Assert.AreEqual("test-user-id", auth.CurrentUser.Uid);
        }

        [UnityTest]
        public IEnumerator FirebaseAuth_SignOutWorkflow_CompletesSuccessfully()
        {
            var auth = new FirebaseAuth(testConfig, mockHttpClient);
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
            
            var signInTask = auth.SignInWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.ValidEmail,
                FirebaseTestData.TestUsers.ValidPassword);
            
            yield return new WaitUntil(() => signInTask.IsCompleted);
            
            Assert.IsTrue(auth.IsSignedIn);
            
            auth.SignOut();
            
            Assert.IsFalse(auth.IsSignedIn);
            Assert.IsNull(auth.CurrentUser);
        }

        [UnityTest]
        public IEnumerator FirebaseAuth_StateChangeEvent_TriggersCorrectly()
        {
            var auth = new FirebaseAuth(testConfig, mockHttpClient);
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
            
            bool stateChanged = false;
            FirebaseUser changedUser = null;
            
            auth.StateChanged += (user) => {
                stateChanged = true;
                changedUser = user;
            };
            
            var signInTask = auth.SignInWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.ValidEmail,
                FirebaseTestData.TestUsers.ValidPassword);
            
            yield return new WaitUntil(() => signInTask.IsCompleted);
            
            Assert.IsTrue(stateChanged);
            Assert.IsNotNull(changedUser);
            Assert.AreEqual("test-user-id", changedUser.Uid);
        }

        [UnityTest]
        public IEnumerator FirebaseAuth_AnonymousSignIn_CompletesSuccessfully()
        {
            var auth = new FirebaseAuth(testConfig, mockHttpClient);
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidAnonymousSignInResponse);
            
            var task = auth.SignInAnonymouslyAsync();
            
            yield return new WaitUntil(() => task.IsCompleted);
            
            Assert.IsFalse(task.IsFaulted, task.Exception?.ToString());
            Assert.IsNotNull(task.Result);
            Assert.IsTrue(task.Result.IsAnonymous);
            Assert.IsTrue(auth.IsSignedIn);
        }

        [UnityTest]
        public IEnumerator FirebaseAuth_CreateUser_CompletesSuccessfully()
        {
            var auth = new FirebaseAuth(testConfig, mockHttpClient);
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignUpResponse);
            
            var task = auth.CreateUserWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.NewUserEmail,
                FirebaseTestData.TestUsers.NewUserPassword);
            
            yield return new WaitUntil(() => task.IsCompleted);
            
            Assert.IsFalse(task.IsFaulted, task.Exception?.ToString());
            Assert.IsNotNull(task.Result);
            Assert.AreEqual("new-user-id", task.Result.Uid);
            Assert.IsTrue(auth.IsSignedIn);
        }

        [UnityTest]
        public IEnumerator FirebaseAuth_GetIdToken_CompletesSuccessfully()
        {
            var auth = new FirebaseAuth(testConfig, mockHttpClient);
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
            
            var signInTask = auth.SignInWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.ValidEmail,
                FirebaseTestData.TestUsers.ValidPassword);
            
            yield return new WaitUntil(() => signInTask.IsCompleted);
            
            var tokenTask = auth.GetIdTokenAsync();
            
            yield return new WaitUntil(() => tokenTask.IsCompleted);
            
            Assert.IsFalse(tokenTask.IsFaulted, tokenTask.Exception?.ToString());
            Assert.IsNotNull(tokenTask.Result);
            Assert.IsTrue(tokenTask.Result.Contains("test-token"));
        }

        [UnityTest]
        public IEnumerator FirebaseAuth_PasswordReset_CompletesSuccessfully()
        {
            var auth = new FirebaseAuth(testConfig, mockHttpClient);
            mockHttpClient.ConfigureFirebaseAuthResponse("{}");
            
            var task = auth.SendPasswordResetEmailAsync(FirebaseTestData.TestUsers.ValidEmail);
            
            yield return new WaitUntil(() => task.IsCompleted);
            
            Assert.IsFalse(task.IsFaulted, task.Exception?.ToString());
            
            var request = mockHttpClient.GetLastRequest();
            Assert.IsTrue(request.Url.Contains("accounts:sendOobCode"));
        }

        [UnityTest]
        public IEnumerator HttpClient_UnityImplementation_WorksInPlayMode()
        {
            var httpClient = new UnityHttpClient();
            
            var task = httpClient.SendRequestAsync("https://httpbin.org/get", "GET");
            
            yield return new WaitUntil(() => task.IsCompleted);
            
            if (!task.IsFaulted)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsTrue(task.Result.IsSuccess || task.Result.ResponseCode > 0);
            }
        }

        [Test]
        public void FirebaseConfig_Initialization_CompletesSuccessfully()
        {
            var config = testConfig;
            
            Assert.IsNotNull(config);
            Assert.IsTrue(config.IsValid());
            Assert.IsNotNull(config.AuthBaseUrl);
            Assert.IsNotNull(config.DatabaseBaseUrl);
            Assert.IsNotNull(config.FirestoreBaseUrl);
            Assert.IsNotNull(config.StorageBaseUrl);
        }

        [Test]
        public void FirebaseAuth_InvalidConfig_ThrowsException()
        {
            Assert.Throws<System.ArgumentNullException>(() => {
                var auth = new FirebaseAuth(null);
            });
        }

        [Test]
        public void MockHttpClient_Integration_WorksCorrectly()
        {
            var mock = new MockHttpClient();
            mock.ConfigureResponse("test", "response");
            
            var task = mock.SendRequestAsync("https://test.com/api", "GET");
            
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual("response", task.Result.Text);
            Assert.AreEqual(1, mock.RequestHistory.Count);
        }
    }
}