# Firebase REST API Wrapper for Unity WebGL

A comprehensive C# wrapper for Google Firebase REST APIs, specifically designed for Unity WebGL builds. This library provides direct REST API access without requiring JavaScript SDK communication, eliminating potential browser-to-Unity communication issues.

## Features

- **Pure C# Implementation**: Uses UnityWebRequest for WebGL compatibility
- **Complete Firebase Services**: Auth, Realtime Database, Firestore, Storage, and Cloud Messaging
- **Async/Await Pattern**: Modern asynchronous programming support
- **Automatic Token Management**: Handles authentication tokens automatically
- **Comprehensive Error Handling**: Detailed error messages and exception handling
- **Unity Optimized**: Designed specifically for Unity development

## Installation

1. Copy the `Firebase` folder into your Unity project's `Assets` directory
2. Add the required using statements to your scripts
3. Configure your Firebase project settings

## Quick Start

### 1. Configuration

```csharp
using Firebase.Core;

// Create Firebase configuration
var config = new FirebaseConfig(
    apiKey: "your-api-key",
    authDomain: "your-project.firebaseapp.com",
    databaseURL: "https://your-project-default-rtdb.firebaseio.com",
    projectId: "your-project-id",
    storageBucket: "your-project.appspot.com",
    messagingSenderId: "123456789",
    appId: "1:123456789:web:abcdef123456"
);
```

### 2. Authentication

```csharp
using Firebase.Core;

// Initialize Firebase Auth
var auth = new FirebaseAuth(config);

// Sign in with email/password
try 
{
    var user = await auth.SignInWithEmailAndPasswordAsync("user@example.com", "password");
    Debug.Log($"Signed in as: {user.Email}");
}
catch (FirebaseException ex)
{
    Debug.LogError($"Sign in failed: {ex.Message}");
}

// Create new user
var newUser = await auth.CreateUserWithEmailAndPasswordAsync("newuser@example.com", "password123");

// Sign in anonymously
var anonymousUser = await auth.SignInAnonymouslyAsync();

// Sign out
auth.SignOut();
```

### 3. Realtime Database

```csharp
using Firebase.Database;

// Initialize database
var database = new FirebaseDatabase(config, auth);

// Write data
await database.SetAsync("users/user1", new { name = "John", age = 30 });

// Read data
var userData = await database.GetAsync<UserData>("users/user1");

// Update data
await database.UpdateAsync("users/user1", new { age = 31 });

// Delete data
await database.DeleteAsync("users/user1");

// Using references
var userRef = database.GetReference("users").Child("user1");
await userRef.SetValueAsync(new { name = "Jane", age = 25 });
var data = await userRef.GetValueAsync<UserData>();

// Querying data
var query = DatabaseQuery.OrderByChild("age").LimitToFirst(10);
var results = await database.GetWithQueryAsync<Dictionary<string, UserData>>("users", query);
```

### 4. Firestore

```csharp
using Firebase.Firestore;

// Initialize Firestore
var firestore = new FirebaseFirestore(config, auth);

// Create document
var usersCollection = firestore.Collection("users");
var docRef = await usersCollection.AddAsync(new Dictionary<string, object>
{
    ["name"] = "John Doe",
    ["email"] = "john@example.com",
    ["age"] = 30
});

// Get document
var document = firestore.Document("users/user123");
var userData = await document.GetAsync<UserData>();

// Update document
await document.UpdateAsync(new Dictionary<string, object>
{
    ["age"] = 31,
    ["lastLogin"] = DateTime.UtcNow
});

// Query collection
var query = firestore.Collection("users")
    .Where("age", ">=", 18)
    .OrderBy("name")
    .Limit(10);
    
var results = await query.GetAsync();

// Delete document
await document.DeleteAsync();
```

### 5. Cloud Storage

```csharp
using Firebase.Storage;

// Initialize Storage
var storage = new FirebaseStorage(config, auth);

// Upload file
var storageRef = storage.GetReference("images/profile.jpg");
var downloadUrl = await storageRef.PutFileAsync("/path/to/local/file.jpg");

// Upload bytes
byte[] imageData = texture.EncodeToPNG();
var url = await storageRef.PutBytesAsync(imageData, "image/png");

// Upload texture
var textureUrl = await storageRef.PutTextureAsync(myTexture, "PNG");

// Download file
var downloadedData = await storageRef.GetBytesAsync();
var downloadedTexture = await storageRef.GetTextureAsync();

// Get download URL
var publicUrl = await storageRef.GetDownloadUrlAsync();

// Delete file
await storageRef.DeleteAsync();

// List files
var files = await storageRef.ListAllAsync();
```

### 6. Cloud Messaging

```csharp
using Firebase.Messaging;

// Initialize FCM (requires server key)
var messaging = new FirebaseCloudMessaging(config, "your-server-key");

// Send notification to token
var notification = messaging.CreateNotification("Hello", "This is a test message");
var response = await messaging.SendToTokenAsync("device-token", notification);

// Send to multiple tokens
string[] tokens = { "token1", "token2", "token3" };
await messaging.SendToTokensAsync(tokens, notification);

// Send to topic
await messaging.SendToTopicAsync("news", notification);

// Send data-only message
var data = new Dictionary<string, string>
{
    ["type"] = "update",
    ["version"] = "1.2.0"
};
await messaging.SendDataMessageAsync("device-token", data);

// Subscribe to topic
await messaging.SubscribeToTopicAsync("device-token", "news");
```

## Data Models

### User Data Example

```csharp
[System.Serializable]
public class UserData
{
    public string name;
    public string email;
    public int age;
    public long timestamp;
}
```

### Firestore Document Mapping

```csharp
// For Firestore, you can work with strongly-typed objects
public class BlogPost
{
    public string title;
    public string content;
    public string author;
    public DateTime createdAt;
    public string[] tags;
}

// Usage
var post = new BlogPost
{
    title = "My First Post",
    content = "Hello World!",
    author = "John Doe",
    createdAt = DateTime.UtcNow,
    tags = new[] { "hello", "world" }
};

await firestore.Collection("posts").AddAsync(post);
```

## Error Handling

All Firebase operations can throw `FirebaseException` with detailed error information:

```csharp
try
{
    await auth.SignInWithEmailAndPasswordAsync(email, password);
}
catch (FirebaseException ex)
{
    Debug.LogError($"Firebase Error [{ex.ErrorCode}]: {ex.Message}");
    
    switch (ex.ErrorCode)
    {
        case "invalid_email":
            // Handle invalid email
            break;
        case "wrong_password":
            // Handle wrong password
            break;
        case "user_not_found":
            // Handle user not found
            break;
    }
}
```

## WebGL Considerations

- All operations use `UnityWebRequest` for WebGL compatibility
- Async/await pattern works correctly in WebGL builds
- No external dependencies required
- CORS settings may need to be configured in Firebase Console
- File uploads work with byte arrays and Unity textures

## Security Rules

Make sure to configure appropriate Firebase Security Rules:

### Realtime Database Rules
```json
{
  "rules": {
    "users": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid"
      }
    }
  }
}
```

### Firestore Rules
```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
  }
}
```

### Storage Rules
```javascript
rules_version = '2';
service firebase.storage {
  match /b/{bucket}/o {
    match /users/{userId}/{allPaths=**} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
  }
}
```

## Limitations

- Cloud Messaging requires a server key (not recommended for client-side use in production)
- Some advanced Firestore features (transactions, batch operations) are not yet implemented
- Real-time listeners are not supported (REST API limitation)
- File uploads are limited by browser memory constraints

## Support

This wrapper covers the most commonly used Firebase features via REST APIs. For advanced features requiring real-time capabilities, consider using the official Firebase SDK for supported platforms.

## License

Zero-Clause BSD License, see License file (if its included, it doesnt have to be thats kind of the point!)