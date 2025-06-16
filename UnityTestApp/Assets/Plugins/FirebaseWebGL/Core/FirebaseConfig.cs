using System;
using UnityEngine;

namespace Firebase.Core
{
    [Serializable]
    public class FirebaseConfig
    {
        [SerializeField] private string apiKey;
        [SerializeField] private string authDomain;
        [SerializeField] private string databaseURL;
        [SerializeField] private string projectId;
        [SerializeField] private string storageBucket;
        [SerializeField] private string messagingSenderId;
        [SerializeField] private string appId;
        
        public string ApiKey => apiKey;
        public string AuthDomain => authDomain;
        public string DatabaseURL => databaseURL;
        public string ProjectId => projectId;
        public string StorageBucket => storageBucket;
        public string MessagingSenderId => messagingSenderId;
        public string AppId => appId;
        
        public string AuthBaseUrl => $"https://identitytoolkit.googleapis.com/v1";
        public string DatabaseBaseUrl => databaseURL?.TrimEnd('/');
        public string FirestoreBaseUrl => $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents";
        public string StorageBaseUrl => $"https://firebasestorage.googleapis.com/v0/b/{storageBucket}/o";
        public string FcmBaseUrl => "https://fcm.googleapis.com/fcm/send";
        
        public FirebaseConfig(string apiKey, string authDomain, string databaseURL, string projectId, 
                            string storageBucket, string messagingSenderId, string appId)
        {
            this.apiKey = apiKey;
            this.authDomain = authDomain;
            this.databaseURL = databaseURL;
            this.projectId = projectId;
            this.storageBucket = storageBucket;
            this.messagingSenderId = messagingSenderId;
            this.appId = appId;
        }
        
        public static FirebaseConfig CreateFromJson(string json)
        {
            return JsonUtility.FromJson<FirebaseConfig>(json);
        }
        
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(apiKey) && 
                   !string.IsNullOrEmpty(projectId) &&
                   !string.IsNullOrEmpty(databaseURL);
        }
    }
}