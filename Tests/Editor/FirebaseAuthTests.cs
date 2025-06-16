using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Firebase.Core;
using Firebase.Tests.Editor.Mocks;
using Firebase.Tests.Editor.TestUtilities;

namespace Firebase.Tests.Editor
{
    public class FirebaseAuthTests
    {
        private FirebaseConfig testConfig;
        private MockHttpClient mockHttpClient;
        private FirebaseAuth firebaseAuth;

        [SetUp]
        public void SetUp()
        {
            testConfig = FirebaseTestData.ValidConfig;
            mockHttpClient = new MockHttpClient();
            firebaseAuth = new FirebaseAuth(testConfig, mockHttpClient);
        }

        [Test]
        public void Constructor_ValidConfig_InitializesCorrectly()
        {
            var auth = new FirebaseAuth(testConfig);
            
            Assert.IsNotNull(auth);
            Assert.IsFalse(auth.IsSignedIn);
            Assert.IsNull(auth.CurrentUser);
        }

        [Test]
        public void Constructor_NullConfig_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => {
                new FirebaseAuth(null);
            });
        }

        [Test]
        public async Task SignInWithEmailAndPasswordAsync_ValidCredentials_ReturnsUser()
        {
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
            
            var user = await firebaseAuth.SignInWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.ValidEmail, 
                FirebaseTestData.TestUsers.ValidPassword);
            
            Assert.IsNotNull(user);
            Assert.AreEqual("test-user-id", user.Uid);
            Assert.AreEqual("test@example.com", user.Email);
            Assert.IsTrue(firebaseAuth.IsSignedIn);
            Assert.AreEqual(user, firebaseAuth.CurrentUser);
        }

        [Test]
        public async Task SignInWithEmailAndPasswordAsync_ValidCredentials_SendsCorrectRequest()
        {
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
            
            await firebaseAuth.SignInWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.ValidEmail, 
                FirebaseTestData.TestUsers.ValidPassword);
            
            var request = mockHttpClient.GetLastRequest();
            TestHelpers.AssertValidHttpRequest(request, "POST");
            TestHelpers.AssertUrlContains(request.Url, "identitytoolkit.googleapis.com", "accounts:signInWithPassword");
            TestHelpers.AssertJsonContent(request, FirebaseTestData.TestUsers.ValidEmail);
            TestHelpers.AssertJsonContent(request, FirebaseTestData.TestUsers.ValidPassword);
        }

        [Test]
        public void SignInWithEmailAndPasswordAsync_InvalidCredentials_ThrowsFirebaseException()
        {
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ErrorResponse_WrongPassword, false);
            
            TestHelpers.AssertFirebaseException(async () => {
                await firebaseAuth.SignInWithEmailAndPasswordAsync(
                    FirebaseTestData.TestUsers.ValidEmail, 
                    FirebaseTestData.TestUsers.InvalidPassword);
            }, "400");
        }

        [Test]
        public async Task CreateUserWithEmailAndPasswordAsync_ValidCredentials_ReturnsUser()
        {
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignUpResponse);
            
            var user = await firebaseAuth.CreateUserWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.NewUserEmail, 
                FirebaseTestData.TestUsers.NewUserPassword);
            
            Assert.IsNotNull(user);
            Assert.AreEqual("new-user-id", user.Uid);
            Assert.AreEqual("newuser@example.com", user.Email);
            Assert.IsTrue(firebaseAuth.IsSignedIn);
        }

        [Test]
        public async Task CreateUserWithEmailAndPasswordAsync_ValidCredentials_SendsCorrectRequest()
        {
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignUpResponse);
            
            await firebaseAuth.CreateUserWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.NewUserEmail, 
                FirebaseTestData.TestUsers.NewUserPassword);
            
            var request = mockHttpClient.GetLastRequest();
            TestHelpers.AssertValidHttpRequest(request, "POST");
            TestHelpers.AssertUrlContains(request.Url, "identitytoolkit.googleapis.com", "accounts:signUp");
            TestHelpers.AssertJsonContent(request, FirebaseTestData.TestUsers.NewUserEmail);
        }

        [Test]
        public void CreateUserWithEmailAndPasswordAsync_InvalidEmail_ThrowsFirebaseException()
        {
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ErrorResponse_InvalidEmail, false);
            
            TestHelpers.AssertFirebaseException(async () => {
                await firebaseAuth.CreateUserWithEmailAndPasswordAsync(
                    FirebaseTestData.TestUsers.InvalidEmail, 
                    FirebaseTestData.TestUsers.ValidPassword);
            }, "400");
        }

        [Test]
        public async Task SignInAnonymouslyAsync_Success_ReturnsAnonymousUser()
        {
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidAnonymousSignInResponse);
            
            var user = await firebaseAuth.SignInAnonymouslyAsync();
            
            Assert.IsNotNull(user);
            Assert.AreEqual("anonymous-user-id", user.Uid);
            Assert.IsTrue(user.IsAnonymous);
            Assert.IsTrue(firebaseAuth.IsSignedIn);
        }

        [Test]
        public async Task SignInAnonymouslyAsync_Success_SendsCorrectRequest()
        {
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidAnonymousSignInResponse);
            
            await firebaseAuth.SignInAnonymouslyAsync();
            
            var request = mockHttpClient.GetLastRequest();
            TestHelpers.AssertValidHttpRequest(request, "POST");
            TestHelpers.AssertUrlContains(request.Url, "identitytoolkit.googleapis.com", "accounts:signUp");
            TestHelpers.AssertJsonContent(request, "returnSecureToken");
        }

        [Test]
        public async Task GetIdTokenAsync_NotSignedIn_ThrowsFirebaseException()
        {
            TestHelpers.AssertFirebaseException(async () => {
                await firebaseAuth.GetIdTokenAsync();
            }, "not_signed_in");
        }

        [Test]
        public async Task GetIdTokenAsync_SignedIn_ReturnsToken()
        {
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
            await firebaseAuth.SignInWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.ValidEmail, 
                FirebaseTestData.TestUsers.ValidPassword);
            
            var token = await firebaseAuth.GetIdTokenAsync();
            
            Assert.IsNotNull(token);
            Assert.IsTrue(token.Contains("test-token"));
        }

        [Test]
        public void SignOut_SignedInUser_ClearsUserState()
        {
            bool stateChangedCalled = false;
            FirebaseUser stateChangedUser = null;
            
            firebaseAuth.StateChanged += (user) => {
                stateChangedCalled = true;
                stateChangedUser = user;
            };
            
            firebaseAuth.SignOut();
            
            Assert.IsFalse(firebaseAuth.IsSignedIn);
            Assert.IsNull(firebaseAuth.CurrentUser);
            Assert.IsTrue(stateChangedCalled);
            Assert.IsNull(stateChangedUser);
        }

        [Test]
        public async Task StateChanged_SignIn_TriggersEvent()
        {
            bool stateChangedCalled = false;
            FirebaseUser stateChangedUser = null;
            
            firebaseAuth.StateChanged += (user) => {
                stateChangedCalled = true;
                stateChangedUser = user;
            };
            
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
            await firebaseAuth.SignInWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.ValidEmail, 
                FirebaseTestData.TestUsers.ValidPassword);
            
            Assert.IsTrue(stateChangedCalled);
            Assert.IsNotNull(stateChangedUser);
            Assert.AreEqual("test-user-id", stateChangedUser.Uid);
        }

        [Test]
        public async Task SendPasswordResetEmailAsync_ValidEmail_SendsRequest()
        {
            mockHttpClient.ConfigureFirebaseAuthResponse("{}");
            
            await firebaseAuth.SendPasswordResetEmailAsync(FirebaseTestData.TestUsers.ValidEmail);
            
            var request = mockHttpClient.GetLastRequest();
            TestHelpers.AssertValidHttpRequest(request, "POST");
            TestHelpers.AssertUrlContains(request.Url, "identitytoolkit.googleapis.com", "accounts:sendOobCode");
            TestHelpers.AssertJsonContent(request, "PASSWORD_RESET");
            TestHelpers.AssertJsonContent(request, FirebaseTestData.TestUsers.ValidEmail);
        }

        [Test]
        public async Task IsSignedIn_AfterSuccessfulSignIn_ReturnsTrue()
        {
            Assert.IsFalse(firebaseAuth.IsSignedIn);
            
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
            await firebaseAuth.SignInWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.ValidEmail, 
                FirebaseTestData.TestUsers.ValidPassword);
            
            Assert.IsTrue(firebaseAuth.IsSignedIn);
        }

        [Test]
        public async Task IsSignedIn_AfterSignOut_ReturnsFalse()
        {
            mockHttpClient.ConfigureFirebaseAuthResponse(FirebaseTestData.ValidSignInResponse);
            await firebaseAuth.SignInWithEmailAndPasswordAsync(
                FirebaseTestData.TestUsers.ValidEmail, 
                FirebaseTestData.TestUsers.ValidPassword);
            
            Assert.IsTrue(firebaseAuth.IsSignedIn);
            
            firebaseAuth.SignOut();
            
            Assert.IsFalse(firebaseAuth.IsSignedIn);
        }
    }
}