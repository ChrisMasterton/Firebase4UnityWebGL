using System;
using System.Collections.Generic;
using Firebase.Core;

namespace Firebase.Tests.Editor.TestUtilities
{
    public static class FirebaseTestData
    {
        public static readonly FirebaseConfig ValidConfig = new FirebaseConfig(
            "test-api-key-12345",
            "test-project.firebaseapp.com",
            "https://test-project-default-rtdb.firebaseio.com",
            "test-project-id",
            "test-project.appspot.com",
            "123456789",
            "1:123456789:web:abcdef123456"
        );

        public static readonly string ValidSignInResponse = @"{
            ""kind"": ""identitytoolkit#VerifyPasswordResponse"",
            ""localId"": ""test-user-id"",
            ""email"": ""test@example.com"",
            ""displayName"": ""Test User"",
            ""idToken"": ""eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test-token"",
            ""registered"": true,
            ""refreshToken"": ""test-refresh-token"",
            ""expiresIn"": ""3600""
        }";

        public static readonly string ValidSignUpResponse = @"{
            ""kind"": ""identitytoolkit#SignupNewUserResponse"",
            ""localId"": ""new-user-id"",
            ""email"": ""newuser@example.com"",
            ""refreshToken"": ""new-refresh-token"",
            ""idToken"": ""eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.new-token"",
            ""expiresIn"": ""3600""
        }";

        public static readonly string ValidAnonymousSignInResponse = @"{
            ""kind"": ""identitytoolkit#SignupNewUserResponse"",
            ""localId"": ""anonymous-user-id"",
            ""refreshToken"": ""anonymous-refresh-token"",
            ""idToken"": ""eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.anonymous-token"",
            ""expiresIn"": ""3600""
        }";

        public static readonly string DatabaseGetResponse = @"{
            ""name"": ""John Doe"",
            ""email"": ""john@example.com"",
            ""age"": 30
        }";

        public static readonly string DatabaseChildrenResponse = @"{
            ""user1"": {
                ""name"": ""John Doe"",
                ""email"": ""john@example.com"",
                ""age"": 30
            },
            ""user2"": {
                ""name"": ""Jane Smith"",
                ""email"": ""jane@example.com"",
                ""age"": 25
            }
        }";

        public static readonly string FirestoreDocumentResponse = @"{
            ""name"": ""projects/test-project/databases/(default)/documents/users/user123"",
            ""fields"": {
                ""name"": {
                    ""stringValue"": ""John Doe""
                },
                ""email"": {
                    ""stringValue"": ""john@example.com""
                },
                ""age"": {
                    ""integerValue"": ""30""
                }
            },
            ""createTime"": ""2023-01-01T00:00:00.000000Z"",
            ""updateTime"": ""2023-01-01T00:00:00.000000Z""
        }";

        public static readonly string FirestoreCollectionResponse = @"{
            ""documents"": [
                {
                    ""name"": ""projects/test-project/databases/(default)/documents/users/user1"",
                    ""fields"": {
                        ""name"": {
                            ""stringValue"": ""John Doe""
                        },
                        ""age"": {
                            ""integerValue"": ""30""
                        }
                    }
                },
                {
                    ""name"": ""projects/test-project/databases/(default)/documents/users/user2"",
                    ""fields"": {
                        ""name"": {
                            ""stringValue"": ""Jane Smith""
                        },
                        ""age"": {
                            ""integerValue"": ""25""
                        }
                    }
                }
            ]
        }";

        public static readonly string StorageUploadResponse = @"{
            ""name"": ""images/test-image.jpg"",
            ""bucket"": ""test-project.appspot.com"",
            ""generation"": ""1640995200000000"",
            ""contentType"": ""image/jpeg"",
            ""size"": ""102400"",
            ""downloadTokens"": ""test-download-token""
        }";

        public static readonly string ErrorResponse_InvalidEmail = @"{
            ""error"": {
                ""code"": 400,
                ""message"": ""INVALID_EMAIL"",
                ""errors"": [
                    {
                        ""message"": ""INVALID_EMAIL"",
                        ""domain"": ""global"",
                        ""reason"": ""invalid""
                    }
                ]
            }
        }";

        public static readonly string ErrorResponse_UserNotFound = @"{
            ""error"": {
                ""code"": 400,
                ""message"": ""EMAIL_NOT_FOUND"",
                ""errors"": [
                    {
                        ""message"": ""EMAIL_NOT_FOUND"",
                        ""domain"": ""global"",
                        ""reason"": ""invalid""
                    }
                ]
            }
        }";

        public static readonly string ErrorResponse_WrongPassword = @"{
            ""error"": {
                ""code"": 400,
                ""message"": ""INVALID_PASSWORD"",
                ""errors"": [
                    {
                        ""message"": ""INVALID_PASSWORD"",
                        ""domain"": ""global"",
                        ""reason"": ""invalid""
                    }
                ]
            }
        }";

        public static class TestUsers
        {
            public const string ValidEmail = "test@example.com";
            public const string ValidPassword = "testpassword123";
            public const string NewUserEmail = "newuser@example.com";
            public const string NewUserPassword = "newpassword456";
            public const string InvalidEmail = "invalid-email";
            public const string InvalidPassword = "wrong";
            public const string NonExistentEmail = "notfound@example.com";
        }

        public static class TestPaths
        {
            public const string UserPath = "users/user123";
            public const string UsersPath = "users";
            public const string PostsPath = "posts";
            public const string ImagePath = "images/test-image.jpg";
        }

        [Serializable]
        public class TestUserData
        {
            public string name;
            public string email;
            public int age;
            public long timestamp;

            public TestUserData()
            {
            }

            public TestUserData(string name, string email, int age)
            {
                this.name = name;
                this.email = email;
                this.age = age;
                this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }
    }
}