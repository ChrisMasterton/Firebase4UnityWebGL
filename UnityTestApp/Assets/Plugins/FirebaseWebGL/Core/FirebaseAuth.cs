using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Firebase.Core
{
    public class FirebaseAuth
    {
        private readonly FirebaseConfig config;
        private readonly IHttpClient httpClient;
        private FirebaseUser currentUser;
        private string idToken;
        private string refreshToken;
        private DateTime tokenExpiration;
        
        public FirebaseUser CurrentUser => currentUser;
        public bool IsSignedIn => currentUser != null && !string.IsNullOrEmpty(idToken) && DateTime.UtcNow < tokenExpiration;
        
        public event Action<FirebaseUser> StateChanged;
        
        public FirebaseAuth(FirebaseConfig config, IHttpClient httpClient = null)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.httpClient = httpClient ?? new UnityHttpClient();
        }
        
        public async Task<FirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            var request = new SignInRequest
            {
                email = email,
                password = password,
                returnSecureToken = true
            };
            
            var response = await SendAuthRequestAsync<SignInRequest, SignInResponse>("accounts:signInWithPassword", request);
            return ProcessAuthResponse(response);
        }
        
        public async Task<FirebaseUser> CreateUserWithEmailAndPasswordAsync(string email, string password)
        {
            var request = new SignUpRequest
            {
                email = email,
                password = password,
                returnSecureToken = true
            };
            
            var response = await SendAuthRequestAsync<SignUpRequest, SignInResponse>("accounts:signUp", request);
            return ProcessAuthResponse(response);
        }
        
        public async Task<FirebaseUser> SignInAnonymouslyAsync()
        {
            var request = new AnonymousSignInRequest
            {
                returnSecureToken = true
            };
            
            var response = await SendAuthRequestAsync<AnonymousSignInRequest, SignInResponse>("accounts:signUp", request);
            return ProcessAuthResponse(response);
        }
        
        public async Task<FirebaseUser> SignInWithCustomTokenAsync(string customToken)
        {
            var request = new CustomTokenRequest
            {
                token = customToken,
                returnSecureToken = true
            };
            
            var response = await SendAuthRequestAsync<CustomTokenRequest, SignInResponse>("accounts:signInWithCustomToken", request);
            return ProcessAuthResponse(response);
        }
        
        public async Task SendPasswordResetEmailAsync(string email)
        {
            var request = new PasswordResetRequest
            {
                requestType = "PASSWORD_RESET",
                email = email
            };
            
            await SendAuthRequestAsync<PasswordResetRequest, object>("accounts:sendOobCode", request);
        }
        
        public async Task<string> GetIdTokenAsync(bool forceRefresh = false)
        {
            if (!IsSignedIn)
                throw new FirebaseException("not_signed_in", "User is not signed in");
                
            if (!forceRefresh && DateTime.UtcNow < tokenExpiration.AddMinutes(-5))
                return idToken;
                
            return await RefreshIdTokenAsync();
        }
        
        public void SignOut()
        {
            currentUser = null;
            idToken = null;
            refreshToken = null;
            tokenExpiration = DateTime.MinValue;
            StateChanged?.Invoke(null);
        }
        
        private async Task<string> RefreshIdTokenAsync()
        {
            if (string.IsNullOrEmpty(refreshToken))
                throw new FirebaseException("no_refresh_token", "No refresh token available");
                
            var request = new RefreshTokenRequest
            {
                grant_type = "refresh_token",
                refresh_token = refreshToken
            };
            
            var response = await SendAuthRequestAsync<RefreshTokenRequest, RefreshTokenResponse>("token", request, useSecureTokenEndpoint: true);
            
            idToken = response.access_token;
            refreshToken = response.refresh_token;
            tokenExpiration = DateTime.UtcNow.AddSeconds(int.Parse(response.expires_in));
            
            return idToken;
        }
        
        private FirebaseUser ProcessAuthResponse(SignInResponse response)
        {
            idToken = response.idToken;
            refreshToken = response.refreshToken;
            tokenExpiration = DateTime.UtcNow.AddSeconds(int.Parse(response.expiresIn));
            
            currentUser = new FirebaseUser
            {
                Uid = response.localId,
                Email = response.email,
                EmailVerified = response.emailVerified,
                DisplayName = response.displayName,
                PhotoUrl = response.photoUrl,
                IsAnonymous = string.IsNullOrEmpty(response.email)
            };
            
            StateChanged?.Invoke(currentUser);
            return currentUser;
        }
        
        private async Task<TResponse> SendAuthRequestAsync<TRequest, TResponse>(string endpoint, TRequest request, bool useSecureTokenEndpoint = false)
        {
            var baseUrl = useSecureTokenEndpoint 
                ? "https://securetoken.googleapis.com/v1" 
                : config.AuthBaseUrl;
            var url = $"{baseUrl}/{endpoint}?key={config.ApiKey}";
            
            var json = FirebaseUtils.ToJson(request);
            var response = await httpClient.SendRequestAsync(url, "POST", json);
            
            if (!response.IsSuccess)
            {
                var exception = CreateFirebaseException(response);
                throw exception;
            }
                
            if (typeof(TResponse) == typeof(object))
                return default;
                
            return FirebaseUtils.FromJson<TResponse>(response.Text);
        }
        
        private FirebaseException CreateFirebaseException(HttpResponse response)
        {
            var errorMessage = "Unknown error";
            var errorCode = "unknown";
            
            if (!string.IsNullOrEmpty(response.Text))
            {
                try
                {
                    var errorResponse = FirebaseUtils.FromJson<FirebaseErrorResponse>(response.Text);
                    if (errorResponse?.error != null)
                    {
                        errorMessage = errorResponse.error.message ?? errorMessage;
                        errorCode = errorResponse.error.code?.ToString() ?? errorCode;
                    }
                }
                catch
                {
                    errorMessage = response.Text;
                }
            }
            else if (response.ResponseCode == 0)
            {
                errorMessage = "Connection error";
                errorCode = "connection_error";
            }
            else if (response.ResponseCode >= 400)
            {
                errorMessage = $"HTTP Error {response.ResponseCode}";
                errorCode = $"http_{response.ResponseCode}";
            }
            
            return new FirebaseException(errorCode, errorMessage);
        }
    }
    
    [Serializable]
    public class FirebaseUser
    {
        public string Uid { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string DisplayName { get; set; }
        public string PhotoUrl { get; set; }
        public bool IsAnonymous { get; set; }
    }
    
    [Serializable]
    internal class SignInRequest
    {
        public string email;
        public string password;
        public bool returnSecureToken;
    }
    
    [Serializable]
    internal class SignUpRequest
    {
        public string email;
        public string password;
        public bool returnSecureToken;
    }
    
    [Serializable]
    internal class AnonymousSignInRequest
    {
        public bool returnSecureToken;
    }
    
    [Serializable]
    internal class CustomTokenRequest
    {
        public string token;
        public bool returnSecureToken;
    }
    
    [Serializable]
    internal class PasswordResetRequest
    {
        public string requestType;
        public string email;
    }
    
    [Serializable]
    internal class RefreshTokenRequest
    {
        public string grant_type;
        public string refresh_token;
    }
    
    [Serializable]
    internal class SignInResponse
    {
        public string kind;
        public string localId;
        public string email;
        public string displayName;
        public string idToken;
        public bool emailVerified;
        public string photoUrl;
        public string refreshToken;
        public string expiresIn;
    }
    
    [Serializable]
    internal class RefreshTokenResponse
    {
        public string access_token;
        public string expires_in;
        public string token_type;
        public string refresh_token;
        public string id_token;
        public string user_id;
        public string project_id;
    }
}